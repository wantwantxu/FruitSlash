using UnityEngine;

namespace FruitSlash
{
    public class AudioDirector : MonoBehaviour
    {
        public static AudioDirector Instance { get; private set; }

        AudioSource music;
        AudioSource sfx;
        AudioClip sliceClip;
        AudioClip bombClip;
        AudioClip comboClip;
        AudioClip uiClip;
        AudioClip overClip;
        AudioClip musicClip;

        public static AudioDirector Ensure()
        {
            if (Instance != null) return Instance;
            var go = new GameObject("AudioDirector");
            return go.AddComponent<AudioDirector>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            music = gameObject.AddComponent<AudioSource>();
            sfx = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.playOnAwake = false;
            sfx.playOnAwake = false;
            BuildClips();
            ApplyVolumes();
            if (!music.isPlaying) music.Play();
        }

        void BuildClips()
        {
            sliceClip = Burst("slice", 740f, 0.09f, 0.35f, true);
            bombClip = Burst("bomb", 90f, 0.35f, 0.7f, true);
            comboClip = Burst("combo", 980f, 0.16f, 0.4f, false);
            uiClip = Burst("ui", 520f, 0.06f, 0.25f, false);
            overClip = Burst("over", 180f, 0.4f, 0.45f, false);
            musicClip = Melody();
            music.clip = musicClip;
        }

        public void ApplyVolumes()
        {
            SaveService.Load();
            var data = SaveService.Data;
            if (music != null) music.volume = data.masterVolume * data.musicVolume;
            if (sfx != null) sfx.volume = data.masterVolume * data.sfxVolume;
        }

        public void PlaySlice(int combo)
        {
            if (sfx == null) return;
            sfx.pitch = Mathf.Clamp(1f + (combo - 1) * 0.04f, 1f, 1.6f);
            sfx.PlayOneShot(combo > 0 && combo % 5 == 0 ? comboClip : sliceClip, 0.9f);
            sfx.pitch = 1f;
        }

        public void PlayBomb()
        {
            if (sfx != null) sfx.PlayOneShot(bombClip, 1f);
        }

        public void PlayUi()
        {
            if (sfx != null) sfx.PlayOneShot(uiClip, 0.7f);
        }

        public void PlayOver()
        {
            if (sfx != null) sfx.PlayOneShot(overClip, 0.9f);
        }

        public void PauseMusic()
        {
            if (music != null) music.Pause();
        }

        public void ResumeMusic()
        {
            if (music != null && music.clip != null && !music.isPlaying) music.UnPause();
        }

        static AudioClip Burst(string name, float freq, float dur, float amp, bool noise)
        {
            const int rate = 44100;
            int samples = Mathf.Max(1, Mathf.CeilToInt(rate * dur));
            var clip = AudioClip.Create(name, samples, 1, rate, false);
            var data = new float[samples];
            var rng = new System.Random(name.GetHashCode());
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Exp(-t * (noise ? 14f : 8f));
                float wave = noise
                    ? (float)(rng.NextDouble() * 2.0 - 1.0)
                    : Mathf.Sin(2f * Mathf.PI * freq * t);
                data[i] = wave * env * amp;
            }
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Melody()
        {
            float[] notes = { 392f, 440f, 494f, 440f, 392f, 349f, 330f, 349f };
            const int rate = 22050;
            float step = 0.42f;
            int samples = Mathf.CeilToInt(rate * step * notes.Length);
            var clip = AudioClip.Create("melody", samples, 1, rate, false);
            var data = new float[samples];
            for (int n = 0; n < notes.Length; n++)
            {
                int start = Mathf.FloorToInt(n * step * rate);
                int end = Mathf.Min(samples, Mathf.FloorToInt((n + 1) * step * rate));
                for (int i = start; i < end; i++)
                {
                    float t = (i - start) / (float)rate;
                    float env = Mathf.Sin(Mathf.Clamp01(t / step) * Mathf.PI);
                    data[i] = Mathf.Sin(2f * Mathf.PI * notes[n] * t) * env * 0.18f;
                }
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
