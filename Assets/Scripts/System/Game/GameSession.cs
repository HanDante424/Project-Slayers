using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Current Game Data")]
    [SerializeField]
    private GameData data = new GameData();

    public GameData Data => data;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Debug.Log("[GameSession] 생성됨");
    }


    public void NewGame()
    {
        data = new GameData();
        InventorySession.Inventory.Clear();

        Debug.Log("[GameSession] 새 게임 데이터 생성");
    }


    public bool LoadGame()
    {
        GameData loadedData = SaveManager.Load();

        if (loadedData == null)
        {
            Debug.Log("[GameSession] 불러올 세이브가 없습니다.");
            return false;
        }

        data = loadedData;

        Debug.Log("[GameSession] 게임 데이터 로드 완료");

        return true;
    }


    public void SaveGame()
    {
        SaveManager.Save(data);
    }


    public void DeleteSave()
    {
        SaveManager.DeleteSave();
    }
}
