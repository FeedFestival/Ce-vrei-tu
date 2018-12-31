using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    public Text TimeSpeedText;

    public GameObject SimulationNetworkPrefab;

    private Dictionary<int, bool> _clientsAcceptance;

    private int _currentPlayerIndex;
    private int[] _orderedPlayers;

    internal void LoadDependencies()
    {

    }

    internal void StartGame()
    {
        if (Main._.IsSimulated) SimulateStartGame();

        if (Persistent.GameData.IsServer)
        {
            Persistent.GameData.RunNetworkServer.OnClientsAcceptedStartingGame = OnClientsAcceptedStartingGame;

            var netMsg = new SimpleMessage()
            {
                MessageCode = (byte)NetMessage.StartingGame
            };

            if (_clientsAcceptance == null)
                InitClientAcceptance();

            DebugPanel.Phone.Log("1. SERVER tell other CLIENTS game is starting. Wait for response from all.");

            Persistent.GameData.RunNetworkServer.SendToClients(netMsg, _clientsAcceptance.Keys.ToArray());
        }
        else
        {
            // here as a CLIENT we came when we recieved from SERVER the:  (byte)NetMessage.StartingGame - MessageCode

            // now we have to send 'OK' TO the SERVER.
            var netMsg = new SimpleMessage()
            {
                MessageCode = (byte)NetMessage.Ok,
                ThisMessageCodeIsFor = (byte)NetMessage.StartingGame
            };
            Persistent.GameData.RunNetworkClient.SendToServer(netMsg);

            DebugPanel.Phone.Log("2.    SERVER will send [1](YourTurnToPick), [2](PickingInfo).");

            // now we wait for SERVER to SEND US WHO IS PICKING THE CATEOGRY.
            Persistent.GameData.RunNetworkClient.OnYourTurnToPickCategory = OnYourTurnToPickCategory;
            Persistent.GameData.RunNetworkClient.OnCategoryPickingInfo = OnCategoryPickingInfo;
            // 
            Persistent.GameData.RunNetworkClient.OnRecievedQuestion = OnRecievedQuestion;
        }
    }

    #region Server Methods


    private void OnClientsAcceptedStartingGame(int fromConnectionId, NetMessage netMessage)
    {
        bool ok = netMessage == NetMessage.Ok;
        DebugPanel.Phone.Log(" ConnectionId: " + fromConnectionId + " replied with " + netMessage);

        _clientsAcceptance[fromConnectionId] = true;

        bool canWeAdvance = true;
        foreach (var client in _clientsAcceptance)
        {
            if (client.Value == false)
            {
                canWeAdvance = false;
                break;
            }
        }

        if (canWeAdvance)
        {
            _clientsAcceptance = null;
            OrderPlayers();
            StartActualGame();
        }
    }

    private void OrderPlayers()
    {
        _orderedPlayers = Persistent.GameData.ServerUsers.Select(u => u.ConnectionId).ToArray();

        System.Random rnd = new System.Random();
        _orderedPlayers = _orderedPlayers.OrderBy(x => rnd.Next()).ToArray();

        DebugPanel.Phone.Log(" 2.   - Order is randomized: " + _orderedPlayers);
    }

    public void StartActualGame()
    {
        _currentPlayerIndex = -1;

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

    private void OncategoryPicked(int fromConnectionId, string category)
    {
        DebugPanel.Phone.Log(" 3. Category was picked, its \"" + category + "\". Now let's find a Good Question.");
    }

    private void InitClientAcceptance()
    {
        if (_clientsAcceptance == null)
            _clientsAcceptance = new Dictionary<int, bool>();

        //if (_clientsAcceptance.Count == Persistent.GameData.ServerUsers)

        var connIds = Persistent.GameData.ServerUsers.Select(u => u.ConnectionId).ToArray();
        foreach (var id in connIds)
        {
            _clientsAcceptance.Add(id, false);
        }
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
    }

    public void OnChosingCategory()
    {

        var netMsg = new SimpleMessage()
        {
            MessageText = "Stiri",
            ThisMessageCodeIsFor = (byte)NetMessage.DoPickCategory
        };
        Persistent.GameData.RunNetworkClient.SendToServer(netMsg);
    }

    #endregion

    #region S/C Shared Methods

    public void OnRecievedQuestion()
    {
        // Display Question

        // Display INput
    }

    #endregion



    #region Simulation Functions

    private void SimulateStartGame()
    {
        var go = Instantiate(SimulationNetworkPrefab);

        if (Persistent.GameData.IsServer)
            Persistent.GameData.RunNetworkServer.gameObject.SetActive(true);
        else
            Persistent.GameData.RunNetworkClient.gameObject.SetActive(true);

        Persistent.GameData.LoggedUser = DomainLogic.DB.DataService.GetDeviceUser();

        Persistent.GameData.ServerUsers = new List<User>() {
            Persistent.GameData.LoggedUser,

            new User() { ConnectionId = 1, Name = "Player2", ProfilePicIndex = 11, Saying = "Player2 Saying" },
            new User() { ConnectionId = 2, Name = "Player3", ProfilePicIndex = 4, Saying = "Player3 Saying" },
            new User() { ConnectionId = 3, Name = "Player4", ProfilePicIndex = 7, Saying = "Player4 Saying" }
        };
    }



    #endregion


    #region Global WaitForSeconds, TimeSpeed

    private float _timeSpeed = 1.0f;

    /// <summary>
    /// Usefull for debuging
    /// </summary>
    public void ChangeTimeSpeed(float timeSpeed)
    {
        Time.timeScale = timeSpeed;
    }

    #endregion

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
}
