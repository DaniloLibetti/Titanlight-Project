using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class SegmentedHealthBar : MonoBehaviour
{
    [Header("Configuração de Segmentos")]
    [Tooltip("Prefab de slot 1-13 (SpriteRenderer) no Canvas")]
    public GameObject prefabNormal;
    [Tooltip("Prefab de slot 14 (SpriteRenderer)")]
    public GameObject prefabPenultimate;
    [Tooltip("Prefab de slot 15 (SpriteRenderer)")]
    public GameObject prefabLast;
    [Tooltip("Número total de slots (ex: 15)")]
    public int totalSlots = 15;
    [Tooltip("Pontos de vida por slot (ex: 10)")]
    public int healthPerSlot = 10;

    private List<GameObject> _slots = new List<GameObject>();
    private Health _targetHealth;

    void Awake()
    {
        // Encontra o Player por Tag e obtém o componente Health
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _targetHealth = player.GetComponent<Health>();
        }
    }

    void Start()
    {
        if (_targetHealth == null)
        {
            Debug.LogError("SegmentedHealthBar: Não encontrou Health no Player.", this);
            return;
        }

        // Instancia os slots como filhos no Canvas
        for (int i = 0; i < totalSlots; i++)
        {
            GameObject prefabToUse = (i == totalSlots - 1)
                ? prefabLast
                : (i == totalSlots - 2)
                    ? prefabPenultimate
                    : prefabNormal;

            if (prefabToUse == null) continue;

            var slot = Instantiate(prefabToUse, transform);
            _slots.Add(slot);
        }

        // Inscreve nos eventos de dano e morte
        _targetHealth.onDamageTaken.AddListener(_ => Refresh());
        _targetHealth.onDeath.AddListener(() => Refresh());

        // Inicializa a barra cheia
        Refresh();
    }

    /// <summary>
    /// Atualiza visibilidade dos slots conforme vida atual
    /// </summary>
    private void Refresh()
    {
        int currHp = Mathf.RoundToInt(_targetHealth.CurrentHealth);
        int filled = Mathf.CeilToInt(currHp / (float)healthPerSlot);
        filled = Mathf.Clamp(filled, 0, _slots.Count);

        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].SetActive(i < filled);
        }
    }

    void OnValidate()
    {
        // Em Editor, refresca para visualizar no Canvas
        if (!Application.isPlaying && transform.childCount > 0)
        {
            // limpa antigos slots
            foreach (Transform child in transform) DestroyImmediate(child.gameObject);
            _slots.Clear();

            // instancia preview
            for (int i = 0; i < totalSlots; i++)
            {
                GameObject prefabToUse = (i == totalSlots - 1)
                    ? prefabLast
                    : (i == totalSlots - 2)
                        ? prefabPenultimate
                        : prefabNormal;

                if (prefabToUse == null) continue;
                Instantiate(prefabToUse, transform);
            }
        }
    }
}
