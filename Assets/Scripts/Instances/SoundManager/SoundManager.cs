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

        [Header("Volume")]
        [Range(0f, 1f)]
        public float masterVolume = 1f;
        [Range(0f, 1f)]
        public float musicVolume = 1f;
        [Range(0f, 1f)]
        public float sfxVolume = 1f;

        [Header("Sound Library")]
        public List<SoundItem> sounds = new List<SoundItem>();

        private SoundItem currentMusic;

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

            ApplyMusicVolume();
        }

        public void Play(string name)
        {
            SoundItem sound = sounds.Find(s => s.name == name);
            if (sound == null || sound.clip == null)
                return;

            AudioSource source = sound.type == SoundType.Music ? musicSource : sfxSource;

            source.clip = sound.clip;
            source.pitch = sound.pitch;
            source.loop = sound.type == SoundType.Music ? true : sound.loop;

            if (sound.type == SoundType.Music)
            {
                currentMusic = sound;
                source.volume = GetEffectiveVolume(sound);
                source.Play();
            }
            else
            {
                source.volume = 1f;
                source.PlayOneShot(sound.clip, GetEffectiveVolume(sound));
            }
        }

        public void StopAll()
        {
            musicSource.Stop();
            sfxSource.Stop();
        }

        private float GetEffectiveVolume(SoundItem sound)
        {
            if (sound == null)
                return 0f;

            float typeVolume = sound.type == SoundType.Music ? musicVolume : sfxVolume;
            return sound.volume * masterVolume * typeVolume;
        }

        private void ApplyMusicVolume()
        {
            if (musicSource == null || currentMusic == null)
                return;

            musicSource.volume = GetEffectiveVolume(currentMusic);
        }

        private void OnValidate()
        {
            ApplyMusicVolume();
        }
    }
}