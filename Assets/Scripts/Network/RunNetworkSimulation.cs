using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunNetworkSimulation : MonoBehaviour
{
    private int[] _connIds;
    private NetMessage Expected_MessageCode;
    private NetMessage Expected_MessageCodeIsFor;

    public delegate void OnDataCallback(int fromConnectionId, int fromChannelId, int fromHostId, NetMsg msg);
    public OnDataCallback OnData;

    public void Init()
    {
        if (Persistent.GameData.IsServer)
        {
            Persistent.GameData.RunNetworkServer.gameObject.SetActive(true);
            Persistent.GameData.RunNetworkServer.StartSimulation(this);
        }
        else
        {
            Persistent.GameData.RunNetworkClient.gameObject.SetActive(true);
            Persistent.GameData.RunNetworkClient.StartSimulation(this);
        }

        Persistent.GameData.LoggedUser = DomainLogic.DB.DataService.GetDeviceUser();

        if (Persistent.GameData.LoggedUser == null)
            Persistent.GameData.LoggedUser = new User() { ConnectionId = 99, Name = "Player1", ProfilePicIndex = 4, Saying = "Player1 Saying" };

        Persistent.GameData.ServerUsers = new List<User>() {
            Persistent.GameData.LoggedUser,

            new User() { ConnectionId = 1, Name = "Player2", ProfilePicIndex = 11, Saying = "Player2 Saying" },
            new User() { ConnectionId = 2, Name = "Player3", ProfilePicIndex = 4, Saying = "Player3 Saying" },
            new User() { ConnectionId = 3, Name = "Player4", ProfilePicIndex = 7, Saying = "Player4 Saying" }
        };
    }

    private void Update()
    {
        if (Main._.IsSimulated)
        {
            if (Persistent.GameData.IsServer)
                SimulationUpdate_server();
            else
                SimulationUpdate_client();
        }
    }

    public void SendToClients(NetMsg msg, int[] connIds = null, int? connectionId = null)
    {

        if (connIds == null && connectionId.HasValue)
            connIds = new int[1] { connectionId.Value };
        SetSimulationForCallback_server(msg, connIds);
    }

    public void SendToServer(NetMsg msg)
    {
        SetSimulationForCallback_client(msg);
    }

    private void SetSimulationForCallback_server(NetMsg msg, int[] connIds)
    {
        switch ((Operation)msg.OP)
        {
            case Operation.SimpleMessage:

                var sMsg = (SimpleMessage)msg;
                Expected_MessageCodeIsFor = (NetMessage)sMsg.MessageCode;

                switch (Expected_MessageCodeIsFor)
                {
                    case NetMessage.StartingGame:
                        Expected_MessageCode = NetMessage.Ok;
                        break;
                    default:
                        break;
                }
                break;

            default:
                break;
        }

        _connIds = connIds;
    }

    private void SetSimulationForCallback_client(NetMsg msg)
    {
        switch ((Operation)msg.OP)
        {
            case Operation.SimpleMessage:

                var sMsg = (SimpleMessage)msg;
                Expected_MessageCodeIsFor = (NetMessage)sMsg.MessageCode;

                switch (Expected_MessageCodeIsFor)
                {
                    case NetMessage.StartingGame:
                        Expected_MessageCode = NetMessage.Ok;
                        break;
                    default:
                        break;
                }
                break;

            default:
                break;
        }
    }

    private void SimulationUpdate_server()
    {
        if (Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            NetMsg sMsg = null;

            switch (Expected_MessageCodeIsFor)
            {
                case NetMessage.StartingGame:

                    sMsg = new SimpleMessage()
                    {
                        MessageCode = (byte)Expected_MessageCode,   // ie: StartingGame
                        ThisMessageCodeIsFor = (byte)NetMessage.StartingGame
                    };
                    break;
                case NetMessage.DoPickCategory:

                    sMsg = new SimpleMessage()
                    {
                        MessageId = 1,
                        ThisMessageCodeIsFor = (byte)NetMessage.DoPickCategory
                    };
                    break;
                default:
                    break;
            }
            if (sMsg != null)
            {
                foreach (int id in _connIds)
                {

                    OnData(id, 0, 0, sMsg);
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Keypad1))
        {
            var categoryId = 1;
            var sMsg = new SimpleMessage()
            {
                MessageCode = (byte)Expected_MessageCode,
                MessageId = (byte)categoryId,
                ThisMessageCodeIsFor = (byte)NetMessage.DoPickCategory
            };
            if (sMsg != null)
            {
                
                OnData(0, 0, 0, sMsg);
            }
        }
    }

    private void SimulationUpdate_client()
    {
        if (Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            NetMsg sMsg = null;

            switch (Expected_MessageCodeIsFor)
            {
                default:
                    break;
            }
            if (sMsg != null)
            {
                OnData(0, 0, 0, sMsg);
            }
        }

        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            NetMsg sMsg = null;
            sMsg = new SimpleMessage()
            {
                MessageCode = (byte)NetMessage.ConnectionIsPickingCategory,
                ConnId = 2
            };

            OnData(0, 0, 0, sMsg);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            NetMsg sMsg = null;
            sMsg = new SimpleMessage()
            {
                MessageCode = (byte)NetMessage.DoPickCategory
            };

            OnData(0, 0, 0, sMsg);
        }

        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            NetMsg sMsg = null;
            sMsg = new SimpleMessage()
            {
                MessageCode = (byte)NetMessage.DoPickCategory
            };

            OnData(0, 0, 0, sMsg);
        }

        if (Input.GetKeyUp(KeyCode.Alpha4)) {
            var question = DomainLogic.DB.DataService.GetRandomQuestionByCategory();
            var qMsg = new QuestionMessage()
            {
                Question = question
            };
            OnData(0, 0, 0, qMsg);
        }
    }
}
