using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "Bullet", menuName = "Bullets/New bullet")]
[System.Serializable]
public class BulletData : SerializableScriptableObject
{
    public string displayName;
    [JsonIgnore]
    public Sprite visual;
    [JsonIgnore]
    public GameObject prefab;

}
