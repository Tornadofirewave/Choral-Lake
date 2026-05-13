using UnityEngine;

namespace ChoralLake.Core
{
    public static class RarityColors
    {
        static readonly Color Common    = new(0.35f, 0.38f, 0.40f);
        static readonly Color Rare      = new(0.15f, 0.30f, 0.55f);
        static readonly Color Epic      = new(0.38f, 0.18f, 0.50f);
        static readonly Color Legendary = new(0.55f, 0.38f, 0.10f);

        public static Color For(Rarity r) => r switch
        {
            Rarity.Rare      => Rare,
            Rarity.Epic      => Epic,
            Rarity.Legendary => Legendary,
            _                => Common,
        };
    }
}
