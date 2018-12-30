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

    public void UpdateClientList(List<User> newUsers, List<User> disconectedUsers)
    {
        UpdateView(newUsers);
    }

    public void UpdateServerList(List<User> newUsers, List<User> disconectedUsers)
    {
        if (Main.Instance.ServerUsers == null)
            Main.Instance.ServerUsers = new List<User>();

        if (newUsers != null)
        {
            var newUser = newUsers[0];
            foreach (var user in Main.Instance.ServerUsers)
            {
                if (newUser.ConnectionId == user.ConnectionId)
                {
                    newUser.AllreadyIn = true;
                    break;
                }
            }
            if (newUser.AllreadyIn == false)
                Main.Instance.ServerUsers.Add(newUser);
        }

        if (disconectedUsers != null)
        {
            int index = 0;
            var disconectedUser = disconectedUsers[0];
            foreach (var user in Main.Instance.ServerUsers)
            {
                if (disconectedUser.ConnectionId == user.ConnectionId)
                {
                    disconectedUser.AllreadyIn = true;
                    break;
                }
                index++;
            }
            if (disconectedUser.AllreadyIn == true)
                Main.Instance.ServerUsers.RemoveAt(index);
        }

        UpdateView(Main.Instance.ServerUsers);
    }

    private void UpdateView(List<User> users)
    {
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
