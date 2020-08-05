using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameClock : MonoBehaviour
{
    public RectTransform ClockTongue;
    public float Time;
    public delegate void OnStopCallback();

    public OnStopCallback OnStop;

    private int? _rotateTweenId;
    private bool timeCompleted;

    public void InitClock(float time)
    {
        ClockTongue.eulerAngles = new Vector3(0, 0, 90f);
        Time = time;
    }

    public void StartClock()
    {
        timeCompleted = false;
        _rotateTweenId = LeanTween.rotateLocal(ClockTongue.gameObject, new Vector3(0, 0, -720f), Time).id;
        LeanTween.descr(_rotateTweenId.Value).setOnComplete(OnClockStopped);
    }

    public void StopClock()
    {
        if (_rotateTweenId.HasValue)
        {
            LeanTween.cancel(_rotateTweenId.Value);
            _rotateTweenId = null;
            if (timeCompleted == false)
                OnStop();
            timeCompleted = true;
        }
    }

    public void OnClockStopped()
    {
        timeCompleted = true;
        OnStop();
        StopClock();
    }
}
