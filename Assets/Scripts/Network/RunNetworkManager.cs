using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RunNetworkManager : NetworkManager
{
    private int hostId;
    private string networkAddress;
    private int networkPort;
    private byte error;

    public void JoinGame(int host, string ipAddress, int port)
    {
        hostId = host;
        networkAddress = ipAddress;
        networkPort = port;

        DebugPanel.Phone.Log("AttemptConnect: " + hostId + ", " + networkAddress + ", " + networkPort + "... error: " + ((NetworkError)error).ToString());

        Main.Instance.ConnectionId = NetworkTransport.Connect(hostId, networkAddress, networkPort, 0, out error);
    }
    
    IEnumerator IfConnectedSendMessage()
    {
        yield return new WaitForSeconds(1f);

        StartCoroutine(IfConnectedSendMessage());

        short s = 1_034;
        var msg = new RegisterHostMessage();
        msg.gameName = "s";
        msg.comment = "test";
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
        UnityEngine.Networking.NetworkSystem.ErrorMessage error = netMsg.ReadMessage<UnityEngine.Networking.NetworkSystem.ErrorMessage>();
        DebugPanel.Phone.Log("Error while connecting: " + error.errorCode);
    }

    private void OnDisconnect(NetworkMessage netMsg)
    {
        UnityEngine.Networking.NetworkSystem.ErrorMessage error = netMsg.ReadMessage<UnityEngine.Networking.NetworkSystem.ErrorMessage>();
        DebugPanel.Phone.Log("Disconnected: " + error.errorCode);
    }

    //Detect when a client connects to the Server
    public override void OnClientConnect(NetworkConnection connection)
    {
        
    }

    //Detect when a client connects to the Server
    public override void OnStopClient()
    {
        DebugPanel.Phone.Log("Stopped Client: ");
    }

    public override void OnClientError(NetworkConnection connection, int errorCode)
    {
        DebugPanel.Phone.Log("Not Connected: " + connection.connectionId + ", errorCode: " + errorCode);
    }

    //Detect when a client connects to the Server
    public override void OnClientDisconnect(NetworkConnection connection)
    {
        DebugPanel.Phone.Log("Not Connected: " + connection.connectionId);
    }
}

public class RegisterHostMessage : MessageBase
{
    public string gameName;
    public string comment;
    public bool passwordProtected;
}