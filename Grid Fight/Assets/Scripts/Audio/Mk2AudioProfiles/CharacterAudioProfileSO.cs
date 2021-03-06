﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Audio Profile", menuName = "ScriptableObjects/Audio/Profiles/Character")]
public class CharacterAudioProfileSO : BaseAudioProfileSO
{
    [Header("General")]
    public AudioClipInfoClass Footsteps;
    [Header("Attacks and such")]
    public CastLoopImpactAudioClipInfoClass RapidAttack;
    public CastLoopImpactAudioClipInfoClass PowerfulAttack;
    public CastLoopImpactAudioClipInfoClass Skill1;
    public CastLoopImpactAudioClipInfoClass Skill2;
    public CastLoopImpactAudioClipInfoClass Skill3;
    [Header("Arriving/Leaving")]
    public AudioClipInfoClass ArrivingCry;
    public AudioClipInfoClass Death;
    // [Header("Shielding")]
    // public AudioClipInfoClass MinorShield;
    // public AudioClipInfoClass MajorShield;

    protected override void OnEnable()
    {
        allAudioClips.Add(Footsteps);
        allAudioClips.Add(RapidAttack.Cast);
        allAudioClips.Add(RapidAttack.Loop);
        allAudioClips.Add(RapidAttack.Impact);
        allAudioClips.Add(PowerfulAttack.Cast);
        allAudioClips.Add(PowerfulAttack.Loop);
        allAudioClips.Add(PowerfulAttack.Impact);
        allAudioClips.Add(Skill1.Cast);
        allAudioClips.Add(Skill1.Loop);
        allAudioClips.Add(Skill1.Impact);
        allAudioClips.Add(Skill2.Cast);
        allAudioClips.Add(Skill2.Loop);
        allAudioClips.Add(Skill2.Impact);
        allAudioClips.Add(Skill3.Cast);
        allAudioClips.Add(Skill3.Loop);
        allAudioClips.Add(Skill3.Impact);
        allAudioClips.Add(ArrivingCry);
        allAudioClips.Add(Death);
        base.OnEnable();
    }
}

[System.Serializable]
public class CastLoopImpactAudioClipInfoClass
{
    public AudioClipInfoClass Cast;
    public AudioClipInfoClass Loop;
    public AudioClipInfoClass Impact;
}
