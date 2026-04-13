using UnityEngine;
using ChoralLake.Gameplay;

namespace ChoralLake.Dialogue
{
    /// <summary>
    /// Scene object that plays a dialogue conversation when interacted with.
    /// Use on NPCs, signs, and interactive world objects.
    /// </summary>
    public class DialogueTrigger : MonoBehaviour, IInteractable
    {
        [Header("Dialogue")]
        [SerializeField] private string conversationId;
        [SerializeField] private string promptText = "Talk";

        [Header("State")]
        [Tooltip("If true, becomes uninteractable after the first interaction.")]
        [SerializeField] private bool oneShot = false;

        private bool _fired;

        public string InteractPrompt => promptText;
        public bool CanInteract => !(oneShot && _fired);
        public Transform Transform => transform;

        public void Interact()
        {
            if (!CanInteract) return;
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[DialogueTrigger] DialogueManager.Instance is null.");
                return;
            }
            DialogueManager.Instance.StartConversation(conversationId);
            _fired = true;
        }
    }
}
