public enum StatType
{
    // Explicit values preserve existing serialized references; retired value 14 is not reused.
    // Basic Stats
    Health = 0,
    Mana = 1,
    Attack = 2,
    Magic = 3,
    Defense = 4,
    Resistance = 5,
    DefensePenetration = 18,
    ResistancePenetration = 19,

    // Detailed Stats (StaggerGauge is derived separately)
    Stagger = 15,
    CriticalChance = 16,
    CriticalDamage = 17,
    AttackSpeed = 6,
    MoveSpeed = 7,
    Accuracy = 10,
    Evasion = 11,
    StatusResistance = 8,
    Recovery = 12,
    CooldownReduction = 13,
    Luck = 9,

    // Element Stats
    FireEnhancement = 20,
    WaterEnhancement = 21,
    EarthEnhancement = 22,
    WindEnhancement = 23,
    LightningEnhancement = 24,
    IceEnhancement = 25,
    LightEnhancement = 26,
    DarkEnhancement = 27,
    FireResistance = 28,
    WaterResistance = 29,
    EarthResistance = 30,
    WindResistance = 31,
    LightningResistance = 32,
    IceResistance = 33,
    LightResistance = 34,
    DarkResistance = 35,
}
