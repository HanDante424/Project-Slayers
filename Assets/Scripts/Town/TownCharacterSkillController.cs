using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class TownCharacterSkillController : MonoBehaviour
{
    [Serializable] public struct SkillList { public SkillTier tier; public Transform content, template; }
    [Serializable] public struct SkillInput { public SkillData skill; public string action; }
    [SerializeField] private TownCharacterStatsController characterStats;
    [SerializeField] private TownCharacterDataSource dataSource;
    [SerializeField] private SkillList[] lists;
    [SerializeField] private SkillInput[] skillInputs;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private TMP_Text nameText, typeText, elementText, levelText, cooldownText, costText, keyText, descriptionText, effectsText;
    [SerializeField] private Image icon;
    [SerializeField] private Button keyBindingButton;
    public event Action<PlayableCharacterData, SkillData> BindingRequested;
    [SerializeField] private TMP_Text[] sampleTexts;
    private readonly List<GameObject> rows = new();
    private readonly Dictionary<Button, SkillData> buttons = new();
    private PlayableCharacterData character;
    private InputActionAsset bindingPreview;
    public SkillData SelectedSkill { get; private set; }

    private void OnEnable()
    {
        TownCharacterSelection.Changed += Refresh;
        if (characterStats != null) characterStats.Refreshed += Refresh;
        if (dataSource != null) dataSource.Changed += Refresh;
        if (keyBindingButton != null) keyBindingButton.onClick.AddListener(RequestBinding);
        Refresh();
    }
    private void OnDisable()
    {
        TownCharacterSelection.Changed -= Refresh;
        if (characterStats != null) characterStats.Refreshed -= Refresh;
        if (dataSource != null) dataSource.Changed -= Refresh;
        if (keyBindingButton != null) keyBindingButton.onClick.RemoveListener(RequestBinding);
        ClearRows(); ClearDetail();
        if (bindingPreview != null) Destroy(bindingPreview);
        bindingPreview = null;
    }
    public static IReadOnlyList<SkillData> GetSkills(PlayableCharacterData character, SkillTier tier)
    {
        if (character == null) return Array.Empty<SkillData>();
        return (tier switch { SkillTier.Passive => character.passiveSkills, SkillTier.Basic => character.basicSkills, SkillTier.Intermediate => character.intermediateSkills, SkillTier.Advanced => character.advancedSkills, SkillTier.Ultimate => character.ultimateSkills, _ => null }) ?? (IReadOnlyList<SkillData>)Array.Empty<SkillData>();
    }
    public void Refresh()
    {
        if (!isActiveAndEnabled) return;
        var next = characterStats != null ? characterStats.CurrentCharacter : TownCharacterSelection.Character;
        bool changed = character != next; character = next;
        var previous = changed ? null : SelectedSkill;
        ClearRows(); ClearDetail();
        if (sampleTexts != null) foreach (var text in sampleTexts) Text(text, "");
        SkillData first = null;
        foreach (var list in lists ?? Array.Empty<SkillList>())
        {
            if (list.template != null) list.template.gameObject.SetActive(false);
            if (list.content == null || list.template == null) continue;
            foreach (var skill in GetSkills(character, list.tier).Where(s => s != null).Distinct())
            {
                if (first == null) first = skill;
                var root = Instantiate(list.template.gameObject, list.content, false); rows.Add(root);
                var button = root.GetComponentInChildren<Button>(true);
                var image = button != null ? button.transform.Find("Image")?.GetComponent<Image>() : null;
                Icon(image, skill.thumbnail);
                if (button != null)
                {
                    button.onClick = new Button.ButtonClickedEvent(); button.onClick.AddListener(() => SelectSkill(skill)); buttons[button] = skill;
                    var visual = button.GetComponent<MenuButtonVisual>() ?? button.gameObject.AddComponent<MenuButtonVisual>();
                    visual.Initialize(button.targetGraphic as Image, null);
                }
                root.SetActive(true);
            }
        }
        SelectSkill(previous != null && buttons.Values.Contains(previous) ? previous : first);
    }
    public void SelectSkill(SkillData skill)
    {
        if (skill == null || !buttons.Values.Contains(skill)) { ClearDetail(); return; }
        SelectedSkill = skill;
        foreach (var pair in buttons) pair.Key.GetComponent<MenuButtonVisual>()?.SetSelected(pair.Value == skill);
        int level = GetLevel(skill);
        Icon(icon, skill.thumbnail); Text(nameText, skill.skillName);
        Text(typeText, "스킬 유형 : " + (skill.category == SkillCategory.Passive ? "패시브" : "액티브") + (skill.skillTypes != null && skill.skillTypes.Count > 0 ? " / " + string.Join(", ", skill.skillTypes.Select(TypeName)) : ""));
        Text(elementText, "속성 : " + ElementName(skill.elementType));
        Text(levelText, "스킬 레벨 : " + level + " / " + skill.maxLevel);
        Text(cooldownText, "재사용 대기시간 : " + Number(skill.cooldown) + "초");
        Text(costText, "자원 소모량 : " + CostName(skill.costType) + (skill.costType == SkillCostType.None ? "" : " " + Number(skill.GetCostAtLevel(level))));
        Text(keyText, "현재 키 : " + GetBinding(skill));
        if (keyBindingButton != null) keyBindingButton.interactable = skill.category != SkillCategory.Passive && BindingRequested != null;
        Text(descriptionText, skill.description); Text(effectsText, DescribeEffects(skill, level));
    }
    private int GetLevel(SkillData skill)
    {
        var learned = dataSource != null ? dataSource.GetLearnedSkill(character, skill) : null;
        return learned != null && learned.SkillData == skill ? learned.CurrentLevel : skill.GetEffectiveLevel(1);
    }
    public string GetBinding(SkillData skill)
    {
        if (skill == null || skill.category == SkillCategory.Passive) return "해당 없음";
        string actionName = (skillInputs ?? Array.Empty<SkillInput>()).FirstOrDefault(s => s.skill == skill).action;
        if (string.IsNullOrEmpty(actionName) || inputActions == null) return "미지정";
        if (bindingPreview == null) bindingPreview = Instantiate(inputActions);
        bindingPreview.RemoveAllBindingOverrides();
        string saved = GameSession.Instance != null ? GameSession.Instance.Data.controlBindingsJson : null;
        if (!string.IsNullOrEmpty(saved))
        {
            try { bindingPreview.LoadBindingOverridesFromJson(saved); }
            catch (ArgumentException) { return "키 설정 확인 필요"; }
        }
        var action = bindingPreview.FindAction(actionName);
        return action != null ? action.GetBindingDisplayString() : "미지정";
    }
    public static string DescribeEffects(SkillData skill, int level)
    {
        if (skill == null) return "";
        var text = new StringBuilder();
        foreach (var effect in skill.effects ?? new List<SkillClassificationEffectData>())
        {
            if (effect == null) continue;
            text.AppendLine("분류 효과 : " + effect.target + " / " + effect.category + " / " + EffectKind(effect));
        }
        var hits = skill.hits ?? Array.Empty<SkillHitData>();
        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i]; if (hit == null) continue;
            text.AppendLine("타격 " + (i + 1));
            text.AppendLine("기본 피해 계수 : " + Number(hit.baseDamagePercent) + "%");
            text.AppendLine("레벨별 계수 증가 : " + Number(hit.damageGrowthPerLevel) + "%");
            text.AppendLine("현재 피해 계수 : " + Number(hit.baseDamagePercent + hit.damageGrowthPerLevel * (skill.GetEffectiveLevel(level) - 1)) + "%");
            text.AppendLine("타격 횟수 : " + hit.damageCount);
            text.AppendLine("경직 시간 : " + Number(hit.hitStunDuration) + "초");
            text.AppendLine("넉백 위력 : " + Number(hit.knockbackPower) + " / 배율 " + Number(hit.knockbackMultiplier));
            text.AppendLine("무력화 계수 : " + Number(hit.staggerPercent) + "%");
            foreach (var effect in hit.effects ?? new List<SkillHitEffectData>())
            {
                if (effect == null) continue;
                text.AppendLine("타격 효과 : " + effect.target + " / " + effect.category + " / " + EffectKind(effect));
                text.AppendLine("기본 수치 : " + Number(effect.optionValue) + " / 레벨별 증가 " + Number(effect.optionGrowthPerLevel));
            }
            text.AppendLine();
        }
        return text.ToString().TrimEnd();
    }
    private static string EffectKind(SkillHitEffectData e) => e.category switch { EffectCategoryType.Damage => e.damageType.ToString(), EffectCategoryType.Buff => e.buffType.ToString(), EffectCategoryType.Debuff => e.debuffType.ToString(), _ => e.uniqueType.ToString() };
    private static string EffectKind(SkillClassificationEffectData e) => e.category switch { EffectCategoryType.Damage => e.damageType.ToString(), EffectCategoryType.Buff => e.buffType.ToString(), EffectCategoryType.Debuff => e.debuffType.ToString(), _ => e.uniqueType.ToString() };
    private static string TypeName(SkillType type) => type switch { SkillType.Attack => "공격", SkillType.Buff => "버프", SkillType.Debuff => "디버프", SkillType.Movement => "이동", SkillType.Recovery => "회복", _ => type.ToString() };
    private static string ElementName(SkillElementType type) => type switch { SkillElementType.None => "없음", SkillElementType.Fire => "화염", SkillElementType.Water => "물", SkillElementType.Earth => "대지", SkillElementType.Wind => "바람", SkillElementType.Lightning => "번개", SkillElementType.Ice => "얼음", SkillElementType.Light => "빛", SkillElementType.Dark => "어둠", _ => type.ToString() };
    private static string CostName(SkillCostType type) => type switch { SkillCostType.None => "없음", SkillCostType.Health => "체력", SkillCostType.Mana => "마나", SkillCostType.UniqueResource => "고유 자원", _ => type.ToString() };
    private void ClearRows() { foreach (var button in buttons.Keys) if (button != null) button.onClick.RemoveAllListeners(); buttons.Clear(); foreach (var row in rows) if (row != null) { row.SetActive(false); Destroy(row); } rows.Clear(); }
    private void RequestBinding()
    {
        if (SelectedSkill == null || SelectedSkill.category == SkillCategory.Passive) return;
        BindingRequested?.Invoke(character, SelectedSkill);
        SelectSkill(SelectedSkill);
    }
    private void ClearDetail() { SelectedSkill = null; foreach (var text in new[] { nameText, typeText, elementText, levelText, cooldownText, costText, keyText, descriptionText, effectsText }) Text(text, ""); Icon(icon, null); if (keyBindingButton != null) keyBindingButton.interactable = false; }
    private static string Number(float value) => CharacterStatPanelView.FormatNumber(value);
    private static void Text(TMP_Text text, string value) { if (text != null) text.text = value; }
    private static void Icon(Image image, Sprite sprite) { if (image != null) { image.sprite = sprite; image.enabled = sprite != null; } }
}
