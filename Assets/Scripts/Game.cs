using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    private float _timeSpeed = 1.0f;

    private DataService _dataService;
    public DataService DataService
    {
        get
        {
            if (_dataService == null)
                _dataService = new DataService("Database.db");
            return _dataService;
        }
    }

    public CanvasController CanvasController;

    public void LoadDependencies()
    {
        CanvasController.LoadDependencies();
        LoadingController.Instance.LoadDependencies();
    }

    public void StartGame()
    {
        CanvasController.Init();

        StartCoroutine(CreateSession());
    }

    #region DataBase Editor Functions

    public void RecreateUserTable()
    {
        DataService.RecreateUserTable();
    }

    public void WriteDefaultData()
    {
        DataService.WriteDefaultData();
    }

    #endregion

    #region UserData

    public IEnumerator CreateSession()
    {
        var time = GameHiddenOptions.Instance.GetTime(GameHiddenOptions.Instance.TimeBeforeSessionCreation);
        yield return new WaitForSeconds(time);

        Main.Instance.LoggedUser = DataService.GetDeviceUser();
    }

    #endregion

    #region Global WaitForSeconds, TimeSpeed

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

    #endregion
}
