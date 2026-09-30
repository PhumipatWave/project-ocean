using Game.Core;
using Game.Gameplay;
using Game.UI;
using NUnit.Framework;

namespace Game.Tests
{
    public class ShopSellValueTests
    {
        private readonly ShopTransactionService shop = new ShopTransactionService();

        [TearDown] public void TearDown() => TestItems.DestroyAll();

        [Test]
        public void SellValue_CountsOnlySellableNormalItems()
        {
            Item fish   = TestItems.Make(1, ItemType.Normal, maxStack: 5, price: 100);
            Item locked = TestItems.Make(2, ItemType.Normal, price: 100, canSell: false);
            Item hub    = TestItems.Make(3, ItemType.Building, price: 100);

            // (int)(100 * 0.9f) truncates to 89, not 90: pins current pricing.
            Assert.AreEqual(267, shop.SellValue(new ItemStack(fish, 3)));
            Assert.AreEqual(0, shop.SellValue(new ItemStack(locked, 1)));
            Assert.AreEqual(0, shop.SellValue(new ItemStack(hub, 1)));
            Assert.AreEqual(0, shop.SellValue(ItemStack.Empty));
        }
    }
}
