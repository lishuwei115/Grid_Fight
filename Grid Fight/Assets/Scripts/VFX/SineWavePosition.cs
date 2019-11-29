﻿
/*
 Made by WATAPAX
 www.tipografico.cl
 Cualquier mejora al script, favor compartir con la comunidad :)
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SineWavePosition : MonoBehaviour
{

    //PUBLIC
    [Range(-1, 1)] public float offsetIndividual;
    [Range(-1, 1)] public float offsetIndividualScale;
    [Range(0, 1)] public float offsetGeneral;
    [Range(0, 1)] public float ChainCount;

    public float velocidad;
    public float amplitud;


    //PRIVATE
    Cadena[] cadenas;
    Transform parentTransform;
    Transform[] tempTransform;
    List<Transform> listRoots = new List<Transform>();


    public struct Cadena
    {
        public Transform rootJoint;
        public Transform[] joints;
        public Vector3[] jointsOriginalRot;

        public void AlimentarJoints()
        {
            joints = rootJoint.gameObject.GetComponentsInChildren<Transform>();
            jointsOriginalRot = new Vector3[joints.Length];
            for (int i = 0; i < joints.Length; i++)
            {
                jointsOriginalRot[i] = joints[i].localPosition;
            }
        }

    }



    void Awake()
    {
        parentTransform = transform;
        tempTransform = GetComponentsInChildren<Transform>();
        cadenas = new Cadena[parentTransform.childCount];

        for (int x = 1; x < tempTransform.Length; x++)
        {
            if (tempTransform[x].parent == parentTransform)
                listRoots.Add(tempTransform[x]);
        }

        for (int i = 0; i < cadenas.Length; i++)
        {
            cadenas[i].rootJoint = listRoots[i];
            cadenas[i].AlimentarJoints();
        }

    }



    void Update()
    {
        for (int i = 0; i < cadenas.Length; i++)
        {
            RotarJoint(cadenas[i], Time.time - (i * offsetGeneral));
        }

    }



    // SINEWAVE TENTACLE
    void RotarJoint(Cadena _cadena, float time)
    {
        if(_cadena.joints.Length>0)
        for (int i = 1; i < _cadena.joints.Length * ChainCount; i++)
        {
                //float angulo =Mathf.Pow( Mathf.Sin((time * velocidad) - (i) * offsetIndividual),3) * (amplitud*i); weird movement
                //float angulo =Mathf.Pow( Mathf.Sin((time * velocidad) - (i) * offsetIndividual),1) * (amplitud*(((_cadena.joints.Length * ChainCount )- i)/(_cadena.joints.Length * ChainCount)));
                float f = (Mathf.Abs(i - i/2f) *2f);
            float angulo =Mathf.Pow( Mathf.Sin((time * velocidad) - (i) * offsetIndividual),1) * (amplitud* (((_cadena.joints.Length * ChainCount) - i) / (_cadena.joints.Length * ChainCount)));
            float angulo2 =Mathf.Pow( Mathf.Cos((time * velocidad) - (i) * offsetIndividualScale),1) * (amplitud* (((_cadena.joints.Length * ChainCount) - i) / (_cadena.joints.Length * ChainCount)));
            //float angulo =Mathf.Pow( Mathf.Sin((time * velocidad) - (i) * offsetIndividual),1) * (amplitud * (f/(_cadena.joints.Length * ChainCount)));
                float rotOriginal = _cadena.jointsOriginalRot[i].y;
            _cadena.joints[i].localPosition = new Vector3(_cadena.jointsOriginalRot[i].x+ angulo2, rotOriginal + angulo, _cadena.jointsOriginalRot[i].z);
        }
    }

}

