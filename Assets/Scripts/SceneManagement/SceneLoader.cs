using UnityEngine;
using UnityEngine.SceneManagement;
using ChoralLake.Core;
using ChoralLake.Player;

namespace ChoralLake.SceneManagement
{
    /// <summary>
    /// Central scene transition coordinator. Every scene change goes through here so we have one
    /// place to:
    ///   - Persist target scene and spawn ID into PlayerSaveData (for future save/load)
    ///   - Trigger a fade out / fade in (placeholder — will be wired to UI later)
    ///   - Position the player at the correct spawn point after load completes
    /// </summary>
    public static class SceneLoader
    {
        private static string _pendingSpawnId = "default";
        private static bool _isLoading;

        /// <summary>
        /// Starts a transition to the given scene. The next scene's SceneBootstrap will place the
        /// player at a PlayerSpawnPoint matching spawnId (or "default" if no match is found).
        /// </summary>
        public static void LoadScene(string sceneName, string spawnId = "default")
        {
            if (_isLoading)
            {
                Debug.LogWarning($"[SceneLoader] Ignoring LoadScene('{sceneName}') — a load is already in progress.");
                return;
            }
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneLoader] LoadScene called with empty scene name.");
                return;
            }

            _isLoading = true;
            _pendingSpawnId = spawnId ?? "default";

            // Persist where the player is going so save/load can restore it later.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveData.currentSceneName = sceneName;
            }

            FadeOut(() => SceneManager.LoadScene(sceneName));
        }

        /// <summary>
        /// Called by SceneBootstrap.Start after the new scene loads. Finds the requested spawn
        /// point, moves the player there, and fades back in.
        /// </summary>
        public static void NotifySceneReady()
        {
            var spawn = FindSpawnPoint(_pendingSpawnId);
            if (spawn == null)
            {
                Debug.LogWarning($"[SceneLoader] No PlayerSpawnPoint found with id '{_pendingSpawnId}' in scene '{SceneManager.GetActiveScene().name}'. Player will not be moved.");
            }
            else
            {
                var player = PlayerRoot.Instance;
                if (player != null)
                {
                    player.TeleportTo(spawn.transform.position, spawn.FacingDirection);
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.SaveData.playerPosition = spawn.transform.position;
                    }
                }
                else
                {
                    Debug.LogError("[SceneLoader] PlayerRoot.Instance is null. Is the Player prefab in the Boot scene?");
                }
            }

            GameManager.Instance?.NotifySceneLoadComplete();
            FadeIn(() => { _isLoading = false; });
        }

        private static PlayerSpawnPoint FindSpawnPoint(string spawnId)
        {
            var all = Object.FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);
            PlayerSpawnPoint fallback = null;
            foreach (var sp in all)
            {
                if (sp.SpawnId == spawnId) return sp;
                if (sp.SpawnId == "default") fallback = sp;
            }
            return fallback;
        }

        // --- Fade placeholders ---
        // TODO: Prompt — polish. Wire to a canvas with a black fade Image and tween its alpha.
        // For now, these just invoke the callback on the next frame so the code path is correct.
        private static void FadeOut(System.Action onComplete) => onComplete?.Invoke();
        private static void FadeIn(System.Action onComplete) => onComplete?.Invoke();
    }
}