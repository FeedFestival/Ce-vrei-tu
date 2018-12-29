using Assets.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Networking")]

    public RunNetworkServer RunNetworkServer;
    public RunNetworkClient RunNetworkClient;
    public RunNetworkManager RunNetworkManager;

    internal void Init()
    {
        Sprite sprite = Resources.Load<Sprite>("Images/Avatars/" + Main.Instance.LoggedUser.ProfilePicIndex);
        TopAvatarElement.SetImage(sprite);

        Name.text = Main.Instance.LoggedUser.Name;
        PrankCoins.text = UsefullUtils.ConvertNumberToKs(Main.Instance.LoggedUser.PrankCoins);

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

                RunNetworkClient.gameObject.SetActive(true);
                RunNetworkClient.OnListenSuccess = (string fromAddress, string data) =>
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

                RunNetworkClient.StopListening();

                ChangeActionButton(ActionButtonFunction.LookForGames);
                break;
            case ActionButtonFunction.Join:

                ConnectionStatusText.color = GameHiddenOptions.Instance.WhiteColor;
                ConnectionStatusText.text = "CONNECTING...";
                RunNetworkManager.JoinGame(RunNetworkClient.HostId, _ipAddress, RunNetworkClient.BroadcastPort, OnJoin, OnJoinFailed);

                break;
            case ActionButtonFunction.CreateCancel:

                RoomName.text = "No one loves you.";

                ChangeActionButton(ActionButtonFunction.LookForGames);

                RunNetworkServer.StopBroadcast();
                break;
            case ActionButtonFunction.GoToLobby:


                break;
            default:
                break;
        }
    }

    private void OnJoin()
    {
        ConnectionStatusText.color = GameHiddenOptions.Instance.LightBlueColor;
        ConnectionStatusText.text = "SUCCESS";

        ChangeActionButton(ActionButtonFunction.GoToLobby);
    }

    private void OnJoinFailed()
    {
        ConnectionStatusText.color = GameHiddenOptions.Instance.RedColor;
        ConnectionStatusText.text = "FAILED";

        ChangeActionButton(ActionButtonFunction.GiveUp);
    }
    
    private int _numberOfConnections;

    public void OnCreateButtonClicked()
    {
        ChangeActionButton(ActionButtonFunction.CreateCancel);
        
        RoomName.text = "Creating game...";
        ConnectionStatusText.color = GameHiddenOptions.Instance.WhiteColor;
        ConnectionStatusText.text = "BROADCASTING";

        RunNetworkServer.gameObject.SetActive(true);
        RunNetworkServer.OnConnectedToServer = () =>
        {
            _numberOfConnections++;
            RoomName.text = "Game Created";
            ConnectionStatusText.color = GameHiddenOptions.Instance.LightBlueColor;
            ConnectionStatusText.text = _numberOfConnections.ToString();

            ShowGoToLobbyButton();
        };

        RunNetworkServer.OnDisconnectedFromServer = () =>
        {
            _numberOfConnections--;
            RoomName.text = "Game Created";
            ConnectionStatusText.color = GameHiddenOptions.Instance.LightBlueColor;
            ConnectionStatusText.text = _numberOfConnections.ToString();

            ShowGoToLobbyButton();
        };
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
        Main.Instance.Game.CanvasController.ShowPanel(Panel.AuthPanel);
    }

    public void OnCloseButtonClicked()
    {
        Application.Quit();
    }
}
