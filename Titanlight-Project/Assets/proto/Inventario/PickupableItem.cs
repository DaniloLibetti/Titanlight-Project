using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))] // obriga ter collider2D (pra detectar proximidade)
public class PickupableItem : MonoBehaviour
{
    [Tooltip("Distância máxima para interagir")]
    public float interactRange = 1.5f; // alcance de coleta

    [Tooltip("Texto de dica (coloque um TextMeshPro no mundo ou na UI)")]
    public TextMeshProUGUI promptText; // texto tipo "Aperte E"

    private int amount = 1; // quantos itens vale (ex: 1 moeda, 5 moedas)
    private Transform player; // referência do jogador
    private bool inRange = false; // tá no alcance?

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform; // acha jogador pela tag
        if (promptText != null)
            promptText.gameObject.SetActive(false); // esconde texto inicialmente
    }

    void Update()
    {
        if (player == null) return; // evita erro se não achar jogador

        float dist = Vector2.Distance(player.position, transform.position); // calcula distância
        bool nowInRange = dist <= interactRange; // verifica se tá no alcance

        // Ativa/desativa texto de prompt
        if (nowInRange && !inRange) // entrou no alcance
        {
            inRange = true;
            if (promptText != null)
            {
                promptText.text = "Pressione E para coletar";
                promptText.gameObject.SetActive(true);
            }
        }
        else if (!nowInRange && inRange) // saiu do alcance
        {
            inRange = false;
            if (promptText != null)
                promptText.gameObject.SetActive(false);
        }

        if (inRange && Input.GetKeyDown(KeyCode.E)) // coletou
            Collect();
    }

    public void SetAmount(int amt) // muda valor do item (ex: de 1 pra 5 moedas)
    {
        amount = amt;
    }

    private void Collect() // ação de coletar
    {
        GameManager.Instance.RegisterScriptableObject(amount); // atualiza contador

        if (promptText != null)
            promptText.gameObject.SetActive(false); // esconde texto
        Destroy(gameObject); // destroi o item cena
    }
}