using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenu : MonoBehaviour
{
    public CanvasController CanvasController;

    public void LoadDependencies()
    {
        CanvasController.LoadDependencies();
        LoadingController.Instance.LoadDependencies();
    }

    public void StartGameMenu()
    {
        CanvasController.Init();
        StartCoroutine(DomainLogic.DB.CreateSession());
    }
}
