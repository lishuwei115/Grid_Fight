﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MyBox;

public class FightGrid : MonoBehaviour
{
    [Tooltip("A unique index for the grid in the environment")][Range(0, 99)][SerializeField] public int index = 99;
    [SerializeField] public bool hasBaseTransitionTime = false;
    [ConditionalField("hasBaseTransitionTime")] public float baseTransitionDuration = 3f;
    [HideInInspector] public Vector3 pivot;

    private void Awake()
    {
        pivot = transform.position;
    }
}
