﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Boss Stage 09 Audio Profile", menuName = "ScriptableObjects/Audio/Profiles/Boss/Stage09")]
public class Boss_Stage09AudioProfileSO : MinionAudioProfileSO
{
    [Header("Boss Specific")]
    public CastLoopImpactAudioClipInfoClass BossAttack3;
    //add stuff here as needed later

    protected override void OnEnable()
    {
        allAudioClips.Add(BossAttack3.Cast);
        allAudioClips.Add(BossAttack3.Loop);
        allAudioClips.Add(BossAttack3.Impact);
        base.OnEnable();
    }
}
