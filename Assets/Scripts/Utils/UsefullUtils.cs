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

        public static int CountLinesInFile(string filePath)
        {
            int count = 0;
            using (System.IO.StreamReader r = new System.IO.StreamReader(filePath))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                    count++;
            }
            return count;
        }

        public static string GetPathToStreamingAssetsFile(string fileName) {
#if UNITY_EDITOR
            var filePath = string.Format(@"Assets/StreamingAssets/{0}", fileName);
#else
            // check if file exists in Application.persistentDataPath
            filePath = string.Format("{0}/{1}", Application.persistentDataPath, fileName);

            if (!File.Exists(filePath))
            {
                Debug.Log("Database not in Persistent path");
                // if it doesn't ->
                // open StreamingAssets directory and load the db ->

#if UNITY_ANDROID
                var loadFile = new WWW("jar:file://" + Application.dataPath + "!/assets/" + fileName);  // this is the path to your StreamingAssets in android
                while (!loadFile.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
                // then save to Application.persistentDataPath
                File.WriteAllBytes(filePath, loadFile.bytes);
#elif UNITY_IOS
                var loadFile = Application.dataPath + "/Raw/" + fileName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadFile, filePath);
#elif UNITY_WP8
                var loadFile = Application.dataPath + "/StreamingAssets/" + fileName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadFile, filePath);
#elif UNITY_WINRT
		        var loadFile = Application.dataPath + "/StreamingAssets/" + fileName;  // this is the path to your StreamingAssets in iOS
		        // then save to Application.persistentDataPath
		        File.Copy(loadFile, filePath);
#else
	            var loadFile = Application.dataPath + "/StreamingAssets/" + fileName;  // this is the path to your StreamingAssets in iOS
	            // then save to Application.persistentDataPath
	            File.Copy(loadFile, filePath);
#endif
                Debug.Log("Database written");
            }
#endif
            return filePath;
        }
    }

    public enum NavbarButton
    {
        HomeButton,
        FriendsButton,
        HistoryButton
    }
}