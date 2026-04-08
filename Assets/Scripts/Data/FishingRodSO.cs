using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.Data
{
    [CreateAssetMenu(fileName = "Rod_", menuName = "Choral Lake/Fishing Rod")]
    public class FishingRodSO : ScriptableObject, IIdentifiable
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;

        [Header("Tier")]
        [Tooltip("The highest rarity of bait this rod can equip. A Rare rod accepts Common and Rare bait.")]
        [SerializeField] private Rarity maxBaitRarity;

        [Header("Catch Weights (relative, normalized at runtime)")]
        [SerializeField, Min(0f)] private float commonWeight;
        [SerializeField, Min(0f)] private float rareWeight;
        [SerializeField, Min(0f)] private float epicWeight;
        [SerializeField, Min(0f)] private float legendaryWeight;

        [Header("Presentation")]
        [SerializeField] private Sprite inventoryIcon;
        [Tooltip("Small sprite shown above the player's head while this rod is equipped (Zelda pickup style).")]
        [SerializeField] private Sprite heldSprite;

        public string Id => id;
        public string DisplayName => displayName;
        public Rarity MaxBaitRarity => maxBaitRarity;
        public Sprite InventoryIcon => inventoryIcon;
        public Sprite HeldSprite => heldSprite;

        public float GetWeight(Rarity rarity) => rarity switch
        {
            Rarity.Common    => commonWeight,
            Rarity.Rare      => rareWeight,
            Rarity.Epic      => epicWeight,
            Rarity.Legendary => legendaryWeight,
            _ => 0f
        };

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                Debug.LogWarning($"[FishingRodSO] '{name}' has no ID set.", this);

            if (commonWeight + rareWeight + epicWeight + legendaryWeight <= 0f)
                Debug.LogWarning($"[FishingRodSO] '{name}' has all-zero weights. It will never catch anything without bait.", this);
        }
    }
}
