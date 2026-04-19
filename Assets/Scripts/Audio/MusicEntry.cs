using System;
using UnityEngine;

namespace ChoralLake.Audio
{
    [Serializable]
    public class MusicEntry
    {
        [SerializeField] private string id;
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.5f;

        public string Id     => id;
        public AudioClip Clip => clip;
        public float Volume  => volume;
    }
}
