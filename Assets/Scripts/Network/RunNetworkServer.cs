using System.Collections;
using System.Collections.Generic;
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

    public delegate void OnConnectedToServerCallback();
    public OnConnectedToServerCallback OnConnectedToServer;
    public delegate void OnDisconnectedFromServerCallback();
    public OnDisconnectedFromServerCallback OnDisconnectedFromServer;
    public delegate void OnMessageRecievedCallback();
    public OnMessageRecievedCallback OnMessageRecieved;
    //

    private HostTopology _topology;
    private QosType QosType;

    private string _broadcastData;
    private byte[] _msgOutBuffer;
    //private byte[] _msgInBuffer;
    private Dictionary<string, NetworkBroadcastResult> _broadcastsReceived;

    private int _hostId;
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
        int num = (int)defaultConfig.AddChannel(QosType);
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

        int recHostId;
        int connectionId;
        int channelId;

        byte[] recBuffer = new byte[GameHiddenOptions.MAX_BYTE_SIZE];
        int dataSize;

        NetworkEventType networkEventType = NetworkTransport.Receive(
            out recHostId,
            out connectionId,
            out channelId,
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

                

                DebugPanel.Phone.Log("SERVER recieved some data: recBuffer[0] = " + recBuffer);
                break;
            case NetworkEventType.ConnectEvent:
                DebugPanel.Phone.Log(string.Format(@"
                    User {0} has connected!
                    ",
                    connectionId
                    ));
                OnConnectedToServer();
                break;
            case NetworkEventType.DisconnectEvent:
                DebugPanel.Phone.Log(string.Format(@"
                    User {0} has left :( !
                    ",
                    connectionId
                    ));
                OnDisconnectedFromServer();
                break;
            case NetworkEventType.BroadcastEvent:
                DebugPanel.Phone.Log("a broadcast event ?");
                OnMessageRecieved();
                break;
            default:
                break;
        }
    }

    public void StopBroadcast()
    {
        NetworkTransport.StopBroadcastDiscovery();
        NetworkTransport.RemoveHost(_hostId);
    }
}
