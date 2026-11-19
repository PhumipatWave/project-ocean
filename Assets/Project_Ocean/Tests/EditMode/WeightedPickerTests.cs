using Game.Gameplay;
using NUnit.Framework;

namespace Game.Tests
{
    public class WeightedPickerTests
    {
        private class Entry : IWeightedEntry
        {
            public float Weight { get; }
            public Entry(float weight) => Weight = weight;
        }

        [Test]
        public void Pick_FollowsCumulativeWeights()
        {
            var a = new Entry(1f);
            var b = new Entry(3f);
            var list = new[] { a, b };

            Assert.AreSame(a, WeightedPicker.Pick(list, 0f));
            Assert.AreSame(a, WeightedPicker.Pick(list, 0.25f));
            Assert.AreSame(b, WeightedPicker.Pick(list, 0.26f));
            Assert.AreSame(b, WeightedPicker.Pick(list, 1f));
        }

        [Test]
        public void Pick_SkipsNullAndZeroWeight()
        {
            var zero = new Entry(0f);
            var real = new Entry(2f);

            Assert.AreSame(real, WeightedPicker.Pick(new[] { null, zero, real }, 0.5f));
        }

        [Test]
        public void Pick_ReturnsNullWhenNothingPickable()
        {
            Assert.IsNull(WeightedPicker.Pick<Entry>(null, 0.5f));
            Assert.IsNull(WeightedPicker.Pick(new Entry[0], 0.5f));
            Assert.IsNull(WeightedPicker.Pick(new[] { new Entry(0f) }, 0.5f));
        }

        [Test]
        public void Pick_NeverReturnsNullEntryOnOvershoot()
        {
            var a = new Entry(1f);
            Assert.AreSame(a, WeightedPicker.Pick(new[] { a, null }, 1.0001f));
        }
    }
}
