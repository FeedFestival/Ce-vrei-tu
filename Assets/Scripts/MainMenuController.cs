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

    public InputFieldCustom RoomName;

    internal void Init()
    {
        Sprite sprite = Resources.Load<Sprite>("Images/Avatars/" + Main.Instance.LoggedUser.ProfilePicIndex);
        TopAvatarElement.SetImage(sprite);

        Name.text = Main.Instance.LoggedUser.Name;
        PrankCoins.text = UsefullUtils.ConvertNumberToKs(Main.Instance.LoggedUser.PrankCoins);

        RoomName.Label.text = "Room Name";
    }

    public void OnJoinButtonClicked()
    {

    }

    public void OnCreateButtonClicked()
    {

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
