using UnityEngine;
using SceneItem;

namespace SceneItem
{
    public class Gear : SceneItemBase
    {
        [Tooltip("Number of grenades this gear provides when picked up.")]
        public int GrenadeCount  = 5;
        [Tooltip("If true, picking up this gear enables the variant grenade throw.")]
        public bool providesVariantThrow = true; 
        
        [Tooltip("Damage reduction percentage provided by this gear (e.g., 0.2 for 20% reduction).")]
        [Range(0f, 1f)]
        public float DamageReductionPercentage = 0.2f;
    }
}