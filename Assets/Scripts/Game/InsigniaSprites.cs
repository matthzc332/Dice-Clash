using UnityEngine;

public static class InsigniaSprites
{
    public static Sprite Get(Insignia ins)
    {
        if (ins == null) return null;

        if (!string.IsNullOrEmpty(ins.id))
        {
            Sprite[] relleno = Resources.LoadAll<Sprite>("Sprites/Relleno_Insignias");
            if (relleno != null)
            {
                foreach (var s in relleno)
                {
                    if (s.name == ins.id || s.name.StartsWith(ins.id + "_")) return s;
                }
            }
        }

        int idx = -1;
        if (ins.id.StartsWith("camp_"))
        {
            int.TryParse(ins.id.Substring(5), out idx);
        }

        if (idx > 0)
        {
            if (idx < 3) idx = 3;
            Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Insignias");
            if (sprites != null)
            {
                foreach (var s in sprites)
                {
                    if (s.name == "insignia" + idx || s.name.StartsWith("insignia" + idx + "_")) return s;
                }
            }
        }

        return CreateIcon(GetRarityColor(ins.rarity));
    }

    public static Sprite CreateIcon(Color color)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color dark = color * 0.6f;
        dark.a = 1f;
        float center = size / 2f;
        float outerR = size / 2f - 4;
        float innerR = outerR * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float angle = Mathf.Atan2(y - center, x - center) * Mathf.Rad2Deg;
                bool star = Mathf.Abs(Mathf.Sin(angle * 2.5f * Mathf.Deg2Rad)) > 0.5f;

                if (dist <= outerR && dist > outerR - 3)
                    tex.SetPixel(x, y, dark);
                else if (dist <= innerR && star)
                    tex.SetPixel(x, y, color);
                else if (dist <= innerR)
                    tex.SetPixel(x, y, color * 0.8f);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public static Color GetRarityColor(string rarity)
    {
        switch (rarity)
        {
            case "common": return new Color(0.7f, 0.7f, 0.7f);
            case "rare": return new Color(0.3f, 0.5f, 1f);
            case "epic": return new Color(0.7f, 0.3f, 0.9f);
            case "legendary": return new Color(1f, 0.75f, 0.1f);
            default: return Color.white;
        }
    }
}
