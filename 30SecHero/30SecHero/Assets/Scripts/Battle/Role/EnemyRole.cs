﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public partial class EnemyRole : Role
{
    [SerializeField]
    GameObject HealthObj;
    [SerializeField]
    float PlayMotionDuration = 0.5f;
    [SerializeField]
    List<BufferData> InitBuffers;
    [SerializeField]
    float KillAvatarTime = 1;

    public int ID { get; protected set; }
    public string Name { get; protected set; }
    public int DebutFloor { get; protected set; }
    public EnemyType Type;
    protected const float FrictionDuringTime = 1;
    protected float FrictionDuringTimer = FrictionDuringTime;
    protected bool StartVelocityDecay;



    AIRoleMove MyAIMove;
    PlayerRole Target;
    Sprite[] MotionSprite = new Sprite[2];
    Image RoleImg;
    SpriteRenderer RoleSR;
    MyTimer MotionTimer;
    MyTimer LifeTimer;
    public int StemFromFloor;

    public void SetEnemyData(EnemyData _data)
    {
        ID = _data.ID;
        Name = _data.Name;
        IsPreAttack = false;
        DebutFloor = _data.DebutFloor;
        Type = _data.Type;
    }
    public void SetStemFromFloor(int _floor)
    {
        StemFromFloor = _floor;
        if (Type == EnemyType.Demogorgon)
        {
            BaseDamage += (int)(StemFromFloor * GameSettingData.BossDMGGrow * BaseDamage);
            MaxHealth += (int)(StemFromFloor * GameSettingData.BossHPGrow * MaxHealth);
        }
        else
        {
            BaseDamage += (int)(StemFromFloor * GameSettingData.EnemyDMGGrow * BaseDamage);
            MaxHealth += (int)(StemFromFloor * GameSettingData.EnemyHPGrow * MaxHealth);
        }
    }
    public void SetLifeTime(float _time)
    {
        LifeTimer = new MyTimer(_time, LifeTimeOut, true, false);
    }
    void LifeTimeOut()
    {
        Health = 0;
        DeathCheck();
    }
    protected override void Start()
    {
        base.Start();
        MyAIMove = GetComponent<AIRoleMove>();
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go)
            Target = go.GetComponent<PlayerRole>();
        InitMotionPic();

        Health = MaxHealth;
        if (Target && Health <= Target.Damage)
            HealthObj.SetActive(false);
        if (InitBuffers != null)
            for (int i = 0; i < InitBuffers.Count; i++)
            {
                AddBuffer(InitBuffers[i].GetMemberwiseClone());
            }
    }
    void InitMotionPic()
    {
        RoleImg = transform.Find("Role/body").GetComponent<Image>();
        RoleSR = transform.Find("Role/body").GetComponent<SpriteRenderer>();
        if (RoleImg != null)//Image版本
        {
            string folderName = RoleImg.sprite.name.TrimEnd("_r".ToCharArray());
            folderName = folderName.TrimEnd("_a".ToCharArray());
            string rPicName = folderName + "_r";
            string aPicName = folderName + "_a";
            MotionSprite[0] = Resources.Load<Sprite>(string.Format("Images/Role/{0}/{1}", folderName, rPicName));
            MotionSprite[1] = Resources.Load<Sprite>(string.Format("Images/Role/{0}/{1}", folderName, aPicName));
            MotionTimer = new MyTimer(PlayMotionDuration, ToReadyMotion, false, false);
            ToReadyMotion();
        }
        if (RoleSR != null)//SpriteRenderer版本
        {
            string folderName = RoleSR.sprite.name;
            folderName = folderName.TrimEnd("_r".ToCharArray());
            folderName = folderName.TrimEnd("_a".ToCharArray());
            AnimationClip ac = Resources.Load<AnimationClip>(string.Format("Animator/EnemyRole/{0}", folderName));
            string rPicName = folderName + "_r";
            string aPicName = folderName + "_a";
            MotionSprite[0] = Resources.Load<Sprite>(string.Format("Images/Role/{0}/{1}", folderName, rPicName));
            MotionSprite[1] = Resources.Load<Sprite>(string.Format("Images/Role/{0}/{1}", folderName, aPicName));
            MotionTimer = new MyTimer(PlayMotionDuration, ToReadyMotion, false, false);
            //改變怪物專屬待機ani
            if (RoleAni)
            {
                if (ac != null)
                {
                    AnimatorOverrideController aoc = new AnimatorOverrideController(RoleAni.runtimeAnimatorController);
                    RoleAni.runtimeAnimatorController = aoc;
                    aoc["Idle"] = ac;
                }
            }
            ToReadyMotion();
        }
    }
    void ToReadyMotion()
    {
        if (MotionSprite[0] == null)
            return;
        if (RoleImg != null)
        {
            RoleImg.sprite = MotionSprite[0];
        }
        if (RoleSR != null)
        {
            RoleSR.sprite = MotionSprite[0];
        }
        if (IsPreAttack)
            PreAttack();
    }
    void ToAttackMotion()
    {
        if (MotionTimer != null)
            MotionTimer.StartRunTimer = true;
        if (MotionSprite[1] == null)
            return;
        if (RoleImg != null)
        {
            RoleImg.sprite = MotionSprite[1];
        }
        if (RoleSR != null)
        {
            RoleSR.sprite = MotionSprite[1];
        }
    }
    void SetEnemyDirection()
    {
        if (!Target)
            return;
        Vector2 dir = Target.transform.position - transform.position;

        if (dir.x >= 0)
            DirectX = Direction.Right;
        else
            DirectX = Direction.Left;

        if (dir.y >= 0)
            DirectY = Direction.Top;
        else
            DirectY = Direction.Bottom;
    }
    protected override void Move()
    {
        FaceTarget();
    }
    void FaceTarget()
    {
        if (!Target)
            return;
        if (BuffersExist(RoleBuffer.Stun))
            return;
        if (transform.position.x > Target.transform.position.x)
        {
            RoleTrans.localScale = Vector3.one;
        }
        else
        {
            RoleTrans.localScale = new Vector3(-1, 1, 1);
        }
    }
    public override void Attack(Skill _skill)
    {
        base.Attack(_skill);
        AniPlayer.PlayTrigger("Attack", 0);
        ToAttackMotion();
        if (_skill.AttackStopMove)
            AddBuffer(RoleBuffer.EnemyAttacking, _skill.StopMoveTime);
    }

    public override void PreAttack()
    {
        base.PreAttack();
        IsPreAttack = true;
        AniPlayer.PlayTrigger_NoPlayback("PreAttack", 0);
    }
    public override void EndPreAttack()
    {
        base.EndPreAttack();
        IsPreAttack = false;
        AniPlayer.PlayTrigger("Idle", 0);
    }
    public override void BeAttack(Force _attackerForce, ref int _dmg, Vector2 _force)
    {
        AniPlayer.PlayTrigger("BeAttack", 0);
        base.BeAttack(_attackerForce, ref _dmg, _force);
        if (!IsAlive && _attackerForce == Force.Player)
        {
            if (Type == EnemyType.Minion)
            {
                BattleManage.AddEnemyKill();
            }
            else if (Type == EnemyType.Demogorgon)
            {
                BattleManage.AddBossKill();
            }
        }
    }
    public void Rush(Vector2 _force)
    {
        //Add KnockForce
        if (_force != Vector2.zero)
            ChangeToKnockDrag();
        MyRigi.velocity = Vector2.zero;
        MyRigi.velocity = _force;
    }
    protected override bool DeathCheck()
    {
        if (base.DeathCheck())
        {
            Drop();            
            return true;
        }
        else
            return false;
    }
    public override void SelfDestroy()
    {
        BattleManage.RemoveEnemy(this);
        base.SelfDestroy();
    }

    protected override void Update()
    {
        base.Update();
        SetEnemyDirection();
        MotionTimer.RunTimer();
        if (LifeTimer != null)
            LifeTimer.RunTimer();
    }
    public override void AddBuffer(BufferData _buffer)
    {
        if (BuffersExist(RoleBuffer.Immortal))
        {
            if (MyEnum.CheckEnumExistInArray<RoleBuffer>(ElementalBuff, _buffer.Type))
            {
                return;
            }
        }
        if (Type == EnemyType.Demogorgon && _buffer.Type == RoleBuffer.Stun)
        {
            if (_buffer.Time > GameSettingData.BossSturn)
                _buffer.Time = GameSettingData.BossSturn;
        }
        base.AddBuffer(_buffer);
        UpdateCanMove();
    }
    public override void RemoveBuffer(BufferData _buffer)
    {
        base.RemoveBuffer(_buffer);
        UpdateCanMove();
    }
    void UpdateCanMove()
    {
        if (MyAIMove)
            MyAIMove.SetCanMove(!BuffersExist(CantMoveBuff));
    }
    public EnemyRole GetMemberwiseClone()
    {
        EnemyRole role = this.MemberwiseClone() as EnemyRole;
        return role;
    }
}
