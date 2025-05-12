using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class EquipmentMenuController : MonoBehaviour
{
    [SerializeField] GameObject range1Options, range2Options, chip1Options, chip2Options, granadeOptions, healOptions;
    [SerializeField] Button button1, button2, button3;
    public int buttonValue;

    public void ShowRange1Options()
    {
        HideOptions();
        range1Options.SetActive(true);
    }

    public void ShowRange2Options()
    {
        HideOptions();
        range2Options.SetActive(true);
    }

    public void Showchip1Options()
    {
        HideOptions();
        chip1Options.SetActive(true);
    }

    public void Showchip2Options()
    {
        HideOptions();
        chip2Options.SetActive(true);
    }

    public void ShowGranadeOptions()
    {
        HideOptions();
        granadeOptions.SetActive(true);
    }
    public void ShowHealOptions()
    {
        HideOptions();
        healOptions.SetActive(true);
    }

    public void HideOptions()
    {
        range1Options.SetActive(false);
        range2Options.SetActive(false);
        chip1Options.SetActive(false);
        chip2Options.SetActive(false);
        granadeOptions.SetActive(false);
        healOptions.SetActive(false);
    }

    public void ChangeRangeAttack1()
    {
        if (button1.onClick != null)
        {
            buttonValue = 1;
        }
    }

    public void ChangeRangeAttack2()
    {
        if (button2.onClick != null)
        {
            buttonValue = 2;
        }
    }

    public void ChangeRangeAttack3()
    {
        if (button3.onClick != null)
        {
            buttonValue = 3;
        }
    }
}
