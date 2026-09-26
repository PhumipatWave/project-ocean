"""Self-test for gen_uml's parser. Run: python tools/uml/test_gen_uml.py"""
import unittest
from pathlib import Path

import gen_uml as g

SAMPLE = '''
using System;
namespace Game.Testing
{
    // class Fake : MonoBehaviour {}   <- comment must be ignored
    public interface IHealth { float Current { get; } void Heal(float amount); }

    public abstract class Entity : MonoBehaviour, IHealth
    {
        public float Current => hp;
        [SerializeField] private float hp = 10f;
        public static event Action<int> OnDied;
        protected abstract void Die();
        public void Heal(float amount) { string s = "class Nope {"; hp += amount; }
    }

    public class Player : Entity
    {
        private Inventory inventory;
        public enum Mode { Idle, Swim = 2 }
        protected override void Die() { Entity.OnDied += HandleDied; }
        void HandleDied(int x) {}
    }

    public class Inventory : ScriptableObject { public int Count { get; set; } }
}
'''


class ParserTest(unittest.TestCase):
    def setUp(self):
        code = g.strip_code(SAMPLE)
        self.types = g.parse_types(code, g.SRC / "Test" / "Sample.cs")
        for t in self.types:
            g.parse_members(t)
        self.by_name, _ = g.resolve(self.types)

    def test_types_found_and_comment_ignored(self):
        self.assertEqual({"IHealth", "Entity", "Player", "Mode", "Inventory"}, set(self.by_name))

    def test_inheritance_and_stereotypes(self):
        e, p = self.by_name["Entity"], self.by_name["Player"]
        self.assertEqual(["IHealth"], e.interfaces)
        self.assertIn("MB", e.stereo)
        self.assertIn("abstract", e.stereo)
        self.assertEqual("Entity", p.base_class)
        self.assertIn("MB", p.stereo)       # inherited through Entity
        self.assertIn("SO", self.by_name["Inventory"].stereo)

    def test_members(self):
        e = {m.name: m for m in self.by_name["Entity"].members}
        self.assertEqual("event", e["OnDied"].kind)
        self.assertTrue(e["OnDied"].static)
        self.assertEqual("property", e["Current"].kind)
        self.assertEqual("private", e["hp"].vis)
        self.assertTrue(e["Die"].abstract)
        self.assertEqual("float", e["Heal"].params)
        self.assertEqual(["Idle", "Swim"], self.by_name["Mode"].enum_values)
        self.assertNotIn("Mode", {m.name for m in self.by_name["Player"].members})

    def test_association_and_event_subscription(self):
        p = self.by_name["Player"]
        self.assertEqual({"Inventory": "inventory"}, p.assoc)
        self.assertEqual([("Entity", "OnDied")], p.subs)
        self.assertEqual("Player", self.by_name["Mode"].outer)

    def test_mermaid_escaping(self):
        self.assertEqual("Dictionary~int, ListArray~", g.mm("Dictionary<int, List[]>"))


if __name__ == "__main__":
    import sys
    sys.path.insert(0, str(Path(__file__).parent))
    unittest.main()
