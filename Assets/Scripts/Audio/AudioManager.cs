using UnityEngine;

namespace ChoralLake.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private SfxLibrarySO sfxLibrary;
        [SerializeField] private AudioSource sfxSource;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        public void PlaySfx(string id)
        {
            if (sfxLibrary == null || sfxSource == null) return;
            if (!sfxLibrary.TryGet(id, out var entry))
            {
                Debug.LogWarning($"[AudioManager] SFX id '{id}' not found in library.");
                return;
            }
            if (entry.Clip == null) return;
            sfxSource.PlayOneShot(entry.Clip, entry.Volume);
        }
    }
}
