﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticleManagerScript : MonoBehaviour
{
    public static ParticleManagerScript Instance;
    public List<ScriptableObjectParticle> ListOfParticles = new List<ScriptableObjectParticle>();
    public List<FiredAttackParticle> AttackParticlesFired = new List<FiredAttackParticle>();
    public List<FiredParticle> ParticlesFired = new List<FiredParticle>();
    bool isTheGamePaused = false;
    public Transform Container;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BattleManagerScript.Instance.CurrentBattleStateChangedEvent += Instance_CurrentBattleStateChangedEvent;
    }

    private void Instance_CurrentBattleStateChangedEvent(BattleState currentBattleState)
    {
        if (currentBattleState == BattleState.Pause && !isTheGamePaused)
        {
            isTheGamePaused = true;
            foreach (var item in AttackParticlesFired.Where(r => r.PS.activeInHierarchy).ToList())
            {
                foreach (ParticleSystem ps in item.PS.GetComponentsInChildren<ParticleSystem>())
                {
                    var main = ps.main;
                    main.simulationSpeed = 0;
                }
            }
        }
        else if (isTheGamePaused && currentBattleState == BattleState.Battle)
        {
            isTheGamePaused = false;
            foreach (var item in AttackParticlesFired.Where(r => r.PS.activeInHierarchy).ToList())
            {
                foreach (ParticleSystem ps in item.PS.GetComponentsInChildren<ParticleSystem>())
                {
                    var main = ps.main;
                    main.simulationSpeed = 1;
                }
            }
        }
    }

    public GameObject FireParticlesInPosition(GameObject ps,CharacterNameType characterId, AttackParticlePhaseTypes particleType, Vector3 pos, SideType side, AttackInputType attackInput)
    {
        using (FiredAttackParticle psToFire = AttackParticlesFired.Where(r => r.ParticleType == particleType && r.CharaterId == characterId &&
        !r.PS.gameObject.activeInHierarchy && r.Side == side && r.AttackInput == attackInput).FirstOrDefault())
        {
            if (psToFire != null)
            {
                psToFire.PS.transform.position = pos;
                psToFire.PS.SetActive(true);
                return psToFire.PS;
            }
            else
            {
                GameObject res = Instantiate(ps, pos, Quaternion.identity, Container);
                res.SetActive(true);
                AttackParticlesFired.Add(new FiredAttackParticle(res, characterId, particleType, side, attackInput));
                return res;
            }
        }


    }


    public GameObject FireParticlesInTransform(GameObject ps, CharacterNameType characterId, AttackParticlePhaseTypes particleType, Transform parent, SideType side, AttackInputType attackInput, bool particlesVisible)
    {
        //pType = AttackParticleTypes.Test_Mesh;
        using (FiredAttackParticle psToFire = AttackParticlesFired.Where(r => r.ParticleType == particleType && r.CharaterId == characterId 
        && !r.PS.gameObject.activeInHierarchy && r.Side == side && r.AttackInput == attackInput).FirstOrDefault())
        {
            if (psToFire != null)
            {
                psToFire.PS.transform.parent = parent;
                psToFire.PS.transform.localPosition = Vector3.zero;
                psToFire.PS.SetActive(particlesVisible);//particlesVisible
                return psToFire.PS;
            }
            else
            {
                GameObject res = Instantiate(ps, parent.position, parent.rotation, parent);
                res.transform.localPosition = Vector3.zero;
                res.SetActive(particlesVisible);//particlesVisible
                AttackParticlesFired.Add(new FiredAttackParticle(res, characterId, particleType, side, attackInput));
                return res;
            }
        }
    }

    public GameObject GetParticle(ParticlesType particle)
    {
        FiredParticle ps = ParticlesFired.Where(r => r.Particle == particle).FirstOrDefault();
        if (ps == null)
        {
            ps = new FiredParticle(Instantiate(ListOfParticles.Where(r => r.PSType == particle).First().PS), particle);
        }
        return ps.PS;
    }
}


public class FiredParticle
{
    public GameObject PS;
    public ParticlesType Particle;

    public FiredParticle()
    {
    }

    public FiredParticle(GameObject ps, ParticlesType particle)
    {
        PS = ps;
        Particle = particle;
    }

}



public class FiredAttackParticle : IDisposable
{
    public GameObject PS;
    public CharacterNameType CharaterId;
    public AttackParticlePhaseTypes ParticleType;
    public SideType Side;
    public AttackInputType AttackInput;
    public FiredAttackParticle()
    {

    }

    public FiredAttackParticle(GameObject ps, CharacterNameType charaterId, AttackParticlePhaseTypes particleType, SideType side, AttackInputType attackInput)
    {
        PS = ps;
        CharaterId = charaterId;
        ParticleType = particleType;
        Side = side;
        AttackInput = attackInput;
    }

    public void Dispose()
    {
    }
}



