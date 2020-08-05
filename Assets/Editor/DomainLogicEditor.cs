using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(DomainLogic))]
public class DomainLogicEditor : Editor
{
    private DomainLogic _myScript { get { return (DomainLogic)target; } }

    private bool _setupConfirm;
    public enum InspectorButton
    {
        RecreateUserTable, RecreateCategoryTable, RecreateQuestionsTable, WriteCategoriesData, WriteDefaultData,
        UpdateCategoriesQuestions
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

    public TextAsset obj = null;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Space(5);

        GUILayout.Label("Database");
        GUILayout.Space(5);

        if (GUILayout.Button("Recreate User Table"))
            _action = InspectorButton.RecreateUserTable;

        if (GUILayout.Button("Recreate Category Table"))
            _action = InspectorButton.RecreateCategoryTable;

        GUILayout.Space(5);
        GUILayout.Label("Write Data");
        GUILayout.Space(5);

        if (GUILayout.Button("Write Categories Data"))
            _action = InspectorButton.WriteCategoriesData;

        if (GUILayout.Button("Update Categories Questions"))
            _action = InspectorButton.UpdateCategoriesQuestions;

        GUILayout.Space(5);

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
            EditorGUILayout.EndVertical();
        }
    }

    private void ConfirmAccepted()
    {
        switch (_action)
        {
            case InspectorButton.RecreateUserTable:

                _myScript.RecreateUserTable();
                break;

            case InspectorButton.RecreateCategoryTable:

                _myScript.RecreateCategoryTable();
                break;

            case InspectorButton.WriteCategoriesData:

                _myScript.WriteCategoriesData();
                break;

            case InspectorButton.UpdateCategoriesQuestions:

                _myScript.UpdateCategoriesQuestions();
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