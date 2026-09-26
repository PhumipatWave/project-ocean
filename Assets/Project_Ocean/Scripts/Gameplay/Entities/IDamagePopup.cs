namespace Game.Testing
{
    /// <summary>Floating damage/death number on an entity; implemented by the UI DamagePoint.</summary>
    public interface IDamagePopup
    {
        void ShowDamagePoint(string damageText);
        void ShowDeath();
    }
}
