using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>A short speech popup (trash, chest) gameplay can show without referencing UI types.</summary>
    public interface IInteractPopupView
    {
        bool IsOpen { get; }
        void Show(string speaker, string text, Sprite portrait, Sprite extraImage = null);
        void Hide();
    }
}
