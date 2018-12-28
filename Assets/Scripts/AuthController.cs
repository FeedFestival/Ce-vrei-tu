using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthController : MonoBehaviour
{
    public InputFieldCustom Name;
    public InputFieldCustom Saying;
    public GameObject AvatarsContainer;

    public Scrollbar AvatarsScrollbar;
    public GameObject CancelButton;
    public Text SaveButtonText;

    private List<AvatarElement> AvatarElements;
    private int _numberOfAvatars = 20;
    private int _selectedPicIndex;

    private void Start()
    {
        Name.Label.text = "Nume";
        Saying.Label.text = "Vorba ta";
    }

    public void Init()
    {
        if (Main.Instance.LoggedUser == null)
        {
            SaveButtonText.text = "That's me!";
            CancelButton.SetActive(false);
        }
        else
        {
            Name.InputField.text = Main.Instance.LoggedUser.Name;
            Name.OnBlur(true);

            Saying.InputField.text = Main.Instance.LoggedUser.Saying;
            Saying.OnBlur(true);

            SaveButtonText.text = "This is me now!";
            CancelButton.SetActive(true);
        }

        if (AvatarElements != null) return;

        AvatarElements = new List<AvatarElement>();
        for (var i = 1; i <= _numberOfAvatars; i++)
        {
            var go = Instantiate(GameHiddenOptions.Instance.AvatarElementPrefab);
            go.name = i + "_Avatar";
            go.transform.SetParent(AvatarsContainer.transform);
            go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

            Sprite sprite = Main.Instance.GetAvatarSprite(i);

            var avatarElement = go.GetComponent<AvatarElement>();

            bool isSelected = (i == 1);
            if (Main.Instance.LoggedUser != null)
            {
                isSelected = (Main.Instance.LoggedUser.ProfilePicIndex == i);
            }

            avatarElement.Init(this, i, isSelected);
            avatarElement.SetImage(sprite);

            AvatarElements.Add(avatarElement);
        }

        if (Main.Instance.LoggedUser != null)
            StartCoroutine(SetScrollToAvatar());
    }

    private IEnumerator SetScrollToAvatar()
    {
        yield return new WaitForEndOfFrame();

        _selectedPicIndex = Main.Instance.LoggedUser.ProfilePicIndex;
        var percent = UsefullUtils.GetValuePercent(_selectedPicIndex, _numberOfAvatars);
        AvatarsScrollbar.value = percent / 100f;
    }

    public void OnAvatarSelect(int selectedPicIndex)
    {
        _selectedPicIndex = selectedPicIndex;
        for (var i = 0; i < AvatarElements.Count; i++)
        {
            if (i != (_selectedPicIndex - 1))
            {
                AvatarElements[i].UnSelect();
            }
        }
    }

    public void OnCancelClick()
    {
        Main.Instance.Game.CanvasController.GoToPreviousPanel(Panel.AuthPanel);
    }

    public void SaveUser()
    {
        if (Main.Instance.LoggedUser == null)
        {

            var user = new User()
            {
                Name = Name.InputField.text,
                Saying = Saying.InputField.text,
                ProfilePicIndex = _selectedPicIndex
            };
            Main.Instance.Game.DataService.CreateUser(user);
            Main.Instance.LoggedUser = Main.Instance.Game.DataService.GetDeviceUser();
        }
        else
        {
            Main.Instance.LoggedUser.Name = Name.InputField.text;
            Main.Instance.LoggedUser.Saying = Saying.InputField.text;
            Main.Instance.LoggedUser.ProfilePicIndex = _selectedPicIndex;

            Main.Instance.Game.DataService.UpdateUser(Main.Instance.LoggedUser);
        }
        Main.Instance.Game.CanvasController.ShowPanel(Panel.MainMenuPanel);
    }
}
