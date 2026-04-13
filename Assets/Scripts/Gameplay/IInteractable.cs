using UnityEngine;

namespace ChoralLake.Gameplay
{
    /// <summary>
    /// Anything the player can walk up to and press Interact on.
    /// Dialogue triggers, pickups, shops, fishing spots — all implement this.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Prompt shown when in range. e.g., "Talk", "Pick up", "Open".</summary>
        string InteractPrompt { get; }

        /// <summary>False to suppress the prompt and ignore input (e.g., a pickup already taken).</summary>
        bool CanInteract { get; }

        /// <summary>Called when the player presses Interact while this is focused.</summary>
        void Interact();

        /// <summary>Used to position the "Press E" prompt in world space.</summary>
        Transform Transform { get; }
    }
}
