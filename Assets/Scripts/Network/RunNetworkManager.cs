using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RunNetworkManager : NetworkManager
{
    public int ConnectionId;

    private int hostId;
    private string networkAddress;
    private int networkPort;
    private byte error;

    public delegate void OnJoinCallback();
    public delegate void OnJoinFailedCallback();

    NetworkClient networkClient;

    OnJoinCallback OnJoin;
    OnJoinFailedCallback OnJoinFailed;

    public void JoinGame(int host, string ipAddress, int port, OnJoinCallback onJoin, OnJoinFailedCallback onJoinFailed)
    {
        hostId = host;
        networkAddress = ipAddress;
        networkPort = port;

        OnJoin = onJoin;
        OnJoinFailed = onJoinFailed;

        StartCoroutine(JoiningGame());
    }

    IEnumerator JoiningGame()
    {
        singleton.networkAddress = networkAddress;
        singleton.networkPort = networkPort;

        //networkClient = singleton.StartClient();
        
        //networkClient.RegisterHandler(MsgType.Connect, OnConnect);
        //networkClient.RegisterHandler(MsgType.Error, OnError);
        //networkClient.RegisterHandler(MsgType.Disconnect, OnDisconnect);

        DebugPanel.Phone.Log("AttemptConnect: " + hostId + ", " + networkAddress + ", " + networkPort);

        ConnectionId = NetworkTransport.Connect(hostId, networkAddress, networkPort, 0, out error);

        yield return new WaitForSeconds(1f);

        DebugPanel.Phone.Log(@"Connected: ConnectionId: " + ConnectionId +
            ", error: " + error
            );
        OnJoin();
    }

    IEnumerator IfConnectedSendMessage()
    {
        yield return new WaitForSeconds(1f);

        StartCoroutine(IfConnectedSendMessage());

        short s = 1_034;
        var msg = new RegisterHostMessage();
        msg.gameName = "s";
        msg.comment = "test";
        networkClient.Send(s, msg);
    }

    public void OnCRCCheck(NetworkMessage msg)
    {
        DebugPanel.Phone.Log("CRCCHECK");
    }

    public void OnConnect(NetworkMessage msg)
    {
        DebugPanel.Phone.Log("Connected to server");
    }

    private void OnError(NetworkMessage netMsg)
    {
        OnJoinFailed();
        UnityEngine.Networking.NetworkSystem.ErrorMessage error = netMsg.ReadMessage<UnityEngine.Networking.NetworkSystem.ErrorMessage>();
        DebugPanel.Phone.Log("Error while connecting: " + error.errorCode);
    }

    private void OnDisconnect(NetworkMessage netMsg)
    {
        OnJoinFailed();
        UnityEngine.Networking.NetworkSystem.ErrorMessage error = netMsg.ReadMessage<UnityEngine.Networking.NetworkSystem.ErrorMessage>();
        DebugPanel.Phone.Log("Disconnected: " + error.errorCode);
    }

    //Detect when a client connects to the Server
    public override void OnClientConnect(NetworkConnection connection)
    {
        OnJoin();
    }

    //Detect when a client connects to the Server
    public override void OnStopClient()
    {
        DebugPanel.Phone.Log("Stopped Client: ");
    }

    public override void OnClientError(NetworkConnection connection, int errorCode)
    {
        OnJoinFailed();
        DebugPanel.Phone.Log("Not Connected: " + connection.connectionId + ", errorCode: " + errorCode);
    }

    //Detect when a client connects to the Server
    public override void OnClientDisconnect(NetworkConnection connection)
    {
        OnJoinFailed();
        DebugPanel.Phone.Log("Not Connected: " + connection.connectionId);
    }
}

public class RegisterHostMessage : MessageBase
{
    public string gameName;
    public string comment;
    public bool passwordProtected;
}