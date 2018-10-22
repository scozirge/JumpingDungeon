﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;

public class AccessoryData : EquipData
{
    protected static int MaxUID;//只使用本地資料才會用到
    public override EquipType Type { get { return EquipType.Accessory; } }
    public override int SellGold { get { return GameSettingData.GetAccessoryGold(LV, Quality); } }
    /// <summary>
    /// 將字典傳入，依json表設定資料
    /// </summary>
    public static void SetData(Dictionary<int, AccessoryData> _dic, string _dataName)
    {
        DataName = _dataName;
        string jsonStr = Resources.Load<TextAsset>(string.Format("Json/{0}", DataName)).ToString();
        JsonData jd = JsonMapper.ToObject(jsonStr);
        JsonData items = jd[DataName];
        for (int i = 0; i < items.Count; i++)
        {
            AccessoryData data = new AccessoryData(items[i]);
            int id = int.Parse(items[i]["ID"].ToString());
            _dic.Add(id, data);
        }
    }
    public override string Name
    {
        get
        {
            if (!GameDictionary.String_AccessoryDic.ContainsKey(ID.ToString()))
            {
                Debug.LogWarning(string.Format("{0}表不包含{1}的文字資料", DataName, ID));
                return "NullText";
            }
            return GameDictionary.String_AccessoryDic[ID.ToString()].GetString(Player.UseLanguage);
        }
        protected set { return; }
    }
    public int Gold
    {
        get
        {
            return GameSettingData.GetAccessoryGold(LV, Quality);
        }
    }
    protected override void SetRandomProperties()
    {
        base.SetRandomProperties();
        Properties[RoleProperty.Shield] += GameSettingData.GetAccessoryShield(LV);
    }
    public AccessoryData(JsonData _item)
        : base(_item)
    {
    }
    static int GetRandomID()
    {
        List<int> keys = new List<int>(GameDictionary.AccessoryDic.Keys);
        int randIndex = UnityEngine.Random.Range(0, keys.Count);
        return keys[randIndex];
    }
    public override int SetUID()
    {
        base.SetUID();
        MaxUID++;
        UID = MaxUID;
        //Debug.Log("Type=" + Type + "  UID=" + UID);
        return UID;
    }
    public static AccessoryData GetRandomNewAccessory(int _lv, int _quality)
    {
        AccessoryData data = GameDictionary.AccessoryDic[GetRandomID()].MemberwiseClone() as AccessoryData;
        data.SetUID();
        data.LV = _lv;
        data.Quality = _quality;
        data.SetRandomProperties();
        return data;
    }
    public static AccessoryData GetNewAccessory(int _uid, int _id, int _equipSlot, int _lv, int _quality)
    {
        AccessoryData data = GameDictionary.AccessoryDic[_id].MemberwiseClone() as AccessoryData;
        data.UID = _uid;
        if (_uid > MaxUID)
            MaxUID = _uid;
        data.LV = _lv;
        data.Quality = _quality;
        data.SetRandomProperties();
        if (_equipSlot == 3)
            Player.Equip(data, 0);
        else if (_equipSlot == 4)
            Player.Equip(data, 1);
        return data;
    }
    public override void SetEquipStatus(bool _isEquiped, int _equipSlot)
    {
        base.SetEquipStatus(_isEquiped, _equipSlot);
        if (_isEquiped)
            EquipSlot = _equipSlot;
        else
            EquipSlot = 0;
    }
}
