﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public partial class ServerRequest : MonoBehaviour
{

    public static ServerRequest Conn;
    const string TestServerURL = "127.0.0.1/30SecHero/";
    const string ServerURL = "https://30sechero.000webhostapp.com/";
    static bool IsFormal;
    static bool ShowLoading = false;//是否顯示loading
    static bool ShowCBLog = false;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                Debug.Log("Android");
                IsFormal = true;
                break;
            case RuntimePlatform.IPhonePlayer:
                Debug.Log("IPhonePlayer");
                IsFormal = true;
                break;
            case RuntimePlatform.WindowsEditor:
                Debug.Log("WindowsEditor");
                IsFormal = false;
                break;
            case RuntimePlatform.OSXEditor:
                Debug.Log("OSXEditor");
                IsFormal = false;
                break;
            default:
                IsFormal = true;
                break;
        }
        //IsFormal = true;
        Conn = this;
        //切場景不移除物件
        DontDestroyOnLoad(gameObject);
    }
    public static string GetServerURL()
    {
        if (IsFormal)
        {
            return ServerURL;
        }
        else
            return TestServerURL;
    }
}
