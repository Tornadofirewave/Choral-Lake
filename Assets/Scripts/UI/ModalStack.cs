using UnityEngine;

namespace ChoralLake.UI
{
    /// <summary>
    /// Tracks how many modal screens are open. Interactables check AnyOpen before allowing interaction.
    /// Each modal must call Push() on open and Pop() on close.
    /// JustClosed holds true for 0.1 s after any close, preventing the dismissal click from
    /// immediately triggering a world interactable underneath regardless of event order.
    /// </summary>
    public static class ModalStack
    {
        private static int _openCount;
        private static float _lastPopTime = -1f;

        private const float CloseGracePeriod = 0.1f;

        public static bool AnyOpen => _openCount > 0;

        public static bool JustClosed => Time.unscaledTime - _lastPopTime < CloseGracePeriod;

        public static void Push() => _openCount++;

        public static void Pop()
        {
            _openCount--;
            if (_openCount < 0) _openCount = 0;
            _lastPopTime = Time.unscaledTime;
        }
    }
}
