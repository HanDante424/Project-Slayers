using UnityEngine;
using UnityEngine.UI;

public sealed class TownResearchNavigationController : MonoBehaviour
{
    [SerializeField] private TownTitleController titleController;
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private Transform skillButtonRoot;

    private Button skillButton;

    private void OnEnable()
    {
        skillButton = TownButtonBinder.Bind(skillButtonRoot, OpenSkillResearch, this);
        OpenSkillResearch();
    }

    private void OnDisable()
    {
        TownButtonBinder.Unbind(skillButton, OpenSkillResearch);
    }

    public void OpenSkillResearch()
    {
        if (skillPanel != null)
            skillPanel.SetActive(true);

        titleController?.SetTitle("스킬 - 해금");
    }
}
