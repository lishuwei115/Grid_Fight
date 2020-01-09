﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage04_BossGirl_Flower_Script : MinionType_Script
{

    public Vector2Int BasePos;
    private float StasyTime = 50;
    public bool CanRebirth = true;
    public GameObject Smoke;

    public override void SetUpEnteringOnBattle()
    {
        StartCoroutine(base.MoveByTile(GridManagerScript.Instance.GetBattleTile(UMS.Pos[0]).transform.position, CharacterAnimationStateType.Growing, SpineAnim.UpMovementSpeed));
    }

    public override void StartMoveCo()
    {
        base.StartMoveCo();
    }

    public override IEnumerator Move()
    {

        while (BattleManagerScript.Instance.CurrentBattleState != BattleState.Battle)
        {
            yield return new WaitForFixedUpdate();
        }
        while (MoveCoOn)
        {
            float timer = 0;
            InputDirection dir = (InputDirection)Random.Range(0, 4);
            float MoveTime = Random.Range(CharInfo.MovementTimer.x, CharInfo.MovementTimer.y);
            while (timer < 1)
            {
                yield return new WaitForFixedUpdate();
                while (BattleManagerScript.Instance.CurrentBattleState != BattleState.Battle)
                {
                    yield return new WaitForFixedUpdate();
                }

                timer += Time.fixedDeltaTime / MoveTime;
            }
            if (CharInfo.Health > 0)
            {
                MoveCharOnDirection(dir);
            }
            timer = 0;
            MoveTime = Random.Range(CharInfo.MovementTimer.x, CharInfo.MovementTimer.y);
            while (timer < 1)
            {
                yield return new WaitForFixedUpdate();
                while (BattleManagerScript.Instance.CurrentBattleState != BattleState.Battle)
                {
                    yield return new WaitForFixedUpdate();
                }

                timer += Time.fixedDeltaTime / MoveTime;
            }
            if (CharInfo.Health > 0)
            {
                MoveCharOnDirection(dir == InputDirection.Down ? InputDirection.Up : dir == InputDirection.Up ? InputDirection.Down : dir == InputDirection.Left ? InputDirection.Right : InputDirection.Left);
            }
        }
    }

    protected override IEnumerator MoveByTile(Vector3 nextPos, CharacterAnimationStateType animState, AnimationCurve curve)
    {
        return base.MoveByTile(nextPos,  CharacterAnimationStateType.Idle, curve);
    }

    public override void StopMoveCo()
    {
        base.StopMoveCo();
    }

    public override void SetCharDead()
    {
        if (SpineAnim.CurrentAnim != CharacterAnimationStateType.Death)
        {
            SetAttackReady(false);
            Call_CurrentCharIsDeadEvent();
            StartCoroutine(DeathStasy());
        }
    }

    private IEnumerator DeathStasy()
    {
        float timer = 0;
        if(Smoke == null)
        {
            Smoke = ParticleManagerScript.Instance.GetParticle(ParticlesType.Stage04FlowersSmoke);
            Smoke.transform.parent = transform;
            Smoke.transform.localPosition = Vector3.zero;
        }
        Smoke.SetActive(true);
        SetAnimation(CharacterAnimationStateType.Death);
        while (timer < StasyTime)
        {
            yield return new WaitForFixedUpdate();
            while (BattleManagerScript.Instance.CurrentBattleState != BattleState.Battle)
            {
                yield return new WaitForFixedUpdate();
            }

            timer += Time.fixedDeltaTime;
        }

        Call_CurrentCharIsRebirthEvent();
    }

    protected override void Call_CurrentCharIsRebirthEvent()
    {
        if(CanRebirth)
        {
            Smoke.SetActive(false);
            SetAttackReady(true);
            SetAnimation(CharacterAnimationStateType.Idle);
            base.Call_CurrentCharIsRebirthEvent();
            CharInfo.Health = CharInfo.HealthStats.Base;
        }
    }
}


