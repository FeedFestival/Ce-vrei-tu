using System.Collections;
using System.Collections.Generic;
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

    public delegate void OnListenSuccessCallback(string fromAddress, string data);
    public OnListenSuccessCallback OnListenSuccess;

    //

    private HostTopology _topology;
    private QosType QosType;

    private const int MAX_BYTE_SIZE = 1024;

    private string _broadcastData;
    private byte[] _msgOutBuffer;
    private byte[] _msgInBuffer;
    private Dictionary<string, NetworkBroadcastResult> _broadcastsReceived;

    public int HostId;
    private byte error;

    private void OnEnable()
    {
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
        int num = (int)defaultConfig.AddChannel(QosType);
        _topology = new HostTopology(defaultConfig, 4);

        DebugPanel.Phone.Log("4. Created default configuration (HostTopology).");
    }

    private void StartAsClient()
    {
        InitClient();

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        HostId = NetworkTransport.AddHost(_topology, BroadcastPort);
        DebugPanel.Phone.Log("4. Added host: " + HostId + ", BroadcastPort: " + BroadcastPort);

        if (HostId < 0)
            Debug.LogError("4. Added host FAILED.");

        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        DebugPanel.Phone.Log("5. Starting Client Listening");
        //_msgOutBuffer = StringToBytes(_broadcastData);
        _msgInBuffer = new byte[MAX_BYTE_SIZE];
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

        int connectionId;
        int channelId;
        int receivedSize;

        NetworkEventType networkEventType = NetworkTransport.ReceiveFromHost(HostId, out connectionId, out channelId, _msgInBuffer, MAX_BYTE_SIZE, out receivedSize, out error);

        switch (networkEventType)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.DataEvent:
                DebugPanel.Phone.Log("a data event ?");
                break;
            case NetworkEventType.ConnectEvent:
                DebugPanel.Phone.Log(string.Format(@"
                    User {0} has connected!
                    ",
                    connectionId
                    ));
                break;
            case NetworkEventType.DisconnectEvent:
                DebugPanel.Phone.Log(string.Format(@"
                    User {0} has left :( !
                    ",
                    connectionId
                    ));
                break;
            case NetworkEventType.BroadcastEvent:
                NetworkTransport.GetBroadcastConnectionMessage(HostId, _msgInBuffer, MAX_BYTE_SIZE, out receivedSize, out error);
                string address;
                int port;
                NetworkTransport.GetBroadcastConnectionInfo(HostId, out address, out port, out error);
                NetworkBroadcastResult networkBroadcastResult = new NetworkBroadcastResult();
                networkBroadcastResult.serverAddress = address;
                networkBroadcastResult.broadcastData = new byte[receivedSize];
                System.Buffer.BlockCopy((System.Array)_msgInBuffer, 0, (System.Array)networkBroadcastResult.broadcastData, 0, receivedSize);
                _broadcastsReceived[address] = networkBroadcastResult;

                OnReceivedBroadcast(address, BytesToString(_msgInBuffer));

                IsRunning = false;

                break;
            default:
                break;
        }
    }

    private void OnReceivedBroadcast(string fromAddress, string data)
    {
        OnListenSuccess(fromAddress, data);


    }

    private static byte[] StringToBytes(string str)
    {
        byte[] numArray = new byte[str.Length * 2];
        System.Buffer.BlockCopy((System.Array)str.ToCharArray(), 0, (System.Array)numArray, 0, numArray.Length);
        return numArray;
    }

    private static string BytesToString(byte[] bytes)
    {
        char[] chArray = new char[bytes.Length / 2];
        System.Buffer.BlockCopy((System.Array)bytes, 0, (System.Array)chArray, 0, bytes.Length);
        return new string(chArray);
    }
}
