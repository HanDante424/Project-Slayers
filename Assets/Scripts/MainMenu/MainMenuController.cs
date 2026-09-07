using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string townSceneName = "Town";

    [Header("Option UI")]
    [SerializeField] private RectTransform optionWindow;

    private void Start()
    {
        if (optionWindow != null)
            optionWindow.localScale = Vector3.zero;
    }

    public void NewGame()
    {
        if (GameSession.Instance == null)
        {
            Debug.LogError("[MainMenu] GameSession이 없습니다.");
            return;
        }

        GameSession.Instance.NewGame();
        SceneManager.LoadScene(townSceneName);
    }

    public void LoadGame()
    {
        if (GameSession.Instance == null)
        {
            Debug.LogError("[MainMenu] GameSession이 없습니다.");
            return;
        }

        bool success = GameSession.Instance.LoadGame();

        if (!success)
        {
            Debug.Log("[MainMenu] 세이브 파일이 없어 Load할 수 없습니다.");
            return;
        }

        SceneManager.LoadScene(townSceneName);
    }

    public void OpenOption()
    {
        if (optionWindow == null)
            return;

        optionWindow.localScale = Vector3.one;
    }

    public void CloseOption()
    {
        if (optionWindow == null)
            return;

        optionWindow.localScale = Vector3.zero;
    }
}