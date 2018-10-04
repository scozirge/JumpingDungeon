﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattleManage
{
    [SerializeField]
    MyText FloorText;
    [SerializeField]
    MyText MeterText;
    [SerializeField]
    int FloorPlate;
    [SerializeField]
    Gate GatePrefab;
    [SerializeField]
    Transform GateParent;
    [SerializeField]
    int PlateSizeX;

    static int StartPlate = -1;
    static int CurPlate = 0;
    void UpdateCurPlate()
    {
        if (!MyPlayer)
            return;
        CurPlate = (int)((BM.MyPlayer.transform.position.x + 1.5 * BM.PlateSizeX) / BM.PlateSizeX);
        BM.MeterText.text = string.Format("{0}{1}", CurPlate, StringData.GetString("Meter"));
    }
    void InitStage()
    {
        if (!MyPlayer)
            return;
        Floor = (int)(CurPlate / BM.FloorPlate) + 1;
        UpdateFloorText();
        SpawnGate(Floor-1);
        SpawnGate(Floor);
    }
    static void UpdateFloorText()
    {
        BM.FloorText.text = string.Format("{0}{1}", Floor, StringData.GetString("Floor"));
    }
    static void SpawnGate(int _floor)
    {
        Gate gate = Instantiate(BM.GatePrefab, Vector3.zero, Quaternion.identity) as Gate;
        gate.transform.SetParent(BM.GateParent);
        gate.Init(_floor);
        gate.transform.position = new Vector2((_floor * BM.FloorPlate * BM.PlateSizeX) - (BM.PlateSizeX * 1.5f), 0);
    }
    public static void SpawnNextGate(int _destroyedFloor)
    {
        if (Floor > _destroyedFloor)
        {
            SpawnGate(_destroyedFloor - 1);
            Floor--;
            UpdateFloorText();
        }
        else
        {
            SpawnGate(_destroyedFloor + 1);
            Floor++;
            UpdateFloorText();
        }

    }
}
