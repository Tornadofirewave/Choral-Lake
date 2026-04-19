using UnityEngine;
using UnityEngine.Rendering;

namespace ChoralLake.Rendering
{
    /// <summary>
    /// Updates a SortingGroup's order each frame based on world Y position.
    /// Lower Y = higher order = renders in front. Attach to any object that
    /// needs depth sorting: player, NPCs, interactables, etc.
    /// </summary>
    [RequireComponent(typeof(SortingGroup))]
    [DisallowMultipleComponent]
    public class YSort : MonoBehaviour
    {
        // Multiplied against Y to produce an integer sort order.
        // 100 gives one step of precision per 0.01 world units.
        [SerializeField] private float precision = 100f;

        private SortingGroup _group;

        private void Awake() => _group = GetComponent<SortingGroup>();

        private void LateUpdate()
        {
            _group.sortingOrder = -(int)(transform.position.y * precision);
        }
    }
}
