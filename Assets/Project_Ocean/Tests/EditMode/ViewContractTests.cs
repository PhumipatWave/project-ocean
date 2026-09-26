using Game.Testing;
using NUnit.Framework;

namespace Game.Tests
{
    public class ViewContractTests
    {
        [Test]
        public void UiComponents_ImplementGameplayContracts()
        {
            Assert.IsTrue(typeof(IQuestOfferView).IsAssignableFrom(typeof(QuestDetailPanel)));
            Assert.IsTrue(typeof(IModalPrompt).IsAssignableFrom(typeof(ConfirmationDialog)));
        }

        [Test]
        public void ModalPrompt_OnlyRegisteredInstanceCanUnregister()
        {
            ModalPrompt.Register(null);
            Assert.IsNull(ModalPrompt.Current);
        }
    }
}
