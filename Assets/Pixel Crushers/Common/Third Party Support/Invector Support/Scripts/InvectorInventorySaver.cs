#if USE_INVECTOR_INVENTORY
using Invector.vItemManager;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.InvectorSupport
{

    /// <summary>
    /// Save System saver for Invector character's inventory.
    /// </summary>
    [AddComponentMenu("Pixel Crushers/Save System/Savers/Invector/Invector Inventory Saver")]
    public class InvectorInventorySaver : Saver
    {

        private vItemManager m_itemManager;
        private vItemManager itemManager
        {
            get
            {
                if (m_itemManager == null) m_itemManager = GetComponent<vItemManager>();
                return m_itemManager;
            }
        }

        // Borrowed from vSaveLoadInventory.cs:
        [System.Serializable]
        class InventoryData
        {
            /// <summary>
            /// List of <see cref="ItemReference"/>
            /// </summary>
            public List<ItemReference> itemReferences = new List<ItemReference>();

            /// <summary>
            /// List of <seealso cref="EquipAreaData"/>
            /// </summary>
            public List<EquipAreaData> equipAreas = new List<EquipAreaData>();

            /// <summary>
            /// Get <seealso cref="vItem"/> from <seealso cref="ItemReference"/>
            /// </summary>
            /// <param name="itemListData"></param>
            /// <returns></returns>
            public List<vItem> GetItems(vItemListData itemListData)
            {
                List<vItem> items = new List<vItem>();
                for (int i = 0; i < itemReferences.Count; i++)
                {
                    vItem item = itemListData.items.Find(a => a.id.Equals(itemReferences[i].id));
                    item = GameObject.Instantiate(item);
                    item.amount = itemReferences[i].amount;
                    item.attributes = itemReferences[i].attributes;
                    item.name = item.name.Replace("(Clone)", string.Empty);
                    items.Add(item);
                }
                return items;
            }
        }

        [System.Serializable]
        class EquipAreaData
        {
            /// <summary>
            /// List of <seealso cref="SlotData"/>
            /// </summary>
            public List<SlotData> slotsData = new List<SlotData>();

            public int indexOfSelectedSlot = 0;
        }

        [System.Serializable]
        class SlotData
        {
            public bool hasItem = false;
            public int indexOfItem = 0;
        }

        [System.Serializable]
        class ItemReference
        {
            [SerializeField] public int amount;
            [SerializeField] public int id;
            [SerializeField] public List<vItemAttribute> attributes;

            public ItemReference(vItem item)
            {
                amount = item.amount;
                id = item.id;
                attributes = item.attributes;
            }
        }

        public override string RecordData()
        {
            if (itemManager == null) return string.Empty;
            return vSaveLoadInventory.InventoryToJsonText(itemManager);
        }

        public override void ApplyData(string s)
        {
            if (string.IsNullOrEmpty(s) || itemManager == null) return;
            InventoryData data = new InventoryData();
            JsonUtility.FromJsonOverwrite(s, data);
            itemManager.items = data.GetItems(itemManager.itemListData);
            vEquipArea[] equipAreas = itemManager.inventory.equipAreas;
            for (int i = 0; i < equipAreas.Length; i++)
            {
                if (i < data.equipAreas.Count)
                {
                    vEquipArea area = equipAreas[i];
                    EquipAreaData areaData = data.equipAreas[i];

                    area.indexOfEquippedItem = areaData.indexOfSelectedSlot;

                    for (int e = 0; e < equipAreas[i].equipSlots.Count; e++)
                    {
                        if (e < areaData.slotsData.Count)
                        {
                            SlotData slotData = areaData.slotsData[e];
                            vEquipSlot slot = equipAreas[i].equipSlots[e];
                            itemManager.temporarilyIgnoreItemAnimation = true;
                            if (slotData.hasItem)
                            {
                                area.AddItemToEquipSlot(e, itemManager.items[slotData.indexOfItem]);
                            }
                            else area.RemoveItemOfEquipSlot(e);
                        }
                    }
                }
            }
            itemManager.inventory.UpdateInventory();
            itemManager.temporarilyIgnoreItemAnimation = false;
            itemManager.onLoadItems.Invoke();
        }

        public override void OnBeforeSceneChange()
        {
            base.OnBeforeSceneChange();
            // We need to start with a clear inventory in the next scene, so destroy this one:
            var vInventory = FindObjectOfType<vInventory>();
            if (vInventory != null) Destroy(vInventory.gameObject);
        }

    }
}
#endif
