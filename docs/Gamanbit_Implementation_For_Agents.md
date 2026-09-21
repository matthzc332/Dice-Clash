# Gamanbit SDK Implementation Guide (For AI Agents)

## Overview
This document serves as an implementation guide for AI agents (and developers) implementing the `GamanbitAnalytics` SDK into a Unity project.

The SDK consists of a single singleton `MonoBehaviour` script (`GamanbitAnalytics.cs`) that handles session tracking, event queueing, persistence, and telemetry dispatching to the Gamanbit API.

## 1. Scene Setup & Configuration
To implement the SDK in a new project:
1. Create an empty `GameObject` in the initial/boot scene of the game (e.g., name it `GamanbitAnalytics`).
2. Attach the `GamanbitAnalytics` component to it.
3. Configure the serialized fields in the Inspector as follows:
   - **Api Url**: Set to `https://api.gamanbit.com/sdk/games`
   - **Game Id**: Set to the specific identifier for the game (e.g., `tasty-out`, `my-new-game`).
   - **Player Id**: **Leave Empty**. The script will automatically generate a UUID and save it to `PlayerPrefs` (`gamanbit_player_id`).
   - **Flush Interval**: `10` (Batches events and sends them every 10 seconds).
   - **Is Event Mode**: `false` by default. (If set to `true`, it deletes all `PlayerPrefs` on start and marks sessions as `is_event: true`, used for specific physical event setups).

*Note: The script uses `DontDestroyOnLoad(gameObject)` on `Awake()`, so it only needs to be instantiated once in the first scene and will persist across scene loads.*

## 2. API Usage

### Initializing a Session
The SDK auto-initializes its internal variables on `Start()`, but you must explicitly start a session to begin tracking time and events. Call this after the game has loaded:
```csharp
if (GamanbitAnalytics.Instance != null)
{
    GamanbitAnalytics.Instance.StartSession(Application.platform.ToString(), Application.version);
}
```

### Tracking Core Loop Hook
This tracks when a player successfully completes their first meaningful interaction (e.g., earning their first coin or finishing the first level). It should only be fired once per session.
```csharp
GamanbitAnalytics.Instance?.TrackCoreLoopHook(Time.timeSinceLevelLoad);
```

### Tracking FTUE (First Time User Experience) Steps
To track tutorial progression:
```csharp
GamanbitAnalytics.Instance?.TrackFtueStep("tutorial_step_1", timeSpentInSeconds);
```

### Tracking Retries
When a player fails and restarts a level:
```csharp
GamanbitAnalytics.Instance?.TrackRetryEvent("level_failed_bomb", attemptNumber);
```

### Tracking Crashes / Errors
```csharp
GamanbitAnalytics.Instance?.TrackCrashLog(exception.Message, exception.StackTrace);
```

### Ending a Session
Sessions are ended automatically if the application quits, but if you have a specific exit flow, you can manually call:
```csharp
GamanbitAnalytics.Instance?.EndSession();
```

## 3. Important Implementation Notes
- **Network Dependency:** The SDK uses `UnityWebRequest`. It automatically handles caching events in `PlayerPrefs` if the game crashes or quits before flushing, and will attempt to resend them on the next run.
- **Heartbeat:** Once `StartSession` is called, a heartbeat is sent automatically every 30 seconds (or 5 seconds in Event Mode) to keep the session alive.
- **Do not destroy the GameObject:** Ensure no other script destroys the `GamanbitAnalytics` GameObject, as it manages coroutines for flushing and heartbeats.