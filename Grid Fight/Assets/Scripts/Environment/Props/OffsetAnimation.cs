﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script requires an animator to work, Randomize the offset of the animation when the game start
[RequireComponent(typeof(Animator))]
public class OffsetAnimation : MonoBehaviour
{
    //Get the attached Animator
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        //Set a random part of the animation to start from
        float randomIdleStart = Random.Range(0, anim.GetCurrentAnimatorStateInfo(0).length); 
        //Play current animation at the random time selected
        anim.Play(anim.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, randomIdleStart);
    }

}
