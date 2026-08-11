using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GoblinDialogue : MonoBehaviour
{
    private const string KEY_SHOWN = "GoblinDialogueShown";

    public static bool HasShown()
    {
        return PlayerPrefs.GetInt(KEY_SHOWN, 0) == 1;
    }

    public static void MarkShown()
    {
        PlayerPrefs.SetInt(KEY_SHOWN, 1);
        PlayerPrefs.Save();
    }

    public void Show()
    {
        if (HasShown()) return;
        StartCoroutine(DialogueSequence());
    }

    IEnumerator DialogueSequence()
    {
        Font pressStart = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (pressStart == null) pressStart = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Sprite[] goblinSprites = Resources.LoadAll<Sprite>("Tutorial/GoblinDialogue");
        Sprite dialogueSprite = goblinSprites != null && goblinSprites.Length > 0 ? goblinSprites[0] : null;

        GameObject canvasObj = new GameObject("GoblinCanvas");
        canvasObj.transform.SetParent(transform, false);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 215;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject overlayObj = new GameObject("GoblinOverlay");
        overlayObj.transform.SetParent(canvasObj.transform, false);
        RectTransform overlayRt = overlayObj.AddComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.sizeDelta = Vector2.zero;
        Image overlay = overlayObj.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.6f);

        GameObject panelObj = new GameObject("GoblinPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        Image panelBg = panelObj.AddComponent<Image>();
        if (dialogueSprite != null)
        {
            panelBg.sprite = dialogueSprite;
            panelBg.preserveAspect = true;
            panelBg.type = Image.Type.Sliced;
            panelRt.sizeDelta = new Vector2(760, 760f / dialogueSprite.bounds.size.x * dialogueSprite.bounds.size.y);
        }
        else
        {
            panelBg.color = new Color(0.1f, 0.15f, 0.08f, 0.95f);
            panelRt.sizeDelta = new Vector2(600, 300);
        }

        string[] lines = new[]
        {
            "Power-ups are FREE in battle!",
            "Earn gold from your wins!",
            "Good luck, adventurer!"
        };

        GameObject dialogueObj = new GameObject("DialogueText");
        dialogueObj.transform.SetParent(panelObj.transform, false);
        Text dialogueText = dialogueObj.AddComponent<Text>();
        dialogueText.font = pressStart;
        dialogueText.fontSize = 14;
        dialogueText.alignment = TextAnchor.MiddleCenter;
        dialogueText.color = Color.black;
        RectTransform dialogueRt = dialogueObj.GetComponent<RectTransform>();
        dialogueRt.anchorMin = new Vector2(0.5f, 0.5f);
        dialogueRt.anchorMax = new Vector2(0.5f, 0.5f);
        dialogueRt.pivot = new Vector2(0.5f, 0.5f);
        dialogueRt.offsetMin = new Vector2(-79f, -84f);
        dialogueRt.offsetMax = new Vector2(79f, 84f);

        foreach (string line in lines)
        {
            dialogueText.text = "";
            foreach (char c in line)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(0.02f);
            }
            yield return new WaitForSeconds(0.6f);
        }

        GameObject btnObj = new GameObject("ContinueBtn");
        btnObj.transform.SetParent(panelObj.transform, false);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.3f, 0.5f, 0.2f, 0.9f);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0f);
        btnRt.anchorMax = new Vector2(0.5f, 0f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(180, 40);
        btnRt.anchoredPosition = new Vector2(0, 28);

        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = pressStart;
        btnText.text = "OK!";
        btnText.fontSize = 12;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        RectTransform btnTextRt = btnTextObj.GetComponent<RectTransform>();
        btnTextRt.anchorMin = Vector2.zero;
        btnTextRt.anchorMax = Vector2.one;
        btnTextRt.sizeDelta = Vector2.zero;

        bool clicked = false;
        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlayButton();
            clicked = true;
        });

        yield return new WaitUntil(() => clicked);

        MarkShown();
        Destroy(overlayObj);
        Destroy(panelObj);
        Destroy(canvasObj);
    }
}
