﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AudioManagerMk2 : MonoBehaviour
{
    public static AudioManagerMk2 Instance = null;
    protected List<ManagedAudioSource> sources = new List<ManagedAudioSource>();
    public GameObject sourceObjectPrefab;

    [Header("Source Type Configuration")]
    public int musicSourcesNum = 3;
    public int ambienceSourcesNum = 2;
    public int uiSourcesNum = 1;
    public int gameSourcesNum = 20;

    private void Awake()
    {
        Instance = this;
        GenerateSources();
    }

    void GenerateSources()
    {
        for (int i = 0; i < musicSourcesNum; i++) CreateSource(AudioSourceType.Music, AudioBus.Music);
        for (int i = 0; i < ambienceSourcesNum; i++) CreateSource(AudioSourceType.Ambience, AudioBus.LowPriority);
        for (int i = 0; i < uiSourcesNum; i++) CreateSource(AudioSourceType.Ui, AudioBus.LowPriority);
        for (int i = 0; i < gameSourcesNum; i++) CreateSource(AudioSourceType.Game, AudioBus.LowPriority);
    }

    void CreateSource(AudioSourceType type, AudioBus bus)
    {
        ManagedAudioSource tempSource;
        tempSource = Instantiate(sourceObjectPrefab, transform).GetComponent<ManagedAudioSource>();
        tempSource.type = type;
        tempSource.bus = bus;
        tempSource.SetParent(transform);
        tempSource.gameObject.SetActive(false);
        sources.Add(tempSource);
    }

    ManagedAudioSource GetFreeSource(AudioBus priority, AudioSourceType sourceType)
    {
        ManagedAudioSource source = sources.Where(r => !r.gameObject.activeInHierarchy && r.type == sourceType).FirstOrDefault();
        if (source == null) source = sources.Where(r => r.bus < priority && r.type == sourceType).FirstOrDefault();
        if (source == null)
        {
            source = sources.Where(r => !r.gameObject.activeInHierarchy && r.bus != AudioBus.Music).FirstOrDefault();
            source.type = sourceType;
        }
        if (source == null) Debug.LogError("Insufficient Sources");
        return source;
    }

    void PlaySound(AudioSourceType sourceType, AudioClipInfoClass clipInfo, AudioBus priority, Transform sourceOrigin)
    {
        ManagedAudioSource source = GetFreeSource(priority, sourceType);

        source.SetParent(sourceOrigin);
        source.SetAudioClipInfo(clipInfo);
        source.bus = priority;
        source.gameObject.SetActive(true);
        source.PlaySound();
    }

    public float GetDampener(AudioSourceType type)
    {
        return sources.Where(r => r.bus == AudioBus.HighPriority && r.type == type).FirstOrDefault() == null ? 1f : 0.5f;
    }
}