using UnityEngine;

[CreateAssetMenu(fileName = "LevelPoint", menuName = "Level Point")]
public class LevelInLevelSelectionData : SerializableScriptableObject
{
    [Header("Level Description")]
    public int levelNumber;
    public int requiredLevelNumber;
    public string levelName;
    public int enemiesMinLevel;
    public int enemiesMaxLevel;

    [Header("Game Mode")]
    public GameMode gameMode = GameMode.Classic;

    [Header("Gain")]
    public RessourceType gainSubType;
    public double gainAmount;

}



