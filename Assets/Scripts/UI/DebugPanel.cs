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

    //

    private RectTransform _rt;
    private float _screenHeight;
    private float _originalYPos;
    private float _expandedYPos;

    private float _animationSpeed = 0.8f;

    private int _logsCount = 0;

    // Start is called before the first frame update
    private void Awake()
    {
        _debugPanel = this;

        if (!ShowInGame())
        {
            ActionPanel.SetActive(false);
            DebugContainerPanel.SetActive(false);
            return;
        }
        else
        {
            ActionPanel.SetActive(true);
            DebugContainerPanel.SetActive(false);
        }

        _rt = GetComponent<RectTransform>();
        _screenHeight = gameObject.transform.parent.GetComponent<RectTransform>().sizeDelta.y;
        var screenWidth = gameObject.transform.parent.GetComponent<RectTransform>().sizeDelta.x;

        _originalYPos = _rt.localPosition.y;
        _expandedYPos = _originalYPos - _screenHeight;

        _rt.sizeDelta = new Vector2(screenWidth, _screenHeight);

        if (IsExpanded)
            Expand();

        LogsCountText.transform.parent.gameObject.SetActive(false);
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
                _rt.localPosition = new Vector3(_rt.localPosition.x, value, _rt.localPosition.z);
            }, _expandedYPos, _originalYPos, _animationSpeed).setEase(LeanTweenType.easeInOutBack);
        }
    }

    private void Expand()
    {
        if (!ShowInGame()) return;

        LeanTween.value(gameObject, (float value) =>
        {
            _rt.localPosition = new Vector3(_rt.localPosition.x, value, _rt.localPosition.z);
        }, _originalYPos, _expandedYPos, _animationSpeed).setEase(LeanTweenType.easeInOutBack);
    }

    public void Log(string message, LogType type = LogType.Log)
    {
        Debug.Log(message);

        if (!ShowInGame()) return;

        var go = Instantiate(GameHiddenOptions.Instance.DebugInfoPrefab);
        go.transform.SetParent(Content);
        go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

        go.GetComponent<DebugInfo>().SetText(message, _screenHeight);

        _logsCount++;

        go.name = _logsCount + (type == LogType.Log ? "_log" : "_error");

        LogsCountText.transform.parent.gameObject.SetActive(true);
        LogsCountText.text = _logsCount.ToString();
    }

    private void OnEnable()
    {
        if (!ShowInGame()) return;

        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        if (!ShowInGame()) return;

        // Remove callback when object goes out of scope
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (!ShowInGame()) return;

        if (type == LogType.Error || type == LogType.Exception)
        {
            Log(logString + " \n " + stackTrace, type);
        }
    }

    private bool ShowInGame()
    {
        if (Application.isEditor)
        {
            if (Show == false && ShowInEditor == false)
                return false;
            else
                return true;
        }
        else
        {
            if (Show == false)
                return false;
            return true;
        }
    }
}
