﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

partial class BattleManage
{
    [SerializeField]
    GameObject SettlementObj;
    [SerializeField]
    Text FloorClearText;
    [SerializeField]
    Text MaxFloorText;
    [SerializeField]
    Text MonsterKillsText;
    [SerializeField]
    Text BossKillsText;
    [SerializeField]
    Text UnlockPartnerText;
    [SerializeField]
    Text GoldText;
    [SerializeField]
    Text EmeraldText;
    [SerializeField]
    ItemSpawner MySpanwer;
    [SerializeField]
    float CallSettlementTime;
    [SerializeField]
    RunAnimatedText MyRunText;



    //結算資料
    static int NewFloorGolds;
    static int EnemyKillGolds;
    static int EnemyDropGolds;
    static int ExtraDropGolds;
    static float GoldsMultiple;
    static int BossDropEmeralds;
    static int PassFloorCount;
    static int PassNewFloorCount;
    static int MaxFloor;
    public static int EnemyKill;
    static int BossKill;
    static int UnlockPartner;

    static int TotalGold;
    static int TotalEmerald;
    public static List<EquipData> ExpectEquipDataList;
    static List<EquipData> GainEquipDataList;

    //Tip標示
    [SerializeField]
    GameObject StrengthenTip;
    static bool GainWeapon;
    static bool GainArmor;
    static bool GainAccessory;
    static bool GetEnchant;
    static bool ToStrengthen;

    static void InitSettlement()
    {
        NewFloorGolds = 0;
        EnemyKillGolds = 0;
        EnemyDropGolds = 0;
        ExtraDropGolds = 0;
        GoldsMultiple = 0;
        BossDropEmeralds = 0;
        PassFloorCount = 0;
        MaxFloor = 0;


        TotalGold = 0;
        TotalEmerald = 0;
        GainEquipDataList = new List<EquipData>();
        ExpectEquipDataList = new List<EquipData>();
        GainWeapon = false;
        GainArmor = false;
        GainAccessory = false;
        GetEnchant = false;
        ToStrengthen = false;
        IsCalculateResult = false;
    }

    public static void EnemyDropGoldAdd(int _gold)
    {
        AudioPlayer.PlaySound(GameManager.GM.CoinSound);
        EnemyDropGolds += _gold;
        //Debug.Log("_gold=" + _gold);
        //Debug.Log("EnemyKillGolds=" + EnemyKillGolds);
    }
    public static void ExtraDropGoldAdd(int _gold)
    {
        AudioPlayer.PlaySound(GameManager.GM.CoinSound);
        ExtraDropGolds += _gold;
    }
    public static void BossDropEmeraldAdd(int _emerald)
    {
        AudioPlayer.PlaySound(GameManager.GM.CoinSound);
        BossDropEmeralds += _emerald;
        //Debug.Log("_emerald=" + _emerald);
        //Debug.Log("BossDropEmeralds=" + BossDropEmeralds);
    }
    public static void GainEquip(EquipData _data)
    {
        for (int i = 0; i < GainEquipDataList.Count;i++ )
        {
            if (_data.UID == GainEquipDataList[i].UID)
                return;
        }
        if (_data.Type == EquipType.Weapon)
            GainWeapon = true;
        if (_data.Type == EquipType.Armor)
            GainArmor = true;
        if (_data.Type == EquipType.Accessory)
            GainAccessory = true;
        GainEquipDataList.Add(_data);
        //ExpectEquipDataList.Add(_data);//改成沒通關還是會獲得裝備

        //Debug.Log("UID=" + _data.UID);
        //Debug.Log("LV=" + _data.LV);
        //Debug.Log("Quaility=" + _data.Quality);
    }
    public static void TransferToGainEquipDataList()
    {
        for (int i = 0; i < ExpectEquipDataList.Count; i++)
        {
            GainEquipDataList.Add(ExpectEquipDataList[i]);
        }
        ExpectEquipDataList = new List<EquipData>();
    }
    static bool IsCalculateResult;
    public void CalculateResult()
    {
        if (IsCalculateResult)
            return;
        IsCalculateResult = true;
        //獎勵計算
        if (MaxFloor > Player.MaxFloor)
        {
            PassNewFloorCount = MaxFloor - Player.MaxFloor;
        }
        //尋寶專家(根據撞碰的城門增加獲得金幣百分比)
        GoldsMultiple = PassFloorCount * MyPlayer.MyEnchant[EnchantProperty.TreasureHunting];
        MaxFloor = (MaxFloor > Player.MaxFloor) ? MaxFloor : Player.MaxFloor;
        NewFloorGolds = (PassFloorCount * GameSettingData.FloorPassGold * Floor) + (PassNewFloorCount * GameSettingData.NewFloorPassGold * Floor);
        EnemyKillGolds = EnemyKill * GameSettingData.EnemyGold;
        TotalGold = (int)((NewFloorGolds + EnemyKillGolds + EnemyDropGolds + ExtraDropGolds) * (1 + GoldsMultiple));
        TotalEmerald = BossDropEmeralds;
        Debug.Log("GainEquipDataList.Count=" + GainEquipDataList.Count);
        //寫入資料
        if (Player.LocalData)
        {
            //敵人擊殺
            if (EnemyKill > Player.MaxEnemyKills)
                Player.SetMaxEnemyKills_Local(EnemyKill);
            //目前樓層
            Player.SetCurFloor_Local(Floor);
            //最高樓層
            Player.SetMaxFloor_Local(MaxFloor);
            //金幣獲得
            Player.GainGold(TotalGold);
            //寶石獲得
            Player.GainEmerald(TotalEmerald);
            //裝備獲得
            Player.GainEquip_Local(GainEquipDataList);
            //顯示結果
            StartCoroutine(WaitToShowResult());
        }
        else
        {
            //送server處理
            Player.Settlement(Player.Gold + TotalGold, Player.Emerald + TotalEmerald, Floor, (MaxFloor > Player.MaxFloor) ? MaxFloor : Player.MaxFloor, GainEquipDataList);
        }
        //設定驚嘆號tip顯示
        Main.ResetTipBool();
        if(GainWeapon || GainArmor || GainAccessory)
        {
            Main.ShowEquipBtnTip = true;
            if (GainWeapon)
                Main.ShowWeaponTagTip = true;
            if (GainArmor)
                Main.ShowArmorTagTip = true;
            if (GainAccessory)
                Main.ShowAccessoryTagTip = true;
            ToStrengthen = true;
        }
        if(Player.CanStrengthenTipChack())
        {
            Main.ShowStrengthenTagTip = true;
            ToStrengthen = true;
        }
        if(GetEnchant || Player.CanEnchantTipCheck())
        {
            Main.ShowEnchantTagTip = true;
            ToStrengthen = true;
        }
        StrengthenTip.SetActive(ToStrengthen);
    }
    public IEnumerator WaitToShowResult()
    {
        yield return new WaitForSeconds(CallSettlementTime);
        ShowResult();
    }
    void ShowResult()
    {
        AudioPlayer.StopAllMusic();
        AudioPlayer.PlayMusicByAudioClip_Static(GameManager.GM.SettlementMusic);
        //顯示介面
        SettlementObj.SetActive(true);
        IsPause = true;
        SoulGo.SetActive(false);
        MyCameraControler.enabled = false;
        SceneObject.SetActive(false);
        //顯示資料
        SpawnEquipItem();
        FloorClearText.text = PassFloorCount.ToString();
        //MaxFloorText.text = Player.MaxFloor.ToString();
        MonsterKillsText.text = EnemyKill.ToString();
        BossKillsText.text = BossKill.ToString();
        UnlockPartnerText.text = UnlockPartner.ToString();
        GoldText.text = TotalGold.ToString();
        EmeraldText.text = TotalEmerald.ToString();
        //設定動畫文字
        MyRunText.Clear();
        MyRunText.SetAnimatedText("FloorClear", 0, PassFloorCount, FloorClearText, "", "");
        MyRunText.SetAnimatedText("MonsterKill", 0, EnemyKill, MonsterKillsText, "", "");
        MyRunText.SetAnimatedText("BossKill", 0, BossKill, BossKillsText, "", "");
        MyRunText.SetAnimatedText("UnlockPartner", 0, UnlockPartner, UnlockPartnerText, "", "");
        MyRunText.SetAnimatedText("Gold", 0, TotalGold, GoldText, "", "");
        MyRunText.SetAnimatedText("Emerald", 0, TotalEmerald, EmeraldText, "", "");

    }
    void SpawnEquipItem()
    {
        for (int i = 0; i < GainEquipDataList.Count; i++)
        {
            EquipItem ei = (EquipItem)MySpanwer.Spawn();
            ei.Set(GainEquipDataList[i], null);
        }
    }
    public void ReTry()
    {
        PopupUI.CallCutScene("Battle");
        //ChangeScene.GoToScene(MyScene.Battle);
    }
    public void BackToMenu()
    {
        PopupUI.CallCutScene("Main");
        //ChangeScene.GoToScene(MyScene.Main);
    }
    public static void AddEnemyKill()
    {
        EnemyKill++;
    }
    public static void AddBossKill()
    {
        BossKill++;
        //播放BOSS擊殺獎勵
        AudioPlayer.FadeOutMusic("BossFight", 0.5f);
        AudioPlayer.FadeInMusic(GameManager.GM.FightMusic1, "Battle", 2f);
        //不屈勇者(擊殺BOSS獲得額外變身秒數)
        BM.MyPlayer.AvatarTimer += BM.MyPlayer.MyEnchant[EnchantProperty.Courage];
        BattleManage.BM.MyPlayer.AddAvarTime(BattleManage.BM.MyPlayer.MyEnchant[EnchantProperty.Courage]);
    }
    public static void AddUnlockPartner()
    {
        UnlockPartner++;
    }
    public static void KillBossAndGetEnchant(int _bossID, EnchantData _ed)
    {
        //擊殺新BOSS
        Player.KillNewBoss(_bossID);
        if (_ed != null)
        {
            GetEnchant = true;
            Player.EnchantUpgrade(_ed);
            BM.GetEnchant_Name.text = _ed.Name;
            BM.GetEnchant_Icon.sprite = _ed.GetICON();
            BM.GetEnchant_Description.text = _ed.Description(0);
            BM.CallGetEnchantUI(true);
        }
    }
}
