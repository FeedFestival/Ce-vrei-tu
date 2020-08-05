using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CategoryButton : MonoBehaviour
{
    public Button Button;
    public Text Text;

    private int _categoryId;

    public delegate void OnClickCallback(int categoryId);
    private OnClickCallback _onClick;

    internal void SetActive(bool active, string name = null, string color = null, int categoryId = 0, OnClickCallback onClick = null)
    {
        Button.gameObject.SetActive(active);

        if (active)
        {
            Text.text = name;
            //Button.colors
            _categoryId = categoryId;
            _onClick = onClick;
        }
    }

    public void OnClick()
    {
        _onClick(_categoryId);
    }
}
