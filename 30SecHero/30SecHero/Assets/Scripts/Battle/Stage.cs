﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattleManage
{
    [SerializeField]
    public int FloorPlate;
    [SerializeField]
    int PlateSizeX;
    [SerializeField]
    int BossDebutPlate;
    [SerializeField]
    MyText FloorText;
    [SerializeField]
    MyText VelocityText;
    [SerializeField]
    Gate GatePrefab;
    [SerializeField]
    Gate EntrancePrefab;
    [SerializeField]
    Transform GateParent;

    [SerializeField]
    protected RectTransform LocationCriterion;
    [SerializeField]
    protected RectTransform Pivot;
    float LocationCriterionWidth;

    void InitStage()
    {
        if (!MyPlayer)
            return;
        LocationCriterionWidth = LocationCriterion.rect.width - 21;
        UpdateCurPlate();
        SpawnGate(Floor - 1);
        SpawnGate(Floor);
    }

    static bool IsFirstHalf
    {
        get
        {
            float t = CurPlate % BM.FloorPlate;
            return t <= (BM.FloorPlate / 2);
        }
    }
    static int CurPlate = 0;
    static float FloorProcessingRatio = 0;
    void UpdateCurPlate()
    {
        if (!MyPlayer)
            return;
        CurPlate = (int)((BM.MyPlayer.transform.position.x + 1.5 * BM.PlateSizeX) / BM.PlateSizeX);
        float distToFirstDoor = BattleManage.BM.MyPlayer.transform.position.x + (BM.PlateSizeX * 1.5f);
        Floor = (int)(distToFirstDoor / (BM.PlateSizeX * BM.FloorPlate)) + StartFloor;
        if (distToFirstDoor >= 0)
        {
            FloorProcessingRatio = (distToFirstDoor % (BM.PlateSizeX * BM.FloorPlate)) / (BM.PlateSizeX * BM.FloorPlate);
        }
        else
        {
            Floor -= 1;
            FloorProcessingRatio = (distToFirstDoor % (BM.PlateSizeX * BM.FloorPlate) + (BM.PlateSizeX * BM.FloorPlate)) / (BM.PlateSizeX * BM.FloorPlate);
        }
        Pivot.localPosition = new Vector2(FloorProcessingRatio * LocationCriterionWidth, Pivot.localPosition.y);
        BM.VelocityText.text = string.Format("{0}{1}", (int)BM.MyPlayer.MoveSpeed, StringData.GetString("Meter"));
        if (IsDemogorgonFloor)
        {
            if (CurPlate == NextDemogorgonFloor * FloorPlate - BossDebutPlate)
                SpawnDemogorgon();
        }
        BM.FloorText.text = string.Format("{0}{1}", Floor, StringData.GetString("Floor"));
    }
    static void SpawnGate(int _floor)
    {
        Gate gate;
        if (_floor != 0)
        {
            gate = Instantiate(BM.GatePrefab, Vector3.zero, Quaternion.identity) as Gate;
        }
        else
        {
            gate = Instantiate(BM.EntrancePrefab, Vector3.zero, Quaternion.identity) as Gate;
        }
        gate.transform.SetParent(BM.GateParent);
        gate.Init(_floor);
        gate.transform.position = new Vector2(((_floor + 1 - StartFloor) * BM.FloorPlate * BM.PlateSizeX) - (BM.PlateSizeX * 1.5f), 0);
    }
    public static void SpawnNextGate(int _destroyedFloor)
    {
        TransferToGainEquipDataList();//將目前吃到的裝備加到獲得裝備清單中
        if (Floor > _destroyedFloor)
        {
            SpawnGate(_destroyedFloor - 1);
            if (!BM.TestMode)
            {
                AvailableMillions = EnemyData.GetAvailableMillions(Floor - 1);
                IsDemogorgonFloor = CheckDemogorgon(Floor - 1);
            }
        }
        else
        {
            SpawnGate(_destroyedFloor + 1);
            if (!BM.TestMode)
            {
                AvailableMillions = EnemyData.GetAvailableMillions(Floor + 1);
                IsDemogorgonFloor = CheckDemogorgon(Floor + 1);
            }
        }
        PassFloorCount++;
        if (Floor > MaxFloor)
            MaxFloor = Floor;
    }
    public static bool CheckDemogorgon(int _floor)
    {
        return (_floor == NextDemogorgonFloor);
    }
}
