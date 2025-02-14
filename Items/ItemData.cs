using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Items/New Item")]
public class ItemData : SerializableScriptableObject
{
    public int itemId;
    public string displayName;
    [JsonIgnore]
    public Sprite visual;
}
