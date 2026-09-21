using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Gamanbit
{
    [Serializable]
    public class TelemetryEvent
    {
        public string session_id;
        public string player_id;
        public string event_type;
        public Dictionary<string, object> event_data;
        public string client_timestamp;
    }

    [Serializable]
    internal class PersistedEvent
    {
        public string player_id;
        public string event_type;
        public string client_timestamp;
        public string event_data_json;
    }

    [Serializable]
    internal class PersistedEventBatch
    {
        public string game_id;
        public List<PersistedEvent> events = new List<PersistedEvent>();
    }

    [Serializable]
    public class TelemetryBatch
    {
        public string game_id;
        public TelemetryEvent[] events;
    }

    [Serializable]
    public class SessionStartRequest
    {
        public string game_id;
        public string player_id;
        public string platform;
        public string app_version;
        public string client_timestamp;
        public bool is_event;
    }

    [Serializable]
    public class SessionEndRequest
    {
        public string session_id;
        public string client_timestamp;
        public float active_time_seconds;
        public bool is_event;
    }

    [Serializable]
    public class ApiResponse
    {
        public bool ok;
        public string session_id;
        public int received;
    }

    public class GamanbitAnalytics : MonoBehaviour
    {
        public static GamanbitAnalytics Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private string apiUrl = "https://api.gamanbit.com/sdk/games";
        [SerializeField] private string gameId = "tasty-out";
        [SerializeField] private string playerId = "";
        [SerializeField] private float flushInterval = 10f;
        
        [Header("Event Mode")]
        [Tooltip("If true, all PlayerPrefs are deleted on start and sessions are marked as event sessions.")]
        public bool isEventMode = false;

        private string currentSessionId;
        private List<TelemetryEvent> eventQueue = new List<TelemetryEvent>();
        private float lastFlushTime;
        private bool isInitialized = false;
        private bool coreLoopHookFired = false;
        private Coroutine heartbeatCoroutine;

        private const float HeartbeatIntervalSeconds = 30f;
        private const float EventHeartbeatIntervalSeconds = 5f;

        private float GetHeartbeatInterval() => isEventMode ? EventHeartbeatIntervalSeconds : HeartbeatIntervalSeconds;

        private const string PendingEventsKey = "gamanbit_pending_events";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (isEventMode)
            {
                PlayerPrefs.DeleteAll();
                Debug.Log("[GamanbitAnalytics] EVENT MODE ENABLED: All PlayerPrefs deleted.");
            }

            if (string.IsNullOrEmpty(gameId))
            {
                gameId = "tasty-out";
                Debug.LogWarning("[GamanbitAnalytics] gameId was empty — using default 'tasty-out'.");
            }

            // Usar PlayerPrefs en lugar de PlayerPrefs directo
            playerId = PlayerPrefs.GetString("gamanbit_player_id", "");
            if (string.IsNullOrEmpty(playerId))
            {
                playerId = GeneratePlayerId();
                PlayerPrefs.SetString("gamanbit_player_id", playerId);
                PlayerPrefs.Save();
            }

            isInitialized = true;
            lastFlushTime = Time.time;

            // Enviar eventos que quedaron pendientes del run anterior
            StartCoroutine(FlushPersistedEvents());
        }

        private void Update()
        {
            if (!isInitialized) return;

            if (Time.time - lastFlushTime >= flushInterval)
            {
                FlushEvents();
                lastFlushTime = Time.time;
            }
        }

        private void OnApplicationQuit()
        {
            PersistPendingEvents();
            
            // Si estamos en el editor, intentamos enviar el fin de sesión sincrónico si es posible,
            // pero como no es posible, al menos forzamos guardar.
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && isInitialized && !string.IsNullOrEmpty(currentSessionId))
            {
                // Enviar un heartbeat manual al pausar la app
                float activeTime = Time.timeSinceLevelLoad;
                string isEventStr = isEventMode ? "true" : "false";
                var body = $"{{\"session_id\":\"{currentSessionId}\",\"active_time_seconds\":{activeTime.ToString(System.Globalization.CultureInfo.InvariantCulture)},\"is_event\":{isEventStr}}}";
                
                // No podemos garantizar que la corutina termine, pero lo intentamos
                StartCoroutine(PostRequest($"{apiUrl}/v1/sessions/heartbeat", body, null));
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                PersistPendingEvents();
            }
        }

        public void Configure(string url, string id, float flush)
        {
            apiUrl = url;
            gameId = id;
            flushInterval = flush;
            isEventMode = false;
        }

        public void StartSession(string platform = "web", string appVersion = "")
        {
            if (!isInitialized) return;

            var request = new SessionStartRequest
            {
                game_id = gameId,
                player_id = playerId,
                platform = platform,
                app_version = appVersion,
                client_timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                is_event = isEventMode
            };

            string json = JsonUtility.ToJson(request);
            StartCoroutine(PostRequest($"{apiUrl}/v1/sessions/start", json, (response) =>
            {
                if (response != null && response.ok)
                {
                    currentSessionId = response.session_id;

                    // Asignar el session_id a todos los eventos que se encolaron antes de recibir la respuesta
                    lock (eventQueue)
                    {
                        foreach (var ev in eventQueue)
                        {
                            if (string.IsNullOrEmpty(ev.session_id))
                            {
                                ev.session_id = currentSessionId;
                            }
                        }
                    }

                    // Forzar un flush para enviar de inmediato los eventos iniciales
                    FlushEvents();

                    // Iniciar heartbeat para mantener la sesión viva aunque el tab se cierre
                    if (heartbeatCoroutine != null) StopCoroutine(heartbeatCoroutine);
                    heartbeatCoroutine = StartCoroutine(HeartbeatLoop());
                }
            }));
        }

        public void EndSession()
        {
            if (!isInitialized || string.IsNullOrEmpty(currentSessionId)) return;

            // Detener el heartbeat
            if (heartbeatCoroutine != null)
            {
                StopCoroutine(heartbeatCoroutine);
                heartbeatCoroutine = null;
            }

            FlushEvents();

            float activeTime = Time.timeSinceLevelLoad;
            var request = new SessionEndRequest
            {
                session_id = currentSessionId,
                client_timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                active_time_seconds = activeTime,
                is_event = isEventMode
            };

            string json = JsonUtility.ToJson(request);
            StartCoroutine(PostRequest($"{apiUrl}/v1/sessions/end", json, (response) =>
            {
                currentSessionId = null;
            }));
        }

        public void TrackFtueStep(string stepId, float timeSpent = 0f)
        {
            TrackEvent("ftue_step", new Dictionary<string, object>
            {
                { "step_id", stepId },
                { "time_spent", timeSpent }
            });
        }

        /// <summary>
        /// Registra de inmediato el enganche al core loop (primera ganancia de oro).
        /// Solo se dispara una vez por sesión.
        /// </summary>
        public void TrackCoreLoopHook(float sessionTimeSeconds = 0f)
        {
            if (!isInitialized) return;
            if (coreLoopHookFired) return;

            coreLoopHookFired = true;

            TrackEvent("core_loop_hook", new Dictionary<string, object>
            {
                { "session_time_seconds", sessionTimeSeconds }
            });

        }

        public void CancelCoreLoopHook()
        {
            // Mantenido para compatibilidad
        }

        private IEnumerator HeartbeatLoop()
        {
            while (!string.IsNullOrEmpty(currentSessionId))
            {
                yield return new WaitForSecondsRealtime(GetHeartbeatInterval());

                if (string.IsNullOrEmpty(currentSessionId)) yield break;

                // Time.timeSinceLevelLoad se pausa automáticamente si la pestaña no tiene foco
                // y el juego está configurado para pausarse en background (WebGL por defecto).
                float activeTime = Time.timeSinceLevelLoad;
                string isEventStr = isEventMode ? "true" : "false";
                var body = $"{{\"session_id\":\"{currentSessionId}\",\"active_time_seconds\":{activeTime.ToString(System.Globalization.CultureInfo.InvariantCulture)},\"is_event\":{isEventStr}}}";
                StartCoroutine(PostRequest($"{apiUrl}/v1/sessions/heartbeat", body, (response) =>
                {
                    if (response == null || !response.ok)
                        Debug.LogWarning("[GamanbitAnalytics] Heartbeat failed");
                }));
            }
        }

        public void TrackRetryEvent(string reason = "", int attemptNumber = 0)
        {
            TrackEvent("retry_event", new Dictionary<string, object>
            {
                { "reason", reason },
                { "attempt_number", attemptNumber }
            });
        }

        public void TrackCrashLog(string errorMessage, string stackTrace = "")
        {
            TrackEvent("crash_log", new Dictionary<string, object>
            {
                { "error_message", errorMessage },
                { "stack_trace", stackTrace }
            });
        }

        private void TrackEvent(string eventType, Dictionary<string, object> eventData)
        {
            if (!isInitialized) return;

            var telemetryEvent = new TelemetryEvent
            {
                session_id = currentSessionId ?? "",
                player_id = playerId,
                event_type = eventType,
                event_data = eventData,
                client_timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            lock (eventQueue)
            {
                eventQueue.Add(telemetryEvent);
            }
        }

        private void FlushEvents()
        {
            if (string.IsNullOrEmpty(currentSessionId)) return;

            List<TelemetryEvent> eventsToSend;

            lock (eventQueue)
            {
                if (eventQueue.Count == 0) return;
                eventsToSend = new List<TelemetryEvent>(eventQueue);
                eventQueue.Clear();
            }

            // Asegurar que todos tengan el session_id actual
            foreach (var ev in eventsToSend)
            {
                if (string.IsNullOrEmpty(ev.session_id))
                {
                    ev.session_id = currentSessionId;
                }
            }

            // JsonUtility no puede serializar Dictionary<string,object>;
            // usamos BuildBatchJson para incluir correctamente event_data (step_id, etc.)
            string json = BuildBatchJson(gameId, eventsToSend);
            StartCoroutine(PostRequest($"{apiUrl}/v1/telemetry/events", json, (response) =>
            {
                if (response != null)
                {
                }
            }));
        }

        private void PersistPendingEvents()
        {
            List<TelemetryEvent> snapshot;
            lock (eventQueue)
            {
                if (eventQueue.Count == 0) return;
                snapshot = new List<TelemetryEvent>(eventQueue);
                eventQueue.Clear();
            }

            var batch = new PersistedEventBatch { game_id = gameId };
            string existing = PlayerPrefs.GetString(PendingEventsKey, "");
            if (!string.IsNullOrEmpty(existing))
            {
                try
                {
                    var prev = JsonUtility.FromJson<PersistedEventBatch>(existing);
                    if (prev?.events != null) batch.events.AddRange(prev.events);
                }
                catch { }
            }

            foreach (var ev in snapshot)
            {
                string dataJson = SerializeEventData(ev.event_data);
                batch.events.Add(new PersistedEvent
                {
                    player_id = ev.player_id,
                    event_type = ev.event_type,
                    client_timestamp = ev.client_timestamp,
                    event_data_json = dataJson
                });
            }

            PlayerPrefs.SetString(PendingEventsKey, JsonUtility.ToJson(batch));
            PlayerPrefs.Save();
        }

        private IEnumerator FlushPersistedEvents()
        {
            string raw = PlayerPrefs.GetString(PendingEventsKey, "");
            if (string.IsNullOrEmpty(raw)) yield break;

            PersistedEventBatch batch = null;
            try { batch = JsonUtility.FromJson<PersistedEventBatch>(raw); }
            catch { PlayerPrefs.DeleteKey(PendingEventsKey); yield break; }

            if (batch?.events == null || batch.events.Count == 0)
            {
                PlayerPrefs.DeleteKey(PendingEventsKey);
                yield break;
            }

            float timeout = 10f;
            while (string.IsNullOrEmpty(currentSessionId) && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            if (string.IsNullOrEmpty(currentSessionId))
            {
                Debug.LogWarning("[GamanbitAnalytics] Could not flush persisted events: no active session.");
                yield break;
            }

            // Construir JSON manualmente para preservar event_data_json de los eventos persistidos
            string json = BuildPersistedBatchJson(gameId, currentSessionId, batch.events);
            yield return StartCoroutine(PostRequest($"{apiUrl}/v1/telemetry/events", json, (response) =>
            {
                if (response != null)
                {
                    PlayerPrefs.DeleteKey(PendingEventsKey);
                    PlayerPrefs.Save();
                }
            }));
        }

        private string SerializeEventData(Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0) return "{}";
            var sb = new System.Text.StringBuilder("{");
            bool first = true;
            foreach (var kv in data)
            {
                if (!first) sb.Append(",");
                first = false;
                sb.Append($"\"{kv.Key}\":");
                if (kv.Value is string s)
                    sb.Append($"\"{EscapeJson(s)}\"");
                else if (kv.Value is float f)
                    sb.Append(f.ToString(System.Globalization.CultureInfo.InvariantCulture));
                else if (kv.Value is double d)
                    sb.Append(d.ToString(System.Globalization.CultureInfo.InvariantCulture));
                else
                    sb.Append(kv.Value?.ToString() ?? "null");
            }
            sb.Append("}");
            return sb.ToString();
        }

        private string BuildBatchJson(string gameId, List<TelemetryEvent> events)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("{\"game_id\":\"");
            sb.Append(EscapeJson(gameId));
            sb.Append("\",\"events\":");
            sb.Append("[");

            for (int i = 0; i < events.Count; i++)
            {
                if (i > 0) sb.Append(",");
                var ev = events[i];
                sb.Append("{\"session_id\":\"");
                sb.Append(EscapeJson(ev.session_id ?? ""));
                sb.Append("\",\"player_id\":\"");
                sb.Append(EscapeJson(ev.player_id ?? ""));
                sb.Append("\",\"event_type\":\"");
                sb.Append(EscapeJson(ev.event_type ?? ""));
                sb.Append("\",\"client_timestamp\":\"");
                sb.Append(EscapeJson(ev.client_timestamp ?? ""));
                sb.Append("\",\"event_data\":");
                sb.Append(SerializeEventData(ev.event_data));
                sb.Append("}");
            }

            sb.Append("]}");
            return sb.ToString();
        }

        private string BuildPersistedBatchJson(string gameId, string sessionId, List<PersistedEvent> events)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("{\"game_id\":\"");
            sb.Append(EscapeJson(gameId));
            sb.Append("\",\"events\":");
            sb.Append("[");

            for (int i = 0; i < events.Count; i++)
            {
                if (i > 0) sb.Append(",");
                var pe = events[i];
                sb.Append("{\"session_id\":\"");
                sb.Append(EscapeJson(sessionId ?? ""));
                sb.Append("\",\"player_id\":\"");
                sb.Append(EscapeJson(pe.player_id ?? ""));
                sb.Append("\",\"event_type\":\"");
                sb.Append(EscapeJson(pe.event_type ?? ""));
                sb.Append("\",\"client_timestamp\":\"");
                sb.Append(EscapeJson(pe.client_timestamp ?? ""));
                sb.Append("\",\"event_data\":");
                sb.Append(string.IsNullOrEmpty(pe.event_data_json) ? "{}" : pe.event_data_json);
                sb.Append("}");
            }

            sb.Append("]}");
            return sb.ToString();
        }

        private string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
        }

        private IEnumerator PostRequest(string url, string jsonBody, Action<ApiResponse> callback)
        {
            using (var request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<ApiResponse>(request.downloadHandler.text);
                        callback?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[GamanbitAnalytics] Failed to parse response: {e.Message}");
                        callback?.Invoke(null);
                    }
                }
                else
                {
                    Debug.LogWarning($"[GamanbitAnalytics] Request failed: {request.error}");
                    callback?.Invoke(null);
                }
            }
        }

        private string GeneratePlayerId()
        {
            return $"player_{Guid.NewGuid():N}";
        }

        public void SetPlayerId(string id)
        {
            playerId = id;
        }

        public string GetPlayerId()
        {
            return playerId;
        }
    }
}