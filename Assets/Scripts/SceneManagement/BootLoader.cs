using UnityEngine;

namespace ChoralLake.SceneManagement
{
    /// <summary>
    /// Placed in the Boot scene. Loads the starting gameplay scene on launch.
    /// Boot itself is never unloaded — it owns the persistent singletons (GameManager, PlayerRoot).
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private string startingScene = SceneIds.Town;
        [SerializeField] private string startingSpawnId = "default";

        private void Start()
        {
            SceneLoader.LoadScene(startingScene, startingSpawnId);
        }
    }
}
