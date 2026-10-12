using UnityEngine;

[CreateAssetMenu(
    fileName = "MonsterStatsData",
    menuName = "Slayers/Monster Stats Data")]
public class MonsterStatsData : ScriptableObject
{
    [Header("Basic Stats")]
    public float health;
    public float mana;
    public float attack;
    public float magic;
    public float defense;
    public float resistance;
    public float staggerGauge;

    [Header("Detailed Stats")]
    public float statusResistance;
    public float accuracy;
    public float evasion;
    public float stagger;
    public float physicalPenetration;
    public float magicPenetration;

    [Header("Element Enhancement")]
    public float fireEnhancement;
    public float waterEnhancement;
    public float earthEnhancement;
    public float windEnhancement;
    public float lightningEnhancement;
    public float iceEnhancement;
    public float lightEnhancement;
    public float darkEnhancement;

    [Header("Element Resistance")]
    public float fireResistance;
    public float waterResistance;
    public float earthResistance;
    public float windResistance;
    public float lightningResistance;
    public float iceResistance;
    public float lightResistance;
    public float darkResistance;
}
