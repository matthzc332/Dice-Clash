using UnityEngine;

[CreateAssetMenu(menuName = "DiceClash/ThemeSpriteSet", fileName = "ThemeSpriteSet")]
public class ThemeSpriteSet : ScriptableObject
{
    public string themeName;
    [Header("Backgrounds")]
    public Sprite backgroundMain;
    public Sprite backgroundMid;
    public Sprite backgroundFondo3;
    public Sprite backgroundFondoTuto;
    [Header("Decor")]
    public Sprite skipTurn;
    public Sprite quitButton;
    [Header("Pieces - Idle Front")]
    public Sprite[] peonIdleFront;
    public Sprite[] knightIdleFront;
    public Sprite[] ninjaIdleFront;
    public Sprite[] paladinIdleFront;
    [Header("Pieces - Idle Back")]
    public Sprite[] peonIdleBack;
    public Sprite[] knightIdleBack;
    public Sprite[] ninjaIdleBack;
    public Sprite[] paladinIdleBack;
    [Header("Pieces - Move Front")]
    public Sprite[] peonMoveFront;
    public Sprite[] knightMoveFront;
    public Sprite[] ninjaMoveFront;
    public Sprite[] paladinMoveFront;
    [Header("Pieces - Move Back")]
    public Sprite[] peonMoveBack;
    public Sprite[] knightMoveBack;
    public Sprite[] ninjaMoveBack;
    public Sprite[] paladinMoveBack;
    [Header("Pieces - Attack Front")]
    public Sprite[] peonAttackFront;
    public Sprite[] knightAttackFront;
    public Sprite[] paladinAttackFront;
    [Header("Pieces - Attack Back")]
    public Sprite[] peonAttackBack;
    public Sprite[] knightAttackBack;
    public Sprite[] paladinAttackBack;
    [Header("Nigromantes extras (compatibilidad)")]
    public Sprite peonNecroMoveFront;
    public Sprite knightNecroMoveFront;
    public Sprite paladinNecroMoveFront2;
}