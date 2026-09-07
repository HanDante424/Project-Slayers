using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadTown()
    {
        SceneManager.LoadScene("Town");
    }

    public void LoadCombat()
    {
        SceneManager.LoadScene("Dev_Combat");
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}