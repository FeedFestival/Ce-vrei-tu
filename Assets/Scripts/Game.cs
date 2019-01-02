using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    [Header("Game States")]
    public GameObject SelectCategoryPanel;

    [Header("Debug Variables")]
    [HideInInspector]
    public Text TimeSpeedText;
    public GameObject SimulationNetworkPrefab;

    //

    private Dictionary<int, bool> _playersAcceptance;
    private Dictionary<int, string> _playersLies;

    private int _currentPlayerIndex;
    private int[] _orderedPlayers;
    private delegate void NextPhaseOfTheGameCallback();

    private Question _currentQuestion;

    private enum PhaseOfTheGame
    {
        StartGame,
        InputLies
    }
    private PhaseOfTheGame _phaseOfTheGame;

    internal void LoadDependencies()
    {

    }

    internal void StartGame()
    {
        if (Main._.IsSimulated)
        {
            var go = Instantiate(SimulationNetworkPrefab);
            go.GetComponent<RunNetworkSimulation>().Init();
        }

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

            OnClientsAcceptedStartingGame(Persistent.GameData.LoggedUser.ConnectionId, NetMessage.Ok);
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
        var currentConnectionId = _orderedPlayers[_currentPlayerIndex];
        var currentPlayer = Persistent.GameData.ServerUsers.Where(u => u.ConnectionId == currentConnectionId).FirstOrDefault();

        DebugPanel.Phone.Log("      - Send other players the info of who is chosing category ");

        sMsg = new SimpleMessage()
        {
            MessageCode = (byte)NetMessage.ConnectionIsPickingCategory,
            ConnId = currentConnectionId
        };
        var remainingPlayers = _orderedPlayers.Where(p => p != currentConnectionId).ToArray();
        Persistent.GameData.RunNetworkServer.SendToClients(sMsg, remainingPlayers);

        DebugPanel.Phone.Log("      - PickOne player and send him the PickCategory order ! User: " + currentPlayer);

        sMsg = new SimpleMessage()
        {
            MessageCode = (byte)NetMessage.DoPickCategory
        };
        Persistent.GameData.RunNetworkServer.OncategoryPicked = OncategoryPicked;
        Persistent.GameData.RunNetworkServer.SendToClients(sMsg, connectionId: currentConnectionId);
    }

    private void OncategoryPicked(int fromConnectionId, int categoryId)
    {
        var category = DomainLogic.DB.DataService.GetCategory(categoryId);
        DebugPanel.Phone.Log(" 3. Category was picked, its \"" + category.Name + "\". Now let's find a Good Question.");

        _currentQuestion = DomainLogic.DB.DataService.GetRandomQuestionByCategory(category.Id);

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
        DebugPanel.Phone.Log("      - The SERVER has said it's my turn !");

        // this needs to be called from an input
        OnChosingCategory();
    }

    private void OnCategoryPickingInfo(int connectionIdThatPicksCategory)
    {
        var userPicking = Persistent.GameData.ServerUsers.Where(u => u.ConnectionId == connectionIdThatPicksCategory).FirstOrDefault();

        DebugPanel.Phone.Log("      - The SERVER tolds us that this user is picking: " + userPicking);

        if (Main._.IsSimulated)
            DebugPanel.Phone.Log("3.    - Press [4](Server will send a question) ...");
    }

    public void OnChosingCategory()
    {
        var netMsg = new SimpleMessage()
        {
            MessageText = "Stiri",
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

        // Display Question

        // Display INput
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
