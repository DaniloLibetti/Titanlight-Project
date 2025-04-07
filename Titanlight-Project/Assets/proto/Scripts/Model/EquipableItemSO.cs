using System.Collections.Generic;
using UnityEngine;
using Inventory.UI;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public class EquipableItemSO : ItemSO, IDestroyableItem //ItemAction
    {
        public string ActionName => "Equip";

        /*public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            EquipmentSlot equipSlot = character.GetComponent<EquipmentSlot>();
            if (equipSlot != null)
            {
                
            }
            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();
            if (weaponSystem != null)
            {
                weaponSystem.SetWeapon(this, itemState == null ?
                    DefaultParameterList : itemState);
                return true;
            }
            return false;
        }*/
    }
}