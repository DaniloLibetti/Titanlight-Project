using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    public class EquipableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        public string ActionName => "Equip";

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            throw new System.NotImplementedException();
        }
    }
}