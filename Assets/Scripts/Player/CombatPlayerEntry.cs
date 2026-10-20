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
    [Header("Directional Camera Framing")]
    [Tooltip("Player feet viewport position when facing right. Left-facing X is mirrored (1-X); Y stays unchanged.")]
    [SerializeField] private Vector2 playerViewportPosition = new(0.3f, 0.15f);
    [Tooltip("Net horizontal world distance in the new facing direction after a turn before changing framing.")]
    [Min(0f)] [SerializeField] private float framingTurnDistance = 3f;
    [Tooltip("Framing transition duration in seconds. Fast start, very slow finish (ease-out quintic).")]
    [Min(0f)] [SerializeField] private float framingTransitionDuration = 0.2f;
    public GameObject ActivePlayer { get; private set; }
    public MapData SelectedMap { get; private set; }
    public PlayableCharacterData SelectedCharacter { get; private set; }
    private bool followSelectedPlayer;
    private CombatEntrySelection.Request entryRequest;
    private int observedFacingDirection, framingDirection;
    private float facingChangeX, lastPlayerX;
    private float framingX, framingFromX, framingTargetX, framingElapsed;

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
        int facing = ActivePlayer.transform.localScale.x < 0f ? -1 : 1;
        UpdateHorizontalFraming(ActivePlayer.transform.position.x, facing, Time.deltaTime);
        Vector3 current = combatCamera.WorldToViewportPoint(ActivePlayer.transform.position);
        Vector3 desired = combatCamera.ViewportToWorldPoint(new Vector3(
            framingX, Mathf.Clamp01(playerViewportPosition.y), current.z));
        combatCamera.transform.position += ActivePlayer.transform.position - desired;
    }

    private void ConfigureCamera()
    {
        followSelectedPlayer = combatCamera != null && ActivePlayer != null;
        if (followSelectedPlayer)
        {
            observedFacingDirection = framingDirection = ActivePlayer.transform.localScale.x < 0f ? -1 : 1;
            facingChangeX = lastPlayerX = ActivePlayer.transform.position.x;
            framingX = framingFromX = framingTargetX = FramingTarget(framingDirection);
            framingElapsed = Mathf.Max(0f, framingTransitionDuration);
        }
        LateUpdate();
    }

    private float FramingTarget(int direction)
    {
        float rightX = Mathf.Clamp01(playerViewportPosition.x);
        return direction < 0 ? 1f - rightX : rightX;
    }

    private void UpdateHorizontalFraming(float playerX, int facing, float deltaTime)
    {
        if (facing != observedFacingDirection)
        {
            observedFacingDirection = facing;
            // Include movement in the first frame of the turn, not just subsequent frames.
            facingChangeX = lastPlayerX;
        }
        if (facing != framingDirection && (playerX - facingChangeX) * facing >= Mathf.Max(0f, framingTurnDistance))
        {
            framingDirection = facing;
            framingFromX = framingX;
            framingTargetX = FramingTarget(facing);
            framingElapsed = 0f;
        }
        lastPlayerX = playerX;
        float duration = Mathf.Max(0f, framingTransitionDuration);
        framingElapsed += Mathf.Max(0f, deltaTime);
        float t = duration > 0f ? Mathf.Clamp01(framingElapsed / duration) : 1f;
        // At ~28% of the duration, ~80% of the shift is done; the remaining travel eases out slowly.
        float eased = 1f - Mathf.Pow(1f - t, 5f);
        framingX = Mathf.Lerp(framingFromX, framingTargetX, eased);
        if (t >= 1f) framingFromX = framingTargetX;
    }

    private void BindPortrait(PlayableCharacterData character)
    {
        if (portraitImage == null) return;
        portraitImage.sprite = character != null ? character.thumbnail : null;
        portraitImage.enabled = portraitImage.sprite != null;
    }
}
