using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public class ConsumableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        [SerializeField]
        private List<ModifierData> modifiersData = new List<ModifierData>();

        public string ActionName => "Use";

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            foreach (ModifierData data in modifiersData)
            {
                data.statModifier.AffectCharacter(character, data.value);
            }
            return true;
        }
    }

    public interface IDestroyableItem
    {

    }

    public interface IItemAction
    {
        public string ActionName { get; }
        bool PerformAction(GameObject character, List<ItemParameter> itemState);
    }

    [Serializable]
    public class ModifierData
    {
        public CharacterStatsModifierSO statModifier;
        public float value;
    }
}