using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AwardKill(Team killerTeam, Vector3 worldPos, PieceType type, bool isPowerUp)
    {
        if (killerTeam != Team.Blue) return;

        int gold = type switch
        {
            PieceType.Paladin => 3,
            PieceType.Knight => 2,
            _ => 1
        };

        if (EconomyManager.Instance != null)
            EconomyManager.Instance.AddGoldFromWorld(gold, worldPos);

        SpawnGoldText(worldPos, gold);
    }

    void SpawnGoldText(Vector3 worldPos, int amount)
    {
        if (Camera.main == null) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        Vector3 worldAtScreen = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x + 40f, screenPos.y + 60f, Mathf.Abs(Camera.main.transform.position.z - worldPos.z)));
        worldAtScreen.z = 0f;

        GameObject txtObj = new GameObject("GoldPopup");
        TextMesh tm = txtObj.AddComponent<TextMesh>();
        tm.text = $"+{amount}g";
        tm.characterSize = 0.55f;
        tm.fontSize = 48;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = new Color(1f, 0.84f, 0.1f);
        txtObj.transform.position = worldAtScreen;
        MeshRenderer mr = txtObj.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 30;

        Font font = Resources.Load<Font>("Fonts/Press_Start_2P/PressStart2P-Regular");
        if (font != null)
        {
            tm.font = font;
            MeshRenderer mr2 = txtObj.GetComponent<MeshRenderer>();
            if (mr2 != null) mr2.sharedMaterial = font.material;
        }

        StartCoroutine(FloatFade(txtObj));
    }

    IEnumerator FloatFade(GameObject txtObj)
    {
        float t = 0f;
        Vector3 start = txtObj.transform.position;
        while (t < 0.8f)
        {
            if (txtObj == null) yield break;
            t += Time.deltaTime;
            txtObj.transform.position = start + Vector3.up * (t * 1.6f);
            TextMesh tm = txtObj.GetComponent<TextMesh>();
            if (tm != null)
            {
                Color c = tm.color;
                c.a = 1f - t / 0.8f;
                tm.color = c;
            }
            yield return null;
        }
        if (txtObj != null) Destroy(txtObj);
    }
}
