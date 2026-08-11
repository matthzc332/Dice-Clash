using UnityEngine;
using System;

public class TimerManager : MonoBehaviour
{
    private static TimerManager _instance;
    public static TimerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("TimerManager");
                _instance = obj.AddComponent<TimerManager>();
            }
            return _instance;
        }
    }

    public float totalTime = 300f;
    public float timeRemaining;
    public bool isRunning;
    public bool isExpired;

    public Action OnTimerExpired;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public void StartTimer(float seconds = 300f)
    {
        if (GameConfig.isAutoPlay) return;
        totalTime = seconds;
        timeRemaining = seconds;
        isRunning = true;
        isExpired = false;
    }

    void Update()
    {
        if (!isRunning || isExpired) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            isExpired = true;
            OnTimerExpired?.Invoke();
        }
    }

    public string GetTimeString()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public void Stop()
    {
        isRunning = false;
    }

    public void Pause()
    {
        isRunning = false;
    }

    public void Resume()
    {
        if (!isExpired)
            isRunning = true;
    }
}
