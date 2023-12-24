
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;
using static Unity.VisualScripting.Member;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public static Dictionary<string, AudioClip> clipCache = new();
    public static Dictionary<string, AudioClip[]> clipsCache = new();

    public AudioSource AudioSourcePrefab;
    public AudioMixerGroup MasterMixerGroup, SFXMixerGroup, BGMMixerGroup;

    private ObjectPool<AudioSource> _sourcePool;

    private AudioSource _bgmSource = null;

    public static AudioClip GetClip(string path)
    {
        if (clipCache.ContainsKey(path)) return clipCache[path];
        var clip = Resources.Load<AudioClip>("Audio/" + path);
        if(clip != null) clipCache[path] = clip;
        return clip;
    }
    public static AudioClip[] GetClips(string path)
    {
        if (clipsCache.ContainsKey(path)) return clipsCache[path];
        var clips = Resources.LoadAll<AudioClip>("Audio/" + path);
        if (clips != null && clips.Length > 0)
        {
            clipsCache[path] = clips;
            return clips;
        }
        return null;
    }

    private void Awake()
    {
        Instance = this;
        _sourcePool = CreateSoundObjectPool();
    }

    private ObjectPool<AudioSource> CreateSoundObjectPool()
    {
        return new(
            createFunc: () => Instantiate(AudioSourcePrefab),
            actionOnGet: source => source.gameObject.SetActive(true),
            actionOnRelease: source => source.gameObject.SetActive(false),
            actionOnDestroy: source => Destroy(source.gameObject)
        );
    }

    private async UniTask AudioSourceReleaseCheckTask(AudioSource source, ObjectPool<AudioSource> pool, Action onRelease = null)
    {
        await UniTask.WaitUntil(() => source == null || source.isPlaying);
        await UniTask.WaitWhile(() => source != null && source.isPlaying);
        if (source != null)
        {
            pool.Release(source);
            onRelease?.Invoke();
        }
    }

    private void SetVolume(string name, float volume)
    {
        if (volume <= 0) volume = 0.000001f;
        MasterMixerGroup.audioMixer.SetFloat(name, Mathf.Log10(volume) * 20f);
    }

    private float GetVolume(string name)
    {
        if (MasterMixerGroup.audioMixer.GetFloat(name, out var value))
            return Mathf.Pow(10, value / 20);
        return 0f;
    }

    public void SetMasterVolume(float volume) => SetVolume("Master", volume);
    public void SetSFXVolume(float volume) => SetVolume("SFX", volume);
    public void SetBGMVolume(float volume) => SetVolume("BGM", volume);
    public void GetMasterVolume() => GetVolume("Master");
    public void GetSFXVolume() => GetVolume("SFX");
    public void GetBGMVolume() => GetVolume("BGM");

    public void PlayBGM(AudioClip clip, bool loop = true, float volume = 1f, float pitch = 1f)
    {
        _bgmSource = _sourcePool.Get();
        _bgmSource.transform.parent = Camera.main.transform;
        _bgmSource.transform.localPosition = Vector3.zero;

        _bgmSource.loop = loop;
        _bgmSource.outputAudioMixerGroup = BGMMixerGroup;
        _bgmSource.clip = clip;
        _bgmSource.volume = volume;
        _bgmSource.pitch = pitch;
        
        _bgmSource.Play();

        AudioSourceReleaseCheckTask(_bgmSource, _sourcePool, () => _sourcePool.Release(_bgmSource)).Forget();
    }

    public void StopBGM()
    {
        if (_bgmSource != null)
        {
            _bgmSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip, Vector3 pos, 
        float volume = 1f, float pitch = 1f, Transform parent = null)
    {
        var source = _sourcePool.Get();
        source.transform.SetParent(parent);
        source.transform.position = pos;

        source.outputAudioMixerGroup = SFXMixerGroup;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.Play();

        AudioSourceReleaseCheckTask(source, _sourcePool).Forget();
    }

    public void PlaySFX(AudioClip clip, Vector3 pos, float volume = 1f, Transform parent = null)
    {
        PlaySFX(clip, pos, volume, 1f, parent);
    }

    public void PlaySFX(AudioClip clip, Transform parent, float volume = 1f, float pitch = 1f)
    {
        PlaySFX(clip, parent.position, volume, pitch, parent);
    }
}