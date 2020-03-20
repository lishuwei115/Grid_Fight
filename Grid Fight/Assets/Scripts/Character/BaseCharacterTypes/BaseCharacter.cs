﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseCharacter : MonoBehaviour, IDisposable
{
  
    public delegate void CurrentCharIsDead(CharacterNameType cName, List<ControllerType> playerController, SideType side);
    public event CurrentCharIsDead CurrentCharIsDeadEvent;

    public delegate void TileMovementComplete(BaseCharacter movingChar);
    public event TileMovementComplete TileMovementCompleteEvent;

    public delegate void HealthStatsChanged(float value, HealthChangedType changeType, Transform charOwner);
    public event HealthStatsChanged HealthStatsChangedEvent;

    public delegate void CurrentCharIsRebirth(CharacterNameType cName, List<ControllerType> playerController, SideType side);
    public event CurrentCharIsRebirth CurrentCharIsRebirthEvent;

    public CharacterInfoScript CharInfo
    {
        get
        {
            if (_CharInfo == null)
            {
                _CharInfo = this.GetComponentInChildren<CharacterInfoScript>(true);
                _CharInfo.BaseSpeedChangedEvent += _CharInfo_BaseSpeedChangedEvent;
                _CharInfo.DeathEvent += _CharInfo_DeathEvent;
            }
            return _CharInfo;
        }
    }
    public CharacterInfoScript _CharInfo;
    public bool isMoving = false;
    public bool IsUsingAPortal = false;
    protected IEnumerator MoveCo;
    [HideInInspector]
    public List<BattleTileScript> CurrentBattleTiles = new List<BattleTileScript>();
    public SpineAnimationManager SpineAnim;
    public bool IsOnField = false;
    public bool CanAttack = false;
    public CharacterLevelType NextAttackLevel = CharacterLevelType.Novice;
    public bool isSpecialLoading = false;
    public bool isSpecialQueueing = false;
    public List<CurrentBuffsDebuffsClass> BuffsDebuffs = new List<CurrentBuffsDebuffsClass>();

    public List<BuffDebuffClass> BuffsDebuffsList = new List<BuffDebuffClass>();

    public bool VFXTestMode = false;
    public UnitManagementScript UMS;
    public BoxCollider CharBoxCollider;
    public ScriptableObjectAttackBase nextAttack = null;
    public AttackPhasesType currentAttackPhase = AttackPhasesType.End;
    public DeathProcessStage currentDeathProcessPhase = DeathProcessStage.None;
    protected IEnumerator attackCoroutine = null;
    public SpecialAttackStatus StopPowerfulAtk;
    private float DefendingHoldingTimer = 0;
    public bool IsSwapping = false;
    public bool SwapWhenPossible = false;
    public GameObject chargeParticles = null;
    protected bool canDefend = true;
    protected bool isDefending = false;
    public int shotsLeftInAttack
    {
        get
        {
            return _shotsLeftInAttack;
        }
        set
        {
            _shotsLeftInAttack = value;
        }
    }

    public int _shotsLeftInAttack = 0;

    // Temp variables to allow the minions without proper animations setup to charge attacks
    public bool sequencedAttacker = false;
    [HideInInspector]
    public bool Attacking = false;
    protected int CharOredrInLayer = 0;

    public virtual void Start()
    {
        if(VFXTestMode)
        {
            StartAttakCo();
            StartMoveCo();
        }
    }

    protected virtual void Update()
    {
        if (CharInfo.BaseCharacterType == BaseCharType.CharacterType_Script && UMS.CurrentAttackType == AttackType.Particles)
        {
            NewIManager.Instance.UpdateVitalitiesOfCharacter(CharInfo, UMS.Side);
        }

        if(transform.parent == null)
        {

        }


        UMS.HPBar.localScale = new Vector3((1f / 100f) * CharInfo.HealthPerc,1,1);

        UMS.StaminaBar.localScale = new Vector3((1f / 100f) * CharInfo.StaminaPerc, 1, 1);
    }

    #region Setup Character
    public virtual void SetupCharacterSide()
    {
       
        if (UMS.PlayerController.Contains(ControllerType.Enemy))
        {
            if (CharInfo.CharacterID != CharacterNameType.Stage00_BossOctopus &&
            CharInfo.CharacterID != CharacterNameType.Stage00_BossOctopus_Head &&
            CharInfo.CharacterID != CharacterNameType.Stage00_BossOctopus_Tentacles &&
            CharInfo.CharacterID != CharacterNameType.Stage00_BossOctopus_Girl)
                UMS.SelectionIndicator.parent.gameObject.SetActive(false);
        }
        else
        {
            UMS.SelectionIndicator.parent.gameObject.SetActive(true);
        }
        
        SpineAnimatorsetup();
        UMS.SetupCharacterSide();
        int layer = UMS.Side == SideType.LeftSide ? 9 : 10;
        if (CharInfo.UseLayeringSystem)
        {
            SpineAnim.gameObject.layer = layer;
        }
    }

    public virtual void StartMoveCo()
    {

    }

    public virtual void StopMoveCo()
    {
       
    }

    private void _CharInfo_BaseSpeedChangedEvent(float baseSpeed)
    {
        SpineAnim.SetAnimationSpeed(baseSpeed);
    }

    private void _CharInfo_DeathEvent()
    {
        if (IsOnField)
        {
           // EventManager.Instance.AddCharacterDeath(this);
            SetCharDead();
        }
    }

    public virtual void SetAttackReady(bool value)
    {
        //Debug.Log(CharInfo.CharacterID + "  " + value);
        if(CharBoxCollider != null)
        {
            CharBoxCollider.enabled = value;
        }
        CanAttack = value;
        IsOnField = value;
        currentAttackPhase = AttackPhasesType.End;
        CharOredrInLayer = 101 + (UMS.CurrentTilePos.x * 10) + (UMS.Facing == FacingType.Right ? UMS.CurrentTilePos.y - 12 : UMS.CurrentTilePos.y);
        if (CharInfo.UseLayeringSystem)
        {
            SpineAnim.SetSkeletonOrderInLayer(CharOredrInLayer);
        }
    }

    public virtual void SetCharDead(bool hasToDisappear = true)
    {
        for (int i = 0; i < UMS.Pos.Count; i++)
        {
            GridManagerScript.Instance.SetBattleTileState(UMS.Pos[i], BattleTileStateType.Empty);
            UMS.Pos[i] = Vector2Int.zero;
        }
        if(attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;

        }
        isMoving = false;
        SetAttackReady(false);
        Call_CurrentCharIsDeadEvent();
        if(hasToDisappear)
        {
            transform.position = new Vector3(100, 100, 100);
            gameObject.SetActive(false);
        }
    }

    protected virtual void Call_CurrentCharIsDeadEvent()
    {
        CurrentCharIsDeadEvent(CharInfo.CharacterID, UMS.PlayerController, UMS.Side);
    }

    protected virtual void Call_CurrentCharIsRebirthEvent()
    {
        CurrentCharIsRebirthEvent(CharInfo.CharacterID, UMS.PlayerController, UMS.Side);
    }

    public virtual void SetUpEnteringOnBattle()
    {

    }

    public virtual void SetUpLeavingBattle()
    {

    }

    public virtual void CharArrivedOnBattleField()
    {
        SetAttackReady(true);
    }


    #endregion
    #region Attack

    public virtual void SpecialAttackImpactEffects()
    {

    }

    public void StartAttakCo()
    {
        if(UMS.CurrentAttackType == AttackType.Tile && attackCoroutine == null)
        {
            attackCoroutine = AttackAction(true);
            StartCoroutine(attackCoroutine);
        }
    }

    //Basic attack Action that will start the attack anim every x seconds
    public virtual IEnumerator AttackAction(bool yieldBefore)
    {
        if (nextAttack == null)
        {
            GetAttack(CharacterAnimationStateType.Atk);
        }

        // DOnt do anything until the unit is free to attack(otherwise attack anim gets interupted by the other ones)
        while (SpineAnim.CurrentAnim != CharacterAnimationStateType.Idle)
        {
            yield return new WaitForSeconds(0.5f);
        }

        while (true)
        {

            //Wait until next attack (if yielding before)
            if (yieldBefore) yield return PauseAttack((CharInfo.SpeedStats.AttackSpeedRatio / 3) * nextAttack.AttackRatioMultiplier);

            while (BattleManagerScript.Instance.CurrentBattleState != BattleState.Battle || !CanAttack || isMoving ||
                (currentAttackPhase != AttackPhasesType.End))
            {
                yield return null;
            }
            yield return AttackSequence();

            while (isMoving)
            {
                yield return null;

            }
            GetAttack(CharacterAnimationStateType.Atk);

            //Wait until next attack
            if (!yieldBefore) yield return PauseAttack((CharInfo.SpeedStats.AttackSpeedRatio / 3) * nextAttack.AttackRatioMultiplier);

        }

    }


    public virtual IEnumerator AttackSequence()
    {
        yield return null;
    }

    public virtual void fireAttackAnimation(Vector3 pos)
    {

    }

    public int GetHowManyAttackAreOnBattleField(List<BulletBehaviourInfoClassOnBattleFieldClass> bulTraj)
    {
        int res = 0;
        foreach (BulletBehaviourInfoClassOnBattleFieldClass item in bulTraj)
        {
            foreach (BattleFieldAttackTileClass target in item.BulletEffectTiles)
            {
                if (GridManagerScript.Instance.isPosOnField(target.Pos))
                {
                    res++;
                }
            }
        }

        return res;
    }

    /*public virtual IEnumerator AttackSequence()
    {
        SetAnimation(nextAttack.Anim);

        while (currentAttackPhase == AttackPhasesType.Start)
        {
            while (SpineAnim.CurrentAnim != CharacterAnimationStateType.Atk)
            {
                yield return null;
                if (SpineAnim.CurrentAnim == CharacterAnimationStateType.Idle && !isMoving)
                {
                    SetAnimation(nextAttack.Anim);
                }
            }
            yield return null;
        }
    }*/

    public IEnumerator PauseAttack(float duration)
    {
        float timer = 0;
        while (timer <= duration)
        {
            yield return new WaitForFixedUpdate();
            while ((!VFXTestMode && BattleManagerScript.Instance.CurrentBattleState == BattleState.Pause))
            {
                yield return new WaitForEndOfFrame();
            }

            while (isSpecialLoading)
            {
                yield return new WaitForEndOfFrame();
                timer = 0;
            }

            timer += Time.fixedDeltaTime;
        }
    }

    public void GetAttack(CharacterAnimationStateType anim = CharacterAnimationStateType.NoMesh)
    {
        if (UMS.CurrentAttackType == AttackType.Particles)
        {
            switch (anim)
            {
                case CharacterAnimationStateType.Atk:
                    nextAttack = CharInfo.CurrentParticlesAttackTypeInfo[0];
                    break;
                case CharacterAnimationStateType.Atk1:
                    nextAttack = CharInfo.CurrentParticlesAttackTypeInfo[1];
                    break;
            }
        }
        else
        {
            foreach (ScriptableObjectAttackTypeOnBattlefield atk in CharInfo.CurrentOnBattleFieldAttackTypeInfo)
            {
                int chances = UnityEngine.Random.Range(0, 101);

                switch (atk.StatToCheck)
                {
                    case WaveStatsType.Health:
                        switch (atk.ValueChecker)
                        {
                            case ValueCheckerType.LessThan:
                                if (CharInfo.HealthPerc < atk.PercToCheck && chances < atk.Chances)
                                {
                                    nextAttack = atk;
                                }
                                break;
                            case ValueCheckerType.EqualTo:
                                if (CharInfo.HealthPerc == atk.PercToCheck && chances < atk.Chances)
                                {
                                    nextAttack = atk;
                                }
                                break;
                            case ValueCheckerType.MoreThan:
                                if (CharInfo.HealthPerc > atk.PercToCheck && chances < atk.Chances)
                                {
                                    nextAttack = atk;
                                }
                                break;
                        }
                        break;
                    case WaveStatsType.Stamina:
                        switch (atk.ValueChecker)
                        {
                            case ValueCheckerType.LessThan:
                                if (CharInfo.StaminaPerc < atk.PercToCheck && chances < atk.Chances)
                                {
                                    nextAttack = atk;
                                }
                                break;
                            case ValueCheckerType.EqualTo:
                                if (CharInfo.StaminaPerc == atk.PercToCheck && chances < atk.Chances)
                                {
                                    nextAttack = atk;
                                }
                                break;
                            case ValueCheckerType.MoreThan:
                                if (CharInfo.StaminaPerc > atk.PercToCheck && chances < atk.Chances)
                                {
                                    nextAttack = atk;
                                }
                                break;
                        }
                        break;
                    case WaveStatsType.None:
                        nextAttack = atk;
                        break;
                }
            }
        }
    }


    public void FireCastParticles(CharacterLevelType nextAttackLevel)
    {
        CastAttackParticles(nextAttackLevel);
    }

    //start the casting particlaes foe the attack
    public virtual void CastAttackParticles(CharacterLevelType nextAttackLevel)
    {
        //Debug.Log("Cast");
        NextAttackLevel = nextAttackLevel;
        GameObject cast = ParticleManagerScript.Instance.FireParticlesInPosition(CharInfo.ParticleID, UMS.Side == SideType.LeftSide ? AttackParticlePhaseTypes.CastLeft : AttackParticlePhaseTypes.CastRight,
            NextAttackLevel == CharacterLevelType.Novice ? SpineAnim.FiringPoint.position : SpineAnim.SpecialFiringPoint.position, UMS.Side);
        cast.GetComponent<DisableParticleScript>().SetSimulationSpeed(CharInfo.BaseSpeed);
        LayerParticleSelection lps = cast.GetComponent<LayerParticleSelection>();
        if (lps != null)
        {
            lps.Shot = NextAttackLevel;
            lps.SelectShotLevel();
        }

        if(UMS.CurrentAttackType == AttackType.Particles)
        {
            if (SpineAnim.CurrentAnim.ToString().Contains("Atk1"))
            {
                CharInfo.Stamina -= CharInfo.RapidAttack.Stamina_Cost_Atk;
                EventManager.Instance?.UpdateStamina(this);
            }
            else if (SpineAnim.CurrentAnim.ToString().Contains("Atk2"))
            {
                CharInfo.Stamina -= CharInfo.PowerfulAttac.Stamina_Cost_Atk;
                EventManager.Instance?.UpdateStamina(this);
            }
        }

        
    }

    //Create and set up the basic info for the bullet
    public void CreateBullet(BulletBehaviourInfoClass bulletBehaviourInfo)
    {
       // Debug.Log(isSpecialLoading);
        GameObject bullet = BulletManagerScript.Instance.GetBullet();
        bullet.transform.position = NextAttackLevel == CharacterLevelType.Novice ? SpineAnim.FiringPoint.position : SpineAnim.SpecialFiringPoint.position;
        BulletScript bs = bullet.GetComponent<BulletScript>();
        bs.BulletEffectTiles = bulletBehaviourInfo.BulletEffectTiles;
        bs.Trajectory_Y = bulletBehaviourInfo.Trajectory_Y;
        bs.Trajectory_Z = bulletBehaviourInfo.Trajectory_Z;
        bs.Facing = UMS.Facing;
        bs.ChildrenExplosionDelay = CharInfo.DamageStats.ChildrenBulletDelay;
        bs.StartingTile = UMS.CurrentTilePos;
        bs.BulletGapStartingTile = bulletBehaviourInfo.BulletGapStartingTile;
        bs.Elemental = CharInfo.DamageStats.CurrentElemental;
        bs.Side = UMS.Side;
        bs.VFXTestMode = VFXTestMode;
        bs.CharInfo = CharInfo;
        if (bulletBehaviourInfo.HasEffect)
        {
            bs.BulletEffects = bulletBehaviourInfo.Effects;
        }

        if (!GridManagerScript.Instance.isPosOnFieldByHeight(UMS.CurrentTilePos + bulletBehaviourInfo.BulletDistanceInTile))
        {
            bs.gameObject.SetActive(false);
            return;
        }

        if (UMS.Facing == FacingType.Right)
        {
            bs.DestinationTile = new Vector2Int(UMS.CurrentTilePos.x + bulletBehaviourInfo.BulletDistanceInTile.x, UMS.CurrentTilePos.y + bulletBehaviourInfo.BulletDistanceInTile.y > 11 ? 11 : UMS.CurrentTilePos.y + bulletBehaviourInfo.BulletDistanceInTile.y);
        }
        else
        {
            bs.DestinationTile = new Vector2Int(UMS.CurrentTilePos.x + bulletBehaviourInfo.BulletDistanceInTile.x, UMS.CurrentTilePos.y - bulletBehaviourInfo.BulletDistanceInTile.y < 0 ? 0 : UMS.CurrentTilePos.y - bulletBehaviourInfo.BulletDistanceInTile.y);
        }
        bs.PS = ParticleManagerScript.Instance.FireParticlesInTransform(CharInfo.ParticleID,UMS.Side == SideType.LeftSide ? AttackParticlePhaseTypes.AttackLeft : AttackParticlePhaseTypes.AttackRight, bullet.transform, UMS.Side,
            CharInfo.BaseCharacterType == BaseCharType.CharacterType_Script ? true : false);

        LayerParticleSelection lps = bs.PS.GetComponent<LayerParticleSelection>();
        if (lps != null)
        {
            bs.attackLevel = NextAttackLevel;
            lps.Shot = NextAttackLevel;
            lps.SelectShotLevel();
        }
        if(CharInfo.BaseCharacterType == BaseCharType.CharacterType_Script)
        {
            bs.gameObject.SetActive(true);
            bs.StartMoveToTile();
        }
        else
        {
            bs.gameObject.SetActive(false);
        }
    }


    public void CreateParticleAttack()
    {
        if (UMS.CurrentAttackType == AttackType.Particles)
        {
            foreach (BulletBehaviourInfoClass item in CharInfo.CurrentParticlesAttackTypeInfo[SpineAnim.CurrentAnim.ToString().Contains("1") ? 0 : 1].BulletTrajectories)
            {
                CreateBullet(item);
            }
        }
       
    }

    public void CreateTileAttack()
    {
        if (UMS.CurrentAttackType == AttackType.Tile)
        {
            ScriptableObjectAttackTypeOnBattlefield currentAtk = (ScriptableObjectAttackTypeOnBattlefield)nextAttack;
            CharInfo.RapidAttack.DamageMultiplier = currentAtk.DamageMultiplier;
            BaseCharacter charTar = null;
            if (currentAtk.AtkType == BattleFieldAttackType.OnTarget)
            {
                charTar = BattleManagerScript.Instance.AllCharactersOnField.Where(r => r.IsOnField).ToList().OrderBy(a => a.CharInfo.HealthPerc).FirstOrDefault();
            }

            foreach (BulletBehaviourInfoClassOnBattleFieldClass item in ((ScriptableObjectAttackTypeOnBattlefield)nextAttack).BulletTrajectories)
            {
                foreach (BattleFieldAttackTileClass target in item.BulletEffectTiles)
                {
                    Vector2Int res = currentAtk.AtkType == BattleFieldAttackType.OnTarget && charTar != null ? target.Pos + charTar.UMS.CurrentTilePos :
                        currentAtk.AtkType == BattleFieldAttackType.OnItSelf ? target.Pos + UMS.CurrentTilePos : target.Pos;
                    if (GridManagerScript.Instance.isPosOnField(res))
                    {
                        BattleTileScript bts = GridManagerScript.Instance.GetBattleTile(res);
                        if (target.IsEffectOnTile)
                        {
                            bts.SetupEffect(target.Effects, target.DurationOnTile, target.TileParticlesID);
                        }
                        else 
                        {
                            if (bts._BattleTileState != BattleTileStateType.Blocked)
                            {
                                shotsLeftInAttack++;

                                if (currentAtk.AtkType == BattleFieldAttackType.OnItSelf)
                                {

                                }
                                else
                                {
                                    bts.BattleTargetScript.SetAttack(item.Delay, BattleManagerScript.Instance.VFXScene ? CharInfo.ParticleID : target.ParticlesID, res,
                                   CharInfo.DamageStats.BaseDamage, CharInfo.Elemental, this,
                                   target.Effects);
                                }
                            }
                        }
                    }
                }
            }

        }
    }

    #endregion
    #region Defence

    protected float defenceCost = 20f;
    protected float partialDefenceCost = 10f;
    protected float defenceAnimSpeedMultiplier = 5f;
    protected float staminaRegenOnPerfectBlock = 10f;
    public void StartDefending()
    {
        if (BattleManagerScript.Instance.CurrentBattleState != BattleState.Battle)
        {
            return;
        }
        SetAnimation(CharacterAnimationStateType.Defending, true, 0.0f);
        SpineAnim.SetAnimationSpeed(defenceAnimSpeedMultiplier);

        if (canDefend && CharInfo.Shield >= defenceCost)
        {
            CharInfo.Shield -= defenceCost;
            isDefending = true;
            DefendingHoldingTimer = 0;
            StartCoroutine(Defending_Co());
        }
        else
        {
            StartCoroutine(RejectDefending_Co());
        }
    }

    private IEnumerator RejectDefending_Co()
    {
        float timer = (SpineAnim.GetAnimLenght(CharacterAnimationStateType.Defending) / defenceAnimSpeedMultiplier) * 0.25f;
        while(timer != 0f)
        {
            timer = Mathf.Clamp(timer - Time.deltaTime, 0f, (SpineAnim.GetAnimLenght(CharacterAnimationStateType.Defending) / defenceAnimSpeedMultiplier) * 0.25f);
            yield return null;
        }
        SetAnimation(CharacterAnimationStateType.Idle, true);
        yield return null;
    }

    private IEnumerator ReloadDefending_Co()
    {
        StopDefending();
        canDefend = false;
        while (CharInfo.ShieldPerc != 100f)
        {
            yield return null;
        }
        canDefend = true;
    }
    
    private IEnumerator Defending_Co()
    {
        while (isDefending && CharInfo.Shield > 0f && canDefend)
        {
            if(SpineAnim.CurrentAnim == CharacterAnimationStateType.Idle)
            {
                SetAnimation(CharacterAnimationStateType.Defending, true, 0.0f);
                SpineAnim.SetAnimationSpeed(5);
            }
            yield return null;
            DefendingHoldingTimer += Time.deltaTime;
            if (CharInfo.ShieldPerc == 0) StartCoroutine(ReloadDefending_Co());
        }
        DefendingHoldingTimer = 0;
    }

    public void StopDefending()
    {
        if(isDefending)
        {
            isDefending = false;
            SetAnimation(CharacterAnimationStateType.Idle, true, 0.1f);
        }

    }

    #endregion
    #region Move
    public virtual void MoveCharOnDirection(InputDirection nextDir)
    {
        if (SpineAnim.CurrentAnim == CharacterAnimationStateType.Reverse_Arriving || SpineAnim.CurrentAnim == CharacterAnimationStateType.Arriving ||
            SpineAnim.CurrentAnim == CharacterAnimationStateType.Atk2_AtkToIdle || SwapWhenPossible || CharInfo.SpeedStats.MovementSpeed <= 0)
        {
            return;
        }

        if ((CharInfo.Health > 0 && !isMoving && IsOnField && SpineAnim.CurrentAnim != CharacterAnimationStateType.Arriving) || BattleManagerScript.Instance.VFXScene)
        {
            /*if(StopPowerfulAtk >= SpecialAttackStatus.Start)
            {
                StopPowerfulAtk++;
            }*/

            if (currentAttackPhase == AttackPhasesType.Loading || currentAttackPhase == AttackPhasesType.Cast_Powerful || currentAttackPhase == AttackPhasesType.Bullet_Powerful)
            {
                return;
            }

            List<BattleTileScript> prevBattleTile = CurrentBattleTiles;
            List<BattleTileScript> CurrentBattleTilesToCheck = new List<BattleTileScript>();

            CharacterAnimationStateType AnimState;
            Vector2Int dir;
            AnimationCurve curve;
            GetDirectionVectorAndAnimationCurve(nextDir, out AnimState, out dir, out curve);

            CurrentBattleTilesToCheck = CheckTileAvailability(dir);

            if (CurrentBattleTilesToCheck.Count > 0 &&
                CurrentBattleTilesToCheck.Where(r => !UMS.Pos.Contains(r.Pos) && r.BattleTileState == BattleTileStateType.Empty).ToList().Count ==
                CurrentBattleTilesToCheck.Where(r => !UMS.Pos.Contains(r.Pos)).ToList().Count && GridManagerScript.Instance.isPosOnField(UMS.CurrentTilePos + dir))
            {
                SetAnimation(AnimState);
                isMoving = true;
                if (prevBattleTile.Count > 1)
                {

                }
                foreach (BattleTileScript item in prevBattleTile)
                {
                    GridManagerScript.Instance.SetBattleTileState(item.Pos, BattleTileStateType.Empty);
                }
                UMS.CurrentTilePos += dir;
                CharOredrInLayer = 101 + (dir.x * 10) + (UMS.Facing == FacingType.Right ? dir.y - 12 : dir.y);
                if (CharInfo.UseLayeringSystem)
                {
                    SpineAnim.SetSkeletonOrderInLayer(CharOredrInLayer);
                }

                CurrentBattleTiles = CurrentBattleTilesToCheck;
                UMS.Pos = new List<Vector2Int>();
                foreach (BattleTileScript item in CurrentBattleTilesToCheck)
                {
                    GridManagerScript.Instance.SetBattleTileState(item.Pos, BattleTileStateType.Occupied);
                    UMS.Pos.Add(item.Pos);
                }

                BattleTileScript resbts = CurrentBattleTiles.Where(r => r.Pos == UMS.CurrentTilePos).First();

                if (resbts != null)
                {
                    if (MoveCo != null)
                    {
                        StopCoroutine(MoveCo);
                    }
                    MoveCo = MoveByTile(resbts.transform.position, curve, SpineAnim.GetAnimLenght(AnimState));
                    StartCoroutine(MoveCo);
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (TileMovementCompleteEvent != null && TileMovementCompleteEvent.Target != null) TileMovementCompleteEvent(this);
            }

            if (CurrentBattleTiles.Count > 0)
            {
                foreach (BattleTileScript item in prevBattleTile)
                {
                    BattleManagerScript.Instance.OccupiedBattleTiles.Remove(item);
                }
                BattleManagerScript.Instance.OccupiedBattleTiles.AddRange(CurrentBattleTiles);
            }
        }
    }

    public void GetDirectionVectorAndAnimationCurve(InputDirection nextDir, out CharacterAnimationStateType AnimState, out Vector2Int dir, out AnimationCurve curve)
    {
        AnimState = CharacterAnimationStateType.Idle;
        curve = new AnimationCurve();
        dir = Vector2Int.zero;
        switch (nextDir)
        {
            case InputDirection.Up:
                dir = new Vector2Int(-1, 0);
                curve = SpineAnim.UpMovementSpeed;
                AnimState = CharacterAnimationStateType.DashUp;
                break;
            case InputDirection.Down:
                dir = new Vector2Int(1, 0);
                curve = SpineAnim.DownMovementSpeed;
                AnimState = CharacterAnimationStateType.DashDown;
                break;
            case InputDirection.Right:
                dir = new Vector2Int(0, 1);
                curve = SpineAnim.RightMovementSpeed;
                AnimState = UMS.Facing == FacingType.Left ? CharacterAnimationStateType.DashRight : CharacterAnimationStateType.DashLeft;
                break;
            case InputDirection.Left:
                dir = new Vector2Int(0, -1);
                curve = SpineAnim.LeftMovementSpeed;
                AnimState = UMS.Facing == FacingType.Left ? CharacterAnimationStateType.DashLeft : CharacterAnimationStateType.DashRight;
                break;
        }
    }


    private List<BattleTileScript> CheckTileAvailability(Vector2Int dir)
    {
        List<Vector2Int> nextPos = CalculateNextPos(dir);
        if (GridManagerScript.Instance.AreBattleTilesInControllerArea(nextPos, UMS.WalkingSide))
        {
            return GridManagerScript.Instance.GetBattleTiles(nextPos, UMS.WalkingSide);
        }
        return new List<BattleTileScript>();
    }

    //Calculate the next position fro the actual 
    public List<Vector2Int> CalculateNextPos(Vector2Int direction)
    {
        List<Vector2Int> res = new List<Vector2Int>();
        UMS.Pos.ForEach(r => res.Add(r + direction));
        return res;
    }


    //Move the character on the determinated Tile position
    public virtual IEnumerator MoveByTile(Vector3 nextPos, AnimationCurve curve, float animLength)
    {
        //  Debug.Log(AnimLength + "  AnimLenght   " + AnimLength / CharInfo.MovementSpeed + " Actual duration" );
        float timer = 0;
        float speedTimer = 0;
        Vector3 offset = transform.position;
        bool isMovCheck = false;
        bool isDefe = false;
        float moveValue = CharInfo.SpeedStats.MovementSpeed;
        while (timer < 1)
        {
          
            yield return BattleManagerScript.Instance.PauseUntil();
            float newAdd = (Time.fixedDeltaTime / (animLength / moveValue));
            timer += (Time.fixedDeltaTime / (animLength / moveValue));
            speedTimer += newAdd * curve.Evaluate(timer + newAdd);
            transform.position = Vector3.Lerp(offset, nextPos, speedTimer);

            if(timer > 0.7f && !isMovCheck)
            {
                isMovCheck = true;
                isMoving = false;
                if(isDefending && !isDefe)
                {
                    isDefe = true;
                    SetAnimation(CharacterAnimationStateType.Defending, true, 0.0f);
                    SpineAnim.SetAnimationSpeed(5);
                }
                TileMovementCompleteEvent?.Invoke(this);
            }

            if(SpineAnim.CurrentAnim == CharacterAnimationStateType.Reverse_Arriving)
            {
                isMoving = false;
                TileMovementCompleteEvent?.Invoke(this);
                MoveCo = null;
                yield break;
            }
        }
        
        
        if (IsOnField)
        {
            transform.position = nextPos;
        }
        MoveCo = null;
    }
    #endregion
    #region Buff/Debuff


    public void Buff_DebuffCo(Buff_DebuffClass bdClass)
    {
        BuffDebuffClass item = BuffsDebuffsList.Where(r => r.Stat == bdClass.Stat).FirstOrDefault();
        string[] newBuffDebuff = bdClass.Name.Split('_');
        if (item == null)
        {
            Debug.Log(bdClass.Name + "   " + newBuffDebuff.Last());
            item = new BuffDebuffClass(bdClass.Name, bdClass.Stat, Convert.ToInt32(newBuffDebuff.Last()), bdClass, bdClass.Duration);
            item.BuffDebuffCo = Buff_DebuffCoroutine(item);
            BuffsDebuffsList.Add(item);
            StartCoroutine(item.BuffDebuffCo);
        }
        else
        {
            if (item.Level <= Convert.ToInt32(newBuffDebuff.Last()))
            {
                string[] currentBuffDebuff = item.Name.Split('_');
                if (newBuffDebuff[1] != currentBuffDebuff[1])
                {
                    StopCoroutine(item.BuffDebuffCo);
                }
                BuffsDebuffsList.Remove(item);
                item = new BuffDebuffClass(bdClass.Name, bdClass.Stat, Convert.ToInt32(newBuffDebuff.Last()), bdClass, bdClass.Duration);
                item.BuffDebuffCo = Buff_DebuffCoroutine(item);
                BuffsDebuffsList.Add(item);
                StartCoroutine(item.BuffDebuffCo);
            }
        }
    }

    //Used to Buff/Debuff the character
    public IEnumerator Buff_DebuffCoroutine(BuffDebuffClass bdClass)
    {
        GameObject ps = null;
        if (bdClass.CurrentBuffDebuff.ParticlesToFire != ParticlesType.None)
        {
            ps = ParticleManagerScript.Instance.GetParticle(bdClass.CurrentBuffDebuff.ParticlesToFire);
            ps.transform.parent = transform;
            ps.transform.localPosition = Vector3.zero;
            ps.SetActive(true);
        }
        System.Reflection.FieldInfo parentField = null, field = null, B_field = null;
        string[] statToCheck = bdClass.Stat.ToString().Split('_');

        if (bdClass.Stat == BuffDebuffStatsType.ElementalResistance)
        {
            ElementalResistance(bdClass.CurrentBuffDebuff);
        }
        else
        {
            parentField = CharInfo.GetType().GetField(statToCheck[0]);
            field = parentField.GetValue(CharInfo).GetType().GetField(statToCheck[1]);

            B_field = parentField.GetValue(CharInfo).GetType().GetField("B_" + statToCheck[1]);
            if (bdClass.CurrentBuffDebuff.StatsChecker == StatsCheckerType.Perc)
            {
                if (field.FieldType == typeof(Vector2))
                {
                    field.SetValue(parentField.GetValue(CharInfo), bdClass.CurrentBuffDebuff.Value == 0 ? Vector2.zero : (Vector2)field.GetValue(parentField.GetValue(CharInfo)) +
                        (((Vector2)B_field.GetValue(parentField.GetValue(CharInfo))) / 100) * bdClass.CurrentBuffDebuff.Value);
                }
                else
                {
                    field.SetValue(parentField.GetValue(CharInfo), bdClass.CurrentBuffDebuff.Value == 0 ? 0 : (float)field.GetValue(parentField.GetValue(CharInfo)) +
                        (((float)B_field.GetValue(parentField.GetValue(CharInfo))) / 100) * bdClass.CurrentBuffDebuff.Value);
                }
            }
            else
            {
                if (field.FieldType == typeof(Vector2))
                {
                    field.SetValue(parentField.GetValue(CharInfo), bdClass.CurrentBuffDebuff.Value == 0 ? Vector2.zero : new Vector2(((Vector2)field.GetValue(parentField.GetValue(CharInfo))).x + bdClass.CurrentBuffDebuff.Value,
                        ((Vector2)field.GetValue(parentField.GetValue(CharInfo))).y + bdClass.CurrentBuffDebuff.Value));
                }
                else
                {
                    field.SetValue(parentField.GetValue(CharInfo), bdClass.CurrentBuffDebuff.Value == 0 ? 0 : (float)field.GetValue(parentField.GetValue(CharInfo)) + bdClass.CurrentBuffDebuff.Value);
                }


            }

            if (statToCheck[1].Contains("Health"))
            {
                HealthStatsChangedEvent?.Invoke(bdClass.CurrentBuffDebuff.Value, bdClass.CurrentBuffDebuff.Value > 0 ? HealthChangedType.Heal : HealthChangedType.Damage, transform);
            }
        }


        SetAnimation(bdClass.CurrentBuffDebuff.AnimToFire);
        int iterator = 0;
        while (bdClass.CurrentBuffDebuff.Timer <= bdClass.Duration)
        {
            yield return BattleManagerScript.Instance.PauseUntil();

            bdClass.CurrentBuffDebuff.Timer += Time.fixedDeltaTime;

            if (((int)bdClass.CurrentBuffDebuff.Timer) > iterator && statToCheck.Length == 3 && statToCheck[2].Contains("Overtime"))
            {
                iterator++;
                if (bdClass.CurrentBuffDebuff.StatsChecker == StatsCheckerType.Perc)
                {
                    field.SetValue(parentField.GetValue(CharInfo), bdClass.CurrentBuffDebuff.Value == 0 ? 0 : (float)field.GetValue(parentField.GetValue(CharInfo)) +
                        (((float)B_field.GetValue(parentField.GetValue(CharInfo))) / 100) * bdClass.CurrentBuffDebuff.Value);
                }
                else
                {
                    field.SetValue(parentField.GetValue(CharInfo), bdClass.CurrentBuffDebuff.Value == 0 ? 0 : (float)field.GetValue(parentField.GetValue(CharInfo)) + bdClass.CurrentBuffDebuff.Value);
                }
                HealthStatsChangedEvent?.Invoke(bdClass.CurrentBuffDebuff.Value, bdClass.CurrentBuffDebuff.Value > 0 ? HealthChangedType.Heal : HealthChangedType.Damage, transform);
            }
        }

        if (bdClass.Stat != BuffDebuffStatsType.ElementalResistance)
        {
            if (bdClass.CurrentBuffDebuff.StatsChecker == StatsCheckerType.Perc)
            {
                if (field.FieldType == typeof(Vector2))
                {
                    field.SetValue(parentField.GetValue(CharInfo), bdClass.CurrentBuffDebuff.Value == 0 ? (Vector2)B_field.GetValue(parentField.GetValue(CharInfo)) :
                   (Vector2)field.GetValue(parentField.GetValue(CharInfo)) - (((Vector2)B_field.GetValue(parentField.GetValue(CharInfo))) / 100) * bdClass.CurrentBuffDebuff.Value);
                }
                else
                {
                    field.SetValue(parentField.GetValue(CharInfo), bdClass.CurrentBuffDebuff.Value == 0 ? (float)B_field.GetValue(parentField.GetValue(CharInfo)) :
                    (float)field.GetValue(parentField.GetValue(CharInfo)) - (((float)B_field.GetValue(parentField.GetValue(CharInfo))) / 100) * bdClass.CurrentBuffDebuff.Value);
                }


            }
            else
            {

                if (field.FieldType == typeof(Vector2))
                {
                    field.SetValue(parentField.GetValue(CharInfo), bdClass.CurrentBuffDebuff.Value == 0 ? (Vector2)B_field.GetValue(parentField.GetValue(CharInfo)) :
                    new Vector2(((Vector2)field.GetValue(parentField.GetValue(CharInfo))).x - bdClass.CurrentBuffDebuff.Value, ((Vector2)field.GetValue(parentField.GetValue(CharInfo))).y - bdClass.CurrentBuffDebuff.Value));
                }
                else
                {
                    field.SetValue(parentField.GetValue(CharInfo), bdClass.CurrentBuffDebuff.Value == 0 ? (float)B_field.GetValue(parentField.GetValue(CharInfo)) :
                    (float)field.GetValue(parentField.GetValue(CharInfo)) - bdClass.CurrentBuffDebuff.Value);
                }
            }
        }
        BuffsDebuffsList.Remove(bdClass);
        ps?.SetActive(false);
    }


    private void ElementalResistance(Buff_DebuffClass bdClass)
    {
        CurrentBuffsDebuffsClass currentBuffDebuff = BuffsDebuffs.Where(r => r.ElementalResistence.Elemental == bdClass.ElementalResistence.Elemental).FirstOrDefault();
        ElementalWeaknessType BaseWeakness = GetElementalMultiplier(CharInfo.DamageStats.ElementalsResistence, bdClass.ElementalResistence.Elemental);
        CurrentBuffsDebuffsClass newBuffDebuff = new CurrentBuffsDebuffsClass();
        newBuffDebuff.ElementalResistence = bdClass.ElementalResistence;
        newBuffDebuff.Duration = bdClass.Duration;
        if (currentBuffDebuff != null)
        {
            StopCoroutine(currentBuffDebuff.BuffDebuffCo);
            BuffsDebuffs.Remove(currentBuffDebuff);
            newBuffDebuff.BuffDebuffCo = ElementalBuffDebuffCo(newBuffDebuff);

            ElementalWeaknessType newBuffDebuffValue = bdClass.ElementalResistence.ElementalWeakness + (int)currentBuffDebuff.ElementalResistence.ElementalWeakness > ElementalWeaknessType.ExtremelyResistent ?
                ElementalWeaknessType.ExtremelyResistent : bdClass.ElementalResistence.ElementalWeakness + (int)currentBuffDebuff.ElementalResistence.ElementalWeakness < ElementalWeaknessType.ExtremelyWeak ? ElementalWeaknessType.ExtremelyWeak :
                bdClass.ElementalResistence.ElementalWeakness + (int)currentBuffDebuff.ElementalResistence.ElementalWeakness;

            newBuffDebuff.ElementalResistence.ElementalWeakness = newBuffDebuffValue + (int)BaseWeakness > ElementalWeaknessType.ExtremelyResistent ?
                ElementalWeaknessType.ExtremelyResistent - (int)BaseWeakness : newBuffDebuffValue + (int)BaseWeakness < ElementalWeaknessType.ExtremelyWeak ? ElementalWeaknessType.ExtremelyWeak + (int)BaseWeakness :
                newBuffDebuffValue + (int)BaseWeakness;

            BuffsDebuffs.Add(newBuffDebuff);
            StartCoroutine(newBuffDebuff.BuffDebuffCo);
        }
        else
        {
            newBuffDebuff.BuffDebuffCo = ElementalBuffDebuffCo(newBuffDebuff);
            newBuffDebuff.ElementalResistence.ElementalWeakness = bdClass.ElementalResistence.ElementalWeakness + (int)BaseWeakness > ElementalWeaknessType.ExtremelyResistent ?
                ElementalWeaknessType.ExtremelyResistent - (int)BaseWeakness : bdClass.ElementalResistence.ElementalWeakness + (int)BaseWeakness < ElementalWeaknessType.ExtremelyWeak ? ElementalWeaknessType.ExtremelyWeak + (int)BaseWeakness :
                bdClass.ElementalResistence.ElementalWeakness + (int)BaseWeakness;
            BuffsDebuffs.Add(newBuffDebuff);
            StartCoroutine(newBuffDebuff.BuffDebuffCo);
        }
    }



    private IEnumerator ElementalBuffDebuffCo(CurrentBuffsDebuffsClass newBuffDebuff)
    {
        float timer = 0;
        float newDuration = newBuffDebuff.Duration - Mathf.Abs((int)newBuffDebuff.ElementalResistence.ElementalWeakness);
        while (timer <= newDuration)
        {
            yield return BattleManagerScript.Instance.PauseUntil();

            timer += Time.fixedDeltaTime;
        }

        for (int i = 0; i < Mathf.Abs((int)newBuffDebuff.ElementalResistence.ElementalWeakness); i++)
        {
            timer = 0;
            while (timer <= 1)
            {
                yield return BattleManagerScript.Instance.PauseUntil();

                timer += Time.fixedDeltaTime;
            }
        }

        BuffsDebuffs.Remove(newBuffDebuff);
    }

    #endregion
    #region Animation

    public void ArrivingEvent()
    {
        CameraManagerScript.Instance.CameraShake(CameraShakeType.Arrival);
        UMS.ArrivingParticle.transform.position = transform.position;
        UMS.ArrivingParticle.SetActive(true);
    }



    public virtual void SetAnimation(CharacterAnimationStateType animState, bool loop = false, float transition = 0)
    {
         Debug.Log(animState.ToString() + SpineAnim.CurrentAnim.ToString() + CharInfo.CharacterID.ToString());
        if (animState == CharacterAnimationStateType.Reverse_Arriving)
        {
        }     

        if(animState == CharacterAnimationStateType.GettingHit && currentAttackPhase != AttackPhasesType.End)
        {
            return;
        }

        if (animState == CharacterAnimationStateType.GettingHit && Attacking)
        {
            return;
        }

     /*   if (isMoving && animState != CharacterAnimationStateType.Reverse_Arriving)
        {
            return;
        }*/

        if (SpineAnim.CurrentAnim == CharacterAnimationStateType.Atk2_AtkToIdle)
        {
            return;
        }

        if (SpineAnim.CurrentAnim == CharacterAnimationStateType.Arriving || SpineAnim.CurrentAnim == CharacterAnimationStateType.Reverse_Arriving)
        {
            return;
        }

        float AnimSpeed = 1;

        if (animState == CharacterAnimationStateType.Atk || animState == CharacterAnimationStateType.Atk1)
        {
            AnimSpeed = CharInfo.SpeedStats.AttackSpeed * CharInfo.BaseSpeed;
        }
        else if (animState == CharacterAnimationStateType.DashDown ||
            animState == CharacterAnimationStateType.DashUp ||
            animState == CharacterAnimationStateType.DashLeft ||
            animState == CharacterAnimationStateType.DashRight)
        {
            AnimSpeed = CharInfo.SpeedStats.MovementSpeed * CharInfo.BaseSpeed;
        }
        else if(animState == CharacterAnimationStateType.Reverse_Arriving || animState == CharacterAnimationStateType.Arriving)
        {
            AnimSpeed = CharInfo.SpeedStats.LeaveSpeed;
        }
        else
        {
            AnimSpeed = CharInfo.BaseSpeed;
        }

        SpineAnim.SetAnim(animState, loop, transition);
        SpineAnim.SetAnimationSpeed(AnimSpeed);
    }

    public void SpineAnimatorsetup()
    {
        SpineAnim = GetComponentInChildren<SpineAnimationManager>(true);
        SpineAnim.CharOwner = this;
    }

    #endregion

    public virtual bool SetDamage(float damage, ElementalType elemental, bool isCritical, bool isAttackBlocking)
    {
        return SetDamage(damage, elemental,isCritical);
    }

    public virtual bool SetDamage(float damage, ElementalType elemental, bool isCritical)
    {
        if(!IsOnField)
        {
            return false;
        }
        HealthChangedType healthCT = HealthChangedType.Damage;
        bool res;
        if(isDefending)
        {
            GameObject go;
            if(DefendingHoldingTimer < CharInfo.DefenceStats.Invulnerability)
            {
                damage = 0;
                go = ParticleManagerScript.Instance.GetParticle(ParticlesType.ShieldTotalDefence);
                go.transform.position = transform.position;
                CharInfo.Stamina += staminaRegenOnPerfectBlock;
                EventManager.Instance.AddBlock(this, BlockInfo.BlockType.full);
            }
            else
            {
                damage = damage - CharInfo.DefenceStats.BaseDefence;
                go = ParticleManagerScript.Instance.GetParticle(ParticlesType.ShieldNormal);
                go.transform.position = transform.position;
                CharInfo.Shield -= partialDefenceCost;
                EventManager.Instance.AddBlock(this, BlockInfo.BlockType.partial);

                damage = damage < 0 ? 1 : damage;
            }
            healthCT = HealthChangedType.Defend;
            res = false;
            if (UMS.Facing == FacingType.Left)
            {
                go.transform.localScale = Vector3.one;
            }
            else
            {
                go.transform.localScale = new Vector3(-1,1,1);
            }
        }
        else
        {
            //Play getting hit sound only if the character is a playable one
            if (CharInfo.BaseCharacterType == BaseCharType.CharacterType_Script)
            {
               // AudioManager.Instance.PlayGeneric("Get_Hit_20200217");
            }
            SetAnimation(CharacterAnimationStateType.GettingHit);
            healthCT = isCritical ? HealthChangedType.CriticalHit : HealthChangedType.Damage;
            res = true;
        }

        /*  ElementalWeaknessType ElaboratedWeakness;
          CurrentBuffsDebuffsClass buffDebuffWeakness = BuffsDebuffs.Where(r => r.ElementalResistence.Elemental == elemental).FirstOrDefault();

          ElementalWeaknessType BaseWeakness = GetElementalMultiplier(CharInfo.DamageStats.ElementalsResistence, elemental);
          if (buffDebuffWeakness == null)
          {
              ElaboratedWeakness = BaseWeakness;
          }
          else
          {
              ElaboratedWeakness = BaseWeakness + (int)buffDebuffWeakness.ElementalResistence.ElementalWeakness;
          }

          switch (ElaboratedWeakness)
          {
              case ElementalWeaknessType.ExtremelyWeak:
                  damage = damage + (damage * 0.7f);
                  break;
              case ElementalWeaknessType.VeryWeak:
                  damage = damage + (damage * 0.5f);
                  break;
              case ElementalWeaknessType.Weak:
                  damage = damage + (damage * 0.3f);
                  break;
              case ElementalWeaknessType.Neutral:
                  break;
              case ElementalWeaknessType.Resistent:
                  damage = damage - (damage * 0.3f);
                  break;
              case ElementalWeaknessType.VeryResistent:
                  damage = damage - (damage * 0.5f);
                  break;
              case ElementalWeaknessType.ExtremelyResistent:
                  damage = damage - (damage * 0.7f);
                  break;
          }*/
        

        CharInfo.Health -= damage;
        if (CharInfo.Health == 0)
        {
            EventManager.Instance?.AddCharacterDeath(this);
        }
        EventManager.Instance?.UpdateHealth(this);
        EventManager.Instance?.UpdateStamina(this);
        HealthStatsChangedEvent?.Invoke(damage, healthCT, transform);
        return res;
    }

    public ElementalWeaknessType GetElementalMultiplier(List<ElementalResistenceClass> armorElelmntals, ElementalType elementalToCheck)
    {
        int resVal = 0;

        foreach (ElementalResistenceClass elemental in armorElelmntals)
        {

            if (elemental.Elemental != elementalToCheck)
            {
                int res = (int)elemental.Elemental + (int)elementalToCheck;
                if (res > 0)
                {
                    res -= 8;
                }

                resVal += (int)(ElementalWeaknessType)System.Enum.Parse(typeof(ElementalWeaknessType), ((RelationshipBetweenElements)res).ToString().Split('_').First()); ;
            }
            else
            {
                resVal = (int)ElementalWeaknessType.Neutral;
            }
        }

        return (ElementalWeaknessType)(resVal);
    }

    public IEnumerator UsePortal(PortalInfoClass outPortal)
    {
        while (BattleManagerScript.Instance.CurrentBattleState == BattleState.Pause || isMoving)
        {
            yield return new WaitForEndOfFrame();
        }
        StopCoroutine(MoveCo);
        IsUsingAPortal = true;
        transform.position = outPortal.PortalPos.transform.position;

    }

    public IEnumerator Freeze(float duration, float speed)
    {

        while (BattleManagerScript.Instance.CurrentBattleState == BattleState.Pause || isMoving)
        {
            yield return new WaitForEndOfFrame();
        }
        SpineAnim.SetAnimationSpeed(speed);
        float timer = 0;
        while (timer <= duration)
        {
            yield return BattleManagerScript.Instance.PauseUntil();

            timer += Time.fixedDeltaTime;
        }

        SpineAnim.SetAnimationSpeed(CharInfo.BaseSpeed);
    }

    public IEnumerator Trap(PortalInfoClass outPortal)
    {
        while (BattleManagerScript.Instance.CurrentBattleState == BattleState.Pause || isMoving)
        {
            yield return new WaitForEndOfFrame();
        }
        StopCoroutine(MoveCo);
        IsUsingAPortal = true;
        transform.position = outPortal.PortalPos.transform.position;

    }


    public void SetValueFromVariableName(string vName, object value)
    {
        GetType().GetField(vName).SetValue(this, value);
    }

    public void Dispose()
    {
        //throw new NotImplementedException();
    }
}



public class ArmorClass
{
    public float Armor;
    public float MovementSpeed;
    public float Health;
}


public class WeaponClass
{
    public float Damage;
    public float MovementSpeed;
    public float Health;
}

[System.Serializable]
public class Buff_DebuffClass
{
    public string Name;
    public float Duration;
    public float Value;
    public CharacterAnimationStateType AnimToFire;
    public BuffDebuffStatsType Stat;
    public StatsCheckerType StatsChecker;
    public ElementalResistenceClass ElementalResistence;
    public ElementalType ElementalPower;
    public ParticlesType ParticlesToFire;
    public float Timer;


    public Buff_DebuffClass(string name, float duration, float value, BuffDebuffStatsType stat,
        StatsCheckerType statsChecker, ElementalResistenceClass elementalResistence, ElementalType elementalPower
        , CharacterAnimationStateType animToFire, ParticlesType particlesToFire)
    {
        Name = name;
        Duration = duration;
        Value = value;
        Stat = stat;
        StatsChecker = statsChecker;
        //AttackT = attackT;
        ElementalResistence = elementalResistence;
        ElementalPower = elementalPower;
        AnimToFire = animToFire;
        ParticlesToFire = particlesToFire;
    }

    public Buff_DebuffClass()
    {

    }
}


[System.Serializable]
public class CurrentBuffsDebuffsClass
{
    public ElementalResistenceClass ElementalResistence;
    public float Duration;
    public IEnumerator BuffDebuffCo;

    public CurrentBuffsDebuffsClass()
    {
    }

    public CurrentBuffsDebuffsClass(ElementalResistenceClass elementalResistence, IEnumerator buffDebuffCo, float duration)
    {
        ElementalResistence = elementalResistence;
        BuffDebuffCo = buffDebuffCo;
        Duration = duration;
    }
}



public class BuffDebuffClass
{
    public string Name;
    public Buff_DebuffClass CurrentBuffDebuff;
    public IEnumerator BuffDebuffCo;
    public float Duration;
    public BuffDebuffStatsType Stat;
    public int Level;

    public BuffDebuffClass()
    {

    }
    public BuffDebuffClass(string name, BuffDebuffStatsType stat, int level, Buff_DebuffClass currentCuffDebuff, float duration)
    {
        Name = name;
        Stat = stat;
        Level = level;
        CurrentBuffDebuff = currentCuffDebuff;
        Duration = duration;
    }

    public BuffDebuffClass(string name, BuffDebuffStatsType stat, int level, Buff_DebuffClass currentCuffDebuff, IEnumerator buffDebuffCo, float duration)
    {
        Name = name;
        Stat = stat;
        Level = level;
        CurrentBuffDebuff = currentCuffDebuff;
        BuffDebuffCo = buffDebuffCo;
        Duration = duration;
    }
}