using TMPro;
using UnityEngine;

public class TownTestMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text skillStatusText;
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private TMP_Text stageClearText;


    private void Start()
    {
        RefreshUI();
    }


    public void UnlockTestSkill()
    {
        GameSession.Instance.Data.testSkillUnlocked = true;

        Debug.Log("[Town] 테스트 스킬 해금");

        RefreshUI();
    }


    public void AddTestCurrency()
    {
        GameSession.Instance.Data.testCurrency += 10;

        Debug.Log(
            $"[Town] 테스트 재화 +10 / 현재: {GameSession.Instance.Data.testCurrency}"
        );

        RefreshUI();
    }


    public void SpendTestCurrency()
    {
        if (GameSession.Instance.Data.testCurrency < 10)
        {
            Debug.Log("[Town] 재화 부족");
            return;
        }

        GameSession.Instance.Data.testCurrency -= 10;

        Debug.Log(
            $"[Town] 테스트 재화 -10 / 현재: {GameSession.Instance.Data.testCurrency}"
        );

        RefreshUI();
    }


    public void SaveGame()
    {
        GameSession.Instance.SaveGame();

        Debug.Log("[Town] 저장 요청");
    }


    public void RefreshUI()
    {
        if (GameSession.Instance == null)
        {
            Debug.LogWarning("[Town] GameSession이 없습니다.");
            return;
        }

        GameData data = GameSession.Instance.Data;

        if (skillStatusText != null)
            skillStatusText.text =
                $"Test Skill : {(data.testSkillUnlocked ? "Unlocked" : "Locked")}";

        if (currencyText != null)
            currencyText.text =
                $"Currency : {data.testCurrency}";

        if (stageClearText != null)
            stageClearText.text =
                $"Stage 1 : {(data.stage1Cleared ? "Clear" : "Not Clear")}";
    }
}