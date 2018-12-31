using System.Collections.Generic;
using Assets.Scripts.Utils;
using SQLite4Unity3d;
using UnityEngine;
using System.Collections;
using System;
using System.Net;

#region User

public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public int ProfilePicIndex { get; set; }
    public string Saying { get; set; }

    public string Email { get; set; }
    public bool IsUsingSound { get; set; }
    public int PrankCoins { get; set; }

    // Game
    public int ConnectionId;
    public bool AllreadyIn;

    public override string ToString()
    {
        return string.Format(@"[User: 
                            Id={0}, 
                            Name={1},
                            IsUsingSound={2}, 
                            ]",
                            Id,
                            Name,
                            IsUsingSound);
    }

    public User()
    {

    }

    public User(string properties)
    {
        Id = DataUtils.GetIntDataValue(properties, "ID:");
        Name = DataUtils.GetDataValue(properties, "Name:");
        IsUsingSound = DataUtils.GetBoolDataValue(properties, "IsUsingSound:");
    }
}

#endregion

#region Category

public class Category
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }

    public Category()
    {

    }

    public Category(string properties)
    {
        Id = DataUtils.GetIntDataValue(properties, "ID:");
        Name = DataUtils.GetDataValue(properties, "Name:");
    }
}

#endregion


#region Question

public class Question
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Text { get; set; }
    public string Corect { get; set; }
    public string Prank { get; set; }
    public int CategoryId { get; set; }

    public string CategoryName;

    public bool Played { get; set; }

    public Question()
    {

    }

    public string JSONString()
    {
        return string.Format(
@"Id: {0},
Text: {1},
Corect: {2},
Prank: {3},
CategoryId: {4},
            ",
Id,
Text,
Corect,
Prank,
CategoryId
            );
    }
}

#endregion

//public class Map
//{
//    [PrimaryKey, AutoIncrement]
//    public int Id { get; set; }

//    public string Name { get; set; }
//    [Unique]
//    public int Number { get; set; }

//    [Ignore]
//    public List<MapTile> MapTiles { get; set; }

//    //[Ignore]
//    //public Transform Transform { get; set; }
//    public GameObject GameObject;

//    public override string ToString()
//    {
//        return string.Format("[Map: Id={0}, Name={1}, Number-{2}, ImportantMapTiles={3}]", Id, Name, Number, MapTiles != null ? MapTiles.Count : 0);
//    }
//}

//public class MapTile
//{
//    [PrimaryKey, AutoIncrement]
//    public int Id { get; set; }

//    public int MapId { get; set; }

//    [Ignore]
//    public TileType TyleType
//    {
//        get { return (TileType)TyleTypeId; }
//        set
//        {
//            var tileType = value;
//            TyleTypeId = (int)tileType;
//        }
//    }
//    public int TyleTypeId
//    {
//        get; set;
//    }

//    [Ignore]
//    public PuzzleObject PuzzleObject
//    {
//        get { return (PuzzleObject)PuzzleObjectId; }
//        set
//        {
//            var puzzleObject = value;
//            PuzzleObjectId = (int)puzzleObject;
//        }
//    }
//    public int PuzzleObjectId
//    {
//        get; set;
//    }

//    public float X { get; set; }
//    public float Y { get; set; }

//    public int BridgeId { get; set; }

//    // Misc
//    [Ignore]
//    public Misc Misc
//    {
//        get { return (Misc)MiscId; }
//        set
//        {
//            var misc = value;
//            MiscId = (int)misc;
//        }
//    }
//    public int MiscId
//    {
//        get; set;
//    }
//    public float Rotation { get; set; }
//    public float Z { get; set; }
//    // Misc - END

//    public string PrintObject(TileType tileType)
//    {
//        if (tileType == TileType.Misc)
//            return string.Format("[Map: TyleType={0}({1}), Rotation={2}, XPos={3}, YPos={4}]", TyleType, TyleTypeId, Rotation, X, Y);
//        if (tileType == TileType.DeathZone)
//            return string.Format("[Map: TyleType={0}({1}), XPos={2}, YPos={3}]", TyleType, TyleTypeId, X, Y);
//        if (tileType == TileType.PuzzleObject)
//            return string.Format("[Map: TyleType={0}({1}), XPos={2}, YPos={3}, PuzzleObject={4}({5}) ]", TyleType, TyleTypeId, X, Y, PuzzleObject, PuzzleObjectId);

//        return "mapTile";
//    }

//    public string Error;

//    public MapTile GetMapTyle(Transform objT, TileType tileType)
//    {
//        MapTile mapTile = null;

//        Misc misc;

//        switch (tileType)
//        {
//            case TileType.Misc:

//                misc = GetEnum(objT.gameObject.name);

//                if (misc != Misc.None)
//                {
//                    mapTile = new MapTile
//                    {
//                        TyleType = tileType,
//                        Misc = misc,
//                        Rotation = objT.eulerAngles.z,
//                        X = objT.position.x,
//                        Y = objT.position.y,
//                        Z = objT.position.z
//                    };
//                }
//                break;

//            case TileType.PuzzleObject:

//                switch (objT.tag)
//                {
//                    case "Player":

//                        mapTile = new MapTile
//                        {
//                            TyleType = tileType,
//                            X = objT.position.x,
//                            Y = objT.position.y,
//                            PuzzleObject = PuzzleObject.Player
//                        };
//                        break;

//                    case "Box":

//                        mapTile = new MapTile
//                        {
//                            TyleType = tileType,
//                            X = objT.position.x,
//                            Y = objT.position.y,
//                            PuzzleObject = PuzzleObject.Box
//                        };
//                        break;

//                    case "Bridge":

//                        mapTile = new MapTile
//                        {
//                            TyleType = tileType,
//                            X = objT.position.x,
//                            Y = objT.position.y,
//                            PuzzleObject = PuzzleObject.Bridge,
//                            BridgeId = objT.GetComponent<Bridge>().Id
//                        };

//                        if (mapTile.BridgeId == 0) mapTile.Error = "Bridge has no Id.";

//                        break;

//                    case "Trigger":

//                        mapTile = new MapTile
//                        {
//                            TyleType = tileType,
//                            X = objT.position.x,
//                            Y = objT.position.y,
//                            PuzzleObject = PuzzleObject.Trigger,
//                            BridgeId = objT.GetComponent<Trigger>().Bridge.Id
//                        };

//                        if (mapTile.BridgeId == 0) mapTile.Error = "Trigger has no BridgeId.";

//                        break;

//                    case "Finish":

//                        mapTile = new MapTile
//                        {
//                            TyleType = tileType,
//                            X = objT.position.x,
//                            Y = objT.position.y,
//                            PuzzleObject = PuzzleObject.Finish
//                        };

//                        break;
//                }
//                break;

//            default:
//                mapTile.Error = "ArgumentOutOfRangeException";
//                break;
//        }
//        return mapTile;
//    }

//    private Misc GetEnum(string goName, bool isPit = false)
//    {
//        if (goName.Contains("Pit00"))
//            return Misc.Pit00;
//        if (goName.Contains("Pit01"))
//            return Misc.Pit01;
//        if (goName.Contains("Pit02"))
//            return Misc.Pit02;
//        if (goName.Contains("Pit10"))
//            return Misc.Pit10;
//        if (goName.Contains("Pit11"))
//            return Misc.Pit11;
//        if (goName.Contains("Pit12"))
//            return Misc.Pit12;
//        if (goName.Contains("Pit20"))
//            return Misc.Pit20;
//        if (goName.Contains("Pit21"))
//            return Misc.Pit21;
//        if (goName.Contains("Pit22"))
//            return Misc.Pit22;
//        if (goName.Contains("PitA00"))
//            return Misc.PitA00;
//        if (goName.Contains("PitA22"))
//            return Misc.PitA22;

//        if (goName.Contains("PitS00"))
//            return Misc.PitS00;
//        if (goName.Contains("PitS01"))
//            return Misc.PitS01;
//        if (goName.Contains("PitS02"))
//            return Misc.PitS02;
//        if (goName.Contains("PitS20"))
//            return Misc.PitS20;
//        if (goName.Contains("PitS21"))
//            return Misc.PitS21;
//        if (goName.Contains("PitS22"))
//            return Misc.PitS22;

//        if (goName.Contains("Hill00"))
//            return Misc.Hill00;
//        if (goName.Contains("Hill01"))
//            return Misc.Hill01;
//        if (goName.Contains("Hill02"))
//            return Misc.Hill02;
//        if (goName.Contains("Hill10"))
//            return Misc.Hill10;
//        if (goName.Contains("Hill11"))
//            return Misc.Hill11;
//        if (goName.Contains("Hill12"))
//            return Misc.Hill12;
//        if (goName.Contains("Hill20"))
//            return Misc.Hill20;
//        if (goName.Contains("Hill21"))
//            return Misc.Hill21;
//        if (goName.Contains("Hill22"))
//            return Misc.Hill22;

//        switch (goName)
//        {
//            case "PipeConnector":
//                return Misc.PipeConnector;
//            case "Tutorial1":
//                return Misc.Tutorial1;
//            case "PipeHorizontal":
//                return Misc.PipeHorizontal;
//        }

//        if (goName.Contains("HillS00"))
//            return Misc.HillS00;
//        if (goName.Contains("HillS02"))
//            return Misc.HillS02;
//        if (goName.Contains("HillS20"))
//            return Misc.HillS20;
//        if (goName.Contains("HillS22"))
//            return Misc.HillS22;

//        if (goName.Contains("Hill"))
//            return Misc.Hill;

//        return Misc.None;
//    }

//    // in game
//    public Box box;
//}