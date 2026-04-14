using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChoralLake.Data;

namespace ChoralLake.UI
{
    /// <summary>
    /// One line of a receipt. Displays a fish's icon, name (with optional quantity suffix), and
    /// total price for this line. Animation-free; a future animator drives the numeric reveal.
    /// </summary>
    public class ReceiptEntryView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected Image iconImage;
        [SerializeField] protected TMP_Text nameText;
        [SerializeField] protected TMP_Text priceText;

        public FishEntry Fish { get; private set; }
        public int Quantity { get; private set; }
        public int LineTotal { get; private set; }

        public virtual void SetData(FishEntry fish, int quantity)
        {
            Fish = fish;
            Quantity = Mathf.Max(1, quantity);
            LineTotal = (fish != null ? fish.SellCost : 0) * Quantity;

            if (iconImage != null)
            {
                if (fish != null && fish.Icon != null)
                {
                    iconImage.sprite = fish.Icon;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            if (nameText != null)
            {
                nameText.text = Quantity > 1
                    ? $"{fish?.DisplayName ?? "???"} \u00d7 {Quantity}"
                    : fish?.DisplayName ?? "???";
            }

            if (priceText != null)
                priceText.text = $"${LineTotal}";
        }

        /// <summary>Resets to pre-animation state. No-op until a receipt animator calls it.</summary>
        public virtual void ClearForAnimation()
        {
            if (nameText != null) nameText.text = string.Empty;
            if (priceText != null) priceText.text = "$0";
        }
    }
}
