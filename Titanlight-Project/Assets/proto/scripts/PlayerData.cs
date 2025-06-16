using System;

public static class PlayerData
{
    // Repasse de eventos
    public static event Action OnStatsChanged
    {
        add { PlayerRuntimeData.OnStatsChanged += value; }
        remove { PlayerRuntimeData.OnStatsChanged -= value; }
    }
    public static event Action<int> OnMoneyChanged
    {
        add { PlayerRuntimeData.OnMoneyChanged += value; }
        remove { PlayerRuntimeData.OnMoneyChanged -= value; }
    }
    public static event Action<int> OnReputationChanged
    {
        add { PlayerRuntimeData.OnReputationChanged += value; }
        remove { PlayerRuntimeData.OnReputationChanged -= value; }
    }

    public static void AddMoney(int amount) => PlayerRuntimeData.AddMoney(amount);
    public static bool SpendMoney(int amount) => PlayerRuntimeData.SpendMoney(amount);
    public static int GetMoney() => PlayerRuntimeData.GetMoney();

    public static void AddReputation(int amount) => PlayerRuntimeData.AddReputation(amount);
    public static bool SpendReputation(int amount) => PlayerRuntimeData.SpendReputation(amount);
    public static int GetReputation() => PlayerRuntimeData.GetReputation();

    public static void Reset() => PlayerRuntimeData.Reset();
}
