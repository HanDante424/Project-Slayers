using UnityEngine;
using UnityEngine.SceneManagement;

public class TestBattleManager : MonoBehaviour
{
    [Header("Clear Condition")]
    [SerializeField] private int requiredSkillUses = 3;

    [Header("Reward")]
    [SerializeField] private int clearReward = 100;

    [Header("Scene")]
    [SerializeField] private string townSceneName = "Town";

    private int currentSkillUses = 0;
    private bool isCleared = false;


    public void RegisterTestSkillUse()
    {
        if (isCleared)
            return;

        currentSkillUses++;

        Debug.Log(
            $"[Battle] TestSkill 사용 {currentSkillUses} / {requiredSkillUses}"
        );

        if (currentSkillUses >= requiredSkillUses)
        {
            ClearBattle();
        }
    }


    private void ClearBattle()
    {
        if (isCleared)
            return;

        isCleared = true;

        if (GameSession.Instance == null)
        {
            Debug.LogError("[Battle] GameSession이 없습니다.");
            return;
        }

        GameSession.Instance.Data.testCurrency += clearReward;
        GameSession.Instance.Data.stage1Cleared = true;

        Debug.Log(
            $"[Battle] 클리어 / 보상 +{clearReward}"
        );

        GameSession.Instance.SaveGame();

        Debug.Log("[Battle] 자동 저장 완료");

        SceneManager.LoadScene(townSceneName);
    }
}