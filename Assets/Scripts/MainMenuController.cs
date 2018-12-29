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

    private bool _isLookingForGames;
    private bool _gamesFound;

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

        RoomName.text = "No games found yet.";
        LookingForRoomsPanel.SetActive(false);

        normalColorBlock = JoinButton.colors;
        normalColorBlock.normalColor = GameHiddenOptions.Instance.RedColor;
        highlightedColorBlock = JoinButton.colors;
        highlightedColorBlock.normalColor = GameHiddenOptions.Instance.LightBlueColor;

        JoinButton.colors = normalColorBlock;
        JoinButtonText.text = "Look for games";
    }

    public void OnJoinButtonClicked()
    {
        if (_isLookingForGames == false)
        {
            _isLookingForGames = true;

            JoinButtonText.text = "Cancel";
            LookingForRoomsPanel.SetActive(true);
            RoomName.text = "No games found yet.";
            ConnectionStatusText.color = GameHiddenOptions.Instance.WhiteColor;
            ConnectionStatusText.text = "";

            //
            RunNetworkClient.gameObject.SetActive(true);
            RunNetworkClient.OnListenSuccess = (string fromAddress, string data) =>
            {
                _gamesFound = true;

                RoomName.text = _ipAddress = fromAddress;
                JoinButton.colors = highlightedColorBlock;
                JoinButtonText.text = "Join";
            };
        }
        else if (_isLookingForGames && _gamesFound == false)    // Cancel
        {
            _isLookingForGames = false;

            JoinButton.colors = normalColorBlock;
            JoinButtonText.text = "Look for games";
        }
        else if (_gamesFound)
        {
            ConnectionStatusText.color = GameHiddenOptions.Instance.WhiteColor;
            ConnectionStatusText.text = "CONNECTING...";
            RunNetworkManager.JoinGame(RunNetworkClient.HostId, _ipAddress, RunNetworkClient.BroadcastPort, OnJoin, OnJoinFailed);
        }
    }

    private void OnJoin()
    {
        ConnectionStatusText.color = GameHiddenOptions.Instance.LightBlueColor;
        ConnectionStatusText.text = "SUCCESS";
    }

    private void OnJoinFailed()
    {
        ConnectionStatusText.color = GameHiddenOptions.Instance.RedColor;
        ConnectionStatusText.text = "FAILED";
    }

    private int _numberOfConnections;

    public void OnCreateButtonClicked()
    {
        RoomName.text = "Creating game...";
        ConnectionStatusText.color = GameHiddenOptions.Instance.WhiteColor;
        ConnectionStatusText.text = "BROADCASTING";

        RunNetworkServer.gameObject.SetActive(true);
        RunNetworkServer.OnMessageRecieved = () =>
        {
            _numberOfConnections++;
            RoomName.text = "Game Created";
            ConnectionStatusText.color = GameHiddenOptions.Instance.LightBlueColor;
            ConnectionStatusText.text = _numberOfConnections.ToString();
        };
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
