using UnityEngine;

public class PremiumHalo : MonoBehaviour
{
    private SpriteRenderer haloSr;
    private SpriteRenderer targetSr;
    private float spriteUnit = 1f;
    private static Sprite cachedGlow;

    public static bool Active()
    {
        return !GameConfig.isTutorial && !GameConfig.isAutoPlay
            && GameConfig.currentPowerupMode == PowerupMode.WithPowerups;
    }

    public static void Attach(GameObject target)
    {
        SpriteRenderer targetSr = target != null ? target.GetComponent<SpriteRenderer>() : null;
        if (targetSr == null) return;

        Sprite glow = GetGlowSprite();
        if (glow == null) return;

        GameObject aura = new GameObject("PremiumHalo");
        SpriteRenderer sr = aura.AddComponent<SpriteRenderer>();
        sr.sprite = glow;
        sr.sortingOrder = targetSr.sortingOrder - 1;
        sr.color = new Color(1f, 0.85f, 0.3f, 0.16f);

        PremiumHalo halo = aura.AddComponent<PremiumHalo>();
        halo.haloSr = sr;
        halo.targetSr = targetSr;
        halo.spriteUnit = Mathf.Max(0.01f, glow.bounds.size.x);
    }

    static Sprite GetGlowSprite()
    {
        if (cachedGlow != null) return cachedGlow;
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color gold = new Color(1f, 0.85f, 0.3f);
        float center = size / 2f;
        float radius = center - 1f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - center, dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy) / radius;
                float alpha = Mathf.Clamp01(1f - dist);
                alpha *= alpha;
                tex.SetPixel(x, y, new Color(gold.r, gold.g, gold.b, alpha));
            }
        tex.Apply();
        cachedGlow = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return cachedGlow;
    }

    void Update()
    {
        if (targetSr == null || targetSr.gameObject == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = targetSr.transform.position;
        haloSr.sortingOrder = targetSr.sortingOrder - 1;

        bool visible = targetSr.gameObject.activeSelf;
        if (visible && targetSr.sprite != null)
        {
            float sx = targetSr.sprite.bounds.size.x * targetSr.transform.localScale.x;
            float sy = targetSr.sprite.bounds.size.y * targetSr.transform.localScale.y;
            float d = Mathf.Max(sx, sy) * 1.25f;
            float pulse = 1f + Mathf.Sin(Time.time * 3f) * 0.025f;
            float s = d / spriteUnit * pulse;
            transform.localScale = new Vector3(s, s, 1f);
        }

        float alpha = visible
            ? 0.1f + (Mathf.Sin(Time.time * 2.5f) + 1f) * 0.5f * 0.08f
            : 0f;
        Color c = haloSr.color;
        c.a = alpha;
        haloSr.color = c;
    }
}