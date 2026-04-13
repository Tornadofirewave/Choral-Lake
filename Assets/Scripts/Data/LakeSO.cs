using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.Data
{
    /// <summary>
    /// Defines a fishable lake location. Referenced by LakeTransition to gate scene entry
    /// and by GameDatabase for ID-based lookups.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLake", menuName = "Choral Lake/Lake")]
    public class LakeSO : ScriptableObject, IIdentifiable
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string sceneName;
        [SerializeField, Min(0)] private int uniqueFishRequiredToUnlock;

        public string Id => id;
        public string DisplayName => displayName;
        public string SceneName => sceneName;
        public int UniqueFishRequiredToUnlock => uniqueFishRequiredToUnlock;
    }
}
