using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPanelController : MonoBehaviour
{
    public Text GameNameText;
    public Text DefinitionText_Name;
    public Text DefinitionText_Def1;
    public Text DefinitionText_Def2;

    [Header("Behaviour Settings")]

    [SerializeField]
    private float _timeBeforeDisplayGameName = 0.3f;
    [SerializeField]
    private float _timeBeforeDisplayDefinitionText = 1.3f;
    [SerializeField]
    private float _timeToFadeIn = 1.3f;
    [SerializeField]
    private float _timeFakeLoadingTime = 3f;

    private void Awake()
    {
        GameNameText.gameObject.SetActive(false);

        DefinitionText_Name.gameObject.SetActive(false);
        DefinitionText_Def1.gameObject.SetActive(false);
        DefinitionText_Def2.gameObject.SetActive(false);
    }

    internal void Init()
    {
        StartCoroutine(DisplayGameName());

        StartCoroutine(DisplayDefinition());
    }

    private IEnumerator DisplayGameName()
    {
        var time = GameHiddenOptions.Instance.GetTime(_timeBeforeDisplayGameName);
        yield return new WaitForSeconds(time);

        GameNameText.gameObject.SetActive(true);

        var color = GameHiddenOptions.Instance.GameNameColor;
        color.a = 0;

        GameNameText.color = color;
        LeanTween.alphaText(GameNameText.gameObject.GetComponent<RectTransform>(), 1f, _timeToFadeIn).setEase(LeanTweenType.linear);
    }

    IEnumerator DisplayDefinition()
    {
        var time = GameHiddenOptions.Instance.GetTime(_timeBeforeDisplayDefinitionText);
        yield return new WaitForSeconds(time);

        DefinitionText_Name.gameObject.SetActive(true);
        DefinitionText_Def1.gameObject.SetActive(true);
        DefinitionText_Def2.gameObject.SetActive(true);

        var color = GameHiddenOptions.Instance.WhiteColor;
        color.a = 0;

        DefinitionText_Name.color = color;
        DefinitionText_Def1.color = color;
        DefinitionText_Def2.color = color;

        LeanTween.alphaText(DefinitionText_Name.gameObject.GetComponent<RectTransform>(), 1f, _timeToFadeIn).setEase(LeanTweenType.linear);
        LeanTween.alphaText(DefinitionText_Def1.gameObject.GetComponent<RectTransform>(), 1f, _timeToFadeIn).setEase(LeanTweenType.linear);
        LeanTween.alphaText(DefinitionText_Def2.gameObject.GetComponent<RectTransform>(), 1f, _timeToFadeIn).setEase(LeanTweenType.linear);

        StartCoroutine(WaitForThemToReadDefinition());
    }

    IEnumerator WaitForThemToReadDefinition()
    {
        var time = GameHiddenOptions.Instance.GetTime(_timeFakeLoadingTime);
        yield return new WaitForSeconds(time);

        LoadingController.Instance.HideLoading(() =>
        {
            if (Main.Instance.LoggedUser == null)
            {
                Main.Instance.Game.CanvasController.ShowPanel(Panel.AuthPanel);
            }
            else
            {
                Main.Instance.Game.CanvasController.ShowPanel(Panel.MainMenuPanel);
            }
        });
    }
}
