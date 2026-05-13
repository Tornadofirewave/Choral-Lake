using UnityEngine;
using UnityEngine.UI;
using ChoralLake.Core;
using ChoralLake.Data;

namespace ChoralLake.UI
{
    public class CompendiumSlot : MonoBehaviour
    {
        [SerializeField] private Image icon;

        public void SetData(FishEntry fish, bool caught)
        {
            if (icon == null || fish == null) return;
            icon.sprite = fish.Icon;
            icon.color  = caught ? Color.white : RarityColors.For(fish.Rarity);
        }
    }
}
