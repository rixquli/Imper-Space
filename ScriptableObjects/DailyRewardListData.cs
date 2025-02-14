using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DailyRewardListData", menuName = "ScriptableObjects/DailyRewardListData")]
public class DailyRewardListData : SerializableScriptableObject
{
  public List<DailyReward> dailyRewards;
}