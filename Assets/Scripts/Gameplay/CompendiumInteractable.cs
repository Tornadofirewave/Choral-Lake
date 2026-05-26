using UnityEngine;
using UnityEngine.EventSystems;
using ChoralLake.Audio;
using ChoralLake.UI;

namespace ChoralLake.Gameplay
{
    public class CompendiumInteractable : MonoBehaviour, IInteractable, IPointerClickHandler
    {
        [SerializeField] private string interactPrompt = "Open compendium";
        [SerializeField] private string openSfxId = "sfx_compendium_open";

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
            if (CompendiumUI.Instance == null) return;
            AudioManager.Instance?.PlaySfx(openSfxId);
            CompendiumUI.Instance.Show();
        }
    }
}
