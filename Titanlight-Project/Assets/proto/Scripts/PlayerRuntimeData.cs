using System;
using UnityEngine;

public static class PlayerRuntimeData
{
    // Evento geral quando dinheiro ou reputação mudam
    public static event Action OnStatsChanged;
    // Eventos específicos (passam o valor atual após a mudança)
    public static event Action<int> OnMoneyChanged;
    public static event Action<int> OnReputationChanged;

    private static int _money = 0;
    private static int _reputation = 0;

    /// <summary>
    /// Inicializa dados em runtime, normalmente ao carregar do disco.
    /// </summary>
    public static void Initialize(int money, int reputation)
    {
        _money = money;
        _reputation = reputation;
        Debug.Log($"[PlayerRuntimeData] Initialize: Money={_money}, Reputation={_reputation}");
        InvokeStatsChanged();
    }

    /// <summary>
    /// Retorna dinheiro atual.
    /// </summary>
    public static int GetMoney() => _money;

    /// <summary>
    /// Retorna reputação atual.
    /// </summary>
    public static int GetReputation() => _reputation;

    /// <summary>
    /// Adiciona (ou subtrai, se amount for negativo) dinheiro.
    /// Para gastar, prefira usar SpendMoney.
    /// </summary>
    public static void AddMoney(int amount)
    {
        if (amount == 0) return;
        _money += amount;
        Debug.Log($"[PlayerRuntimeData] AddMoney: amount={amount}, new Money={_money}");
        InvokeStatsChanged();
    }

    /// <summary>
    /// Tenta gastar dinheiro. Retorna true se suficiente, false caso contrário.
    /// </summary>
    public static bool SpendMoney(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[PlayerRuntimeData] SpendMoney chamado com amount inválido: {amount}");
            return false;
        }
        if (_money >= amount)
        {
            _money -= amount;
            Debug.Log($"[PlayerRuntimeData] SpendMoney: amount={amount}, new Money={_money}");
            InvokeStatsChanged();
            return true;
        }
        else
        {
            Debug.LogWarning($"[PlayerRuntimeData] SpendMoney falhou: saldo insuficiente. Saldo={_money}, tentou gastar {amount}");
            return false;
        }
    }

    /// <summary>
    /// Adiciona (ou subtrai, se negativo) reputação.
    /// Para gastar reputação, prefira usar SpendReputation.
    /// </summary>
    public static void AddReputation(int amount)
    {
        if (amount == 0) return;
        _reputation += amount;
        Debug.Log($"[PlayerRuntimeData] AddReputation: amount={amount}, new Reputation={_reputation}");
        InvokeStatsChanged();
    }

    /// <summary>
    /// Tenta gastar reputação. Retorna true se suficiente, false caso contrário.
    /// </summary>
    public static bool SpendReputation(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[PlayerRuntimeData] SpendReputation chamado com amount inválido: {amount}");
            return false;
        }
        if (_reputation >= amount)
        {
            _reputation -= amount;
            Debug.Log($"[PlayerRuntimeData] SpendReputation: amount={amount}, new Reputation={_reputation}");
            InvokeStatsChanged();
            return true;
        }
        else
        {
            Debug.LogWarning($"[PlayerRuntimeData] SpendReputation falhou: reputação insuficiente. Reputation={_reputation}, tentou gastar {amount}");
            return false;
        }
    }

    /// <summary>
    /// Reseta dinheiro e reputação para zero (novo jogo).
    /// </summary>
    public static void Reset()
    {
        _money = 0;
        _reputation = 0;
        Debug.Log("[PlayerRuntimeData] Reset: Money=0, Reputation=0");
        InvokeStatsChanged();
    }

    private static void InvokeStatsChanged()
    {
        OnMoneyChanged?.Invoke(_money);
        OnReputationChanged?.Invoke(_reputation);
        OnStatsChanged?.Invoke();
    }
}
