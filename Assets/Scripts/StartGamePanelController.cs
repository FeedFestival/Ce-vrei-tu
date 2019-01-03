using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartGamePanelController : MonoBehaviour
{
    public Text TutorialText;
    public Button SkipButton;

    public LoadingController LoadingController;

    public delegate void OnGameReadyCallback();
    OnGameReadyCallback _onGameReady;

    //

    private bool _infoFullyRead;

    public void LoadDependencies()
    {
        SkipButton.gameObject.SetActive(false);
    }

    public void Init()
    {
        LoadingController.ShowLoading();
    }

    internal void GameIsReady(OnGameReadyCallback onGameReady)
    {
        LoadingController.HideLoading();

        if (_infoFullyRead)
        {
            onGameReady();
            gameObject.SetActive(false);
        }
        else
        {
            _onGameReady = onGameReady;
            SkipButton.gameObject.SetActive(true);
        }
    }

    // this is also called when _infoFullyRead is set to true !!!!
    // !!!!
    public void OnSkipButtonPress()
    {
        _onGameReady();
    }
}
