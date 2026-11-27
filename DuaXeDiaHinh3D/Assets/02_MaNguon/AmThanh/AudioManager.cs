using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DuaXe3D
{
    /// <summary>Quản lý nhạc nền, âm thanh động cơ và hiệu ứng UI (Yêu cầu 10).</summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _musicA;
        private AudioSource _musicB;
        private AudioSource _sfx;
        private AudioSource _activeMusic;
        private Coroutine _fadeRoutine;

        public float MusicVolume = 0.34f;

        public static void Ensure()
        {
            if (Instance != null) return;
            GameObject go = new GameObject("AudioManager");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<AudioManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildSources();
        }

        private void BuildSources()
        {
            _musicA = gameObject.AddComponent<AudioSource>();
            _musicA.playOnAwake = false;
            _musicA.loop = true;
            _musicA.spatialBlend = 0f;
            _musicA.volume = 0f;

            _musicB = gameObject.AddComponent<AudioSource>();
            _musicB.playOnAwake = false;
            _musicB.loop = true;
            _musicB.spatialBlend = 0f;
            _musicB.volume = 0f;

            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.loop = false;
            _sfx.spatialBlend = 0f;
            _sfx.volume = 1f;
        }

        public void PlayMenuMusic()
        {
            if (!GameSession.MusicOn)
            {
                StopMusic();
                return;
            }
            PlayMusic(SynthAudio.MenuMusic);
        }

        public void PlayRaceMusic()
        {
            if (!GameSession.MusicOn)
            {
                StopMusic();
                return;
            }
            PlayMusic(SynthAudio.RaceMusic);
        }

        public void PlayMusic(AudioClip clip, float fade = 0.9f)
        {
            if (clip == null) return;
            if (_activeMusic != null && _activeMusic.clip == clip && _activeMusic.isPlaying) return;

            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(Crossfade(clip, fade));
        }

        private IEnumerator Crossfade(AudioClip clip, float duration)
        {
            AudioSource old = _activeMusic;
            AudioSource next = old == _musicA ? _musicB : _musicA;
            if (next == null) next = _musicA;

            next.Stop();
            next.clip = clip;
            next.loop = true;
            next.volume = 0f;
            next.Play();

            float startVol = old != null ? old.volume : 0f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                if (old != null) old.volume = Mathf.Lerp(startVol, 0f, k);
                next.volume = Mathf.Lerp(0f, MusicVolume, k);
                yield return null;
            }

            if (old != null)
            {
                old.Stop();
                old.clip = null;
            }
            next.volume = MusicVolume;
            _activeMusic = next;
            _fadeRoutine = null;
        }

        public void StopMusic(float fade = 0.4f)
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeOutRoutine(fade));
        }

        private IEnumerator FadeOutRoutine(float duration)
        {
            if (_activeMusic == null) yield break;
            float start = _activeMusic.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _activeMusic.volume = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / duration));
                yield return null;
            }
            _activeMusic.Stop();
            _activeMusic.clip = null;
            _activeMusic = null;
            _fadeRoutine = null;
        }

        public void SetMusicEnabled(bool enabled)
        {
            GameSession.MusicOn = enabled;
            if (!enabled)
            {
                StopMusic(0.35f);
            }
            else
            {
                PlayMusicForCurrentScene();
            }
        }

        public void PlayMusicForCurrentScene()
        {
            string scene = SceneManager.GetActiveScene().name;
            if (scene == SceneFlow.Race) PlayRaceMusic();
            else PlayMenuMusic();
        }

        public void PlaySfx(AudioClip clip, float volume = 0.7f)
        {
            if (clip == null || _sfx == null) return;
            if (!GameSession.SfxOn) return;
            _sfx.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        public void PlayUiClick()
        {
            PlaySfx(SynthAudio.Click, 0.45f);
        }

        public void PlayCountdownBeep(bool go)
        {
            PlaySfx(go ? SynthAudio.GoBeep : SynthAudio.Beep, go ? 0.8f : 0.65f);
        }

        public void PlayVictory()
        {
            PlaySfx(SynthAudio.Win, 0.9f);
        }

        public void PlayPickup()
        {
            PlaySfx(SynthAudio.Pickup, 0.6f);
        }

        public void PlayCrash(float intensity)
        {
            PlaySfx(SynthAudio.Crash, Mathf.Clamp(intensity, 0.15f, 0.8f));
        }

        public void PlayBoost()
        {
            PlaySfx(SynthAudio.Whoosh, 0.5f);
        }

        public AudioClip EngineClip
        {
            get { return SynthAudio.Engine; }
        }

        public static void PlayUiClickStatic()
        {
            if (Instance != null) Instance.PlayUiClick();
        }
    }

    /// <summary>Tiếng động cơ thay đổi cao độ / âm lượng theo tốc độ (Yêu cầu 10).</summary>
    [RequireComponent(typeof(ArcadeCarController))]
    public class EngineAudio : MonoBehaviour
    {
        private ArcadeCarController _car;
        private AudioSource _source;

        private void Start()
        {
            _car = GetComponent<ArcadeCarController>();
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = true;
            _source.spatialBlend = 0f;
            _source.dopplerLevel = 0f;
            _source.volume = 0.08f;
            _source.pitch = 0.8f;
            if (AudioManager.Instance != null)
            {
                _source.clip = AudioManager.Instance.EngineClip;
                _source.Play();
            }
        }

        private void Update()
        {
            if (_source == null || _car == null) return;

            float speed01 = Mathf.Clamp01(_car.Speed01);
            float throttle = Mathf.Abs(_car.ThrottleInput);
            float boost = _car.IsBoosting ? 0.12f : 0f;
            float targetVolume = 0.055f + speed01 * 0.42f + throttle * 0.14f + boost;
            if (!GameSession.SfxOn) targetVolume = 0f;

            float targetPitch = 0.72f + speed01 * 1.25f + throttle * 0.18f + (_car.IsBoosting ? 0.1f : 0f);
            float k = 1f - Mathf.Exp(-6f * Time.deltaTime);
            _source.volume = Mathf.Lerp(_source.volume, targetVolume, k);
            _source.pitch = Mathf.Lerp(_source.pitch, targetPitch, k);
        }
    }
}
