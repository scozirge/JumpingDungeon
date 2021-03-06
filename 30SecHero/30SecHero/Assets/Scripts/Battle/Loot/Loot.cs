﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Loot : MonoBehaviour
{
    [SerializeField]
    ParticleSystem SpawnEffect;
    [Tooltip("在可以吃掉前需求的等待時間")]
    [SerializeField]
    protected float WaitToBeAcquire = 0;
    [Tooltip("掉落隨機位置半徑")]
    [SerializeField]
    protected int RandomPosRadius = 0;
    AILootMove MyAIMove;
    protected bool ReadyToAcquire;
    [SerializeField]
    protected AudioClip GainSound;
    WaitToDo<float> WaitToAcquire;

    protected virtual void Start()
    {
        MyAIMove = GetComponent<AILootMove>();
        if (MyAIMove)
            MyAIMove.ReadyToMove = false;
        RandomPos();
        if (WaitToBeAcquire != 0)
            WaitToAcquire = new WaitToDo<float>(WaitToBeAcquire, WaitToMoveToAcquire, true);
        else
            WaitToMoveToAcquire();
        if (SpawnEffect)
            EffectEmitter.EmitParticle(SpawnEffect, Vector3.zero, Vector3.zero, transform);
    }
    void RandomPos()
    {
        if (RandomPosRadius == 0)
            return;
        int randX = Random.Range(-RandomPosRadius, RandomPosRadius);
        int randY = Random.Range(-RandomPosRadius, RandomPosRadius);
        transform.position += new Vector3(randX, randY);
    }
    void Update()
    {
        if (WaitToAcquire != null)
            WaitToAcquire.RunTimer();
    }
    void WaitToMoveToAcquire()
    {
        ReadyToAcquire = true;
        if (MyAIMove)
        {
            MyAIMove.DebutSpeed = 2000;
            MyAIMove.ReadyToMove = true;
        }
    }

}
