public interface IDraggable
{
    bool canBeDragged { get; set; }
    bool CanBeMove { get; set; }
    bool CanBePickup { get; set; }
    bool canBeRotated { get; set; }
    bool canOpenRecruitPanel { get; set; }
    bool DisableDragObject { get; set; }
    bool IsPurchaseable { get; set; }
    bool IsBought { get; set; }
    GadgetsData GadgetsData { get; }
    int Level { get; set; }
    void OnDrag();
    void OnDragEnd();
    void SpawnAsPreview();
    void SpawnToVillage();
    void Upgrade();
    void Buy();
}
