using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;


public class SegmentedHealthBar : MonoBehaviour
{
    private Health healthPlayer;
    [SerializeField] private RectTransform barRect;
    [SerializeField] private RectMask2D mask;

    private float maxRightMask;
    private float initialRightMask;


    private void Start()
    {
        maxRightMask = barRect.rect.width - mask.padding.x - mask.padding.z;
        initialRightMask = mask.padding.z;
        healthPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
        
    }

    public void SetValue(float newValue)
    {
        var targetWidth = healthPlayer.CurrentHealth * maxRightMask / healthPlayer.MaxHealth;
        var newRightMask = maxRightMask + initialRightMask - targetWidth;
        var padding = mask.padding;
        padding.z = newRightMask;
        mask.padding = padding;
        
    }
}
