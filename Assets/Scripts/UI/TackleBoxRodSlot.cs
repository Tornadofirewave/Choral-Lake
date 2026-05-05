using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ChoralLake.Core;
using ChoralLake.Data;

namespace ChoralLake.UI
{
    public class TackleBoxRodSlot : MonoBehaviour
    {
        [SerializeField] private FishingRodSO rod;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button button;

        private static readonly Color EquippedColor = Color.white;
        private static readonly Color OwnedColor    = new Color(1f, 1f, 1f, 0.55f);
        private static readonly Color UnownedColor  = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        public void Refresh()
        {
            if (rod == null) return;
            iconImage.sprite = rod.InventoryIcon;

            var gm = GameManager.Instance;
            bool owns     = gm != null && gm.OwnsRod(rod.Id);
            bool equipped = gm != null && gm.SaveData.equippedRodId == rod.Id;

            iconImage.color    = equipped ? EquippedColor : owns ? OwnedColor : UnownedColor;
            button.interactable = owns;
        }

        private void OnClick()
        {
            if (rod == null) return;
            var gm = GameManager.Instance;
            if (gm == null) return;
            KeepButtonSelected();
            gm.EquipRod(rod.Id);
            gm.SaveGame();
        }

        private void KeepButtonSelected()
        {
            if (button == null || EventSystem.current == null) return;
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}
