using UnityEngine;
using UnityEngine.EventSystems;
using ChoralLake.Core;
using ChoralLake.UI;

namespace ChoralLake.Gameplay
{
    public class FishingTutorialInteractable : MonoBehaviour, IInteractable, IPointerClickHandler
    {
        [SerializeField] private string interactPrompt = "Open tutorial";

        public string InteractPrompt => interactPrompt;
        public bool CanInteract => true;
        public Transform Transform => transform;

        public void Interact() => Open();

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ModalStack.AnyOpen || ModalStack.JustClosed) return;
            Open();
        }

        private void Open()
        {
            FishingTutorialUI.Instance?.Show();
        }
    }
}