using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopMenu : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI coinsText; // texto das moedas
    [SerializeField] private GameObject shopMenu; // painel da loja

    private int coins = 0;

    void Start()
    {
        UpdateCoinsText(); // atualiza moedas inicial
        shopMenu.SetActive(false); // fecha loja no começo
        Time.timeScale = 1f; // jogo roda normal
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) // tecla I abre/fecha loja
        {
            if (shopMenu.activeSelf) Hide();
            else Show();
        }
    }

    public void Show() // abre loja
    {
        shopMenu.SetActive(true);
        Time.timeScale = 0f; // pausa jogo
        Cursor.visible = true; // mostra cursor
    }

    public void Hide() // fecha loja
    {
        shopMenu.SetActive(false);
        Time.timeScale = 1f; // volta jogo
        Cursor.visible = false; // esconde cursor
    }

    public void UpdateCoinsText() // atualiza UI das moedas
    {
        coins = GameManager.Instance.ScriptableObjectCount;
        coinsText.text = $"Coins: {coins}";
    }

    public void SellButton(int itemPrice) // compra item
    {
        if (coins >= itemPrice)
        {
            coins -= itemPrice;
            GameManager.Instance.RegisterScriptableObject(-itemPrice);
            UpdateCoinsText();
        }
        else Debug.Log("Moedas insuficientes!");
    }
}