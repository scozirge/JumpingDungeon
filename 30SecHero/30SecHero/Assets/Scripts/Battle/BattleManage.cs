﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattleManage : MonoBehaviour
{
    [SerializeField]
    bool TestMode;
    [SerializeField]
    float EnemyFirstHalfInterval;
    [SerializeField]
    float EnemySecondHalfInterval;
    [SerializeField]
    float PotionInterval;
    [SerializeField]
    float PotionProportion;
    [SerializeField]
    int EnemyFirstHalfMinCount;
    [SerializeField]
    int EnemyFirstHalfMaxCount;
    [SerializeField]
    int EnemySecondHalfMinCount;
    [SerializeField]
    int EnemySecondHalfMaxCount;
    [SerializeField]
    float EnemySpawnInterval;
    [SerializeField]
    int MaxEnemy;
    [SerializeField]
    int MaxLoot;
    public float EnemyDropPotionProportion;
    [SerializeField]
    List<EnemyRole> Enemys;
    [SerializeField]
    EnemyRole DesignatedEnemy;
    int FloorPassGold;
    int NewFloorPassGold;
    float BossEmeraldProportion;
    int BossEmerald;
    int NewBossEmerald;
    int EnemyGold;
    int NoEquipWeight;
    int EquipQuality1Weight;
    int EquipQuality2Weight;
    int EquipQuality3Weight;
    int EquipQuality4Weight;
    int EquipQuality5Weight;
    int EnemyDropGold;
    float EnemyDropGoldOffset;
    [SerializeField]
    Loot LootPrefab;
    [SerializeField]
    CameraController CameraControler;
    [SerializeField]
    public PlayerRole MyPlayer;
    [SerializeField]
    GameObject SceneObject;

    static List<EnemyRole> AvailableMillions;
    static List<EnemyRole> AvailableDemonGergons;
    int CurSpawnCount;
    Transform EnemyParent;
    Transform LootParetn;
    MyTimer SpawnEnemyTimer;
    MyTimer SpawnLootTimer;
    public static BattleManage BM;
    public static CameraController MyCameraControler;
    public static int Floor;
    public static Vector2 ScreenSize;
    float DestructMargin_Left;
    float DestructMargin_Right;
    List<EnemyRole> EnemyList = new List<EnemyRole>();
    List<Loot> LootList = new List<Loot>();
    static int NextDemogorgonFloor;
    static bool IsDemogorgonFloor;
    bool IsInit;
    int EnemySpawnCount;
    public static int EnemyKill;
    

    // Use this for initialization
    void Awake()
    {
        if (!GameManager.IsInit)
            GameManager.DeployGameManager();
        SceneObject.SetActive(false);
    }
    void Init()
    {
        SceneObject.SetActive(true);
        InitBattleSetting();
        BM = this;
        EnemyParent = GameObject.FindGameObjectWithTag("EnemyParent").GetComponent<Transform>();
        LootParetn = GameObject.FindGameObjectWithTag("LootParent").GetComponent<Transform>();
        InitStage();
        MyCameraControler = CameraControler;
        CurSpawnCount = 0;
        ScreenSize = MyCameraControler.ScreenSize;
        //SpawnEnemySet
        if (TestMode)
        {
            for (int i = 0; i < Enemys.Count; i++)
            {
                if (Enemys[i] == null)
                    Enemys.RemoveAt(i);
            }
            AvailableMillions = Enemys;
        }
        else
        {
            AvailableMillions = EnemyData.GetAvailableMillions(Floor);
            AvailableDemonGergons = EnemyData.GetNextDemogorgon(Floor, out NextDemogorgonFloor);
        }
        SpawnEnemyTimer = new MyTimer(EnemyFirstHalfInterval, SpanwEnemy, true, false);
        SpawnLootTimer = new MyTimer(PotionInterval, SpawnLoot, true, false);
        //Debug.Log("NextDemogorgonFloor=" + NextDemogorgonFloor);
        IsDemogorgonFloor = CheckDemogorgon(Floor);
        IsInit = true;
        Debug.Log("Init BattleManager");
    }
    void InitBattleSetting()
    {
        PotionInterval = GameSettingData.PotionInterval;
        PotionProportion = GameSettingData.PotionProportion;
        EnemyFirstHalfInterval = GameSettingData.EnemyFirstHalfInterval;
        EnemySecondHalfInterval = GameSettingData.EnemySecondHalfInterval;
        EnemyFirstHalfMinCount = GameSettingData.EnemyFirstHalfMinCount;
        EnemyFirstHalfMaxCount = GameSettingData.EnemyFirstHalfMaxCount;
        EnemySecondHalfMinCount = GameSettingData.EnemySecondHalfMinCount;
        EnemySecondHalfMaxCount = GameSettingData.EnemySecondHalfMaxCount;
        EnemySpawnInterval = GameSettingData.EnemySpawnInterval;
        FloorPassGold = GameSettingData.FloorPassGold;
        NewFloorPassGold = GameSettingData.NewFloorPassGold;
        BossEmeraldProportion = GameSettingData.BossEmeraldProportion;
        BossEmerald = GameSettingData.BossEmerald;
        NewBossEmerald = GameSettingData.NewBossEmerald;
        EnemyGold = GameSettingData.EnemyGold;
        NoEquipWeight = GameSettingData.NoEquipWeight;
        EquipQuality1Weight = GameSettingData.EquipQuality1Weight;
        EquipQuality2Weight = GameSettingData.EquipQuality2Weight;
        EquipQuality3Weight = GameSettingData.EquipQuality3Weight;
        EquipQuality4Weight = GameSettingData.EquipQuality4Weight;
        EquipQuality5Weight = GameSettingData.EquipQuality5Weight;
        EnemyDropPotionProportion = GameSettingData.EnemyDropPotionProportion;
        EnemyDropGolds = GameSettingData.EnemyDropGold;
        EnemyDropGoldOffset = GameSettingData.EnemyDropGoldOffset;
        MaxEnemy = GameSettingData.MaxEnemy;
        MaxLoot = GameSettingData.MaxLoot;
        FloorPlate = GameSettingData.FloorPlate;
        BossDebutPlate = GameSettingData.BossDebutPlate;
    }
    public static void AddEnemyKill()
    {
        EnemyKill++;
    }
    int GetRandomEnemySpawnfCount()
    {
        int spawnCount = 0;
        if (IsFirstHalf)
            spawnCount = Random.Range(EnemyFirstHalfMinCount, EnemyFirstHalfMaxCount);
        else
            spawnCount = Random.Range(EnemySecondHalfMinCount, EnemySecondHalfMaxCount);
        return spawnCount;
    }
    void SpawnDemogorgon()
    {
        for (int i = 0; i < AvailableDemonGergons.Count; i++)
        {
            EnemyRole er = Instantiate(AvailableDemonGergons[i], Vector3.zero, Quaternion.identity) as EnemyRole;
            er.SetEnemyData(GameDictionary.EnemyDic[AvailableDemonGergons[i].ID]);
            //Set SpawnPos
            int quadrant = 1;//象限
            int nearMargin = 0;//靠近左右邊(0)或靠近上下邊(1)
            er.transform.SetParent(EnemyParent);
            AIMove am = er.GetComponent<AIRoleMove>();
            SetQuadrantAndNearMargin(am, ref quadrant, ref nearMargin);
            er.transform.position = GetSpawnPos(quadrant, nearMargin);
            EnemyList.Add(er);
        }
        AvailableDemonGergons = EnemyData.GetNextDemogorgon(Floor + 1, out NextDemogorgonFloor);
        //Debug.Log("NextDemogorgonFloor=" + NextDemogorgonFloor);
        IsDemogorgonFloor = false;
    }
    void SpanwEnemy()
    {
        if (!CheckEnemySpawnLimit())
        {
            CurSpawnCount = 0;
            SpawnEnemyTimer.StartRunTimer = true;
            UpdateSpawnEnmeyTimer();
            return;
        }
        if (AvailableMillions.Count == 0)
            return;

        EnemyRole er;
        if (DesignatedEnemy && TestMode)
            er = Instantiate(DesignatedEnemy, Vector3.zero, Quaternion.identity) as EnemyRole;
        else
        {
            int rndEnemy = Random.Range(0, AvailableMillions.Count);
            er = Instantiate(AvailableMillions[rndEnemy], Vector3.zero, Quaternion.identity) as EnemyRole;
            if (GameDictionary.EnemyDic.ContainsKey(AvailableMillions[rndEnemy].ID))
                er.SetEnemyData(GameDictionary.EnemyDic[AvailableMillions[rndEnemy].ID]);
        }
        //Set SpawnPos
        int quadrant = 1;//象限
        int nearMargin = 0;//靠近左右邊(0)或靠近上下邊(1)
        er.transform.SetParent(EnemyParent);
        AIMove am = er.GetComponent<AIRoleMove>();
        SetQuadrantAndNearMargin(am, ref quadrant, ref nearMargin);
        er.transform.position = GetSpawnPos(quadrant, nearMargin);
        CurSpawnCount++;
        if (CurSpawnCount < GetRandomEnemySpawnfCount())
            StartCoroutine(WaitToSpawnEnemy());
        else
        {
            CurSpawnCount = 0;
            SpawnEnemyTimer.StartRunTimer = true;
            UpdateSpawnEnmeyTimer();
        }
        EnemyList.Add(er);
    }
    void UpdateSpawnEnmeyTimer()
    {
        //每次出怪後重新確認出怪時間
        if (IsFirstHalf)
            SpawnEnemyTimer.ResetMaxTime(EnemyFirstHalfInterval);
        else
            SpawnEnemyTimer.ResetMaxTime(EnemySecondHalfInterval);
    }
    bool CheckEnemySpawnLimit()
    {
        if (MaxEnemy == 0)
            return true;
        int cout = 0;
        for (int i = 0; i < EnemyList.Count; i++)
        {
            if (EnemyList[i].isActiveAndEnabled)
                cout++;
        }
        if (cout < MaxEnemy)
            return true;
        return false;
    }
    bool CheckLootSpawnLimit()
    {
        if (MaxLoot == 0)
            return true;
        int cout = 0;
        for (int i = 0; i < LootList.Count; i++)
        {
            if (LootList[i].isActiveAndEnabled)
                cout++;
        }
        if (cout < MaxLoot)
            return true;
        return false;
    }
    void SpawnLoot()
    {
        if (!CheckLootSpawnLimit())
        {
            SpawnLootTimer.StartRunTimer = true;
            return;
        }
        Loot loot = Instantiate(LootPrefab, Vector3.zero, Quaternion.identity) as Loot;

        //Set SpawnPos
        int quadrant = 1;//象限
        int nearMargin = 0;//靠近左右邊(0)或靠近上下邊(1)
        loot.transform.SetParent(LootParetn);
        AIMove am = loot.GetComponent<AILootMove>();
        SetQuadrantAndNearMargin(am, ref quadrant, ref nearMargin);
        loot.transform.position = GetSpawnPos(quadrant, nearMargin);
        SpawnLootTimer.StartRunTimer = true;
        LootList.Add(loot);
    }
    void SetQuadrantAndNearMargin(AIMove _am, ref int _quadrant, ref int _nearMargin)
    {
        if (_am != null)
        {
            Vector2 erScreenPos = _am.Destination;
            if (erScreenPos == Vector2.zero)
            {
                erScreenPos = _am.SetRandDestination();
            }

            if (erScreenPos.x >= 0 && erScreenPos.y >= 0)
            {
                _quadrant = 1;//第1象限
                _nearMargin = Mathf.Abs(ScreenSize.x / 2 - erScreenPos.x) < Mathf.Abs(ScreenSize.y / 2 - erScreenPos.y) ? 0 : 1;
            }
            else if (erScreenPos.x < 0 && erScreenPos.y >= 0)
            {
                _quadrant = 2;//第2象限
                _nearMargin = Mathf.Abs(-ScreenSize.x / 2 - erScreenPos.x) < Mathf.Abs(ScreenSize.y / 2 - erScreenPos.y) ? 0 : 1;
            }
            else if (erScreenPos.x < 0 && erScreenPos.y < 0)
            {
                _quadrant = 3;//第3象限
                _nearMargin = Mathf.Abs(-ScreenSize.x / 2 - erScreenPos.x) < Mathf.Abs(-ScreenSize.y / 2 - erScreenPos.y) ? 0 : 1;
            }
            else if (erScreenPos.x > 0 && erScreenPos.y < 0)
            {
                _quadrant = 4;//第4象限
                _nearMargin = Mathf.Abs(ScreenSize.x / 2 - erScreenPos.x) < Mathf.Abs(-ScreenSize.y / 2 - erScreenPos.y) ? 0 : 1;
            }
        }
        else
        {
            _quadrant = Random.Range(1, 5);
        }
    }
    Vector3 GetSpawnPos(int _quadrant, int _nearMargin)
    {
        Vector3 spawnPos = Vector3.zero;
        switch (_quadrant)
        {
            case 1:
                spawnPos = (_nearMargin == 0) ? new Vector3(ScreenSize.x / 2, Random.Range(0, ScreenSize.y / 2)) + MyCameraControler.transform.position :
                 new Vector3(Random.Range(0, ScreenSize.x / 2), ScreenSize.y / 2) + MyCameraControler.transform.position;
                break;
            case 2:
                spawnPos = (_nearMargin == 0) ? new Vector3(-ScreenSize.x / 2, Random.Range(0, ScreenSize.y / 2)) + MyCameraControler.transform.position :
                new Vector3(Random.Range(-ScreenSize.x / 2, 0), ScreenSize.y / 2) + MyCameraControler.transform.position;
                break;
            case 3:
                spawnPos = (_nearMargin == 0) ? new Vector3(-ScreenSize.x / 2, Random.Range(-ScreenSize.y / 2, 0)) + MyCameraControler.transform.position :
                new Vector3(Random.Range(-ScreenSize.x / 2, 0), -ScreenSize.y / 2) + MyCameraControler.transform.position;
                break;
            case 4:
                spawnPos = (_nearMargin == 0) ? new Vector3(ScreenSize.x / 2, Random.Range(-ScreenSize.y / 2, 0)) + MyCameraControler.transform.position :
                new Vector3(Random.Range(0, ScreenSize.x / 2), -ScreenSize.y / 2) + MyCameraControler.transform.position;
                break;
        }
        spawnPos.z = 0;
        return spawnPos;
    }
    IEnumerator WaitToSpawnEnemy()
    {
        yield return new WaitForSeconds(EnemySpawnInterval);
        SpanwEnemy();
    }
    public static void RemoveEnemy(EnemyRole _er)
    {
        BM.EnemyList.Remove(_er);
    }
    public static void RemoveLoot(Loot _loot)
    {
        BM.LootList.Remove(_loot);
    }
    // Update is called once per frame
    void Update()
    {
        if (IsInit)
        {
            InActivityOutSideEnemysAndLoots();
            UpdateCurPlate();
            if (SpawnEnemyTimer!=null)
                SpawnEnemyTimer.RunTimer();
            if (SpawnLootTimer!=null)
                SpawnLootTimer.RunTimer();
        }
        else if (Player.IsInit)
            Init();
    }
    void InActivityOutSideEnemysAndLoots()
    {
        DestructMargin_Left = (MyCameraControler.transform.position.x - (ScreenSize.x / 2 + 200));
        DestructMargin_Right = (MyCameraControler.transform.position.x + (ScreenSize.x / 2 + 200));
        //Enemys
        for (int i = 0; i < EnemyList.Count; i++)
        {
            if (EnemyList[i] == null)
                EnemyList.RemoveAt(i);
            else
            {
                if (EnemyList[i].Type != EnemyType.Demogorgon)
                {
                    if (EnemyList[i].transform.position.x < DestructMargin_Left ||
    EnemyList[i].transform.position.x > DestructMargin_Right)
                    {
                        EnemyList[i].gameObject.SetActive(false);
                    }
                    else
                    {
                        EnemyList[i].gameObject.SetActive(true);
                    }
                }
            }
        }
        //Loots
        for (int i = 0; i < LootList.Count; i++)
        {
            if (LootList[i] == null)
                LootList.RemoveAt(i);
            else
            {
                if (LootList[i].transform.position.x < DestructMargin_Left ||
LootList[i].transform.position.x > DestructMargin_Right)
                {
                    LootList[i].gameObject.SetActive(false);
                }
                else
                    LootList[i].gameObject.SetActive(true);
            }
        }
    }
}
