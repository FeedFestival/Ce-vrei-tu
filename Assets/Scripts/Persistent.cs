using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Persistent : MonoBehaviour
{
    private static Persistent _gameData;
    public static Persistent GameData
    {
        get { return _gameData; }
    }

    [HideInInspector]
    public User LoggedUser;
    [HideInInspector]
    public List<Sprite> AvatarSprites;

    [Header("Networking")]
    public RunNetworkServer RunNetworkServer;
    public RunNetworkClient RunNetworkClient;
    public RunNetworkManager RunNetworkManager;

    public bool IsServer;
    //public int ConnectionId;
    public List<User> ServerUsers;

    //

    private void Awake()
    {
        _gameData = this;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
