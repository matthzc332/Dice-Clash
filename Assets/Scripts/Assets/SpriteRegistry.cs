using System.Collections.Generic;
using UnityEngine;

public class SpriteRegistry : MonoBehaviour
{
    public static SpriteRegistry Instance { get; private set; }

    [Header("Themes")]
    public ThemeSpriteSet human;
    public ThemeSpriteSet orc;
    public ThemeSpriteSet beastfolk;
    public ThemeSpriteSet nigromantes;
    public ThemeSpriteSet tutorial;

    [Header("UI Global")]
    public Sprite[] winSprites;
    public Sprite nextSprite;
    public Sprite retrySprite;
    public Sprite quitSprite;
    public Sprite blueWinSprite;
    public Sprite redWinSprite;
    public Sprite panelCartaBlue;
    public Sprite panelVictoria;
    public Sprite botonPlay2;
    public Sprite botonOK;
    public Sprite rankedSprite;

    [Header("PowerUps Icons")]
    public Sprite shakeIcon;
    public Sprite explosionIcon;
    public Sprite fireballIcon;
    public Sprite lightningIcon;
    public Sprite magicIcon;
    public Sprite glowCircle;

    [Header("PowerUps Efect")]
    public Sprite ritualCircle;
    public Sprite fistPunch;
    public Sprite fireHit1;
    public Sprite fireHit2;
    public Sprite lightningHit1;
    public Sprite lightningHit2;
    public Sprite mageIdle;
    public Sprite mageAttack;
    public Sprite mageBack;
    public Sprite trumpetSprite;

    [Header("FightCloud (6 combinaciones)")]
    public Sprite[] fightCloud_HH;
    public Sprite[] fightCloud_HO;
    public Sprite[] fightCloud_HB;
    public Sprite[] fightCloud_OO;
    public Sprite[] fightCloud_OB;
    public Sprite[] fightCloud_BB;

    [Header("Emojis")]
    public Sprite[] humanEmojisHappy;
    public Sprite[] humanEmojisSad;
    public Sprite[] humanEmojisAngry;
    public Sprite[] orcEmojisHappy;
    public Sprite[] orcEmojisSad;
    public Sprite[] orcEmojisAngry;
    public Sprite[] beastEmojisHappy;
    public Sprite[] beastEmojisSad;
    public Sprite[] beastEmojisAngry;

    [Header("Campaign/Map/Exhibidor")]
    public Sprite enemyBannerSprite;
    public Sprite cupSpriteIron;
    public Sprite cupSpriteBlood;
    public Sprite cupSpriteWild;
    public Sprite cupSpriteVoid;
    public Sprite ribbonHuman;
    public Sprite ribbonOrc;
    public Sprite ribbonBeast;
    public Sprite ribbonNigro;
    public Sprite insigniaMadera;
    public Sprite insigniaTutorial;
    public Sprite copaTuto;
    public Sprite listonTutorial;

    private readonly Dictionary<string, Sprite[]> spriteArrayCache = new Dictionary<string, Sprite[]>();
    private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public ThemeSpriteSet GetThemeSet(string theme)
    {
        switch (theme)
        {
            case "Human": return human;
            case "Orc": return orc;
            case "Beastfolk": case "Beast": return beastfolk;
            case "Nigromantes": case "NewRace": return nigromantes;
            case "Tutorial": return tutorial;
            default: return human;
        }
    }
}