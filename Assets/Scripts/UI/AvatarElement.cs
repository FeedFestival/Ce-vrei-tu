using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarElement : MonoBehaviour
{
    public Image Avatar;

    public bool IsSelected;
    public int Index;

    private AuthController _authController;
    private int _minWidth = 60;
    private int _maxWidth = 100;

    private float _animationSpeed = 0.2f;

    public void Init(AuthController authController, int index, bool isSelected)
    {
        _authController = authController;
        Index = index;

        if (isSelected)
            SetInitialSelected();
        else
            UnSelect(true);
    }

    public void SetImage(Sprite sprite)
    {
        Avatar.sprite = sprite;
    }

    private void SetInitialSelected()
    {
        Avatar.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        GetComponent<LayoutElement>().minWidth = _maxWidth;
        IsSelected = true;
    }

    public void SetSelected()
    {
        LeanTween.value(Avatar.gameObject, (float value) =>
        {
            Avatar.GetComponent<RectTransform>().sizeDelta = new Vector2(value, value);
        }, 50, 100, _animationSpeed);
        LeanTween.value(gameObject, (float value) =>
        {
            GetComponent<LayoutElement>().minWidth = value;
        }, _minWidth, _maxWidth, _animationSpeed);

        IsSelected = true;
        _authController.OnAvatarSelect(Index);
    }

    public void UnSelect(bool initial = false)
    {
        if (!IsSelected && !initial) return;

        IsSelected = false;

        if (initial)
        {
            Avatar.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
            GetComponent<LayoutElement>().minWidth = _minWidth;
        }
        else
        {
            LeanTween.value(Avatar.gameObject, (float value) =>
            {
                Avatar.GetComponent<RectTransform>().sizeDelta = new Vector2(value, value);
            }, 100, 50, _animationSpeed);
            LeanTween.value(gameObject, (float value) =>
            {
                GetComponent<LayoutElement>().minWidth = value;
            }, _maxWidth, _minWidth, _animationSpeed);
        }
    }
}
