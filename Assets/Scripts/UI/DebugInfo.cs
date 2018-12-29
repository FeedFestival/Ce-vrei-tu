using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugInfo : MonoBehaviour
{
    public Text Text;
    private LayoutElement _layoutElement;

    private bool _isExpanded;
    private float _normalHeight;
    private float _screenHeight;

    public void SetText(string text, float screenHeight)
    {
        Text.text = text;
        _screenHeight = screenHeight;
        _layoutElement = GetComponent<LayoutElement>();
        _normalHeight = _layoutElement.preferredHeight;
    }

    public void Expand()
    {
        _isExpanded = !_isExpanded;

        if (_isExpanded)
        {
            _layoutElement.preferredHeight = _screenHeight;
        }
        else
        {
            _layoutElement.preferredHeight = _normalHeight;
        }
    }
}
