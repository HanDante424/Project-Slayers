public static class MonsterStatCalculator
{
    public static float CalculateMonsterStat(
        float baseStat,
        int rank,
        int corruption)
    {
        return baseStat * rank * (1f + corruption / 10f);
    }
}
