using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class UsefullUtils
    {
        public static float GetPercent(float value, float percent)
        {
            return (value / 100f) * percent;
        }

        public static float GetValuePercent(float value, float maxValue)
        {
            return (value * 100f) / maxValue;
        }

        public static string ConvertNumberToKs(int num)
        {
            if (num >= 1000)
                return string.Concat(num / 1000, "k");
            else
                return num.ToString();
        }

        public static Color white;
        public static Color placeholderTextColor;  // fadeGrey
        public static Color textColor;  // grey
        public static Color black;
        public static Color importantText;

        public static void InitColors()
        {
            ColorUtility.TryParseHtmlString("#E6E6E6FF", out white);
            ColorUtility.TryParseHtmlString("#18191AFF", out black);
            ColorUtility.TryParseHtmlString("#FF3232", out importantText);
            ColorUtility.TryParseHtmlString("#909090FF", out textColor); //AAAAAAFF
            ColorUtility.TryParseHtmlString("#323232FF", out placeholderTextColor);
        }
    }

    public enum NavbarButton
    {
        HomeButton,
        FriendsButton,
        HistoryButton
    }
}