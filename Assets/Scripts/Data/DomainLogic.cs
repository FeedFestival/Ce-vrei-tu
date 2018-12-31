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

    [SerializeField]
    public TextAsset Categories;

    [SerializeField]
    public TextAsset Questions;

    private int _minReplacementTextLength = 7;

    #region DataBase Editor Functions

    public void RecreateUserTable()
    {
        DataService.RecreateUserTable();
    }

    public void RecreateCategoryTable()
    {
        DataService.RecreateCategoryTable();
    }

    public void RecreateQuestionTable()
    {
        DataService.RecreateQuestionTable();
    }

    public void WriteCategoriesData()
    {
        var categoryList = new List<Category>();
        string fs = Categories.text;
        string[] fLines = System.Text.RegularExpressions.Regex.Split(fs, "\n|\r|\r\n");

        for (int i = 0; i < fLines.Length; i++)
        {
            string valueLine = fLines[i];
            string[] values = System.Text.RegularExpressions.Regex.Split(valueLine, ";"); // your splitter here

            foreach (var value in values)
            {
                if (string.IsNullOrEmpty(value) == false)
                    categoryList.Add(new Category { Name = value });
            }
        }

        DataService.WriteCategoriesData(categoryList);
    }

    public void WriteQuestionsData()
    {
        var savedCategories = DataService.GetAllCategories();
        var unsavedCategories = new List<Category>();

        var questionList = new List<Question>();

        string fs = Questions.text;
        string[] fLines = System.Text.RegularExpressions.Regex.Split(fs, "\n|\r|\r\n");

        for (int i = 0; i < fLines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(fLines[i]))
                continue;

            string[] values = fLines[i].Split('<', '>');

            var question = new Question();

            int variableIndex = 0;
            foreach (string value in values)
            {
                if (value.Length == 0)
                    continue;

                if (value[0] == '_')
                {
                    if (variableIndex == 0)
                    {
                        question.Corect = value.Substring(1, value.Length - 1).ToUpper();
                        for (var c = 0; c < _minReplacementTextLength; c++)
                        {
                            question.Text += "_";
                        }
                    }
                    else if (variableIndex == 1)
                    {
                        question.Prank = value.Substring(1, value.Length - 1).ToUpper();
                    }
                    else if (variableIndex == 2)
                    {
                        try
                        {
                            var categoryName = value.Substring(1, value.Length - 1);
                            var category = savedCategories.Where(c => c.Name == categoryName).FirstOrDefault();
                            if (category == null)
                            {
                                unsavedCategories.Add(new Category() { Name = categoryName });
                                question.CategoryName = categoryName;
                            }
                            else
                                question.CategoryId = category.Id;
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning("Could not add category to Question. Check for text errors: " + fLines[i]);
                        }
                    }

                    variableIndex++;
                }
                else
                {
                    question.Text += value;
                }
            }

            if (string.IsNullOrWhiteSpace(question.Corect) == false)
            {
                questionList.Add(question);

            }
            Debug.Log(question.JSONString());
        }

        if (unsavedCategories.Count > 0)
        {
            Debug.Log("There are unsaved categories.");
            DataService.WriteCategoriesData(unsavedCategories);
            savedCategories = DataService.GetAllCategories();

            foreach (var question in questionList)
            {
                if (question.CategoryId == 0)
                {
                    question.CategoryId = savedCategories.Where(c => c.Name == question.CategoryName).FirstOrDefault().Id;
                }
            }
        }

        DataService.WriteQuestionsData(questionList);
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
