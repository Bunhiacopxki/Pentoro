using System;
using System.Collections.Generic;
using UnityEngine;

public enum SfxType
{
    ChooseNumber,
    PairClear,
    RowClear,
    Pop2
}

public class AudioManager : MonoBehaviour
{
    [Serializable]
    public class SfxEntry
    {
        public SfxType type;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
    }

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("SFX Clips")]
    [SerializeField] private List<SfxEntry> sfxEntries = new List<SfxEntry>();

    [Header("Global Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterSfxVolume = 1f;

    [SerializeField] private bool sfxMuted = false;

    [Header("Binder")]
    [SerializeField] GameAudioEventBinder gameAudioEventBinder;

    private readonly Dictionary<SfxType, SfxEntry> _sfxDict =
        new Dictionary<SfxType, SfxEntry>();

    public GameAudioEventBinder Binder => gameAudioEventBinder;

    private void Awake()
    {
        BuildSfxDictionary();
        sfxSource.playOnAwake = false;
    }

    private void BuildSfxDictionary()
    {
        _sfxDict.Clear();

        for (int i = 0; i < sfxEntries.Count; i++)
        {
            SfxEntry entry = sfxEntries[i];

            if (entry == null || entry.clip == null)
                continue;

            if (_sfxDict.ContainsKey(entry.type)) continue;
            entry.volume = PlayerPrefs.GetFloat(
                GetSfxVolumeKey(entry.type),
                entry.volume
            );
            _sfxDict.Add(entry.type, entry);
        }
    }

    public void PlaySfx(SfxType type)
    {
        if (sfxMuted) return;

        if (!_sfxDict.TryGetValue(type, out SfxEntry entry))
        {
            Debug.LogWarning("Missing SFX: " + type);
            return;
        }

        if (entry.clip == null) return;

        float finalVolume = masterSfxVolume * entry.volume;
        sfxSource.PlayOneShot(entry.clip, finalVolume);
    }

    public void SetSfxVolume(SfxType type, float volume)
    {
        if (!_sfxDict.TryGetValue(type, out SfxEntry entry))
        {
            Debug.LogWarning("Cannot set volume.");
            return;
        }

        entry.volume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat(GetSfxVolumeKey(type), entry.volume);
        PlayerPrefs.Save();
    }

    public float GetSfxVolume(SfxType type)
    {
        if (!_sfxDict.TryGetValue(type, out SfxEntry entry))
        {
            Debug.LogWarning("Cannot get volume.");
            return 1f;
        }

        return entry.volume;
    }

    private string GetSfxVolumeKey(SfxType type)
    {
        return "SFX_VOLUME_" + type;
    }
}