using UnityEngine;
using UnityEngine.UI;

public sealed class TownStorageNavigationController : MonoBehaviour
{
    [SerializeField] private TownTitleController titleController;

    [Header("Category Buttons")]
    [SerializeField] private Transform allButtonRoot;
    [SerializeField] private Transform equipmentButtonRoot;
    [SerializeField] private Transform materialsButtonRoot;
    [SerializeField] private Transform questButtonRoot;

    private Button allButton;
    private Button equipmentButton;
    private Button materialsButton;
    private Button questButton;

    private void OnEnable()
    {
        allButton = TownButtonBinder.Bind(allButtonRoot, OpenAll, this);
        equipmentButton = TownButtonBinder.Bind(equipmentButtonRoot, OpenEquipment, this);
        materialsButton = TownButtonBinder.Bind(materialsButtonRoot, OpenMaterials, this);
        questButton = TownButtonBinder.Bind(questButtonRoot, OpenQuest, this);
        OpenAll();
    }

    private void OnDisable()
    {
        TownButtonBinder.Unbind(allButton, OpenAll);
        TownButtonBinder.Unbind(equipmentButton, OpenEquipment);
        TownButtonBinder.Unbind(materialsButton, OpenMaterials);
        TownButtonBinder.Unbind(questButton, OpenQuest);
    }

    public void OpenAll()
    {
        SelectCategory(allButton);
        titleController?.SetTitle("창고 - 전체");
    }

    public void OpenEquipment()
    {
        SelectCategory(equipmentButton);
        titleController?.SetTitle("창고 - 장비");
    }

    public void OpenMaterials()
    {
        SelectCategory(materialsButton);
        titleController?.SetTitle("창고 - 소재");
    }

    public void OpenQuest()
    {
        SelectCategory(questButton);
        titleController?.SetTitle("창고 - 퀘스트");
    }

    private void SelectCategory(Button button) => TownButtonBinder.Select(button,
        allButton, equipmentButton, materialsButton, questButton);
}
