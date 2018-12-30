using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [Header("Properties")]

    public bool IsExpanded;
    public Transform LobbyUserContainer;

    //

    private List<LobbyUser> _lobbyUsers;

    private RectTransform _rt;
    private float _screenWidth;
    private float _originalXPos;
    private float _expandedXPos;

    private float _animationSpeed = 0.8f;

    public void Init()
    {
        _rt = GetComponent<RectTransform>();

        var screenHeight = GameHiddenOptions.Instance.CanvasScaler.referenceResolution.y;
        _screenWidth = GameHiddenOptions.Instance.CanvasScaler.referenceResolution.x;

        _originalXPos = 720;
        _expandedXPos = 0;

        _rt.sizeDelta = new Vector2(_screenWidth, _rt.sizeDelta.y);
        _rt.anchoredPosition = new Vector3(_originalXPos, _rt.position.y, _rt.position.z);

        if (IsExpanded)
            Expand();
    }

    public void ShowLobby()
    {
        IsExpanded = !IsExpanded;

        if (IsExpanded)
        {
            Expand();
        }
        else
        {
            LeanTween.value(gameObject, (float value) =>
            {
                _rt.anchoredPosition = new Vector3(value, _rt.position.y, _rt.position.z);
            }, _expandedXPos, _originalXPos, _animationSpeed).setEase(LeanTweenType.easeInOutBack);
        }
    }

    private void Expand()
    {
        LeanTween.value(gameObject, (float value) =>
        {
            _rt.anchoredPosition = new Vector3(value, _rt.position.y, _rt.position.z);
        }, _originalXPos, _expandedXPos, _animationSpeed).setEase(LeanTweenType.easeInOutBack);

    }

    public void UpdateClientList(List<User> newUsers)
    {
        UpdateView(newUsers);
    }

    public void UpdateServerList(User newUser, int? disconectedConnectionId = null)
    {
        if (Persistent.GameData.ServerUsers == null)
            Persistent.GameData.ServerUsers = new List<User>();

        if (Persistent.GameData.ServerUsers.Count == 0)
            Persistent.GameData.ServerUsers.Add(Persistent.GameData.LoggedUser);

        if (newUser != null)
        {
            foreach (var user in Persistent.GameData.ServerUsers)
            {
                if (newUser.ConnectionId == user.ConnectionId)
                {
                    newUser.AllreadyIn = true;
                    break;
                }
            }
            if (newUser.AllreadyIn == false)
                Persistent.GameData.ServerUsers.Add(newUser);
        }

        if (disconectedConnectionId != null && Persistent.GameData.ServerUsers.Count > 0)
        {
            int index = 0;
            foreach (var user in Persistent.GameData.ServerUsers)
            {
                if (disconectedConnectionId == user.ConnectionId)
                    break;
                index++;
            }
            Persistent.GameData.ServerUsers.RemoveAt(index);
        }

        UpdateView(Persistent.GameData.ServerUsers);
    }

    private void UpdateView(List<User> users)
    {
        if (users == null || users.Count == 0) return;

        if (_lobbyUsers == null)
            _lobbyUsers = new List<LobbyUser>();

        if (_lobbyUsers.Count == 0)
        {
            foreach (var user in users)
            {
                var lobbyUser = AddLobbyUser(user);
                _lobbyUsers.Add(lobbyUser);
                lobbyUser.Change(user);
            }
        }
        else if (users.Count > _lobbyUsers.Count)
        {
            var index = 0;
            foreach (var user in users)
            {
                if (index >= _lobbyUsers.Count)
                {
                    var lobbyUser = AddLobbyUser(user);
                    _lobbyUsers.Add(lobbyUser);
                }

                _lobbyUsers[index].Change(user);
                index++;
            }
        }
        else if (users.Count <= _lobbyUsers.Count)
        {
            var diff = System.Math.Abs(_lobbyUsers.Count - users.Count);
            var startIndex = _lobbyUsers.Count - diff;
            if (users.Count < _lobbyUsers.Count)
            {
                for (var i = startIndex; i < _lobbyUsers.Count; i++)
                {
                    _lobbyUsers[i].Change(null);
                }
            }

            if (users.Count >= startIndex)
            {
                var index = 0;
                foreach (var user in users)
                {
                    if (index >= _lobbyUsers.Count)
                    {
                        var lobbyUser = AddLobbyUser(user);
                        _lobbyUsers.Add(lobbyUser);
                    }

                    _lobbyUsers[index].Change(user);
                    index++;
                }
            }
        }
    }

    private LobbyUser AddLobbyUser(User user)
    {
        var go = Instantiate(GameHiddenOptions.Instance.LobbyUserPrefab);
        go.name = user.Name;
        go.transform.SetParent(LobbyUserContainer);
        go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

        return go.GetComponent<LobbyUser>();
    }

    public void StartGame()
    {
        // ChangeScene
        // remove StuffFrom
    }

    /*
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {
            var users = new List<User>()
            {
                new User {
                    ConnectionId = 1,
                    ProfilePicIndex = 11,
                    Name = "User_1"
                },
                new User {
                    ConnectionId = 2,
                    ProfilePicIndex = 4,
                    Name = "User_2"
                }
            };
            UpdateClientList(users, null);
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            var users = new List<User>()
            {
                new User {
                    ConnectionId = 1,
                    ProfilePicIndex = 11,
                    Name = "User_1"
                },
                new User {
                    ConnectionId = 3,
                    ProfilePicIndex = 6,
                    Name = "User_3"
                },
                new User {
                    ConnectionId = 4,
                    ProfilePicIndex = 16,
                    Name = "User_4"
                }
            };
            UpdateClientList(users, null);
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            var users = new List<User>()
            {
                new User {
                    ConnectionId = 1,
                    ProfilePicIndex = 11,
                    Name = "User_1"
                },
                new User {
                    ConnectionId = 4,
                    ProfilePicIndex = 16,
                    Name = "User_4"
                }
            };
            UpdateClientList(users, null);
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            var users = new List<User>()
            {
                new User {
                    ConnectionId = 1,
                    ProfilePicIndex = 4,
                    Name = "User_1"
                }
            };
            UpdateServerList(users, null);
        }

        if (Input.GetKeyUp(KeyCode.X))
        {
            var users = new List<User>()
            {
                new User {
                    ConnectionId = 2,
                    ProfilePicIndex = 8,
                    Name = "User_2"
                }
            };
            UpdateServerList(users, null);
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            var users = new List<User>()
            {
                new User {
                    ConnectionId = 2,
                    ProfilePicIndex = 8,
                    Name = "User_2"
                }
            };
            UpdateServerList(null, users);
        }
    }
    */
}
