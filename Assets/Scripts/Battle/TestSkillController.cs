using UnityEngine;

public class TestSkillController : MonoBehaviour
{
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private TestBattleManager battleManager;


    private void Update()
    {
        if (!inputReader.ConsumeTestSkill())
            return;

        if (GameSession.Instance == null)
            return;

        if (!GameSession.Instance.Data.testSkillUnlocked)
        {
            Debug.Log("[TestSkill] 스킬이 해금되지 않았습니다.");
            return;
        }

        Debug.Log("[TestSkill] 사용 성공");

        if (battleManager != null)
        {
            battleManager.RegisterTestSkillUse();
        }
    }
}