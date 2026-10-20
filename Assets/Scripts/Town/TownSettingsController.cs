using UnityEngine;
using UnityEngine.UI;

public sealed class TownSettingsController : MonoBehaviour
{
    [SerializeField] private TownTitleController titleController;
    [SerializeField] private Transform resetDataButtonRoot;
    private Button resetDataButton;

    private void OnEnable()
    {
        resetDataButton = TownButtonBinder.Bind(resetDataButtonRoot, ResetInventory, this);
        titleController?.SetTitle("설정");
    }

    private void OnDisable()
    {
        TownButtonBinder.Unbind(resetDataButton, ResetInventory);
    }

    public void ResetInventory()
    {
        // Keep session objects/subscribers alive; Clear also removes every wearer's equipment links.
        if (GameSession.Instance != null)
            GameSession.Instance.Data.testCurrency = 0;
        InventorySession.Inventory.Clear();
        Debug.Log("[Town] 재화, 아이템, 장비 및 장착 상태를 초기화했습니다.", this);
    }
}
