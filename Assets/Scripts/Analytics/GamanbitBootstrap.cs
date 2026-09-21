using UnityEngine;
using Gamanbit;

public static class GamanbitBootstrap
{
    private const string ApiUrl = "https://api.gamanbit.com/sdk/games";
    private const string GameId = "dice-clash-tactics";
    private const float FlushInterval = 10f;

    private static bool booted;
    private static bool sessionStarted;
    private static bool crashTracked;

    public static void Boot()
    {
        if (booted) return;
        booted = true;

        if (GamanbitAnalytics.Instance == null)
        {
            GameObject go = new GameObject("GamanbitAnalytics");
            GamanbitAnalytics analytics = go.AddComponent<GamanbitAnalytics>();
            analytics.Configure(ApiUrl, GameId, FlushInterval);
        }

        Application.logMessageReceived += OnLogMessage;
    }

    public static void StartSessionOnce()
    {
        if (sessionStarted) return;
        if (GamanbitAnalytics.Instance == null) return;
        sessionStarted = true;
        GamanbitAnalytics.Instance.StartSession(Application.platform.ToString(), Application.version);
    }

    static void OnLogMessage(string condition, string stackTrace, LogType type)
    {
        if (type != LogType.Exception) return;
        if (crashTracked) return;
        crashTracked = true;
        GamanbitAnalytics.Instance?.TrackCrashLog(condition, stackTrace);
    }
}