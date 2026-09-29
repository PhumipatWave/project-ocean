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


SAMPLE_NAMES = '''
namespace Game.Testing
{
    public enum Kind { Quest, Other }
    public static class Names { public const string Quest = "q"; }
    public class Quest { }
    public class User { void F() { var s = Names.Quest; var k = Kind.Quest; } }
    public class Real { void F() { var q = new Quest(); } }
}
'''


class DependencyFalsePositiveTest(unittest.TestCase):
    def setUp(self):
        code = g.strip_code(SAMPLE_NAMES)
        types = g.parse_types(code, g.SRC / "Test" / "Names.cs")
        for t in types:
            g.parse_members(t)
        self.by_name, _ = g.resolve(types)

    def test_enum_values_are_not_dependencies(self):
        self.assertEqual(set(), self.by_name["Kind"].deps)

    def test_own_member_name_is_not_a_dependency(self):
        self.assertNotIn("Quest", self.by_name["Names"].deps)

    def test_member_access_is_not_a_dependency(self):
        self.assertEqual({"Names", "Kind"}, self.by_name["User"].deps)

    def test_real_type_use_still_counts(self):
        self.assertEqual({"Quest"}, self.by_name["Real"].deps)


class DrawioTest(unittest.TestCase):
    def setUp(self):
        import xml.etree.ElementTree as ET
        from collections import defaultdict
        import gen_drawio
        code = g.strip_code(SAMPLE)
        types = g.parse_types(code, g.SRC / "Gameplay" / "Entities" / "Players" / "Sample.cs")
        for t in types:
            g.parse_members(t)
        by_name, _ = g.resolve(types)
        modules = defaultdict(list)
        for t in types:
            modules[t.module].append(t)
        self.root = ET.fromstring(gen_drawio.build(types, by_name, modules, g.module_graph(types, by_name)))
        self.cells = self.root.findall(".//mxCell")

    def label_of(self, cid):
        return next(c.get("value") for c in self.cells if c.get("id") == cid)

    def test_every_type_is_a_box_with_private_members(self):
        values = " ".join(c.get("value") or "" for c in self.cells)
        for name in ("IHealth", "Entity", "Player", "Inventory", "Mode"):
            self.assertIn(f"<b>{name}", values)
        self.assertIn("- hp: float", values)          # private member shown ("Everything" detail)

    def test_inheritance_and_implements_edges(self):
        edges = [(self.label_of(c.get("source")), self.label_of(c.get("target")), c.get("style"))
                 for c in self.cells if c.get("edge") == "1"]
        self.assertTrue(any("<b>Player" in s and "<b>Entity" in t and "endArrow=block" in st and "dashed" not in st
                            for s, t, st in edges))
        self.assertTrue(any("<b>Entity" in s and "<b>IHealth" in t and "dashed=1" in st for s, t, st in edges))

    def test_page_names(self):
        names = [d.get("name") for d in self.root.findall("diagram")]
        self.assertEqual(["Overview", "Gameplay – Player & Entities"], names)


if __name__ == "__main__":
    import sys
    sys.path.insert(0, str(Path(__file__).parent))
    unittest.main()
