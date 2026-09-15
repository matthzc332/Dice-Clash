using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartupVideo : MonoBehaviour
{
    private static bool shownThisSession = false;

    private Canvas canvas;
    private RawImage videoImage;
    private VideoPlayer videoPlayer;
    private RenderTexture rt;
    private bool fading = false;

    public static void Show()
    {
        if (shownThisSession) return;
        shownThisSession = true;
        GameObject go = new GameObject("StartupVideo");
        DontDestroyOnLoad(go);
        go.AddComponent<StartupVideo>();
    }

    void Start()
    {
        GameObject canvasObj = new GameObject("Canvas", typeof(RectTransform));
        canvasObj.transform.SetParent(transform, false);
        canvas = canvasObj.AddComponent<Canvas>();
        if (canvas == null)
        {
            Destroy(gameObject);
            return;
        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 250;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject bg = new GameObject("Bg", typeof(RectTransform));
        bg.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = Color.black;
        Stretch(bg.GetComponent<RectTransform>());

        GameObject rawObj = new GameObject("Video", typeof(RectTransform));
        rawObj.transform.SetParent(canvasObj.transform, false);
        videoImage = rawObj.AddComponent<RawImage>();
        Stretch(videoImage.GetComponent<RectTransform>());

        GameObject skipObj = new GameObject("Skip", typeof(RectTransform));
        skipObj.transform.SetParent(canvasObj.transform, false);
        Stretch(skipObj.GetComponent<RectTransform>());
        Image skipImg = skipObj.AddComponent<Image>();
        skipImg.color = new Color(0, 0, 0, 0);
        Button skipBtn = skipObj.AddComponent<Button>();
        skipBtn.targetGraphic = skipImg;
        skipBtn.onClick.AddListener(Skip);
        skipObj.SetActive(false);

        videoPlayer = canvasObj.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        videoPlayer.targetTexture = rt;
        if (videoImage != null) videoImage.texture = rt;
        videoPlayer.isLooping = false;
        videoPlayer.skipOnDrop = true;
        videoPlayer.prepareCompleted += OnPrepareCompleted;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.errorReceived += OnVideoError;

        StartCoroutine(ResolveAndPlay());
    }

    IEnumerator ResolveAndPlay()
    {
        string url = Application.streamingAssetsPath + "/Video/gamanbit.mp4";

        if (Application.platform == RuntimePlatform.Android)
        {
            string dest = Path.Combine(Application.persistentDataPath, "gamanbit.mp4");
            if (!File.Exists(dest))
            {
                UnityWebRequest req = UnityWebRequest.Get(url);
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    req.Dispose();
                    StartCoroutine(FadeOut());
                    yield break;
                }
                if (req.downloadHandler.data == null || req.downloadHandler.data.Length == 0)
                {
                    req.Dispose();
                    StartCoroutine(FadeOut());
                    yield break;
                }
                File.WriteAllBytes(dest, req.downloadHandler.data);
                req.Dispose();
            }
            url = dest;
        }
        else if (Application.platform != RuntimePlatform.WebGLPlayer && !File.Exists(url))
        {
            StartCoroutine(FadeOut());
            yield break;
        }

        videoPlayer.url = url;
        videoPlayer.Prepare();
        StartCoroutine(Timeout());
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void OnPrepareCompleted(VideoPlayer vp)
    {
        if (fading || canvas == null) return;
        if (rt == null)
        {
            rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
            videoPlayer.targetTexture = rt;
            if (videoImage != null) videoImage.texture = rt;
        }

        Transform skip = canvas.transform.Find("Skip");
        if (skip != null) skip.gameObject.SetActive(true);
        videoPlayer.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        StartCoroutine(FadeOut());
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError("[StartupVideo] " + message);
        StartCoroutine(FadeOut());
    }

    void Skip()
    {
        if (videoPlayer != null) videoPlayer.Stop();
        StartCoroutine(FadeOut());
    }

    IEnumerator Timeout()
    {
        float t = 0f;
        while (videoPlayer != null && !videoPlayer.isPrepared && t < 6f)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        if (videoPlayer == null) yield break;
        if (!videoPlayer.isPrepared)
            StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        if (fading) yield break;
        fading = true;
        if (videoPlayer != null) videoPlayer.Stop();
        CanvasGroup group = null;
        if (canvas != null)
        {
            group = canvas.gameObject.GetComponent<CanvasGroup>();
            if (group == null) group = canvas.gameObject.AddComponent<CanvasGroup>();
        }
        if (group != null)
            group.blocksRaycasts = false;
        float t = 0f;
        while (group != null && t < 0.4f)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(1f, 0f, t / 0.4f);
            yield return null;
        }
        if (rt != null)
        {
            rt.Release();
            rt = null;
        }
        Destroy(gameObject);
    }
}