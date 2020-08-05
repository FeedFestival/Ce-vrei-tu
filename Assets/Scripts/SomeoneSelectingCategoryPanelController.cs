using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SomeoneSelectingCategoryPanelController : MonoBehaviour
{
    public Text WhoIsPickingCategoryInfo;
    public InGameClock InGameClock;

    private string _nameColor;

    public void SetWhoIsPicking(string userName)
    {
        if (_nameColor == null)
            _nameColor = ColorUtility.ToHtmlStringRGBA(GameHiddenOptions.Instance.RedColor).ToLower();

        WhoIsPickingCategoryInfo.text = "<b><color=#" + _nameColor  + ">" + userName + "</color></b> is picking Category.";

        InGameClock.InitClock(30f);
        InGameClock.StartClock();
    }
}
