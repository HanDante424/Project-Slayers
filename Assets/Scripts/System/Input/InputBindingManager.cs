using UnityEngine;
using UnityEngine.InputSystem;

public class InputBindingManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private InputAction testSkillAction;


    private void Awake()
    {
        if (inputActions == null)
        {
            Debug.LogError("[InputBinding] InputActionAsset이 없습니다.");
            return;
        }

        testSkillAction = inputActions.FindAction("player/TestSkill");

        if (testSkillAction == null)
        {
            Debug.LogError("[InputBinding] player/TestSkill을 찾을 수 없습니다.");
            return;
        }

        ApplySavedBindings();
    }


    public void BindTestSkillToQ()
    {
        BindTestSkill("<Keyboard>/q");
    }


    public void BindTestSkillToE()
    {
        BindTestSkill("<Keyboard>/e");
    }


    private void BindTestSkill(string controlPath)
    {
        if (testSkillAction == null)
            return;

        // 현재 TestSkill에는 단일 키 Binding 하나가 있으므로 index 0
        testSkillAction.ApplyBindingOverride(0, controlPath);

        SaveBindingsToGameData();

        Debug.Log(
            $"[InputBinding] TestSkill 변경 완료 : {controlPath}"
        );
    }


    private void SaveBindingsToGameData()
    {
        if (GameSession.Instance == null)
        {
            Debug.LogError("[InputBinding] GameSession이 없습니다.");
            return;
        }

        string json = inputActions.SaveBindingOverridesAsJson();

        GameSession.Instance.Data.controlBindingsJson = json;

        Debug.Log("[InputBinding] 키 설정을 GameData에 기록");
    }


    private void ApplySavedBindings()
    {
        if (GameSession.Instance == null)
            return;

        string json =
            GameSession.Instance.Data.controlBindingsJson;

        if (string.IsNullOrEmpty(json))
            return;

        inputActions.LoadBindingOverridesFromJson(json);

        Debug.Log("[InputBinding] 저장된 키 설정을 Town에 적용");
    }
}