using System;

[Serializable]
public class EquipmentBaseStats
{
    public float health;
    public float mana;
    public float attack;
    public float magic;
    public float defense;
    public float resistance;
}

[Serializable]
public class EquipmentDetailStats
{
    public float attackSpeed;
    public float moveSpeed;
    public float statusResistance;
    public float luck;
    public float accuracy;
    public float evasion;
    public float recovery;
    public float cooldownReduction;
    public float awakening;
    public float stagger;
    public float criticalChance;
    public float criticalDamage;
    public float defensePenetration;
    public float resistancePenetration;
}

[Serializable]
public class EquipmentElementEnhancementStats
{
    public float fire;
    public float water;
    public float earth;
    public float wind;
    public float lightning;
    public float ice;
    public float light;
    public float dark;
}

[Serializable]
public class EquipmentElementResistanceStats
{
    public float fire;
    public float water;
    public float earth;
    public float wind;
    public float lightning;
    public float ice;
    public float light;
    public float dark;
}
