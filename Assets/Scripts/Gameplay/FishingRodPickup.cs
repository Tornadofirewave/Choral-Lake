using UnityEngine;
using UnityEngine.EventSystems;
using ChoralLake.Audio;
using ChoralLake.Core;
using ChoralLake.Data;
using ChoralLake.UI;

namespace ChoralLake.Gameplay
{
    public class FishingRodPickup : MonoBehaviour, IInteractable, IPointerClickHandler
    {
        [SerializeField] private FishingRodSO rod;
        [SerializeField] private string sfxId = "sfx_pickup_rod";
        [SerializeField] private string interactPrompt = "Pick up rod";

        private bool collected;

        private void Start()
        {
            if (rod != null && GameManager.Instance != null && GameManager.Instance.OwnsRod(rod.Id))
                Destroy(gameObject);
        }

        public string InteractPrompt => interactPrompt;
        public bool CanInteract => rod != null && !collected;
        public Transform Transform => transform;

        public void Interact() => Collect();

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ModalStack.AnyOpen || ModalStack.JustClosed) return;
            Collect();
        }

        private void Collect()
        {
            if (rod == null || collected) return;
            collected = true;

            var gm = GameManager.Instance;
            gm.GrantRod(rod.Id);
            gm.EquipRod(rod.Id);
            AudioManager.Instance?.PlaySfx(sfxId);
            RewardPopup.Instance?.Show(rod.InventoryIcon, rod.DisplayName);
            gm.SaveGame();

            Destroy(gameObject);
        }
    }
}
