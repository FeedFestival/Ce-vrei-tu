using SQLite4Unity3d;
using UnityEngine;
#if !UNITY_EDITOR
using System.Collections;
using System.IO;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Assets.Scripts.Utils;
using System;

public class DataService
{
    private SQLiteConnection _connection;

    public DataService(string DatabaseName)
    {

        #region DataServiceInit


#if UNITY_EDITOR
        var dbPath = string.Format(@"Assets/StreamingAssets/{0}", DatabaseName);
#else
        // check if file exists in Application.persistentDataPath
        var filepath = string.Format("{0}/{1}", Application.persistentDataPath, DatabaseName);

        if (!File.Exists(filepath))
        {
            Debug.Log("Database not in Persistent path");
            // if it doesn't ->
            // open StreamingAssets directory and load the db ->

#if UNITY_ANDROID
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + DatabaseName);  // this is the path to your StreamingAssets in android
            while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
                 var loadDb = Application.dataPath + "/Raw/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);
#elif UNITY_WP8
                var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);

#elif UNITY_WINRT
		var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath);
#else
	var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
	// then save to Application.persistentDataPath
	File.Copy(loadDb, filepath);

#endif

            Debug.Log("Database written");
        }

        var dbPath = filepath;
#endif

        #endregion

        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        Debug.Log("Final PATH: " + dbPath);

    }

    public void RecreateUserTable()
    {
        _connection.DropTable<User>();
        _connection.CreateTable<User>();

        Debug.Log("Created USER TABLE");
    }

    public void RecreateCategoryTable()
    {
        _connection.DropTable<Category>();
        _connection.CreateTable<Category>();

        Debug.Log("Created CATEGORY TABLE");
    }

    public void RecreateQuestionTable()
    {
        _connection.DropTable<Question>();
        _connection.CreateTable<Question>();

        Debug.Log("Created QUESTION TABLE");
    }

    public void WriteCategoriesData(List<Category> categories)
    {
        _connection.InsertAll(categories);
    }

    public List<Category> GetAllCategories()
    {
        return _connection.Table<Category>().ToList();
    }

    public Category GetCategory(int categoryId)
    {
        return _connection.Table<Category>().Where(c => c.Id == categoryId).FirstOrDefault();
    }

    public void WriteQuestionsData(List<Question> questions)
    {
        var savedCount = 0;
        var updatedCount = 0;
        foreach (Question question in questions)
        {
            var existingQuestion = _connection.Table<Question>().Where(x => x.Corect == question.Corect && x.Prank == question.Prank && x.CategoryId == question.CategoryId).FirstOrDefault();
            if (existingQuestion == null)
            {
                _connection.Insert(question);
                savedCount++;
            }
            else
            {
                _connection.Update(question);
                updatedCount++;
            }
        }
        //_connection.InsertAll(questions);

        Debug.Log("Saved " + savedCount + " questions and updated " + updatedCount + " questions.");
    }

    public List<Question> GetAllQuestions()
    {
        return _connection.Table<Question>().ToList();
    }

    public int[] GetQuestionsIdsByCategory(int categoryId)
    {
        return _connection.Table<Question>().Where(q => q.CategoryId == categoryId && q.Played == false).Select(q => q.Id).ToArray();
    }

    public Question GetQuestion(int questionId)
    {
        return _connection.Table<Question>().Where(q => q.Id == questionId).FirstOrDefault();
    }

    public Question GetRandomQuestionByCategory(int? categoryId = null)
    {
        int[] questionIds;
        if (categoryId == null)
            questionIds = _connection.Table<Question>().ToList().Select(q => q.Id).ToArray();
        else
            questionIds = _connection.Table<Question>().Where(q => q.CategoryId == categoryId && q.Played == false).ToList().Select(q => q.Id).ToArray();

        if (questionIds == null || questionIds.Length == 0)
        {
            // reset the question played state
            ResetQuestionPlayedState(categoryId);
            return GetRandomQuestionByCategory(categoryId);
        }

        var index = UnityEngine.Random.Range(0, questionIds.Length - 1);
        var questionId = questionIds[index];

        return GetQuestion(questionId);
    }

    public void ResetQuestionPlayedState(int? categoryId)
    {
        if (categoryId == null)
        {
            _connection.ExecuteSql(" UPDATE Question SET Played = 0 ");
            return;
        }
        _connection.ExecuteSql(" UPDATE Question SET Played = 0 WHERE CategoryId = " + categoryId);
    }

    internal void WriteDefaultData()
    {

        Debug.Log("No Default data to write.");
    }

    public void CreateUser(User user)
    {
        _connection.Insert(user);
    }

    public void UpdateUser(User user)
    {
        int rowsAffected = _connection.Update(user);
        Debug.Log("(UPDATE User) rowsAffected : " + rowsAffected);
    }

    public User GetDeviceUser()
    {
        return _connection.Table<User>().Where(x => x.Id == 1).FirstOrDefault();
    }

    //public User GetLastUser()
    //{
    //    return _connection.Table<User>().Last();
    //}

    //public User GetUserByFacebookId(int facebookId)
    //{
    //    return _connection.Table<User>().Where(x => x.FacebookApp.FacebookId == facebookId).FirstOrDefault();
    //}

    /*
    * User - END
    * * --------------------------------------------------------------------------------------------------------------------------------------
    */

    /*
     * Map
     * * --------------------------------------------------------------------------------------------------------------------------------------
     */

    // X : 0 - 11
    // Y : 0 - 8
    //public int CreateMap(Map map)
    //{
    //    _connection.Insert(map);
    //    return map.Id;
    //}

    //public int UpdateMap(Map map)
    //{
    //    _connection.Update(map);
    //    return map.Id;
    //}

    //public Map GetMap(int mapId)
    //{
    //    return _connection.Table<Map>().Where(x => x.Id == mapId).FirstOrDefault();
    //}

    //public int GetNextMapId(int number)
    //{
    //    return _connection.Table<Map>().Where(x => x.Number == number).FirstOrDefault().Id;
    //}

    //public void CreateTiles(List<MapTile> mapTiles)
    //{
    //    _connection.InsertAll(mapTiles);
    //}

    //public IEnumerable<Map> GetMaps()
    //{
    //    return _connection.Table<Map>();
    //}

    //public IEnumerable<MapTile> GetTiles(int mapId)
    //{
    //    return _connection.Table<MapTile>().Where(x => x.MapId == mapId);
    //}

    //public void DeleteMapTiles(int mapId)
    //{
    //    var sql = string.Format("delete from MapTile where MapId = {0}", mapId);
    //    _connection.ExecuteSql(sql);
    //}

    /*
     * Map - END
     * * --------------------------------------------------------------------------------------------------------------------------------------
     */
}
