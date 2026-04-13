using System.Collections;
using UnityEngine;
using ChoralLake.Core;
using ChoralLake.SceneManagement;

namespace ChoralLake.DebugTools
{
    /// <summary>
    /// TEMPORARY: verifies scene transition wiring. Delete after acceptance criteria pass.
    /// </summary>
    public class SmokeTest : MonoBehaviour
    {
        [SerializeField] private float loadDelaySeconds = 1f;
        [SerializeField] private string targetScene = SceneIds.Lake01;

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[SmokeTest] GameManager.Instance is null.");
                return;
            }

            GameManager.Instance.OnSceneLoadComplete += OnLoaded;
            Debug.Log($"[SmokeTest] Starting scene: {GameManager.Instance.SaveData.currentSceneName}");
            StartCoroutine(LoadAfterDelay());
        }

        private IEnumerator LoadAfterDelay()
        {
            yield return new WaitForSeconds(loadDelaySeconds);
            Debug.Log($"[SmokeTest] Requesting transition to '{targetScene}'...");
            SceneLoader.LoadScene(targetScene, "default");
        }

        private void OnLoaded()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            Debug.Log($"[SmokeTest] Scene load complete. Current scene: {gm.SaveData.currentSceneName}");
            Debug.Log($"[SmokeTest] Player position: {gm.SaveData.playerPosition}");
            gm.OnSceneLoadComplete -= OnLoaded;
            enabled = false;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnSceneLoadComplete -= OnLoaded;
        }
    }
}
