﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Universal Audio Profile", menuName = "ScriptableObjects/Audio/Profiles/Universal")]
public class UniversalAudioProfileSO : ScriptableObject
{
    [Header("Battle General")]
    public AudioClipInfoClass ArrivalSpawn;
    public AudioClipInfoClass ArrivalImpact;

    [Header("Shielding")]
    public AudioClipInfoClass BasicShield;
    public AudioClipInfoClass MegaShield;

    [Header("Power Ups")]
    public AudioClipInfoClass PowerUp_Health;
    public AudioClipInfoClass PowerUp_Speed;
    public AudioClipInfoClass PowerUp_Stamina;
    public AudioClipInfoClass PowerUp_Damage;
}
