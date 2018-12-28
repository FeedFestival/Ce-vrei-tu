using Assets.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Game))]
public class GameEditor : Editor
{
    private Game _myScript { get { return (Game)target; } }

    private bool _setupConfirm;
    public enum InspectorButton
    {
        RecreateUserTable, WriteDefaultData
    }
    private InspectorButton _actionTool;
    private InspectorButton _action
    {
        get { return _actionTool; }
        set
        {
            _actionTool = value;
            _setupConfirm = true;
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Space(5);

        GUILayout.Label("Database");
        GUILayout.Space(5);

        if (GUILayout.Button("Recreate User Table"))
            _action = InspectorButton.RecreateUserTable;

        if (GUILayout.Button("Write Default Data"))
            _action = InspectorButton.WriteDefaultData;

        GUILayout.Space(5);
        EditorGUILayout.EndVertical();

        //--------------------------------------------------------------------------------------------------------------------------------------------------------
        GUILayout.Space(20);    // CONFIRM
        //--------------------------------------------------------------------------------------------------------------------------------------------------------

        if (_setupConfirm)
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Confirm", GUILayout.Width(UsefullUtils.GetPercent(Screen.width, 25)), GUILayout.Height(50)))
                ConfirmAccepted();

            if (GUILayout.Button("Cancel", GUILayout.Width(UsefullUtils.GetPercent(Screen.width, 25)), GUILayout.Height(50)))
                _setupConfirm = false;

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
        }
    }

    private void ConfirmAccepted()
    {
        switch (_action)
        {
            case InspectorButton.RecreateUserTable:

                _myScript.RecreateUserTable();
                break;

            case InspectorButton.WriteDefaultData:

                _myScript.WriteDefaultData();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
        _setupConfirm = false;
    }
}
