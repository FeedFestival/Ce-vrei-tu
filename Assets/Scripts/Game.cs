using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{





    #region Global WaitForSeconds, TimeSpeed

    private float _timeSpeed = 1.0f;

    /// <summary>
    /// Usefull for debuging
    /// </summary>
    public void ChangeTimeSpeed(float timeSpeed)
    {
        Time.timeScale = timeSpeed;
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.KeypadPlus))
        //{
        //    var newSpeed = _timeSpeed + 0.1f;
        //    if (newSpeed > 1f)
        //        return;
        //    _timeSpeed = _timeSpeed + 0.1f;

        //    UiController.TimeSpeedText.text = _timeSpeed.ToString();
        //    Time.timeScale = _timeSpeed;
        //}

        //if (Input.GetKeyDown(KeyCode.KeypadMinus))
        //{
        //    var newSpeed = _timeSpeed - 0.1f;
        //    if (newSpeed < 0f)
        //        return;
        //    _timeSpeed = _timeSpeed - 0.1f;

        //    UiController.TimeSpeedText.text = _timeSpeed.ToString();
        //    Time.timeScale = _timeSpeed;
        //}
    }

    internal void LoadDependencies()
    {
        throw new NotImplementedException();
    }

    internal void StartGame()
    {
        throw new NotImplementedException();
    }

    #endregion
}
