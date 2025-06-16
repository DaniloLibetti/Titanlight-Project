using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public int savedMoney;
    public int savedReputation;
    public string lastSaveTime;           // Ex: DateTime.UtcNow.ToString("o")
    public string playerName;             // opcional: nome do jogador
    public bool[] unlockedAchievements;   // opcional
    public List<string> inventoryItems = new List<string>(); // opcional
}
