﻿using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    private static Main _main;
    public static Main Instance
    {
        get { return _main; }
    }

    [Header("Game Debug Options")]
    public bool DebugScript;
    public bool SaveMemory;

    [HideInInspector]
    public GameMenu GameMenu;

    [HideInInspector]
    public Game Game;

    void Awake()
    {
        _main = GetComponent<Main>();

        Game = GetComponent<Game>();
        if (Game == null) {
            GameMenu = GetComponent<GameMenu>();
        }
    }

    void Start()
    {
        /*
         * ---------------------------------------------------------------------
         * * ---------------------------------------------------------------------
         * * ---------------------------------------------------------------------
         */

        if (Application.isEditor == false)
            GameHiddenOptions.Instance.InstantDebug = false;

        if (Game == null)
            GameMenu.LoadDependencies();
        else
            Game.LoadDependencies();

        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        var time = GameHiddenOptions.Instance.GetTime(GameHiddenOptions.Instance.TimeBeforeGameStart);
        yield return new WaitForSeconds(time);

        if (Game == null)
            GameMenu.StartGameMenu();
        else
            Game.StartGame();
    }

    public Sprite GetAvatarSprite(int index)
    {
        if ((index - 1) < Persistent.GameData.AvatarSprites.Count)
            return Persistent.GameData.AvatarSprites[index - 1];

        Sprite sprite = Resources.Load<Sprite>("Images/Avatars/" + index);
        Persistent.GameData.AvatarSprites.Add(sprite);
        return sprite;
    }
}

public enum Panel {
    None,
    StartPanel, AuthPanel, MainMenuPanel,
    All
}