using UnityEngine;
using System;

public static class PlayerRuntimeData
{
    public static event Action OnStatsChanged;

    private static int _money = 0;
    private static int _reputation = 0;

    // Método corrigido para mudança precisa
    public static void ChangeReputation(int amount)
    {
        _reputation -= amount;
        OnStatsChanged?.Invoke();
        Debug.Log($"[REPUTAÇÃO] Alterada por: {amount} | Total: {_reputation}");
    }

    public static void Initialize(int money, int reputation)
    {
        _money = money;
        _reputation = reputation;
        OnStatsChanged?.Invoke();
    }

    public static void AddMoney(int amount)
    {
        _money += amount;
        OnStatsChanged?.Invoke();
    }

    public static int GetMoney() => _money;
    public static int GetReputation() => _reputation;

    public static void Reset()
    {
        _money = 0;
        _reputation = 0;
        OnStatsChanged?.Invoke();
    }
}