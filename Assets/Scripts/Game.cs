using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [Header("Game States")]
    public StartGamePanelController StartGamePanel;
    public SelectCategoryPanelController SelectCategoryPanel;
    public SomeoneSelectingCategoryPanelController SomeoneSelectingCategoryPanel;
    public QuestionAndAnswerPanelController QuestionAndAnswerPanel;

    [Header("Debug Variables")]
    [HideInInspector]
    public Text TimeSpeedText;
    public GameObject SimulationNetworkPrefab;

    public bool CategoryPicked;
    //

    private Dictionary<int, bool> _playersAcceptance;
    private Dictionary<int, string> _playersLies;

    private int _currentPlayerIndex;
    private int _currentlyPickingConnectionId;

    private int[] _orderedPlayers;
    private delegate void NextPhaseOfTheGameCallback();

    List<Category> AllCategories;
    private Question _currentQuestion;

    private enum PhaseOfTheGame
    {
        StartGame,
        InputLies
    }
    private PhaseOfTheGame _phaseOfTheGame;

    internal void LoadDependencies()
    {
        StartGamePanel.gameObject.SetActive(true);
        SelectCategoryPanel.gameObject.SetActive(false);
        SomeoneSelectingCategoryPanel.gameObject.SetActive(false);
        QuestionAndAnswerPanel.gameObject.SetActive(false);
    }

    internal void StartGame()
    {
        CategoryPicked = false;

        if (Main._.IsSimulated)
        {
            var go = Instantiate(SimulationNetworkPrefab);
            go.GetComponent<RunNetworkSimulation>().Init();
        }

        StartGamePanel.Init();

        if (Persistent.GameData.IsServer)
        {
            Persistent.GameData.RunNetworkServer.OnClientsAcceptedStartingGame = OnClientsAcceptedStartingGame;

            InitClientAcceptance();

            var netMsg = new SimpleMessage()
            {
                MessageCode = (byte)NetMessage.StartingGame
            };
            Persistent.GameData.RunNetworkServer.SendToClients(netMsg, _playersAcceptance.Keys.ToArray());

            DebugPanel.Phone.Log("1. SERVER tell other CLIENTS game is starting. Wait for response from all.");

            // this user hosting accept automatically.
            OnClientsAcceptedStartingGame(Persistent.GameData.LoggedUser.ConnectionId, NetMessage.Ok);

            DebugPanel.Phone.Log("      - Press ENTER to recieve Ok's from Clients.");
        }
        else
        {
            DebugPanel.Phone.Log(@"1. - here as a CLIENT we came when we recieved from SERVER the StartingGame MessageCode.
    - now we have to send 'OK' TO the SERVER.
            ");

            var netMsg = new SimpleMessage()
            {
                MessageCode = (byte)NetMessage.Ok,
                ThisMessageCodeIsFor = (byte)NetMessage.StartingGame
            };
            Persistent.GameData.RunNetworkClient.SendToServer(netMsg);

            if (Main._.IsSimulated)
                DebugPanel.Phone.Log("2.    SERVER will eventually send [1](YourTurnToPick), [2](PickingInfo).");

            Persistent.GameData.RunNetworkClient.OnYourTurnToPickCategory = OnYourTurnToPickCategory;
            Persistent.GameData.RunNetworkClient.OnCategoryPickingInfo = OnCategoryPickingInfo;
            // 
            Persistent.GameData.RunNetworkClient.OnRecievedQuestion = OnRecievedQuestion;
        }

        StartCoroutine(GetCategoriesAsync());
    }

    #region Server Methods


    private void OnClientsAcceptedStartingGame(int fromConnectionId, NetMessage netMessage)
    {
        DebugPanel.Phone.Log(" ConnectionId: " + fromConnectionId + " replied with " + netMessage);

        _playersAcceptance[fromConnectionId] = netMessage == NetMessage.Ok;
        CanWeAdvance(StartActualGame);
    }

    private void OrderPlayers()
    {
        _orderedPlayers = Persistent.GameData.ServerUsers.Select(u => u.ConnectionId).ToArray();

        System.Random rnd = new System.Random();
        _orderedPlayers = _orderedPlayers.OrderBy(x => rnd.Next()).ToArray();
        _currentPlayerIndex = -1;

        DebugPanel.Phone.Log(" 2.   - Order is randomized: " + _orderedPlayers);
    }

    public void StartActualGame()
    {
        OrderPlayers();
        NextPlayerTurn();
    }

    public void NextPlayerTurn()
    {
        NetMsg sMsg;
        _currentPlayerIndex++;
        _currentlyPickingConnectionId = _orderedPlayers[_currentPlayerIndex];

        // [TESTING] - this is so we can hardcode the user picking category
        _currentlyPickingConnectionId = Persistent.GameData.LoggedUser.ConnectionId;

        var currentPlayer = Persistent.GameData.ServerUsers.Where(u => u.ConnectionId == _currentlyPickingConnectionId).FirstOrDefault();

        DebugPanel.Phone.Log("      - Pick one player ! User: " + currentPlayer);
        DebugPanel.Phone.Log("      - Send other players the info of who is chosing category ");

        sMsg = new SimpleMessage()
        {
            MessageCode = (byte)NetMessage.ConnectionIsPickingCategory,
            ConnId = _currentlyPickingConnectionId
        };
        var remainingPlayers = _orderedPlayers.Where(p => p != _currentlyPickingConnectionId).ToArray();
        Persistent.GameData.RunNetworkServer.SendToClients(sMsg, remainingPlayers);

        if (Persistent.GameData.IsServer)
        {
            if (Persistent.GameData.LoggedUser.ConnectionId != _currentlyPickingConnectionId)
            {
                DebugPanel.Phone.Log("      - Send the PickCategory order ! User: " + currentPlayer);

                sMsg = new SimpleMessage()
                {
                    MessageCode = (byte)NetMessage.DoPickCategory
                };
                Persistent.GameData.RunNetworkServer.OnCategoryPicked = OnCategoryPicked;
                Persistent.GameData.RunNetworkServer.SendToClients(sMsg, connectionId: _currentlyPickingConnectionId);

                // here the host needs to recieve the category info too. if he is not to pick.
                OnCategoryPickingInfo();
            }
            else
            {
                // the server can be asked to pick a category.
                OnYourTurnToPickCategory();
            }
        }
    }

    private void OnCategoryPicked(int fromConnectionId, int categoryId, bool randomlyPicked = false)
    {
        if (CategoryPicked) return;

        CategoryPicked = true;
        var category = AllCategories.FirstOrDefault(c => c.Id == categoryId);
        DebugPanel.Phone.Log(" 3. Category was picked, its \"" + category.Name + "\". Now let's find a Good Question.");

        _currentQuestion = DomainLogic.DB.DataService.GetRandomQuestionByCategory(category);

        DebugPanel.Phone.Log(" 4. Found a question now lets send it to everyone");
        DebugPanel.Phone.Log("       - AND - Wait for everyones response: ");

        _phaseOfTheGame = PhaseOfTheGame.InputLies;
        InitClientLies();

        var qMsg = new QuestionMessage()
        {
            Question = _currentQuestion
        };
        Persistent.GameData.RunNetworkServer.OnClientsAddedLies = OnClientsAddedLies;
        Persistent.GameData.RunNetworkServer.SendToClients(qMsg, _playersLies.Keys.ToArray());

        OnRecievedQuestion();
    }

    public void OnClientsAddedLies(int fromConnectionId, string lie)
    {


        CanWeAdvance(StartPickAnswersPhase);
    }

    public void StartPickAnswersPhase()
    {

    }

    #endregion



    #region Client Methods

    private void OnYourTurnToPickCategory()
    {
        if (StartGamePanel.gameObject.activeSelf)
            StartGamePanel.GameIsReady(() =>
            {
                // show input for picking category
                SelectCategoryPanel.gameObject.SetActive(true);
                if (Persistent.GameData.IsServer)
                    SelectCategoryPanel.OnChosingCategory = (categoryId) =>
                    {
                        OnCategoryPicked(Persistent.GameData.LoggedUser.ConnectionId, categoryId);
                    };
                else
                    SelectCategoryPanel.OnChosingCategory = OnClientChoseCategory;
                SelectCategoryPanel.Init(AllCategories);
                SelectCategoryPanel.InGameClock.OnStop = OnCategoryPickingTimeExpired;
            });

        DebugPanel.Phone.Log("      - The SERVER has said it's my turn !");
    }

    private void OnCategoryPickingInfo(int connectionIdThatPicksCategory = 0)
    {
        if (connectionIdThatPicksCategory != 0)
            _currentlyPickingConnectionId = connectionIdThatPicksCategory;

        var userPicking = Persistent.GameData.ServerUsers.Where(u => u.ConnectionId == _currentlyPickingConnectionId).FirstOrDefault();

        DebugPanel.Phone.Log("      - The SERVER tolds us that this user is picking: " + userPicking);

        if (StartGamePanel.gameObject.activeSelf)
            StartGamePanel.GameIsReady(() =>
            {
                // show info message on who is picking.
                SomeoneSelectingCategoryPanel.gameObject.SetActive(true);
                SomeoneSelectingCategoryPanel.SetWhoIsPicking(userPicking.Name);
                if (Persistent.GameData.IsServer)
                {
                    SomeoneSelectingCategoryPanel.InGameClock.OnStop = OnCategoryPickingTimeExpired;
                }
            });

        if (Main._.IsSimulated)
        {
            if (Persistent.GameData.IsServer)
            {
                DebugPanel.Phone.Log("3.    - Press [1](That user will send a Category) ...");
                DebugPanel.Phone.Log("      - Press [2](Time will expire) ...");
            }
            else
            {
                DebugPanel.Phone.Log("3.    - Press [4](Server will send a question) ...");
            }
        }
    }

    IEnumerator GetCategoriesAsync()
    {

        yield return new WaitForSeconds(0.1f);
        AllCategories = DomainLogic.DB.DataService.GetAllCategories();
    }

    public void OnClientChoseCategory(int categoryId)
    {
        SelectCategoryPanel.InGameClock.StopClock();

        var category = AllCategories.FirstOrDefault(c => c.Id == categoryId);
        var netMsg = new SimpleMessage()
        {
            MessageText = category.Name,
            ThisMessageCodeIsFor = (byte)NetMessage.DoPickCategory
        };

        Persistent.GameData.RunNetworkClient.SendToServer(netMsg);

        if (Main._.IsSimulated)
            DebugPanel.Phone.Log("3.    - Press [4](Server will send a question) ...");
    }

    #endregion

    #region S/C Shared Methods

    private void CanWeAdvance(NextPhaseOfTheGameCallback nextPhaseOfTheGame)
    {
        bool canWeAdvance = true;
        switch (_phaseOfTheGame)
        {
            case PhaseOfTheGame.StartGame:
                foreach (var client in _playersAcceptance)
                {
                    if (client.Value == false)
                    {
                        canWeAdvance = false;
                        break;
                    }
                }
                break;
            case PhaseOfTheGame.InputLies:
                foreach (var client in _playersLies)
                {
                    if (string.IsNullOrWhiteSpace(client.Value))
                    {
                        canWeAdvance = false;
                        break;
                    }
                }
                break;
            default:
                break;
        }

        if (canWeAdvance)
            nextPhaseOfTheGame();
    }

    public void OnRecievedQuestion(QuestionMessage questionMessage = null)
    {
        if (questionMessage != null)
            _currentQuestion = questionMessage.Question;

        DebugPanel.Phone.Log("3.    SERVER sent question: " + _currentQuestion.Text);

        if (SelectCategoryPanel.gameObject.activeSelf)
        {
            SelectCategoryPanel.InGameClock.StopClock();
            SelectCategoryPanel.gameObject.SetActive(false);
        }
        else if (SomeoneSelectingCategoryPanel.gameObject.activeSelf)
        {
            SomeoneSelectingCategoryPanel.InGameClock.StopClock();
            SomeoneSelectingCategoryPanel.gameObject.SetActive(false);
        }

        // Display Question
        QuestionAndAnswerPanel.gameObject.SetActive(true);
        QuestionAndAnswerPanel.Init(_currentQuestion);
        QuestionAndAnswerPanel.InGameClock.OnStop = OnLieInputTimeExpired;
        QuestionAndAnswerPanel.OnLieComplete = OnLieComplete;
    }

    public void OnLieInputTimeExpired()
    {
        OnLieComplete(string.Empty);
    }

    public void OnLieComplete(string lie)
    {
        if (Persistent.GameData.IsServer)
        {
            OnOthersAddingLies(Persistent.GameData.LoggedUser.ConnectionId, lie);
        }
        else
        {
            var netMsg = new SimpleMessage()
            {
                MessageText = lie,
                MessageCode = (byte)NetMessage.LieAdded
            };
            Persistent.GameData.RunNetworkClient.SendToServer(netMsg);

            // => the client will recieve from the server the fact that he added his lie.
        }
    }

    public void OnOthersAddingLies(int connectionId, string lie)
    {
        _playersLies[connectionId] = lie;
        var userThatAddedLie = Persistent.GameData.ServerUsers.FirstOrDefault(su => su.ConnectionId == connectionId);
        QuestionAndAnswerPanel.ShowAvatarInputed(userThatAddedLie);
    }

    #endregion

    private void InitClientAcceptance()
    {
        if (_playersAcceptance == null)
        {
            _playersAcceptance = new Dictionary<int, bool>();

            var connIds = Persistent.GameData.ServerUsers.Select(u => u.ConnectionId).ToArray();
            foreach (var id in connIds) { _playersAcceptance.Add(id, false); }
        }
        else
        {
            var keyArray = _playersAcceptance.Keys.ToArray();
            foreach (var playerId in keyArray) { _playersAcceptance[playerId] = false; }
        }
    }

    private void InitClientLies()
    {
        if (_playersLies == null)
        {
            _playersLies = new Dictionary<int, string>();

            var connIds = Persistent.GameData.ServerUsers.Select(u => u.ConnectionId).ToArray();
            foreach (var id in connIds) { _playersLies.Add(id, string.Empty); }
        }
        else
        {
            var keyArray = _playersLies.Keys.ToArray();
            foreach (var playerId in keyArray)
            {
                _playersLies[playerId] = string.Empty;
            }
        }
    }

    private void OnCategoryPickingTimeExpired()
    {
        // 1. pick a category randomly so the game can progress.
        var index = UnityEngine.Random.Range(0, AllCategories.Count - 1);
        OnCategoryPicked(_currentlyPickingConnectionId, AllCategories[index].Id, true);
    }

    #region TimeSpeed DEBUG Functions

    private float _timeSpeed = 1.0f;
    public void ChangeTimeSpeed(float timeSpeed)
    {
        Time.timeScale = timeSpeed;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            var newSpeed = _timeSpeed + 0.1f;
            if (newSpeed > 1f)
                return;
            _timeSpeed = _timeSpeed + 0.1f;

            TimeSpeedText.text = _timeSpeed.ToString();
            Time.timeScale = _timeSpeed;
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            var newSpeed = _timeSpeed - 0.1f;
            if (newSpeed < 0f)
                return;
            _timeSpeed = _timeSpeed - 0.1f;

            TimeSpeedText.text = _timeSpeed.ToString();
            Time.timeScale = _timeSpeed;
        }
    }

    #endregion
}
