using System;
using UnityEngine;

namespace ChoralLake.Audio
{
    [Serializable]
    public class SfxEntry
    {
        [SerializeField] private string id;
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        public string Id => id;
        public AudioClip Clip => clip;
        public float Volume => volume;
    }
}
