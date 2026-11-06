using UnityEngine;
using UnityEngine.UI;

public sealed class TownResearchNavigationController : MonoBehaviour, ITownPanel
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TownTitleController titleController;
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private Transform skillButtonRoot;

    private Button skillButton;

    public GameObject Panel => panel;

    public void OnPanelShown()
    {
        skillButton = TownButtonBinder.Bind(skillButtonRoot, OpenSkillResearch, this);
        OpenSkillResearch();
    }

    public void OnPanelHidden()
    {
        TownButtonBinder.Unbind(skillButton, OpenSkillResearch);
        skillButton = null;
    }

    public void OpenSkillResearch()
    {
        TownButtonBinder.Select(skillButton, skillButton);
        if (skillPanel != null)
            skillPanel.SetActive(true);

        titleController?.SetTitle("스킬 - 해금");
    }
}
