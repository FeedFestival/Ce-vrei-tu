using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public void RecreateCategoryTable()
    {
        DataService.RecreateCategoryTable();
    }

    public void WriteCategoriesData()
    {
        var categoryList = new List<Category>();

        categoryList.Add(new Category()
        {
            Name = "Stiri",
            File = "Questions_Stiri"
        });

        categoryList.Add(new Category()
        {
            Name = "Monden",
            File = "Questions_Monden"
        });

        categoryList.Add(new Category()
        {
            Name = "Manele",
            File = "Questions_Manele"
        });

        DataService.WriteCategoriesData(categoryList);
    }

    public void UpdateCategoriesQuestions()
    {
        var savedCategories = DataService.GetAllCategories();

        foreach (var category in savedCategories)
        {
            var filePath = UsefullUtils.GetPathToStreamingAssetsFile(category.File + ".html");
            category.MaxLines = UsefullUtils.CountLinesInFile(filePath);
        }

        DataService.WriteCategoriesData(savedCategories, update: true);
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

    public void RecreateQuestionTable()
    {

    }

    public void WriteQuestionsData()
    {

    }

    #endregion
}
