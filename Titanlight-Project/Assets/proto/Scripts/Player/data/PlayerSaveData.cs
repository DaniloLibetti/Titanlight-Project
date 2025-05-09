using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public int coins;
    public float reputation;
    public DateTime lastSaveTime;
    public string playerName;
    public bool[] unlockedAchievements;
    public List<string> inventoryItems = new List<string>();
}