using UnityEngine;

namespace ChoralLake.SceneManagement
{
    /// <summary>
    /// Place one of these in every gameplay scene (Town, Lake_01, Lake_02, Lake_03).
    /// It notifies the SceneLoader that the scene is ready so the player can be positioned
    /// at the requested spawn point.
    /// </summary>
    public class SceneBootstrap : MonoBehaviour
    {
        private void Start()
        {
            SceneLoader.NotifySceneReady();
        }
    }
}