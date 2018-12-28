using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHiddenOptions : MonoBehaviour
{
    private static GameHiddenOptions _gameHiddenOptions;
    public static GameHiddenOptions Instance
    {
        get { return _gameHiddenOptions; }
    }

    void Awake()
    {
        _gameHiddenOptions = this;
    }

    public bool InstantDebug;

    [Header("Execution Timers")]
    public float TimeBeforeSessionCreation = 0.2f;
    public float TimeBeforeGameStart = 0.1f;

    [Header("Base Colors")]
    public Color32 BlackColor;
    public Color32 WhiteColor;
    public Color32 RedColor;
    public Color32 FullTransparentColor;
    public Color32 GameNameColor;

    [Header("Input Colors")]
    public Color32 LabelColor;

    [Header("Prefabs")]
    public GameObject AvatarElementPrefab;

    internal float GetTime(float normalTime)
    {
        return InstantDebug ? 0f : normalTime;
    }
    //public Color32 TextOverBlackColor;
}
