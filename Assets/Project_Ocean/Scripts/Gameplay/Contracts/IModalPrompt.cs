using Game.Core;

namespace Game.Gameplay
{
    /// <summary>A yes/no prompt gameplay can raise; CanShow is false while a menu owns the screen.</summary>
    public interface IModalPrompt
    {
        bool CanShow { get; }
        void Show(string message, System.Action onConfirm, System.Action onCancel = null);
        void Cancel();
    }

    /// <summary>Access point to the live modal prompt (registered by ConfirmationDialog).</summary>
    public static class ModalPrompt
    {
        public static IModalPrompt Current { get; private set; }

        public static void Register(IModalPrompt prompt) => Current = prompt;

        // Only the registered instance may clear it, so a destroyed duplicate can't wipe the live one.
        public static void Unregister(IModalPrompt prompt)
        {
            if (Current == prompt) Current = null;
        }
    }
}
