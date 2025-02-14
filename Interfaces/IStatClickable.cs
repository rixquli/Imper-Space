using System.Collections.Generic;

public interface IStatClickable
{
  Dictionary<XPManager.Stats, int> BaseStat { get; set; }
  Dictionary<XPManager.Stats, int> AddedStat { get; set; }
}