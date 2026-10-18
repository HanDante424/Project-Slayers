using System;
using System.Globalization;
using TMPro;
using UnityEngine;

public sealed class CharacterStatPanelView : MonoBehaviour
{
    [Serializable]
    public struct StatField
    {
        public StatType stat;
        public TMP_Text text;
    }

    [SerializeField] private StatField[] statFields = Array.Empty<StatField>();
    [SerializeField] private TMP_Text staggerGaugeText;
    private CharacterStats source;
    private CharacterStats subscribedSource;

    public void Bind(CharacterStats stats)
    {
        Unsubscribe();
        source = stats;
        Subscribe();
        Refresh();
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable() => Unsubscribe();

    private void Subscribe()
    {
        if (!isActiveAndEnabled || source == null || subscribedSource != null)
            return;
        subscribedSource = source;
        subscribedSource.StatsChanged += Refresh;
    }

    private void Unsubscribe()
    {
        if (subscribedSource != null)
            subscribedSource.StatsChanged -= Refresh;
        subscribedSource = null;
    }

    public void Refresh()
    {
        bool valid = source != null && source.Data != null;
        foreach (StatField field in statFields)
        {
            if (field.text != null)
                field.text.text = valid ? FormatStat(field.stat, source.GetFinalStat(field.stat)) : "—";
        }
        if (staggerGaugeText != null)
            staggerGaugeText.text = valid ? FormatNumber(source.FinalStaggerGauge) : "—";
    }

    public static string FormatNumber(float value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    public static string FormatStat(StatType stat, float value)
    {
        bool number = stat == StatType.Health || stat == StatType.Mana ||
            stat == StatType.Attack || stat == StatType.Magic;
        return FormatNumber(value) + (number ? "" : "%");
    }
}
