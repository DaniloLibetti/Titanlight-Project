using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private List<Itens> itemList;
    public Inventory()
    {
        itemList = new List<Itens>();

        AddItem(new Itens { itemType = Itens.ItemType.Scrap, amount = 1 });
        AddItem(new Itens { itemType = Itens.ItemType.Coin, amount = 1 });
        AddItem(new Itens { itemType = Itens.ItemType.Potion, amount = 1 });
    }

    public void AddItem(Itens item)
    {
        itemList.Add(item);
    }

    public List<Itens> GetItemList()
    {
        return itemList;
    }

}

