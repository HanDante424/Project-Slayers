using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterStats))]
public class CharacterStatsEditor : Editor
{
    private bool showBasicStats = true;
    private bool showDetailedStats = true;
    private bool showElementEnhancement = false;
    private bool showElementResistance = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CharacterStats stats = (CharacterStats)target;

        EditorGUILayout.Space(15);

        if (stats.Data == null)
        {
            EditorGUILayout.HelpBox(
                "Character Data가 연결되어 있지 않습니다.",
                MessageType.Warning
            );

            return;
        }

        EditorGUILayout.LabelField(
            "Final Stats Preview",
            EditorStyles.boldLabel
        );

        EditorGUILayout.LabelField(
            $"Level {stats.Level} 기준 / Modifier 미적용 상태",
            EditorStyles.miniLabel
        );

        EditorGUILayout.Space(5);


        // ==================================================
        // Basic Stats
        // ==================================================

        showBasicStats = EditorGUILayout.Foldout(
            showBasicStats,
            "Basic Stats",
            true
        );

        if (showBasicStats)
        {
            EditorGUI.indentLevel++;

            DrawStat("체력", stats.MaxHealth, stats.FinalMaxHealth);
            DrawStat("마나", stats.MaxMana, stats.FinalMaxMana);
            DrawStat("공격력", stats.Attack, stats.FinalAttack);
            DrawStat("마력", stats.Magic, stats.FinalMagic);
            DrawStat("방어력", stats.Defense, stats.FinalDefense);
            DrawStat("저항력", stats.Resistance, stats.FinalResistance);

            EditorGUI.indentLevel--;
        }


        // ==================================================
        // Detailed Stats
        // ==================================================

        showDetailedStats = EditorGUILayout.Foldout(
            showDetailedStats,
            "Detailed Stats",
            true
        );

        if (showDetailedStats)
        {
            EditorGUI.indentLevel++;

            DrawStat(
                "공격속도",
                stats.AttackSpeed,
                stats.FinalAttackSpeed
            );

            DrawStat(
                "이동속도",
                stats.MoveSpeed,
                stats.FinalMoveSpeed
            );

            DrawStat(
                "상태이상 저항력",
                stats.StatusResistance,
                stats.FinalStatusResistance
            );

            DrawStat(
                "행운",
                stats.Luck,
                stats.FinalLuck
            );

            DrawStat(
                "적중률",
                stats.Accuracy,
                stats.FinalAccuracy
            );

            DrawStat(
                "회피율",
                stats.Evasion,
                stats.FinalEvasion
            );

            DrawStat(
                "회복률",
                stats.Recovery,
                stats.FinalRecovery
            );

            DrawStat(
                "쿨타임 감소",
                stats.CooldownReduction,
                stats.FinalCooldownReduction
            );

            DrawStat(
                "각성",
                stats.Awakening,
                stats.FinalAwakening
            );

            DrawStat(
                "무력화",
                stats.Stagger,
                stats.FinalStagger
            );

            DrawStat(
                "무력화 게이지",
                stats.StaggerGauge,
                stats.FinalStaggerGauge
            );

            DrawStat(
                "크리티컬 확률",
                stats.CriticalChance,
                stats.FinalCriticalChance
            );

            DrawStat(
                "크리티컬 데미지",
                stats.CriticalDamage,
                stats.FinalCriticalDamage
            );

            DrawStat(
                "물리 관통력",
                stats.DefensePenetration,
                stats.FinalDefensePenetration
            );

            DrawStat(
                "마법 관통력",
                stats.ResistancePenetration,
                stats.FinalResistancePenetration
            );

            EditorGUI.indentLevel--;
        }


        // ==================================================
        // Element Enhancement
        // ==================================================

        showElementEnhancement = EditorGUILayout.Foldout(
            showElementEnhancement,
            "Element Enhancement",
            true
        );

        if (showElementEnhancement)
        {
            EditorGUI.indentLevel++;

            DrawStat(
                "화염 강화",
                stats.FireEnhancement,
                stats.FinalFireEnhancement
            );

            DrawStat(
                "물 강화",
                stats.WaterEnhancement,
                stats.FinalWaterEnhancement
            );

            DrawStat(
                "대지 강화",
                stats.EarthEnhancement,
                stats.FinalEarthEnhancement
            );

            DrawStat(
                "바람 강화",
                stats.WindEnhancement,
                stats.FinalWindEnhancement
            );

            DrawStat(
                "번개 강화",
                stats.LightningEnhancement,
                stats.FinalLightningEnhancement
            );

            DrawStat(
                "얼음 강화",
                stats.IceEnhancement,
                stats.FinalIceEnhancement
            );

            DrawStat(
                "빛 강화",
                stats.LightEnhancement,
                stats.FinalLightEnhancement
            );

            DrawStat(
                "어둠 강화",
                stats.DarkEnhancement,
                stats.FinalDarkEnhancement
            );

            EditorGUI.indentLevel--;
        }


        // ==================================================
        // Element Resistance
        // ==================================================

        showElementResistance = EditorGUILayout.Foldout(
            showElementResistance,
            "Element Resistance",
            true
        );

        if (showElementResistance)
        {
            EditorGUI.indentLevel++;

            DrawStat(
                "화염 저항",
                stats.FireResistance,
                stats.FinalFireResistance
            );

            DrawStat(
                "물 저항",
                stats.WaterResistance,
                stats.FinalWaterResistance
            );

            DrawStat(
                "대지 저항",
                stats.EarthResistance,
                stats.FinalEarthResistance
            );

            DrawStat(
                "바람 저항",
                stats.WindResistance,
                stats.FinalWindResistance
            );

            DrawStat(
                "번개 저항",
                stats.LightningResistance,
                stats.FinalLightningResistance
            );

            DrawStat(
                "얼음 저항",
                stats.IceResistance,
                stats.FinalIceResistance
            );

            DrawStat(
                "빛 저항",
                stats.LightResistance,
                stats.FinalLightResistance
            );

            DrawStat(
                "어둠 저항",
                stats.DarkResistance,
                stats.FinalDarkResistance
            );

            EditorGUI.indentLevel--;
        }

        if (!Application.isPlaying)
        {
            Repaint();
        }
    }


    private static void DrawStat(
        string statName,
        float levelStat,
        float finalStat
    )
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(
            statName,
            GUILayout.Width(130)
        );

        EditorGUILayout.LabelField(
            $"현재: {levelStat:F2}",
            GUILayout.Width(120)
        );

        EditorGUILayout.LabelField(
            $"최종: {finalStat:F2}"
        );

        EditorGUILayout.EndHorizontal();
    }
}