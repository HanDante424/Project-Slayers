using System;

[Serializable]
public class GameData
{
    // 테스트용 스킬
    public bool testSkillUnlocked = false;

    // Input System의 Binding Override 저장용
    // 지금은 사용하지 않고 나중에 연결한다.
    public string controlBindingsJson = "";

    // 테스트용 재화
    public int testCurrency = 0;

    // 테스트 스테이지 클리어 여부
    public bool stage1Cleared = false;
}