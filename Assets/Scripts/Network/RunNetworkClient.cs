using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class RunNetworkClient : MonoBehaviour
{
    public int BroadcastPort = 7777;
    public int BroadcastKey = 2222;
    public int BroadcastVersion = 1;
    public int BroadcastSubVersion = 1;
    public int BroadcastInterval = 1000;

    public bool IsRunning;
    public bool FoundBroadscast;

    public delegate void OnListenSuccessCallback(string fromAddress, string data);
    public OnListenSuccessCallback OnListenSuccess;

    public delegate void OnJoinCallback();
    public delegate void OnJoinFailedCallback();

    public OnJoinCallback OnJoin;
    public OnJoinFailedCallback OnJoinFailed;

    public delegate void OnRecievedServerInfoCallback(int fromConnectionId, int fromChannelId, int fromHostId, List<User> users);
    public OnRecievedServerInfoCallback OnRecievedServerInfo;
    //

    private HostTopology _topology;
    private QosType QosType;

    private string _broadcastData;
    private byte[] _msgOutBuffer;
    private byte[] _msgInBuffer;
    private Dictionary<string, NetworkBroadcastResult> _broadcastsReceived;

    public int HostId;
    public int ReliableChannel;

    private byte error;

    private void OnEnable()
    {
        InitClient();
        StartAsClient();
    }

    private void Update()
    {
        UpdateMessagePump();
    }

    private void InitClient()
    {
        DebugPanel.Phone.Log("1. Starting as client...");

        if (!NetworkTransport.IsStarted)
        {
            DebugPanel.Phone.Log("2. Starting NetworkTransport");
            NetworkTransport.Init();
        }

        //_broadcastData = "NetworkManager:" + NetworkManager.singleton.networkAddress + ":" + (object)NetworkManager.singleton.networkPort;

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

    public void StartAsClient()
    {
        HostId = NetworkTransport.AddHost(_topology, BroadcastPort);
        DebugPanel.Phone.Log("4. Added host: " + HostId + ", BroadcastPort: " + BroadcastPort);

        if (HostId < 0)
            Debug.LogError("4. Added host FAILED.");

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        DebugPanel.Phone.Log("5. Starting Client Listening");
        //_msgOutBuffer = StringToBytes(_broadcastData);
        _msgInBuffer = new byte[GameHiddenOptions.MAX_BYTE_SIZE];
        _broadcastsReceived = new Dictionary<string, NetworkBroadcastResult>();

        NetworkTransport.SetBroadcastCredentials(
            HostId,
            BroadcastKey,
            BroadcastVersion,
            BroadcastSubVersion,
            out error);

        IsRunning = true;

        DontDestroyOnLoad(gameObject);

        DebugPanel.Phone.Log("6. Listening is ON!");
    }

    private void UpdateMessagePump()
    {
        if (IsRunning == false) return;

        RecieveClient();
    }

    public void RecieveClient()
    {
        int fromHostId;
        int fromConnectionId;
        int fromChannelId;

        byte[] recBuffer = new byte[GameHiddenOptions.MAX_BYTE_SIZE];
        int dataSize;

        NetworkEventType networkEventType = NetworkTransport.Receive(
            out fromHostId,
            out fromConnectionId,
            out fromChannelId,
            //out HostId, out Main.Instance.ConnectionId, out ReliableChannel,
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

                OnData(fromHostId, fromConnectionId, fromChannelId, msg);
                break;
            case NetworkEventType.ConnectEvent:
                OnJoin();
                break;
            case NetworkEventType.DisconnectEvent:
                //OnDisconnectedFromServer();
                break;
            case NetworkEventType.BroadcastEvent:

                if (FoundBroadscast) return;

                NetworkTransport.GetBroadcastConnectionMessage(fromHostId, _msgInBuffer, GameHiddenOptions.MAX_BYTE_SIZE, out dataSize, out error);
                string address;
                int port;
                NetworkTransport.GetBroadcastConnectionInfo(fromHostId, out address, out port, out error);
                NetworkBroadcastResult networkBroadcastResult = new NetworkBroadcastResult();
                networkBroadcastResult.serverAddress = address;
                networkBroadcastResult.broadcastData = new byte[dataSize];
                System.Buffer.BlockCopy((System.Array)_msgInBuffer, 0, (System.Array)networkBroadcastResult.broadcastData, 0, dataSize);
                _broadcastsReceived[address] = networkBroadcastResult;

                OnListenSuccess(address, GameHiddenOptions.BytesToString(_msgInBuffer));

                DebugPanel.Phone.Log("Found SOMETHING! address: " + address + ", BytesToString(_msgInBuffer): " + GameHiddenOptions.BytesToString(_msgInBuffer));

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

                break;
            case Operation.ServerHandshake:

                var cast = (NetServerUsers)msg;
                var users = new List<User>();
                foreach (var usr in cast.Users)
                {
                    users.Add(
                        new User()
                        {
                            Name = usr.Name,
                            ProfilePicIndex = usr.Pic,
                            ConnectionId = usr.ConnId
                        });
                }
                OnRecievedServerInfo(fromConnectionId, fromChannelId, fromHostId, users);
                break;
            default:
                break;
        }
    }

    public void StopListening()
    {
        NetworkTransport.RemoveHost(HostId);
        IsRunning = false;

        DebugPanel.Phone.Log("Stop Listening HostId: " + HostId);
    }

    internal void SendToServer(NetMsg msg)
    {
        // this is where we hold our data.
        byte[] buffer = new byte[GameHiddenOptions.MAX_BYTE_SIZE];

        // this is where we will crush our data into a byte array

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);


        NetworkTransport.Send(HostId, Main.Instance.ConnectionId, ReliableChannel, buffer, GameHiddenOptions.MAX_BYTE_SIZE, out error);
        DebugPanel.Phone.Log("Try Sent message... error: " + (NetworkError)error);
    }
}
