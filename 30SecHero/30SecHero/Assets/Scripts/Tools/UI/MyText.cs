﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class MyText : Text
{
    public string UIString;
    public bool IsAddTextList;
    public static List<MyText> MyTextList = new List<MyText>();
    public delegate void MyFunction();
    public MyFunction MyTextFunc;
    static List<MyFunction> MyFuncList = new List<MyFunction>();

    protected override void OnEnable()
    {
        base.OnEnable();
        if (!Application.isPlaying)
            return;
        if (!IsAddTextList)
            MyTextList.Add(this);
        SetText();
        IsAddTextList = true;
    }
    void SetText()
    {
        if (UIString == "" || !GameDictionary.IsInit)
            return;
        if (GameDictionary.String_UIDic.ContainsKey(UIString))
        {
            text = GameDictionary.String_UIDic[UIString].GetString(Player.UseLanguage);
        }
        else
        {
            text = "undefined";
        }
    }
    ~MyText()
    {
        MyFuncList.Remove(MyTextFunc);
        MyTextList.Remove(this);
    }
    public static void RefreshActivityTexts()
    {
        for (int i = 0; i < MyTextList.Count; i++)
        {
            if (MyTextList[i] != null && MyTextList[i].isActiveAndEnabled)
                MyTextList[i].SetText();
        }
        for (int i = 0; i < MyFuncList.Count; i++)
        {
            if (MyFuncList[i] != null)
                MyFuncList[i]();
        }
    }
    public static void AddRefreshFunc(MyFunction _func)
    {
        MyFuncList.Add(_func);
    }

}