using System;
using UnityEngine;

namespace Game.Core
{
    public class MoveKeyBindUI : RebindButtonBase
    {
        [Header("Move Direction")]
        [SerializeField] private MoveDirection direction;

        protected override void BeginRebind(KeyBindManager manager, Action<string, bool> onComplete) =>
            manager.RebindMove(direction, onComplete);

        protected override string GetDisplayString(KeyBindManager manager) =>
            manager.GetMoveBindingDisplayString(direction);
    }
}
