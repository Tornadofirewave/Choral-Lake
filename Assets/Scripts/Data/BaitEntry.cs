using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.Data
{
    [System.Serializable]
    public class BaitEntry : IIdentifiable
    {
        [Header("Base")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Rarity guaranteedRarity;
        [SerializeField] private Sprite icon;

        [Header("Fishing Effects")]
        [Tooltip("Decreases the total number of circles that can spawn during a fishing encounter.")]
        [SerializeField] private int circleSpawnDecrease;
        [Tooltip("Decreases the base threshold for successful catches.")]
        [SerializeField] private int thresholdDecrease;

        public string Id => id;
        public string DisplayName => displayName;
        public Rarity GuaranteedRarity => guaranteedRarity;
        public Sprite Icon => icon;
        public int CircleSpawnDecrease => circleSpawnDecrease;
        public int ThresholdDecrease => thresholdDecrease;
    }
}
