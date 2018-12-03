﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
public class EnchantData : Data
{
    public string Name
    {
        get
        {
            if (!GameDictionary.String_EnchantDic.ContainsKey(ID.ToString()))
            {
                Debug.LogWarning(string.Format("{0}表不包含{1}的文字資料", DataName, ID));
                return "NullText";
            }
            return GameDictionary.String_EnchantDic[ID.ToString()].GetString(0, Player.UseLanguage);
        }
        private set { return; }
    }
    public string Description(int _offset)
    {
        if (!GameDictionary.String_EnchantDic.ContainsKey(ID.ToString()))
        {
            Debug.LogWarning(string.Format("{0}表不包含{1}的文字資料", DataName, ID));
            return "NullText";
        }
        string valueString = "";
        if (MyEnchantType == EnchantType.Enchant)
        {
            if (ShowPercentage)
                valueString = string.Format("{0}{1}", TextManager.ToPercent(BaseValue + (LV + _offset) * LevelUpValue).ToString("0.0"), "%");
            else
                valueString = string.Format("{0}", BaseValue + (LV + _offset) * LevelUpValue);
        }
        else
        {
            if (ShowPercentage)
                valueString = string.Format("{0}{1}", TextManager.ToPercent(GetValue()).ToString("0.0"), "%");
            else
                valueString = string.Format("{0}", GetValue());
        }
        return string.Format(GameDictionary.String_EnchantDic[ID.ToString()].GetString(1, Player.UseLanguage), valueString);
    }
    bool ShowPercentage;
    public EnchantProperty PropertyType;
    public EnchantType MyEnchantType;
    public Dictionary<EnchantProperty, float> Properties = new Dictionary<EnchantProperty, float>();
    public float BaseValue;
    public float LevelUpValue;
    public int BaseEmerald;
    public int LevelUpEmerald;
    public int MaxLevel;
    public string IconString;
    public int LV { get; private set; }
    static List<int> WeaponEnchantList = new List<int>();
    static List<int> ArmorEnchantList = new List<int>();

    public string GetLVString(int _plus)
    {
        return string.Format("{0}{1}", StringData.GetString("LV"), LV + _plus);
    }
    /// <summary>
    /// 將字典傳入，依json表設定資料
    /// </summary>
    public static void SetData(Dictionary<int, EnchantData> _dic, string _dataName)
    {
        string jsonStr = Resources.Load<TextAsset>(string.Format("Json/{0}", _dataName)).ToString();
        JsonData jd = JsonMapper.ToObject(jsonStr);
        JsonData items = jd[_dataName];
        for (int i = 0; i < items.Count; i++)
        {
            EnchantData data = new EnchantData(items[i]);
            data.DataName = _dataName;
            data.Properties.Add(data.PropertyType, 0);
            int id = int.Parse(items[i]["ID"].ToString());
            _dic.Add(id, data);
        }
    }
    EnchantData(JsonData _item)
    {
        try
        {
            JsonData item = _item;
            foreach (string key in item.Keys)
            {
                switch (key)
                {
                    case "ID":
                        ID = int.Parse(item[key].ToString());
                        break;
                    case "EnchantType":
                        PropertyType = (EnchantProperty)Enum.Parse(typeof(EnchantProperty), item[key].ToString());
                        break;
                    case "BaseValue":
                        BaseValue = float.Parse(item[key].ToString());
                        break;
                    case "LevelUpValue":
                        LevelUpValue = float.Parse(item[key].ToString());
                        break;
                    case "ShowPercentage":
                        if (bool.Parse(item[key].ToString()))
                            ShowPercentage = true;
                        break;
                    case "BaseEmerald":
                        BaseEmerald = int.Parse(item[key].ToString());
                        break;
                    case "LevelUpEmerald":
                        LevelUpEmerald = int.Parse(item[key].ToString());
                        break;
                    case "MaxLevel":
                        MaxLevel = int.Parse(item[key].ToString());
                        break;
                    case "Icon":
                        IconString = item[key].ToString();
                        break;
                    case "Type":
                        MyEnchantType = MyEnum.ParseEnum<EnchantType>(item[key].ToString());
                        if (MyEnchantType == EnchantType.Weapon)
                            WeaponEnchantList.Add(ID);
                        else if (MyEnchantType == EnchantType.Armor)
                            ArmorEnchantList.Add(ID);
                        break;
                    default:
                        Debug.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    public static int GetRandomEquipEnchant(EnchantType _type)
    {
        int enchantID = 0;
        if (_type == EnchantType.Weapon)
        {
            enchantID = WeaponEnchantList[UnityEngine.Random.Range(0, WeaponEnchantList.Count)];
        }
        else if (_type == EnchantType.Armor)
        {
            enchantID = ArmorEnchantList[UnityEngine.Random.Range(0, ArmorEnchantList.Count)];
        }
        return enchantID;
    }
    public void InitSet(int _lv)
    {
        if (_lv < 0)
            return;
        LV = _lv;
        Properties[PropertyType] = GetValue();
    }
    public void LVUP()
    {
        LV++;
        Properties[PropertyType] = GetValue();
    }
    public bool CanUpgrade()
    {
        if (LV >= MaxLevel)
            return false;
        else
            return true;
    }
    public int GetPrice()
    {
        return BaseEmerald + LV * LevelUpEmerald;
    }
    public float GetValue()
    {
        if (MyEnchantType == EnchantType.Enchant)
        {
            if (LV > 0)
                return BaseValue + (LV - 1) * LevelUpValue;
            else
                return 0;
        }
        else
        {
            return BaseValue;
        }
    }
    public Sprite GetICON()
    {
        return Resources.Load<Sprite>(string.Format(GameSettingData.EnchantPath, IconString));
    }
    public static Dictionary<int, EnchantData> GetNewEnchantDic(int _lv)
    {
        Dictionary<int, EnchantData> dic = new Dictionary<int, EnchantData>();
        List<int> keys = new List<int>(GameDictionary.EnchantDic.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            dic.Add(keys[i], GetNewEnchantData(keys[i], _lv));
        }
        return dic;
    }
    public static EnchantData GetNewEnchantData(int _id, int _lv)
    {
        EnchantData data = GameDictionary.EnchantDic[_id].MemberwiseClone() as EnchantData;
        data.InitSet(_lv);
        return data;
    }
    public static EnchantData GetAvailableRandomEnchant()
    {
        for (int i = 1; i < 4; i++)
        {
            EnchantData ed = GetRandomEnchant(i);
            if (ed != null)
            {
                return ed;
            }
        }
        return null;
    }
    static EnchantData GetRandomEnchant(int _lv)
    {
        List<EnchantData> availableEnchant = new List<EnchantData>();
        List<int> keys = new List<int>(Player.EnchantDic.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            if (Player.EnchantDic[keys[i]].MyEnchantType == EnchantType.Enchant && Player.EnchantDic[keys[i]].LV < _lv)
            {
                availableEnchant.Add(Player.EnchantDic[keys[i]]);
            }
        }
        if (availableEnchant.Count != 0)
        {
            int random = UnityEngine.Random.Range(0, availableEnchant.Count);
            return availableEnchant[random];
        }
        else
        {
            return null;
        }
    }
}
