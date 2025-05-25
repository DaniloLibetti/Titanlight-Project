using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class EquipmentMenuController : MonoBehaviour
{
    [Header("Player1")]
    [SerializeField] GameObject range1OptionsP1;
    [SerializeField] GameObject range2OptionsP1;
    [SerializeField] GameObject chip1OptionsP1;
    [SerializeField] GameObject chip2OptionsP1;
    [SerializeField] GameObject granadeOptionsP1;
    [SerializeField] GameObject healOptionsP1;
    [SerializeField] Button button1, button2, button3;

    [Header("Player2")]
    [SerializeField] GameObject range1OptionsP2;
    [SerializeField] GameObject range2OptionsP2;
    [SerializeField] GameObject chip1OptionsP2;
    [SerializeField] GameObject chip2OptionsP2;
    [SerializeField] GameObject granadeOptionsP2;
    [SerializeField] GameObject healOptionsP2;

    public int buttonValue;

    public void ShowRange1OptionsP1()
    {
        HideOptionsP1();
        range1OptionsP1.SetActive(true);
    }

    public void ShowRange2OptionsP1()
    {
        HideOptionsP1();
        range2OptionsP1.SetActive(true);
    }

    public void Showchip1OptionsP1()
    {
        HideOptionsP1();
        chip1OptionsP1.SetActive(true);
    }

    public void Showchip2OptionsP1()
    {
        HideOptionsP1();
        chip2OptionsP1.SetActive(true);
    }

    public void ShowGranadeOptionsP1()
    {
        HideOptionsP1();
        granadeOptionsP1.SetActive(true);
    }
    public void ShowHealOptionsP1()
    {
        HideOptionsP1();
        healOptionsP1.SetActive(true);
    }

    public void HideOptionsP1()
    {
        range1OptionsP1.SetActive(false);
        range2OptionsP1.SetActive(false);
        chip1OptionsP1.SetActive(false);
        chip2OptionsP1.SetActive(false);
        granadeOptionsP1.SetActive(false);
        healOptionsP1.SetActive(false);
    }

    public void ShowRange1OptionsP2()
    {
        HideOptionsP2();
        range1OptionsP2.SetActive(true);
    }

    public void ShowRange2OptionsP2()
    {
        HideOptionsP2();
        range2OptionsP2.SetActive(true);
    }

    public void Showchip1OptionsP2()
    {
        HideOptionsP2();
        chip1OptionsP2.SetActive(true);
    }

    public void Showchip2OptionsP2()
    {
        HideOptionsP2();
        chip2OptionsP2.SetActive(true);
    }

    public void ShowGranadeOptionsP2()
    {
        HideOptionsP2();
        granadeOptionsP2.SetActive(true);
    }
    public void ShowHealOptionsP2()
    {
        HideOptionsP2();
        healOptionsP2.SetActive(true);
    }

    public void HideOptionsP2()
    {
        range1OptionsP2.SetActive(false);
        range2OptionsP2.SetActive(false);
        chip1OptionsP2.SetActive(false);
        chip2OptionsP2.SetActive(false);
        granadeOptionsP2.SetActive(false);
        healOptionsP2.SetActive(false);
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
