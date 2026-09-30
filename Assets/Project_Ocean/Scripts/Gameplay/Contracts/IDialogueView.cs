using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>A dialogue panel gameplay can open, label and fill without referencing UI types.</summary>
    public interface IDialogueView
    {
        GameObject Panel { get; }
        bool IsOpen { get; }
        void SetOpen(bool open);
        void SetSpeaker(string characterId, string fallbackName = null, Sprite fallbackPortrait = null);
        void SetText(string text);
    }
}
