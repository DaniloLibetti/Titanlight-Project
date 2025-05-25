using UnityEngine;

public static class PlayerData
{
    private static int money = 0;
    private static int reputation = 0;

    public static void AddMoney(int amount)
    {
        money += amount;
        Debug.Log($"Dinheiro adicionado: {amount}. Total: {money}");
    }

    public static void ChangeReputation(int amount)
    {
        reputation += amount;
        Debug.Log($"Reputação alterada: {amount}. Total: {reputation}");
    }

    public static int GetMoney() => money;
    public static int GetReputation() => reputation;

    public static void Reset()
    {
        money = 0;
        reputation = 0;
    }
}
