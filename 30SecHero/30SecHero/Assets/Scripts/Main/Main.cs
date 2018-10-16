﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Main : MonoBehaviour
{
    bool Isinit = false;
    [SerializeField]
    Text GoldText;
    [SerializeField]
    Text EmeraldText;
    [SerializeField]
    Strengthen MyStrengthen;
    [SerializeField]
    Equip MyEquip;
    [SerializeField]
    Purchase MyPurchase;
    [SerializeField]
    Set MySet;
    [SerializeField]
    MainUI CurUI;


    static Text MyGoldText;
    static Text MyEmeraldText;

    Dictionary<MainUI, MyUI> UIDic = new Dictionary<MainUI, MyUI>();

    // Use this for initialization
    void Awake()
    {
        if (!GameManager.IsInit)
            GameManager.DeployGameManager();
    }
    void Init()
    {
        if (Isinit)
            return;
        UIDic.Add(MainUI.Strengthen, MyStrengthen);
        UIDic.Add(MainUI.Purchase, MyPurchase);
        UIDic.Add(MainUI.Equip, MyEquip);
        List<MainUI> keys = new List<MainUI>(UIDic.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i] != CurUI)
                UIDic[keys[i]].SetActive(false);
            else
                UIDic[keys[i]].SetActive(true);
        }
        MyGoldText = GoldText;
        MyEmeraldText = EmeraldText;
        Isinit = true;
    }
    void OnEnable()
    {
        if (!Isinit)
            return;
        GoldText.text = Player.Gold.ToString();
        EmeraldText.text = Player.Emerald.ToString();
    }
    void Update()
    {
        if (Player.IsInit)
            Init();
    }
    public static void UpdateResource()
    {
        if (MyGoldText == null)
            return;
        MyGoldText.text = Player.Gold.ToString();
        MyEmeraldText.text = Player.Emerald.ToString();
    }
    public void ChangeUI(int _enumID)
    {
        MainUI type = (MainUI)_enumID;
        ChangeUI(type);
    }
    public void ChangeUI(MainUI _ui)
    {
        if (_ui == CurUI)
            return;
        if (!UIDic.ContainsKey(_ui))
            return;
        UIDic[CurUI].SetActive(false);
        UIDic[_ui].SetActive(true);
        CurUI = _ui;
    }
    public void Set(bool _bool)
    {
        MySet.SetActive(_bool);
    }
    public void Battle()
    {
        ChangeScene.GoToScene(MyScene.Battle);
    }

}
