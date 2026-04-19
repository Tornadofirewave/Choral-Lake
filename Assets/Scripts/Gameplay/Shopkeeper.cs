using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ChoralLake.Core;
using ChoralLake.UI;

namespace ChoralLake.Gameplay
{
    public class Shopkeeper : MonoBehaviour, IInteractable, IPointerClickHandler
    {
        [SerializeField] private string promptText = "Sell fish";

        public string InteractPrompt => promptText;
        public bool CanInteract => !ModalStack.AnyOpen;
        public Transform Transform => transform;

        public void Interact()
        {
            var gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogError("[Shopkeeper] GameManager.Instance is null.");
                return;
            }
            if (ReceiptUI.Instance == null)
            {
                Debug.LogError("[Shopkeeper] ReceiptUI.Instance is null. Is ReceiptUI in the Boot scene?");
                return;
            }

            // Snapshot before selling — SellAllFish clears the list.
            var snapshot = new List<string>(gm.SaveData.unsoldFishIds);

            if (snapshot.Count == 0)
            {
                ReceiptUI.Instance.ShowEmpty();
                return;
            }

            int earned = gm.SellAllFish();
            var fishDb = gm.Database != null ? gm.Database.FishDatabase : null;
            ReceiptUI.Instance.ShowReceipt(snapshot, earned, fishDb);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ModalStack.AnyOpen || ModalStack.JustClosed) return;
            Interact();
        }
    }
}
