using UnityEngine;

// Panel controllers under [System]/TownUI stay active while their panel toggles,
// so TownMenuController notifies them instead of relying on OnEnable/OnDisable.
public interface ITownPanel
{
    GameObject Panel { get; }
    void OnPanelShown();
    void OnPanelHidden();
}
