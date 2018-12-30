using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUser : MonoBehaviour
{
    public Image Image;
    public Text Name;
    //
    public int? ConnectionId;

    public void Change(User user)
    {
        if (user == null)
        {
            Image.gameObject.SetActive(false);
            Name.gameObject.SetActive(false);
            ConnectionId = null;
            return;
        }

        if (Image.gameObject.activeSelf == false)
            Image.gameObject.SetActive(true);
        if (Name.gameObject.activeSelf == false)
            Name.gameObject.SetActive(true);

        Image.sprite = Main.Instance.GetAvatarSprite(user.ProfilePicIndex);
        Name.text = user.Name;
        ConnectionId = user.ConnectionId;
    }
}
