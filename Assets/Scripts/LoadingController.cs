using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    private static LoadingController _loadingController;
    public static LoadingController Instance
    {
        get { return _loadingController; }
    }

    private void Awake()
    {
        _loadingController = this;
    }

    public Text LoadingText;
    public Button ContinueButton;
    public List<GameObject> PlaceholderElements;

    public delegate void OnContinueButtonPress();

    [SerializeField]
    private float _timeBetweenFadings = 1.2f;
    [SerializeField]
    private float _fadeInTimeout = 0.5f;
    [SerializeField]
    private string _loadingTextString = "Loading";
    [SerializeField]
    private float _dotsAnimationTimeout = 1f;
    private int _dotCount = 0;
    private IEnumerator _animateDots;
    private LTDescr fadeAnimation;
    private OnContinueButtonPress _onContinueButtonPress;

    private bool _isFadeIn;

    public void LoadDependencies()
    {
        ContinueButton.gameObject.SetActive(false);
        LoadingText.gameObject.SetActive(false);
    }

    public void ShowLoading()
    {
        LoadingText.gameObject.SetActive(true);
        var color = GameHiddenOptions.Instance.RedColor;
        color.a = 0;
        LoadingText.color = color;

        Fade(true);

        LoadingText.text = _loadingTextString;

        _animateDots = AnimateDots();
        StartCoroutine(_animateDots);
    }

    public void HideLoading(OnContinueButtonPress onContinueButtonPress = null)
    {
        LeanTween.cancel(fadeAnimation.id);
        StopCoroutine(_animateDots);

        LoadingText.gameObject.SetActive(false);
        foreach (var item in PlaceholderElements)
        {
            item.SetActive(false);
        }

        if (onContinueButtonPress != null)
        {
            _onContinueButtonPress = onContinueButtonPress;

            ContinueButton.gameObject.SetActive(true);
        }
    }

    IEnumerator AnimateDots()
    {
        var time = GameHiddenOptions.Instance.GetTime(_dotsAnimationTimeout);
        yield return new WaitForSeconds(time);

        if (_dotCount > 3)
            _dotCount = 1;
        var dots = "";
        for (var i = 0; i < _dotCount; i++)
        {
            dots += ".";
        }
        _dotCount++;
        LoadingText.text = _loadingTextString + dots;

        _animateDots = AnimateDots();
        StartCoroutine(_animateDots);
    }

    public void ContinueButtonDown()
    {
        ContinueButton.gameObject.SetActive(false);
        _onContinueButtonPress();
    }

    public void Fade(bool fadeIn)
    {
        _isFadeIn = fadeIn;

        StartCoroutine(FadeRoutine(_isFadeIn ? _fadeInTimeout : 0f));
    }

    IEnumerator FadeRoutine(float timeToWait)
    {
        var time = GameHiddenOptions.Instance.GetTime(timeToWait);
        yield return new WaitForSeconds(time);

        byte from = 255;
        float to = 0f;
        LeanTweenType leanTweenType = LeanTweenType.easeInBack;
        if (_isFadeIn)
        {
            from = 0;
            to = 1f;
            leanTweenType = LeanTweenType.easeOutBack; 
        }

        var color = GameHiddenOptions.Instance.RedColor;
        color.a = from;

        LoadingText.color = color;
        fadeAnimation = LeanTween.alphaText(LoadingText.gameObject.GetComponent<RectTransform>(), to, _timeBetweenFadings).setEase(leanTweenType);

        if (_isFadeIn)
            fadeAnimation.setOnComplete(OnFadeInComplete);
        else
            fadeAnimation.setOnComplete(OnFadeOutComplete);
    }

    public System.Action OnFadeInComplete = delegate ()
    {
        Instance.Fade(false);
    };

    public System.Action OnFadeOutComplete = delegate ()
    {
        Instance.Fade(true);
    };
}
