using System;
using UnityEngine;

[Serializable]
public class StatGrowth
{
    [SerializeField] private float level1Value;
    [SerializeField] private float growthPerLevel;

    public float Level1Value => level1Value;
    public float GrowthPerLevel => growthPerLevel;

    public StatGrowth(float level1Value, float growthPerLevel)
    {
        this.level1Value = level1Value;
        this.growthPerLevel = NormalizeGrowth(growthPerLevel);
    }

    public float GetValue(int level)
    {
        level = Mathf.Max(1, level);

        return level1Value +
               growthPerLevel * (level - 1);
    }

    public float GetLevel50Value()
    {
        return GetValue(50);
    }

    public void Validate()
    {
        growthPerLevel = NormalizeGrowth(growthPerLevel);
    }

    private static float NormalizeGrowth(float value)
    {
        // 성장하지 않는 스탯
        if (value == 0f)
            return 0f;

        // 양수 성장량의 최소값
        if (value > 0f && value < 0.05f)
            value = 0.05f;

        // 소수 둘째 자리까지
        value = Mathf.Round(value * 100f) / 100f;

        return value;
    }
}