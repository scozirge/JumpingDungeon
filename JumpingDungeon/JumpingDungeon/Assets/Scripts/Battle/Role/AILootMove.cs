﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AILootMove : MonoBehaviour
{
    [Tooltip("移動到指定座標後會不會遊蕩")]
    [SerializeField]
    bool Wander;
    [Tooltip("移動速度")]
    [SerializeField]
    int MoveSpeed;
    [Tooltip("是否要跟著攝影機")]
    [SerializeField]
    bool FollowCamera;
    [Tooltip("遊蕩時間間隔")]
    [SerializeField]
    float WanderInterval;
    [Tooltip("轉向係數")]
    [SerializeField]
    float RotateFactor;
    [Tooltip("遊蕩範圍")]
    [SerializeField]
    float WanderRange;
    [SerializeField]
    public Vector2 Destination;

    bool CanMove;

    static float InRangeStartWander = 50;

    Vector3 RandomOffset;
    float WanderIntervalTimer;
    Vector3 WanderVelocity;
    Vector3 RandDestination;
    bool StartWander;
    Rigidbody2D MyRigi;
    bool KeepDebut;

    void Start()
    {
        MyRigi = GetComponent<Rigidbody2D>();
        WanderIntervalTimer = WanderInterval;
        if (RotateFactor < 0.02f)
            RotateFactor = 0.02f;
        KeepDebut = true;
        CanMove = true;
        if (Destination == Vector2.zero)
        {
            Destination = transform.position;
        }

    }
    public Vector2 SetRandDestination()
    {
        float randPosX = Random.Range(100, BattleManage.ScreenSize.x / 2);
        float randPosY = Random.Range(-BattleManage.ScreenSize.y / 2 + 100, BattleManage.ScreenSize.y / 2 - 100);
        RandomOffset = new Vector2(randPosX, randPosY);
        Vector2 cameraPos = BattleManage.MyCameraControler.transform.position;
        Destination = new Vector3(randPosX + cameraPos.x, randPosY + cameraPos.y, 0);
        return RandomOffset;
    }

    public void Debut()
    {
        if (FollowCamera)
        {
            //Follow camera
            Vector2 cameraPos = BattleManage.MyCameraControler.transform.position;
            Destination = new Vector3(cameraPos.x, cameraPos.y, 0) + RandomOffset;
        }

        if (!Wander && KeepDebut)
            if (Mathf.Abs(Vector3.Distance(Destination, transform.position)) < 30)
            {
                KeepDebut = false;
                MyRigi.velocity = Vector3.zero;
            }

        if (KeepDebut || FollowCamera)
        {
            Vector2 targetVel = (Destination - (Vector2)transform.position).normalized * MoveSpeed;
            MyRigi.velocity = Vector2.Lerp(MyRigi.velocity, targetVel, RotateFactor);
        }
    }

    void WanderTimerFunc()
    {
        if (!Wander)
            return;
        if (!StartWander)
            return;
        if (WanderIntervalTimer > 0)
            WanderIntervalTimer -= Time.deltaTime;
        else
        {
            WanderIntervalTimer = WanderInterval;
            CalculateRandDestination();
        }
    }
    void WanderMovement()
    {
        if (!Wander)
            return;
        if (!StartWander)
        {
            float dist = Mathf.Abs(Vector2.Distance(Destination, transform.position));
            if (dist < InRangeStartWander)
            {
                StartWander = true;
                CalculateRandDestination();
            }
            return;
        }
        WanderVelocity = (RandDestination - transform.position).normalized * MoveSpeed * 1.2f;
        MyRigi.velocity = Vector2.Lerp(MyRigi.velocity, WanderVelocity, RotateFactor);
    }
    void CalculateRandDestination()
    {
        RandDestination = new Vector2(Random.Range(-WanderRange, WanderRange), Random.Range(-WanderRange, WanderRange)) + Destination;
    }
    void FixedUpdate()
    {
        if (CanMove)
        {
            Debut();
            WanderMovement();
        }
    }
    public void SetCanMove(bool _bool)
    {
        CanMove = _bool;
        if (!CanMove)
            MyRigi.velocity = Vector3.zero;
    }
    void Update()
    {
        if (CanMove)
        {
            WanderTimerFunc();
        }
    }

}
