

using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "Ships", menuName = "Ships/New Ship")]
[System.Serializable]
public class ShipData : SerializableScriptableObject
{
  public int shipId;
  public string displayName;
  [JsonIgnore]
  public Sprite icon;
  [JsonIgnore]
  public GameObject prebaf;
  public Color color = Color.white;
  public Dictionary<ShipStats, float> stats;
  public readonly EquipeSlot slot = EquipeSlot.Ship;
}

public enum ShipStats
{
  AttackDamage,
  Speed,
  Health,
  Shield,

  //TODO: en rajouter
}