using System.Collections;
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

    public User LoggedUser;
    public List<Sprite> AvatarSprites;
    public List<User> ServerUsers;

    [Header("Game Debug Options")]
    public bool DebugScript;
    public bool SaveMemory;

    [HideInInspector]
    public Game Game;

    void Awake()
    {
        _main = GetComponent<Main>();
        Game = GetComponent<Game>();
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

        Game.LoadDependencies();

        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        var time = GameHiddenOptions.Instance.GetTime(GameHiddenOptions.Instance.TimeBeforeGameStart);
        yield return new WaitForSeconds(time);

        Game.StartGame();
    }

    public Sprite GetAvatarSprite(int index)
    {
        if ((index - 1) < Main.Instance.AvatarSprites.Count)
            return Main.Instance.AvatarSprites[index - 1];

        Sprite sprite = Resources.Load<Sprite>("Images/Avatars/" + index);
        Main.Instance.AvatarSprites.Add(sprite);
        return sprite;
    }
}

public enum Panel {
    None,
    StartPanel, AuthPanel, MainMenuPanel,
    All
}