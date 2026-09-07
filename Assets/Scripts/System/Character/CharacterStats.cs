using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Character Data")]
    [SerializeField] private CharacterStatsData characterData;

    [Header("Level")]
    [SerializeField]
    [Min(1)]
    private int level = 1;

    public int Level => level;
    public CharacterStatsData Data => characterData;


    // ==================================================
    // Current Level Stats
    // 현재는 CharacterStatsData의 레벨 성장값
    // ==================================================

    // Basic Stats
    public float MaxHealth =>
        characterData.health.GetValue(level);

    public float MaxMana =>
        characterData.mana.GetValue(level);

    public float Attack =>
        characterData.attack.GetValue(level);

    public float Magic =>
        characterData.magic.GetValue(level);

    public float Defense =>
        characterData.defense.GetValue(level);

    public float Resistance =>
        characterData.resistance.GetValue(level);


    // Detailed Stats
    public float AttackSpeed =>
        characterData.attackSpeed.GetValue(level);

    public float MoveSpeed =>
        characterData.moveSpeed.GetValue(level);

    public float StatusResistance =>
        characterData.statusResistance.GetValue(level);

    public float Luck =>
        characterData.luck.GetValue(level);

    public float Accuracy =>
        characterData.accuracy.GetValue(level);

    public float Evasion =>
        characterData.evasion.GetValue(level);

    public float Recovery =>
        characterData.recovery.GetValue(level);

    public float CooldownReduction =>
        characterData.cooldownReduction.GetValue(level);

    public float Awakening =>
        characterData.awakening.GetValue(level);

    public float Stagger =>
        characterData.stagger.GetValue(level);

    public float CriticalChance =>
        characterData.criticalChance.GetValue(level);

    public float CriticalDamage =>
        characterData.criticalDamage.GetValue(level);

    public float DefensePenetration =>
        characterData.defensePenetration.GetValue(level);

    public float ResistancePenetration =>
        characterData.resistancePenetration.GetValue(level);


    // Derived Stats
    public float StaggerGauge =>
        MaxHealth *
        (characterData.staggerGaugeHealthPercent / 100f);


    // Element Enhancement
    public float FireEnhancement =>
        characterData.fireEnhancement.GetValue(level);

    public float WaterEnhancement =>
        characterData.waterEnhancement.GetValue(level);

    public float EarthEnhancement =>
        characterData.earthEnhancement.GetValue(level);

    public float WindEnhancement =>
        characterData.windEnhancement.GetValue(level);

    public float LightningEnhancement =>
        characterData.lightningEnhancement.GetValue(level);

    public float IceEnhancement =>
        characterData.iceEnhancement.GetValue(level);

    public float LightEnhancement =>
        characterData.lightEnhancement.GetValue(level);

    public float DarkEnhancement =>
        characterData.darkEnhancement.GetValue(level);


    // Element Resistance
    public float FireResistance =>
        characterData.fireResistance.GetValue(level);

    public float WaterResistance =>
        characterData.waterResistance.GetValue(level);

    public float EarthResistance =>
        characterData.earthResistance.GetValue(level);

    public float WindResistance =>
        characterData.windResistance.GetValue(level);

    public float LightningResistance =>
        characterData.lightningResistance.GetValue(level);

    public float IceResistance =>
        characterData.iceResistance.GetValue(level);

    public float LightResistance =>
        characterData.lightResistance.GetValue(level);

    public float DarkResistance =>
        characterData.darkResistance.GetValue(level);


    // ==================================================
    // Final Stats
    //
    // 현재 Modifier 시스템이 없기 때문에
    // Current Level Stat과 동일하다.
    //
    // 이후 장비 / 버프 / 디버프를 이 계층에 적용한다.
    // ==================================================

    public float FinalMaxHealth => MaxHealth;
    public float FinalMaxMana => MaxMana;

    public float FinalAttack => Attack;
    public float FinalMagic => Magic;
    public float FinalDefense => Defense;
    public float FinalResistance => Resistance;

    public float FinalAttackSpeed => AttackSpeed;
    public float FinalMoveSpeed => MoveSpeed;
    public float FinalStatusResistance => StatusResistance;

    public float FinalLuck => Luck;
    public float FinalAccuracy => Accuracy;
    public float FinalEvasion => Evasion;
    public float FinalRecovery => Recovery;

    public float FinalCooldownReduction => CooldownReduction;
    public float FinalAwakening => Awakening;
    public float FinalStagger => Stagger;

    public float FinalCriticalChance => CriticalChance;
    public float FinalCriticalDamage => CriticalDamage;

    public float FinalDefensePenetration => DefensePenetration;
    public float FinalResistancePenetration => ResistancePenetration;


    // Final Derived Stat
    public float FinalStaggerGauge =>
        FinalMaxHealth *
        (characterData.staggerGaugeHealthPercent / 100f);


    // Final Element Enhancement
    public float FinalFireEnhancement => FireEnhancement;
    public float FinalWaterEnhancement => WaterEnhancement;
    public float FinalEarthEnhancement => EarthEnhancement;
    public float FinalWindEnhancement => WindEnhancement;
    public float FinalLightningEnhancement => LightningEnhancement;
    public float FinalIceEnhancement => IceEnhancement;
    public float FinalLightEnhancement => LightEnhancement;
    public float FinalDarkEnhancement => DarkEnhancement;


    // Final Element Resistance
    public float FinalFireResistance => FireResistance;
    public float FinalWaterResistance => WaterResistance;
    public float FinalEarthResistance => EarthResistance;
    public float FinalWindResistance => WindResistance;
    public float FinalLightningResistance => LightningResistance;
    public float FinalIceResistance => IceResistance;
    public float FinalLightResistance => LightResistance;
    public float FinalDarkResistance => DarkResistance;


    public void SetLevel(int newLevel)
    {
        level = Mathf.Max(1, newLevel);
    }


    private void OnValidate()
    {
        level = Mathf.Max(1, level);
    }
}