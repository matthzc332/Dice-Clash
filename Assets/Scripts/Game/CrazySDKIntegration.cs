using CrazyGames;
using UnityEngine;

public static class CrazySDKIntegration
{
    private static bool booted;
    private static bool gameplayStarted;

    public static bool IsReady => CrazySDK.IsAvailable && CrazySDK.IsInitialized;

    public static void Boot()
    {
        if (booted) return;
        booted = true;

        if (!CrazySDK.IsAvailable) return;

        CrazySDK.Init(() =>
        {
            CrazySDK.Game.AddSettingsChangeListener(OnSettingsChanged);
            ApplySettings(CrazySDK.Game.Settings);
        });
    }

    static void OnSettingsChanged(GameSettings settings)
    {
        ApplySettings(settings);
    }

    static void ApplySettings(GameSettings settings)
    {
        AudioListener.volume = settings != null && settings.muteAudio ? 0f : 1f;
    }

    public static void GameplayStart()
    {
        if (gameplayStarted) return;
        gameplayStarted = true;
        if (!IsReady) return;
        CrazySDK.Game.GameplayStart();
    }

    public static void GameplayStop()
    {
        if (!gameplayStarted) return;
        gameplayStarted = false;
        if (!IsReady) return;
        CrazySDK.Game.GameplayStop();
    }

    public static void HappyTime()
    {
        if (!IsReady) return;
        CrazySDK.Game.HappyTime();
    }
}