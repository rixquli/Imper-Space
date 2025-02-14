using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPointInLevelSelection : MonoBehaviour, IDataPersistence
{
    public LevelInLevelSelectionData levelData;
    public Button button;
    [SerializeField] private Image levelIcon;
    [SerializeField] private Image lineImage;
    [SerializeField] private Sprite dottedLineSprite;
    [SerializeField] private Sprite filledLineSprite;
    [SerializeField] private TextMeshProUGUI levelNumberText;

    [SerializeField] private Color reachedColor = new Color(0, 50f / 255, 1);
    [SerializeField] private Color unreachedColor = new Color(0, 0, 0);

    [Header("Utilities")]
    public Vector2 previousLevelPosition;
    public bool hasCompleted = false;
    public bool hasReached = false;

    private void Start()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (levelIcon != null)
        {
            levelIcon.color = hasReached ? reachedColor : unreachedColor;
        }

        if (lineImage != null)
        {
            // Vector2 newPosition = new Vector2(Mathf.Abs(previousLevelPosition.x - transform.position.x), Mathf.Abs(previousLevelPosition.y - transform.position.y));
            // lineImage.transform.position = newPosition;

            lineImage.color = hasReached ? new Color(lineImage.color.r, lineImage.color.g, lineImage.color.b, 1) : new Color(lineImage.color.r, lineImage.color.g, lineImage.color.b, 0);
            lineImage.sprite = hasCompleted ? filledLineSprite : dottedLineSprite;
        }
    }

    public void LoadData(GameData data)
    {
        var level = data.levels.Find(level => level.data == levelData);
        if (level != null)
        {
            hasCompleted = level.hasCompleted;
            hasReached = level.hasReached;
            levelNumberText.text = $"lvl.{levelData.levelNumber}";
        }
        UpdateVisuals();
    }

    public void SaveData(GameData data)
    {
    }
}
