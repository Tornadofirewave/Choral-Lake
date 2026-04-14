using System.Collections.Generic;
using UnityEngine;
using ChoralLake.Core;
using ChoralLake.UI;

namespace ChoralLake.Gameplay
{
    /// <summary>
    /// Interactable shopkeeper. On Interact, sells all unsold fish and opens the receipt UI
    /// with a snapshot of what was sold. The receipt is a summary of a completed transaction.
    /// </summary>
    public class Shopkeeper : MonoBehaviour, IInteractable
    {
        [SerializeField] private string promptText = "Sell fish";

        public string InteractPrompt => promptText;
        public bool CanInteract => ReceiptUI.Instance == null || !ReceiptUI.Instance.IsOpen;
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
    }
}
