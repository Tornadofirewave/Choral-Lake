using System.Collections;
using UnityEngine;

namespace ChoralLake.Audio
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        [SerializeField] private MusicLibrarySO musicLibrary;
        [SerializeField] private float fadeDuration = 1f;
        private AudioSource _sourceA;
        private AudioSource _sourceB;
        private AudioSource _active;
        private string _currentId;
        private Coroutine _crossfade;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _sourceA = AddSource();
            _sourceB = AddSource();
            _active  = _sourceA;
        }

        public void PlayTrack(string id)
        {
            if (id == _currentId) return;
            if (musicLibrary == null || !musicLibrary.TryGet(id, out var entry))
            {
                Debug.LogWarning($"[MusicManager] Music id '{id}' not found.");
                return;
            }

            _currentId = id;
            if (_crossfade != null) StopCoroutine(_crossfade);
            _crossfade = StartCoroutine(Crossfade(entry));
        }

        public void StopMusic()
        {
            if (_crossfade != null) StopCoroutine(_crossfade);
            _currentId = null;
            _crossfade = StartCoroutine(FadeOut(_active, fadeDuration));
        }

        private IEnumerator Crossfade(MusicEntry entry)
        {
            var incoming = _active == _sourceA ? _sourceB : _sourceA;
            incoming.pitch = entry.PlaybackSpeed;
            incoming.clip   = entry.Clip;
            incoming.volume = 0f;
            incoming.loop   = true;
            incoming.Play();

            float elapsed = 0f;
            float outStart = _active.volume;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                _active.volume  = Mathf.Lerp(outStart, 0f, t);
                incoming.volume = Mathf.Lerp(0f, entry.Volume, t);
                yield return null;
            }

            _active.Stop();
            _active  = incoming;
            _crossfade = null;
        }

        private IEnumerator FadeOut(AudioSource source, float duration)
        {
            float start = source.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(start, 0f, elapsed / duration);
                yield return null;
            }
            source.Stop();
        }

        private AudioSource AddSource()
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop        = true;
            src.volume      = 0f;
            src.pitch       = 1f;
            return src;
        }

        // Per-track playback speed is defined on MusicEntry.PlaybackSpeed.
    }
}
