using ChoralLake.Audio;
using UnityEngine;

namespace ChoralLake.SceneManagement
{
    /// <summary>Place in each scene alongside SceneBootstrap. Drives MusicManager on scene load.</summary>
    public class SceneMusic : MonoBehaviour
    {
        [SerializeField] private string musicId;

        private void Start()
        {
            if (!string.IsNullOrEmpty(musicId))
                MusicManager.Instance?.PlayTrack(musicId);
        }
    }
}
