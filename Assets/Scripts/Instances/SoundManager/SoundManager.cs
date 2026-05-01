 using UnityEngine;
using System.Collections.Generic;

namespace SoundManager
{
    public enum SoundType { Music, SFX }

    [System.Serializable]
    public class SoundItem
    {
        public string name;
        public SoundType type;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(-3f, 3f)]
        public float pitch = 1f;
        public bool loop;
    }

    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource musicSource;
        private AudioSource sfxSource;

        [Header("Sound Library")]
        public List<SoundItem> sounds = new List<SoundItem>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        public void Play(string name)
        {
            SoundItem sound = sounds.Find(s => s.name == name);
            if (sound == null || sound.clip == null)
                return;

            AudioSource source = sound.type == SoundType.Music ? musicSource : sfxSource;

            source.clip = sound.clip;
            source.volume = sound.volume;
            source.pitch = sound.pitch;
            source.loop = sound.type == SoundType.Music ? true : sound.loop;

            if (sound.type == SoundType.Music)
            {
                source.Play();
            }
            else
            {
                source.PlayOneShot(sound.clip);
            }
        }

        public void StopAll()
        {
            musicSource.Stop();
            sfxSource.Stop();
        }
    }
}