using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DomainLogic : MonoBehaviour
{
    private static DomainLogic _db;
    public static DomainLogic DB
    {
        get { return _db; }
    }

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

    private void Awake()
    {
        _db = this;
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

        Persistent.GameData.LoggedUser = DataService.GetDeviceUser();
    }

    #endregion
}
