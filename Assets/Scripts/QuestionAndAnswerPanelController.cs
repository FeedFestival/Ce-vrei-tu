using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionAndAnswerPanelController : MonoBehaviour
{
    public Text QuestionText;
    public InputFieldCustom LieInput;
    public Button LieButton;

    public InGameClock InGameClock;

    public List<AvatarElement> AvatarElements;

    public delegate void OnLieCompleteCallback(string lie);
    public OnLieCompleteCallback OnLieComplete;

    private void Start()
    {
        LieInput.Label.text = "Input your lie";
        foreach (var avatar in AvatarElements)
        {
            avatar.gameObject.SetActive(false);
        }
    }

    internal void Init(Question currentQuestion)
    {
        LieButton.interactable = false;
        LieInput.OnChangeDelegate = () =>
        {
            if (string.IsNullOrWhiteSpace(LieInput.InputField.text))
                LieButton.interactable = false;
            else
                LieButton.interactable = true;
        };

        SetText(currentQuestion.Text);

        InGameClock.InitClock(30f);
        InGameClock.StartClock();
    }

    public void Lie()
    {
        OnLieComplete(LieInput.InputField.text);
    }

    public void SetText(string text)
    {
        QuestionText.text = text;

        if (text.Length > 300)
        {
            QuestionText.fontSize = 25;
        }
        else
        {
            switch (text.Length)
            {
                case 35:
                    QuestionText.fontSize = 65;
                    break;
                case 50:
                    QuestionText.fontSize = 60;
                    break;
                case 70:
                    QuestionText.fontSize = 55;
                    break;
                case 80:
                    QuestionText.fontSize = 50;
                    break;
                case 100:
                    QuestionText.fontSize = 46;
                    break;
                case 115:
                    QuestionText.fontSize = 43;
                    break;
                case 135:
                    QuestionText.fontSize = 40;
                    break;
                case 145:
                    QuestionText.fontSize = 38;
                    break;
                case 170:
                    QuestionText.fontSize = 37;
                    break;
                case 180:
                    QuestionText.fontSize = 36;
                    break;
                case 190:
                    QuestionText.fontSize = 35;
                    break;
                case 205:
                    QuestionText.fontSize = 33;
                    break;
                case 240:
                    QuestionText.fontSize = 29;
                    break;
                case 300:
                    QuestionText.fontSize = 26;
                    break;
                default:
                    QuestionText.fontSize = 67;
                    break;
            }
        }
    }

    public void ShowAvatarInputed(User userThatAddedLie)
    {
        AvatarElement avatarElement = GetAvailableAvatarElement();
        Sprite sprite = Resources.Load<Sprite>("Images/Avatars/" + userThatAddedLie.ProfilePicIndex);
        avatarElement.SetImage(sprite);
    }

    private AvatarElement GetAvailableAvatarElement()
    {
        foreach (AvatarElement avatar in AvatarElements)
        {
            if (avatar.gameObject.activeSelf == false)
            {
                avatar.gameObject.SetActive(true);
                return avatar;
            }
        }
        return null;
    }
}
