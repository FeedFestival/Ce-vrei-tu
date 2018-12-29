using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour
{
    private static DebugPanel _debugPanel;
    public static DebugPanel Phone
    {
        get { return _debugPanel; }
    }

    [Header("IMPORTANT")]

    public bool Show;
    public bool ShowInEditor;

    [Header("Properties")]

    public bool IsExpanded;
    public GameObject DebugContainerPanel;
    public GameObject ActionPanel;
    public Transform Content;
    public Text LogsCountText;

    public Scrollbar DebugScrollbar;
    //

    private RectTransform _rt;
    private float _screenHeight;
    private float _originalYPos;
    private float _expandedYPos;

    private float _animationSpeed = 0.8f;

    private int _logsCount = 0;
    private string[] _lastErrors;

    private bool _wasInitialised = false;

    private int _toShowButtonCount;
    private int _toHideButtonCount;

    // Start is called before the first frame update
    private void Awake()
    {
        _debugPanel = this;

        Init();
    }

    private void Init()
    {
        if (!ShowInGame())
        {
            ShowDebugger(false);
            return;
        }
        else
        {
            ShowDebugger(true);
        }

        _wasInitialised = true;

        _rt = GetComponent<RectTransform>();

        var actualHeight = gameObject.transform.parent.GetComponent<RectTransform>().sizeDelta.y;
        var actualWidth = gameObject.transform.parent.GetComponent<RectTransform>().sizeDelta.x;

        _screenHeight = gameObject.transform.parent.GetComponent<CanvasScaler>().referenceResolution.y;
        var screenWidth = gameObject.transform.parent.GetComponent<CanvasScaler>().referenceResolution.x;

        //Debug.Log(" actualWidth: " + actualWidth + ", actualHeight: " + actualHeight + ", _screenHeight: " + _screenHeight + ", screenWidth: " + screenWidth);

        _originalYPos = 1280;
        _expandedYPos = 0;

        _rt.sizeDelta = new Vector2(_rt.sizeDelta.x, _screenHeight);
        _rt.anchoredPosition = new Vector3(_rt.position.x, _originalYPos, _rt.position.z);

        if (IsExpanded)
            Expand();

        LogsCountText.transform.parent.gameObject.SetActive(false);

        ShowSavedErrors();
    }

    public void DebugClicked()
    {
        if (!ShowInGame()) return;

        IsExpanded = !IsExpanded;

        if (IsExpanded)
        {
            Expand();
        }
        else
        {
            LeanTween.value(gameObject, (float value) =>
            {
                _rt.anchoredPosition = new Vector3(_rt.position.x, value, _rt.position.z);
            }, _expandedYPos, _originalYPos, _animationSpeed).setEase(LeanTweenType.easeInOutBack);
        }
    }

    private void Expand()
    {
        if (!ShowInGame()) return;

        LeanTween.value(gameObject, (float value) =>
        {
            _rt.anchoredPosition = new Vector3(_rt.position.x, value, _rt.position.z);
        }, _originalYPos, _expandedYPos, _animationSpeed).setEase(LeanTweenType.easeInOutBack);

        DebugScrollbar.value = 0f;
        SetLogCount(reset: true);
    }

    public void Log(string message, LogType type = LogType.Log)
    {
        Debug.Log(message);

        if (!ShowInGame()) return;

        var go = Instantiate(GameHiddenOptions.Instance.DebugInfoPrefab);
        go.transform.SetParent(Content);
        go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

        System.DateTime localDate = System.DateTime.Now;
        go.GetComponent<DebugInfo>().SetText(localDate.ToString() + "\n" + message, _screenHeight);

        SetLogCount();

        go.name = _logsCount + (type == LogType.Log ? "_log" : "_error");
    }

    private void SetLogCount(bool reset = false)
    {
        if (reset)
        {
            _logsCount = 0;
            LogsCountText.transform.parent.gameObject.SetActive(false);
            return;
        }
        _logsCount++;
        LogsCountText.transform.parent.gameObject.SetActive(true);
        LogsCountText.text = _logsCount.ToString();
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        // Remove callback when object goes out of scope
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            System.DateTime localDate = System.DateTime.Now;
            var error = "<color=#ff0000ff>" + localDate.ToString() + "</color>\n" + logString + "\n" + stackTrace;
            if (!ShowInGame())
            {
                SaveErrors(error);
                return;
            }

            Log(error, type);
        }
    }

    private void SaveErrors(string error)
    {
        if (_lastErrors == null)
        {
            _lastErrors = new string[3] { "", "", "" };
        }

        _lastErrors[0] = _lastErrors[1];
        _lastErrors[1] = _lastErrors[2];
        _lastErrors[2] = error;
    }

    private void ShowSavedErrors()
    {
        if (_lastErrors != null)
        {
            for (var i = 0; i < _lastErrors.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(_lastErrors[i]) == false)
                    Log(_lastErrors[i], LogType.Error);
            }
            _lastErrors = null;
        }
    }

    private bool ShowInGame()
    {
        if (Application.isEditor)
        {
            if (Show == true && ShowInEditor == false)
                return false;
            //else if (Show == false && ShowInEditor == true)
            //    return true;
            else
                return Show;
        }
        else
        {
            return Show;
        }
    }

    public void ShowDebugPanel()
    {
        if (ShowInGame()) return;

        _toShowButtonCount++;
        if (_toShowButtonCount > 5)
        {
            Show = true;
            _toHideButtonCount = 0;

            if (Application.isEditor)
                ShowInEditor = true;

            if (_wasInitialised == false)
                Init();
            else
            {
                ShowDebugger(true);
                ShowSavedErrors();
            }
        }
    }
    public void HideDebugPanel()
    {
        if (!ShowInGame()) return;

        _toHideButtonCount++;
        if (_toHideButtonCount > 5)
        {
            Show = false;
            _toShowButtonCount = 0;

            if (Application.isEditor)
                ShowInEditor = false;

            ShowDebugger(false);
        }
    }

    private void ShowDebugger(bool show)
    {
        ActionPanel.SetActive(show);
        DebugContainerPanel.SetActive(show);
    }
}
