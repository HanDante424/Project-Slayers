using System.IO;
using UnityEngine;

public static class SaveManager
{
    private const string SaveFileName = "save.json";

    private static string SavePath
    {
        get
        {
            return Path.Combine(
                Application.persistentDataPath,
                SaveFileName
            );
        }
    }

    public static void Save(GameData data)
    {
        if (data == null)
        {
            Debug.LogError("[SaveManager] 저장할 GameData가 없습니다.");
            return;
        }

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(SavePath, json);

        Debug.Log($"[SaveManager] 저장 완료\n{SavePath}");
    }

    public static GameData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[SaveManager] 세이브 파일이 없습니다.");
            return null;
        }

        string json = File.ReadAllText(SavePath);

        GameData data = JsonUtility.FromJson<GameData>(json);

        Debug.Log($"[SaveManager] 로드 완료\n{SavePath}");

        return data;
    }

    public static bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }

    public static void DeleteSave()
    {
        if (!File.Exists(SavePath))
        {
            return;
        }

        File.Delete(SavePath);

        Debug.Log("[SaveManager] 세이브 파일 삭제 완료");
    }
}