using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>Builds throwaway Item assets for EditMode tests; call DestroyAll in TearDown.</summary>
    public static class TestItems
    {
        private static readonly List<Item> created = new List<Item>();

        public static Item Make(int id, ItemType type = ItemType.Normal, int maxStack = 1, int price = 0, bool canSell = true)
        {
            var item = ScriptableObject.CreateInstance<Item>();
            var so = new UnityEditor.SerializedObject(item);
            so.FindProperty("itemID").intValue = id;
            so.FindProperty("itemName").stringValue = "TestItem" + id;
            so.FindProperty("itemType").enumValueIndex = (int)type;
            so.FindProperty("maxStackable").intValue = maxStack;
            so.FindProperty("itemPrice").intValue = price;
            so.FindProperty("canSell").boolValue = canSell;
            so.ApplyModifiedPropertiesWithoutUndo();
            created.Add(item);
            return item;
        }

        public static void DestroyAll()
        {
            foreach (var item in created)
                if (item != null) Object.DestroyImmediate(item);
            created.Clear();
        }
    }
}
