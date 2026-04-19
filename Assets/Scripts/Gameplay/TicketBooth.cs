using UnityEngine;
using UnityEngine.EventSystems;
using ChoralLake.UI;

namespace ChoralLake.Gameplay
{
    public class TicketBooth : MonoBehaviour, IInteractable, IPointerClickHandler
    {
        [SerializeField] private string promptText = "Buy ticket";

        public string InteractPrompt => promptText;
        public bool CanInteract => !ModalStack.AnyOpen;
        public Transform Transform => transform;

        public void Interact()
        {
            if (TicketBoothUI.Instance == null)
            {
                Debug.LogError("[TicketBooth] TicketBoothUI.Instance is null. Is it in the Boot scene?");
                return;
            }
            TicketBoothUI.Instance.Open();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ModalStack.AnyOpen || ModalStack.JustClosed) return;
            Interact();
        }
    }
}
