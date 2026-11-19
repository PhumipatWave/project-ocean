using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Floating damage/death number on an entity; implemented by the UI DamagePopup.</summary>
    public interface IDamagePopup
    {
        void ShowDamagePoint(string damageText);
        void ShowDeath();
    }
}
