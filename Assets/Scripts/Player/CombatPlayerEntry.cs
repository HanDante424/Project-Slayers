using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public sealed class CombatPlayerEntry : MonoBehaviour
{
    [SerializeField] private GameObject standalonePlayer;
    [SerializeField] private PlayableCharacterData standaloneCharacter;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GoblinPool goblinPool;
    [SerializeField] private PlayerHealthUI healthUI;
    [SerializeField] private PlayerManaUI manaUI;
    [SerializeField] private SkillCooldownUI cooldownUI;
    [SerializeField] private Camera combatCamera;
    [Tooltip("Player feet position in the camera viewport (0..1). Applies to Town entry and standalone testing.")]
    [SerializeField] private Vector2 playerViewportPosition = new(0.19f, 0.15f);
    public GameObject ActivePlayer { get; private set; }
    public MapData SelectedMap { get; private set; }
    public PlayableCharacterData SelectedCharacter { get; private set; }
    private bool followSelectedPlayer;
    private CombatEntrySelection.Request entryRequest;

    public bool PrepareRetry()
    {
        if (entryRequest == null) { CombatEntrySelection.Clear(); return true; }
        return CombatEntrySelection.TrySet(entryRequest.Map, entryRequest.Character,
            entryRequest.PlayerPrefab, entryRequest.Level);
    }

    private void Awake()
    {
        ActivePlayer = standalonePlayer;
        BindPortrait(standaloneCharacter);
        var request = CombatEntrySelection.Consume();
        if (request == null)
        {
            ConfigureCamera();
            return; // Direct Dev_Combat retains its authored player and spawn setup.
        }
        if (standalonePlayer == null || request.PlayerPrefab == null || request.Character == null ||
            request.Character.statsData == null || request.Map == null)
        { Debug.LogError("Invalid combat selection; keeping standalone player.", this); return; }
        Vector3 position = standalonePlayer.transform.position;
        Quaternion rotation = standalonePlayer.transform.rotation;
        standalonePlayer.SetActive(false);
        // Authored entry prefabs are inactive so data is configured before gameplay Awake/OnEnable.
        ActivePlayer = Instantiate(request.PlayerPrefab, position, rotation);
        ActivePlayer.GetComponent<CharacterStats>().Initialize(request.Character.statsData, request.Level);
        string characterId = InventorySession.GetCharacterId(request.Character);
        if (characterId != null)
        {
            var binding = ActivePlayer.GetComponent<CharacterEquipmentBinding>() ?? ActivePlayer.AddComponent<CharacterEquipmentBinding>();
            binding.Initialize(ActivePlayer.GetComponent<CharacterStats>(), characterId);
        }
        ActivePlayer.GetComponent<PlayerActionController>().BindCooldownUI(cooldownUI);
        SelectedMap = request.Map;
        entryRequest = request;
        SelectedCharacter = request.Character;
        BindPortrait(SelectedCharacter);
        ActivePlayer.SetActive(true);
        goblinPool?.SetPlayer(ActivePlayer.transform);
        healthUI?.Bind(ActivePlayer.GetComponent<Health>());
        manaUI?.Bind(ActivePlayer.GetComponent<PlayerMana>());
        ConfigureCamera();
        Destroy(standalonePlayer);
    }

    private void LateUpdate()
    {
        if (!followSelectedPlayer || combatCamera == null || ActivePlayer == null) return;
        Vector3 current = combatCamera.WorldToViewportPoint(ActivePlayer.transform.position);
        Vector3 desired = combatCamera.ViewportToWorldPoint(new Vector3(
            Mathf.Clamp01(playerViewportPosition.x), Mathf.Clamp01(playerViewportPosition.y), current.z));
        combatCamera.transform.position += ActivePlayer.transform.position - desired;
    }

    private void ConfigureCamera()
    {
        followSelectedPlayer = combatCamera != null && ActivePlayer != null;
        LateUpdate();
    }

    private void BindPortrait(PlayableCharacterData character)
    {
        if (portraitImage == null) return;
        portraitImage.sprite = character != null ? character.thumbnail : null;
        portraitImage.enabled = portraitImage.sprite != null;
    }
}
