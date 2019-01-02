using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class RunNetworkServer : MonoBehaviour
{
    public int BroadcastPort = 7777;
    public int BroadcastKey = 2222;
    public int BroadcastVersion = 1;
    public int BroadcastSubVersion = 1;
    public int BroadcastInterval = 1000;

    public bool IsRunning;

    private RunNetworkSimulation _networkSimulation;

    public delegate void OnConnectedToServerCallback();
    public OnConnectedToServerCallback OnConnectedToServer;
    public delegate void OnDisconnectedFromServerCallback(int fromConnectionId);
    public OnDisconnectedFromServerCallback OnDisconnectedFromServer;
    public delegate void OnMessageRecievedCallback();
    public OnMessageRecievedCallback OnMessageRecieved;

    public delegate void OnRecievedClientInfoCallback(int fromConnectionId, int fromChannelId, int fromHostId, User user);
    public OnRecievedClientInfoCallback OnRecievedClientInfo;

    public delegate void OnClientsAcceptedStartingGameCallback(int fromConnectionId, NetMessage netMessage);
    public OnClientsAcceptedStartingGameCallback OnClientsAcceptedStartingGame;

    public delegate void OncategoryPickedCallback(int fromConnectionId, int categoryId);
    public OncategoryPickedCallback OncategoryPicked;

    public delegate void OnClientsAddedLiesCallback(int fromConnectionId, string lie);
    public OnClientsAddedLiesCallback OnClientsAddedLies;

    private HostTopology _topology;
    private QosType QosType;

    private string _broadcastData;
    private byte[] _msgOutBuffer;
    //private byte[] _msgInBuffer;
    private Dictionary<string, NetworkBroadcastResult> _broadcastsReceived;

    private int _hostId;
    private int ReliableChannel;
    private byte error;

    private void OnEnable()
    {
        InitServer();
        StartAsServer();
    }

    private void Update()
    {
        UpdateMessagePump();
    }

    private void InitServer()
    {
        DebugPanel.Phone.Log("1. Starting as server...");

        if (!NetworkTransport.IsStarted)
        {
            DebugPanel.Phone.Log("2. Starting NetworkTransport");
            NetworkTransport.Init();
        }

        _broadcastData = "NetworkManager:" + NetworkManager.singleton.networkAddress + ":" + (object)NetworkManager.singleton.networkPort;

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        DebugPanel.Phone.Log("3. Creating _broadcastData");
        DebugPanel.Phone.Log(" - " + _broadcastData);

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        ConnectionConfig defaultConfig = new ConnectionConfig();
        QosType = QosType.Reliable;
        ReliableChannel = (int)defaultConfig.AddChannel(QosType);
        _topology = new HostTopology(defaultConfig, 4);

        DebugPanel.Phone.Log("4. Created default configuration (HostTopology).");
    }

    public void StartAsServer()
    {
        _hostId = NetworkTransport.AddHost(_topology, BroadcastPort);
        DebugPanel.Phone.Log("4. Added host: " + _hostId);

        if (_hostId < 0)
            Debug.LogError("4. Added host FAILED.");

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        DebugPanel.Phone.Log("5. Starting Broadcast");
        _msgOutBuffer = GameHiddenOptions.StringToBytes(_broadcastData);
        //_msgInBuffer = new byte[GameHiddenOptions.MAX_BYTE_SIZE];
        _broadcastsReceived = new Dictionary<string, NetworkBroadcastResult>();

        bool isBroadcasting = NetworkTransport.StartBroadcastDiscovery(
            _hostId,
            BroadcastPort,
            BroadcastKey,
            BroadcastVersion,
            BroadcastSubVersion,
            _msgOutBuffer,
            _msgOutBuffer.Length,
            BroadcastInterval,
            out error
            );
        if (!isBroadcasting)
        {
            Debug.LogError((object)("NetworkDiscovery StartBroadcast failed err: " + (object)error));
            return;
        }
        IsRunning = true;

        DontDestroyOnLoad(gameObject);

        DebugPanel.Phone.Log("6. Broadcasting is ON!");
    }

    private void UpdateMessagePump()
    {
        if (IsRunning == false) return;

        int fromHostId;
        int fromConnectionId;
        int fromChannelId;

        byte[] recBuffer = new byte[GameHiddenOptions.MAX_BYTE_SIZE];
        int dataSize;

        NetworkEventType networkEventType = NetworkTransport.Receive(
            out fromHostId,
            out fromConnectionId,
            out fromChannelId,
            recBuffer,
            GameHiddenOptions.MAX_BYTE_SIZE,
            out dataSize,
            out error
            );

        switch (networkEventType)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.DataEvent:

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer);
                NetMsg msg = (NetMsg)formatter.Deserialize(ms);

                OnData(fromConnectionId, fromChannelId, fromHostId, msg);

                break;
            case NetworkEventType.ConnectEvent:
                OnConnectedToServer();
                break;
            case NetworkEventType.DisconnectEvent:
                OnDisconnectedFromServer(fromConnectionId);
                break;
            case NetworkEventType.BroadcastEvent:
                DebugPanel.Phone.Log("a broadcast event ?");
                OnMessageRecieved();
                break;
            default:
                break;
        }
    }

    private void OnData(int fromConnectionId, int fromChannelId, int fromHostId, NetMsg msg)
    {
        switch ((Operation)msg.OP)
        {
            case Operation.None:
                break;
            case Operation.ClientHandshake:

                var cast = (NetClientUser)msg;
                var user = new User()
                {
                    Name = cast.Name,
                    ProfilePicIndex = cast.Pic,
                    ConnectionId = fromConnectionId
                };
                OnRecievedClientInfo(fromConnectionId, fromChannelId, fromHostId, user);

                break;
            case Operation.ServerHandshake:
                break;

            case Operation.SimpleMessage:

                var sMsg = (SimpleMessage)msg;

                if ((NetMessage)sMsg.ThisMessageCodeIsFor == NetMessage.StartingGame)
                {
                    OnClientsAcceptedStartingGame(fromConnectionId, (NetMessage)sMsg.MessageCode);
                }
                else if ((NetMessage)sMsg.ThisMessageCodeIsFor == NetMessage.DoPickCategory)
                {
                    OncategoryPicked(fromConnectionId, sMsg.MessageId);
                }

                //OnSimpleMessage(fromConnectionId, msg);
                break;
            default:
                break;
        }
    }

    internal void SendToClients(NetMsg msg, int[] connIds = null, int? connectionId = null)
    {
        if (Main._.IsSimulated)
        {
            _networkSimulation.SendToClients(msg, connIds, connectionId);
            return;
        }

        // this is where we hold our data.
        byte[] buffer = new byte[GameHiddenOptions.MAX_BYTE_SIZE];

        // this is where we will crush our data into a byte array

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);

        if (connIds == null)
            connIds = new int[1] { connectionId.Value };

        if (connIds.Length == 0) return;

        foreach (var connId in connIds)
        {
            if (connId == Persistent.GameData.LoggedUser.ConnectionId)
                continue;

            NetworkTransport.Send(_hostId, connId, ReliableChannel, buffer, GameHiddenOptions.MAX_BYTE_SIZE, out error);
            DebugPanel.Phone.Log("Try Sent message to CLIENTS[" + connId + "]... error: " + (NetworkError)error);
        }
    }

    public void StopBroadcast()
    {
        NetworkTransport.StopBroadcastDiscovery();
        NetworkTransport.RemoveHost(_hostId);
    }

    public void StartSimulation(RunNetworkSimulation networkSimulation)
    {
        if (IsRunning)
            StopBroadcast();

        _networkSimulation = networkSimulation;
        _networkSimulation.OnData = OnData;
    }
}
