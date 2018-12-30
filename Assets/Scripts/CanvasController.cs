using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasController : MonoBehaviour
{
    public StartPanelController StartPanelController;
    public AuthController AuthController;
    public MainMenuController MainMenuController;
    public LobbyController LobbyController;

    public Panel CurrentPanel;
    public Panel PreviousPanel;

    public void Init()
    {
        PreviousPanel = Panel.None;
        CurrentPanel = Panel.StartPanel;
        ShowCurrentPanel();
        LoadingController.Instance.ShowLoading();
    }

    internal void LoadDependencies()
    {
        Main.Instance.Game.CanvasController.LobbyController.Init();

        HidePanel(Panel.All);
    }

    public void ShowPanel(Panel panel)
    {
        PreviousPanel = CurrentPanel;
        CurrentPanel = panel;

        HidePanel();
        ShowCurrentPanel();
    }

    private void HidePanel(Panel panel = Panel.None)
    {
        if (panel == Panel.All)
        {
            StartPanelController.gameObject.SetActive(false);
            AuthController.gameObject.SetActive(false);
            MainMenuController.gameObject.SetActive(false);

            return;
        }

        switch (PreviousPanel)
        {
            case Panel.None:
                break;
            case Panel.StartPanel:
                StartPanelController.gameObject.SetActive(false);
                break;
            case Panel.AuthPanel:
                AuthController.gameObject.SetActive(false);
                break;
            case Panel.MainMenuPanel:
                MainMenuController.gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    private void ShowCurrentPanel() {
        switch (CurrentPanel)
        {
            case Panel.None:
                break;
            case Panel.StartPanel:
                StartPanelController.gameObject.SetActive(true);
                StartPanelController.Init();
                break;
            case Panel.AuthPanel:
                AuthController.gameObject.SetActive(true);
                AuthController.Init();
                break;
            case Panel.MainMenuPanel:
                MainMenuController.gameObject.SetActive(true);
                MainMenuController.Init();
                break;
            default:
                break;
        }
    }

    internal void GoToPreviousPanel(Panel fromPanel)
    {
        CurrentPanel = PreviousPanel;
        PreviousPanel = fromPanel;

        HidePanel();
        ShowCurrentPanel();
    }
}
