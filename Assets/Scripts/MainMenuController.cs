using Assets.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public AvatarElement TopAvatarElement;
    public Text Name;
    public Text PrankCoins;

    public Text RoomName;
    public Text ConnectionStatusText;
    public GameObject LookingForRoomsPanel;
    public Text InQueueTime;

    public Button JoinButton;
    private ColorBlock normalColorBlock;
    private ColorBlock highlightedColorBlock;
    public Text JoinButtonText;

    private string _ipAddress;

    internal void Init()
    {
        Sprite sprite = Resources.Load<Sprite>("Images/Avatars/" + Persistent.GameData.LoggedUser.ProfilePicIndex);
        TopAvatarElement.SetImage(sprite);

        Name.text = Persistent.GameData.LoggedUser.Name;
        PrankCoins.text = UsefullUtils.ConvertNumberToKs(Persistent.GameData.LoggedUser.PrankCoins);

        normalColorBlock = JoinButton.colors;
        normalColorBlock.normalColor = GameHiddenOptions.Instance.RedColor;
        highlightedColorBlock = JoinButton.colors;
        highlightedColorBlock.normalColor = GameHiddenOptions.Instance.LightBlueColor;

        JoinButton.colors = normalColorBlock;
        JoinButtonText.text = "Look for games";
        RoomName.text = "No games found yet.";
        LookingForRoomsPanel.SetActive(false);
        ConnectionStatusText.color = GameHiddenOptions.Instance.WhiteColor;
        ConnectionStatusText.text = "";

        ChangeActionButton(ActionButtonFunction.LookForGames);
    }

    // TODO: rename this to OnActionButtonClicked
    public void OnJoinButtonClicked()
    {
        switch (CurrentActionButtonFunction)
        {
            case ActionButtonFunction.LookForGames:

                LookingForRoomsPanel.SetActive(true);
                RoomName.text = "No games found yet.";
                ConnectionStatusText.color = GameHiddenOptions.Instance.WhiteColor;
                ConnectionStatusText.text = "";

                if (Persistent.GameData.RunNetworkClient.gameObject.activeSelf)
                    Persistent.GameData.RunNetworkClient.StartAsClient();
                else
                    Persistent.GameData.RunNetworkClient.gameObject.SetActive(true);
                Persistent.GameData.RunNetworkClient.OnListenSuccess = (string fromAddress, string data) =>
                {
                    RoomName.text = _ipAddress = fromAddress;
                    ChangeActionButton(ActionButtonFunction.Join);
                };

                ChangeActionButton(ActionButtonFunction.JoinCancel);
                break;
            case ActionButtonFunction.JoinCancel:
            case ActionButtonFunction.GiveUp:

                RoomName.text = "There is no one out there.";
                LookingForRoomsPanel.SetActive(false);
                ConnectionStatusText.text = "";

                Persistent.GameData.RunNetworkClient.StopListening();

                ChangeActionButton(ActionButtonFunction.LookForGames);
                break;
            case ActionButtonFunction.Join:

                ConnectionStatusText.color = GameHiddenOptions.Instance.WhiteColor;
                ConnectionStatusText.text = "CONNECTING...";

                Persistent.GameData.RunNetworkClient.FoundBroadscast = true;
                Persistent.GameData.RunNetworkClient.OnJoin = OnClientJoin;
                Persistent.GameData.RunNetworkClient.OnJoinFailed = OnClientJoinFailed;
                Persistent.GameData.RunNetworkClient.OnRecievedServerInfo = OnRecievedServerInfo;

                Persistent.GameData.RunNetworkManager.JoinGame(Persistent.GameData.RunNetworkClient.HostId, _ipAddress, Persistent.GameData.RunNetworkClient.BroadcastPort);

                break;
            case ActionButtonFunction.CreateCancel:

                RoomName.text = "No one loves you.";

                ChangeActionButton(ActionButtonFunction.LookForGames);

                Persistent.GameData.RunNetworkServer.StopBroadcast();
                break;
            case ActionButtonFunction.GoToLobby:

                Main._.GameMenu.CanvasController.LobbyController.ShowLobby();
                break;
            default:
                break;
        }
    }

    private void OnClientJoin()
    {
        ConnectionStatusText.color = GameHiddenOptions.Instance.LightBlueColor;
        ConnectionStatusText.text = "SUCCESS";

        NetClientUser msg = new NetClientUser()
        {
            Name = Persistent.GameData.LoggedUser.Name,
            Pic = Persistent.GameData.LoggedUser.ProfilePicIndex
        };
        Persistent.GameData.RunNetworkClient.SendToServer(msg);

        ChangeActionButton(ActionButtonFunction.GoToLobby);
    }

    private void OnClientJoinFailed()
    {
        ConnectionStatusText.color = GameHiddenOptions.Instance.RedColor;
        ConnectionStatusText.text = "FAILED";

        Persistent.GameData.RunNetworkClient.FoundBroadscast = false;

        ChangeActionButton(ActionButtonFunction.GiveUp);
    }

    private void OnRecievedServerInfo(int fromConnectionId, int fromChannelId, int fromHostId, List<User> users)
    {
        Main._.GameMenu.CanvasController.LobbyController.UpdateClientList(users);
    }

    private int _numberOfConnections;

    public void OnCreateButtonClicked()
    {
        ChangeActionButton(ActionButtonFunction.CreateCancel);

        RoomName.text = "Creating game...";
        ConnectionStatusText.color = GameHiddenOptions.Instance.WhiteColor;
        ConnectionStatusText.text = "BROADCASTING";

        if (Persistent.GameData.RunNetworkServer.gameObject.activeSelf)
            Persistent.GameData.RunNetworkServer.StartAsServer();
        else
            Persistent.GameData.RunNetworkServer.gameObject.SetActive(true);

        // delegates
        Persistent.GameData.RunNetworkServer.OnConnectedToServer = () =>
        {
            _numberOfConnections++;
            RoomName.text = "Game Created";
            ConnectionStatusText.color = GameHiddenOptions.Instance.LightBlueColor;
            ConnectionStatusText.text = _numberOfConnections.ToString();

            ShowGoToLobbyButton();
        };
        Persistent.GameData.RunNetworkServer.OnDisconnectedFromServer = (fromConnectionId) =>
        {
            _numberOfConnections--;
            RoomName.text = "Game Created";
            ConnectionStatusText.color = GameHiddenOptions.Instance.LightBlueColor;
            ConnectionStatusText.text = _numberOfConnections.ToString();

            Main._.GameMenu.CanvasController.LobbyController.UpdateServerList(null, fromConnectionId);

            CreateAndSendUserListDataForLobbyClients();

            ShowGoToLobbyButton();
        };
        Persistent.GameData.RunNetworkServer.OnRecievedClientInfo = (int fromConnectionId, int fromChannelId, int fromHostId, User user) =>
        {
            DebugPanel.Phone.Log(" Someone is in lobby - " + user.ToString());

            Main._.GameMenu.CanvasController.LobbyController.UpdateServerList(user);

            CreateAndSendUserListDataForLobbyClients();
        };
    }

    private void CreateAndSendUserListDataForLobbyClients()
    {
        if (Persistent.GameData.ServerUsers == null && Persistent.GameData.ServerUsers.Count == 0)
            return;

        var users = new List<NetClientUser>();
        foreach (var user in Persistent.GameData.ServerUsers)
        {
            users.Add(new NetClientUser()
            {
                Name = user.Name,
                Pic = user.ProfilePicIndex,
                ConnId = user.ConnectionId
            });
        }
        var netServerUsers = new NetServerUsers() { Users = users };
        var connectedUsersIds = Persistent.GameData.ServerUsers.Select(u => u.ConnectionId).ToArray();
        Persistent.GameData.RunNetworkServer.SendToClients(netServerUsers, connectedUsersIds);
    }

    private void ShowGoToLobbyButton()
    {
        if (_numberOfConnections > 0)
        {
            ConnectionStatusText.color = GameHiddenOptions.Instance.LightBlueColor;

            ChangeActionButton(ActionButtonFunction.GoToLobby);
        }
        else
        {
            ConnectionStatusText.color = GameHiddenOptions.Instance.WhiteColor;

            ChangeActionButton(ActionButtonFunction.CreateCancel);
        }
    }

    private enum ActionButtonFunction
    {
        LookForGames, JoinCancel, Join, CreateCancel, GoToLobby, GiveUp
    }

    private ActionButtonFunction CurrentActionButtonFunction;

    private void ChangeActionButton(ActionButtonFunction actionButtonFunction)
    {
        CurrentActionButtonFunction = actionButtonFunction;

        switch (actionButtonFunction)
        {
            case ActionButtonFunction.LookForGames:

                JoinButton.colors = normalColorBlock;
                JoinButtonText.text = "Look for games";
                break;
            case ActionButtonFunction.JoinCancel:

                JoinButton.colors = normalColorBlock;
                JoinButtonText.text = "Cancel";
                break;
            case ActionButtonFunction.GiveUp:

                JoinButton.colors = normalColorBlock;
                JoinButtonText.text = "Give up";
                break;
            case ActionButtonFunction.Join:

                JoinButton.colors = highlightedColorBlock;
                JoinButtonText.text = "Join";
                break;
            case ActionButtonFunction.CreateCancel:

                JoinButton.colors = normalColorBlock;
                JoinButtonText.text = "Cancel";
                break;
            case ActionButtonFunction.GoToLobby:

                JoinButton.colors = highlightedColorBlock;
                JoinButtonText.text = "Go to Lobby";
                break;
            default:
                break;
        }
    }

    public void OnSettingsButtonClicked()
    {
        Main._.GameMenu.CanvasController.ShowPanel(Panel.AuthPanel);
    }

    public void OnCloseButtonClicked()
    {
        Application.Quit();
    }
}
