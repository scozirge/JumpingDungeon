﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AnimationPlayer))]
public abstract class Role : MonoBehaviour
{
    [Tooltip("勢力")]
    public Force MyForce;
    [SerializeField]
    protected RectTransform HealthBar;
    float HPBarWidth;
    protected Rigidbody2D MyRigi;

    [HideInInspector]
    public Direction DirectX;
    [HideInInspector]
    public Direction DirectY;
    private int health;
    public int Health
    {
        get { return health; }
        set
        {
            if (value < 0)
                value = 0;
            else if (value > MaxHealth)
                value = MaxHealth;
            health = value;
            HealthBar.sizeDelta = new Vector2(HPBarWidth * HealthRatio, HealthBar.rect.height);
        }
    }
    [Tooltip("最大血量")]
    [SerializeField]
    public int MaxHealth;
    public int OriginMaxHealth;
    public float HealthRatio { get { return (float)Health / (float)MaxHealth; } }
    public virtual int Damage
    {
        get
        {
            return (int)(BaseDamage *
            (1 + (Buffers.ContainsKey(RoleBuffer.DamageUp) ? Buffers[RoleBuffer.DamageUp].Value : 0 +
            (Buffers.ContainsKey(RoleBuffer.DamageDown) ? -GameSettingData.CurseDamageReduce : 0))));
        }
    }
    [Tooltip("基礎傷害")]
    [SerializeField]
    protected int BaseDamage;
    public virtual float MoveSpeed
    {
        get
        {
            return (BaseMoveSpeed + (Buffers.ContainsKey(RoleBuffer.SpeedUp) ? Buffers[RoleBuffer.SpeedUp].Value : 0)) * (1 + (Buffers.ContainsKey(RoleBuffer.Freeze) ?
                -GameSettingData.FreezeMove : 0));
        }
    }
    [Tooltip("基礎移動速度")]
    [SerializeField]
    protected int BaseMoveSpeed;
    [Tooltip("死亡特效")]
    [SerializeField]
    ParticleSystem DeathEffect;
    [Tooltip("腳色動畫播放器(舊版)")]
    [SerializeField]
    protected AnimationPlayer AniPlayer;
    [SerializeField]
    public Transform RoleTrans;
    [Tooltip("死亡音效")]
    [SerializeField]
    AudioClip DeathSound;
    [Tooltip("擊退摩擦力")]
    [SerializeField]
    protected float KnockDrag;
    [Tooltip("一般摩擦力")]
    [SerializeField]
    protected float NormalDrag;
    [Tooltip("擊退摩擦力持續時間")]
    [SerializeField]
    protected float KnockDragDuration;
    protected MyTimer DragTimer;
    [Tooltip("腳色Animator")]
    [SerializeField]
    protected Animator RoleAni;
    protected MyTimer BurningTimer;
    public float DamageBuff { get; protected set; }
    protected RoleBuffer[] CantMoveBuff = new RoleBuffer[2] { RoleBuffer.Stun, RoleBuffer.EnemyAttacking };
    protected RoleBuffer[] ElementalBuff = new RoleBuffer[3] { RoleBuffer.Burn, RoleBuffer.Freeze, RoleBuffer.DamageDown };
    protected RoleBuffer[] NegativeBuff = new RoleBuffer[4] { RoleBuffer.Burn, RoleBuffer.Freeze, RoleBuffer.DamageDown, RoleBuffer.Stun };
    public bool IsPreAttack;
    public int ExtraDefence { get; protected set; }
    public bool OnRush;
    public bool IsAlive { get; protected set; }
    public Dictionary<RoleBuffer, BufferData> Buffers = new Dictionary<RoleBuffer, BufferData>();
    Dictionary<RoleBuffer, ParticleSystem> BufferParticles = new Dictionary<RoleBuffer, ParticleSystem>();
    protected List<Skill> ActiveMonsterSkills = new List<Skill>();
    public int ActiveMonsterSkillCount { get { return ActiveMonsterSkills.Count; } }
    ParticleSystem[] MyParticles;

    protected Vector3 SaveVelocity;

    protected virtual void OnDisable()
    {
        if (MyRigi != null)
            SaveVelocity = MyRigi.velocity;
        //MyParticles = GetComponentsInChildren<ParticleSystem>();
    }
    protected virtual void OnEnable()
    {
        if (MyRigi != null)
            MyRigi.velocity = SaveVelocity;
        /*
        if (MyParticles != null && MyParticles.Length > 0)
        {
            for (int i = 0; i < MyParticles.Length; i++)
            {
                MyParticles[i].Play();
            }
        }
        */
    }
    protected virtual void Awake()
    { }
    List<PassiveBomb> PassiveList;
    protected virtual void Start()
    {
        IsAlive = true;
        HPBarWidth = HealthBar.rect.width;
        MyForce = MyEnum.ParseEnum<Force>(gameObject.tag);
        DragTimer = new MyTimer(KnockDragDuration, DragRecovery, false, false);
        MyRigi = GetComponent<Rigidbody2D>();
        if (RoleAni == null)
            RoleAni = GetComponent<Animator>();
        Skill[] coms = GetComponents<Skill>();
        PassiveList = GetComponents<PassiveBomb>().ToList();
        //ActiveMonsterSkills = coms.ToList<Skill>();
        //ActiveMonsterSkills.RemoveAll(item => item.BehaviorSkill == true);
        BurningTimer = new MyTimer(GameSettingData.BurnInterval, Burn, false, false);
        BurningTimer.StartRunTimer = false;
        DragRecovery();
    }
    protected void ChangeToKnockDrag()
    {
        if (DragTimer != null)
        {
            DragTimer.RestartCountDown();
            DragTimer.StartRunTimer = true;
            MyRigi.drag = KnockDrag;
        }
    }
    protected void DragRecovery()
    {
        if (!DragTimer.StartRunTimer)
        {
            MyRigi.drag = NormalDrag;
        }
    }
    protected virtual void Burn()
    {
        BurningTimer.StartRunTimer = true;
        int damage = (int)(MaxHealth * GameSettingData.BurnDamage);
        ReceiveDmg(ref damage);
    }
    protected virtual void Move()
    {
    }
    protected virtual void FixedUpdate()
    {
        Move();
    }
    protected virtual void Update()
    {
        ConditionTimerFunc();
        BurningTimer.RunTimer();
        DragTimer.RunTimer();
    }

    public virtual void BeAttack(Force _attackerForce, ref int _dmg, Vector2 _force)
    {
        //Add KnockForce
        if (_force != Vector2.zero)
            ChangeToKnockDrag();
        MyRigi.velocity = Vector2.zero;
        MyRigi.velocity = _force;
        if (EvitableAttack())
            _dmg = 0;
        //ShieldBlock
        ShieldBlock(ref _dmg);
        //被攻擊觸發被動
        if (PassiveList.Count > 0)
        {
            for (int i = 0; i < PassiveList.Count; i++)
            {
                PassiveList[i].TriggerPassiveCheck();
            }
        }
        //take damage
        ReceiveDmg(ref _dmg);
    }
    protected virtual bool EvitableAttack()
    {
        //if Immortal or Block, can't take damage
        if (Buffers.ContainsKey(RoleBuffer.Immortal))
            return true;
        if (Buffers.ContainsKey(RoleBuffer.Block))
        {
            if (--Buffers[RoleBuffer.Block].Value <= 0)
            {
                RemoveBuffer(Buffers[RoleBuffer.Block]);
            }
            return true;
        }
        return false;
    }
    protected virtual void ShieldBlock(ref int _dmg)
    {
    }
    public virtual void ReceiveDmg(ref int _dmg)
    {
        Health -= _dmg;
        DeathCheck();
    }
    public virtual void HealHP(int _heal)
    {
        if (!IsAlive)
            return;
        EffectEmitter.EmitParticle(GameManager.GetOtherParticle("Heal"), Vector3.zero, Vector3.zero, transform);
        Health += _heal;
    }
    public virtual void ExtendMaxHP(int _value)
    {
        MaxHealth += _value;
        HealHP(_value);
    }
    protected virtual bool DeathCheck()
    {
        if (Health <= 0)
        {
            IsAlive = false;
            EffectEmitter.EmitParticle(DeathEffect, transform.position, Vector3.zero, null);
            AudioPlayer.PlaySound(DeathSound);
            SelfDestroy();
        }
        else IsAlive = true;
        return !IsAlive;
    }
    public void AddBuffer(RoleBuffer _bufferType, params float[] _values)
    {
        if (_values.Length > 0 && _values[0] != 0)
        {
            switch (_values.Length)
            {
                case 1:
                    AddBuffer(new BufferData(_bufferType, _values[0]));
                    break;
                case 2:
                    AddBuffer(new BufferData(_bufferType, _values[0], _values[1]));
                    break;
            }
        }
    }
    public virtual void AddBuffer(BufferData _buffer)
    {
        if (Buffers.ContainsKey(_buffer.Type))
        {
            Buffers[_buffer.Type] = _buffer;
        }
        else
        {
            Buffers.Add(_buffer.Type, _buffer);
            if (!BufferParticles.ContainsKey(_buffer.Type))
            {
                if (GameManager.GetBufferParticle(_buffer.Type) != null)
                {
                    if (_buffer.Time >= 0.5f)//如果效果時間低於0.5秒就不要播特效
                    {
                        ParticleSystem ps = EffectEmitter.EmitParticle(GameManager.GetBufferParticle(_buffer.Type), Vector3.zero, Vector3.zero, transform).MyParticle;
                        if (ps)
                            BufferParticles.Add(_buffer.Type, ps);
                    }
                }
            }
            BufferEffectChange(_buffer, true);
        }
    }
    protected virtual void ConditionTimerFunc()
    {
        List<RoleBuffer> keyList = new List<RoleBuffer>(Buffers.Keys);
        List<BufferData> removeList = new List<BufferData>();
        for (int i = 0; i < keyList.Count; i++)
        {
            Buffers[keyList[i]].Time -= Time.deltaTime;
            if (Buffers[keyList[i]].Time <= 0)
            {
                //加入移除狀態清單
                removeList.Add(Buffers[keyList[i]]);
            }
        }
        //移除狀態
        for (int i = 0; i < removeList.Count; i++)
        {
            RemoveBuffer(removeList[i]);
        }
    }
    public virtual void RemoveBuffer(BufferData _buffer)
    {
        if (Buffers.ContainsKey(_buffer.Type))
            Buffers.Remove(_buffer.Type);
        if (BufferParticles.ContainsKey(_buffer.Type))
        {
            if (BufferParticles[_buffer.Type])
                Destroy(BufferParticles[_buffer.Type].gameObject);
            else
                Debug.LogWarning(string.Format("特效被非預期的移除了，有可能是該buff特效沒有勾loop:{0}", _buffer.Type));
            BufferParticles.Remove(_buffer.Type);
        }
        BufferEffectChange(_buffer, false);
    }
    public void RemoveBufferByType(params RoleBuffer[] _buffers)
    {
        List<RoleBuffer> keyList = new List<RoleBuffer>(Buffers.Keys);
        for (int i = 0; i < _buffers.Length; i++)
        {
            for (int j = 0; j < keyList.Count; j++)
            {
                if (keyList[j] == _buffers[i])
                    RemoveBuffer(Buffers[keyList[j]]);
            }
        }
    }
    public bool BuffersExist(params RoleBuffer[] _buffs)
    {
        for (int i = 0; i < _buffs.Length; i++)
        {
            if (Buffers.ContainsKey(_buffs[i]))
                return true;
        }
        return false;
    }
    public bool BuffersExistExcept(RoleBuffer _except, params RoleBuffer[] _buffs)
    {
        for (int i = 0; i < _buffs.Length; i++)
        {
            if (_buffs[i] == _except)
                continue;
            if (Buffers.ContainsKey(_buffs[i]))
                return true;
        }
        return false;
    }
    void BufferEffectChange(BufferData _buffer, bool _add)
    {
        switch (_buffer.Type)
        {
            case RoleBuffer.Stun:
                break;
            case RoleBuffer.Burn:
                if (_add)
                    Burn();
                else
                {
                    BurningTimer.RestartCountDown();
                    BurningTimer.StartRunTimer = false;
                }
                break;
            case RoleBuffer.Freeze:
                for (int i = 0; i < ActiveMonsterSkills.Count; i++)
                {
                    ActiveMonsterSkills[i].Freeze(_add);
                }
                break;
            case RoleBuffer.Untouch:
                if (RoleAni != null)
                    if (_add)
                        RoleAni.Play("Untouchable", RoleAni.GetLayerIndex("Buffer"), 0);
                    else
                        RoleAni.Play("Normal", RoleAni.GetLayerIndex("Buffer"), 0);
                break;
            case RoleBuffer.Immortal:
                RemoveBufferByType(NegativeBuff);
                break;
        }
    }
    public void RemoveAllBuffer()
    {
        List<RoleBuffer> keyList = new List<RoleBuffer>(Buffers.Keys);
        for (int i = 0; i < keyList.Count; i++)
        {
            RemoveBuffer(Buffers[keyList[i]]);
            //BufferEffectChange(Buffers[keyList[i]], false);
        }
        Buffers = new Dictionary<RoleBuffer, BufferData>();
    }
    public virtual void RemoveAllSill()
    {
        for (int i = 0; i < ActiveMonsterSkills.Count; i++)
        {
            ActiveMonsterSkills[i].InactivePlayerSkill();
        }
        ActiveMonsterSkills = new List<Skill>();
    }
    public virtual void SelfDestroy()
    {
        Destroy(gameObject);
    }
    public virtual void Attack(Skill _skill)
    {
    }
    public virtual void PreAttack()
    {
    }
    public virtual void EndPreAttack()
    {
    }
}
