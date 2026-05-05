using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using ChoralLake.Core;

namespace ChoralLake.UI
{
    public class TackleBoxBaitSlot : MonoBehaviour
    {
        [SerializeField] private string baitId;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private Button button;

        private static readonly Color EquippedColor = Color.white;
        private static readonly Color OwnedColor    = new Color(1f, 1f, 1f, 0.55f);
        private static readonly Color UnownedColor  = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        private void Awake()
        {
            if (countText != null)
                countText.raycastTarget = false;

            button.onClick.AddListener(OnClick);
        }

        public void Refresh()
        {
            if (string.IsNullOrEmpty(baitId)) return;
            var gm = GameManager.Instance;
            if (gm == null) return;

            var entry = gm.Database?.GetBaitById(baitId);
            if (entry != null)
                iconImage.sprite = entry.Icon;

            int count    = gm.GetBaitCount(baitId);
            bool owns    = count > 0;
            bool equipped = gm.SaveData.equippedBaitId == baitId;

            iconImage.color     = equipped ? EquippedColor : owns ? OwnedColor : UnownedColor;
            button.interactable = owns;

            if (countText != null)
            {
                countText.gameObject.SetActive(owns);
                countText.text = $"x{count}";
            }
        }

        private void OnClick()
        {
            if (string.IsNullOrEmpty(baitId)) return;
            var gm = GameManager.Instance;
            if (gm == null) return;

            KeepButtonSelected();
            string toEquip = gm.SaveData.equippedBaitId == baitId ? string.Empty : baitId;
            Debug.Log($"[TackleBoxBaitSlot] {gameObject.name} | baitId={baitId} | equipping={toEquip}");
            gm.EquipBait(toEquip);
            gm.SaveGame();
        }

        private void KeepButtonSelected()
        {
            if (button == null || EventSystem.current == null) return;
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}
