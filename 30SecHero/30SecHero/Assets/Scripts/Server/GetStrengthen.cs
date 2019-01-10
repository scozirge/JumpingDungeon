﻿using UnityEngine;
using System.Collections;
using System;


public partial class ServerRequest : MonoBehaviour
{
    public static bool WaitCB_GetStrengthen { get; private set; }
    //需求重送要求給Server次數
    static byte ReSendQuestTimes_GetStrengthen { get; set; }
    //每次需求最大重送次數
    const byte MaxReSendQuestTimes_GetStrengthen = 3;

    public static void GetStrengthen()
    {
        ReSendQuestTimes_GetStrengthen = MaxReSendQuestTimes_GetStrengthen;//重置重送要求給Server的次數
        SendGetStrengthenQuest();
    }
    static void SendGetStrengthenQuest()
    {
        if (Conn == null)
            return;
        WWWForm form = new WWWForm();
        //string requestTime = DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss");//命令時間，格式2015-11-25 15:39:36
        form.AddField("ownUserID", Player.ID);
        WWW w = new WWW(string.Format("{0}{1}", GetServerURL(), "GetStrengthen.php"), form);
        //設定為正等待伺服器回傳
        WaitCB_GetStrengthen = true;
        Conn.StartCoroutine(Coroutine_GetStrengthenCB(w));
        Conn.StartCoroutine(GetStrengthenTimeOutHandle(2f, 0.5f, 12));
    }
    /// <summary>
    /// 回傳
    /// </summary>
    static IEnumerator Coroutine_GetStrengthenCB(WWW w)
    {
        if (ReSendQuestTimes_GetStrengthen == MaxReSendQuestTimes_GetStrengthen)
            if (ShowLoading) CaseTableData.ShowPopLog(1003);//帳號建立中
        yield return w;
        if (ShowCBLog)
            Debug.LogWarning(w.text);
        if (WaitCB_GetStrengthen)
        {
            WaitCB_GetStrengthen = false;
            if (w.error == null)
            {
                try
                {
                    string[] result = w.text.Split(':');
                    //////////////////成功////////////////
                    if (result[0] == ServerCBCode.Success.ToString())
                    {
                        if (result[1] != "")
                        {
                            string[] data = result[1].Split('/');
                            Player.GetStrengthen_CB(data);

                        }
                        else
                        {
                            Player.GetStrengthen_CB(null);
                        }
                        PopupUI.HideLoading();//隱藏Loading
                    }
                    //////////////////失敗///////////////
                    else if (result[0] == ServerCBCode.Fail.ToString())
                    {
                        int caseID = int.Parse(result[1]);
                        if (ShowLoading) CaseTableData.ShowPopLog(caseID);
                        PopupUI.HideLoading();//隱藏Loading
                    }
                    else
                    {
                        if (ShowLoading) CaseTableData.ShowPopLog(6);//錯誤的命令
                        PopupUI.HideLoading();//隱藏Loading
                    }
                }
                //////////////////例外//////////////////
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    if (ShowLoading) CaseTableData.ShowPopLog(6);//錯誤的命令
                    PopupUI.HideLoading();//隱藏Loading
                }
            }
            //////////////////回傳null////////////////
            else
            {
                Debug.LogWarning(w.error);
                if (ShowLoading) CaseTableData.ShowPopLog(2);//連線不到server
                PopupUI.HideLoading();//隱藏Loading
            }
        }
    }
    static IEnumerator GetStrengthenTimeOutHandle(float _firstWaitTime, float _perWaitTime, byte _checkTimes)
    {
        yield return new WaitForSeconds(_firstWaitTime);
        byte checkTimes = _checkTimes;
        //經過_fristWaitTime時間後，每_perWaitTime檢查一次資料是否回傳了，若檢查checkTimes次數後還是沒回傳就重送資料
        while (WaitCB_GetStrengthen && checkTimes > 0)
        {
            checkTimes--;
            yield return new WaitForSeconds(_perWaitTime);
        }
        if (WaitCB_GetStrengthen)//如果還沒接收到CB就重送需求
        {
            //若重送要求的次數達到上限次數則代表連線有嚴重問題，直接報錯
            if (ReSendQuestTimes_GetStrengthen > 0)
            {
                ReSendQuestTimes_GetStrengthen--;
                if (ShowLoading) CaseTableData.ShowPopLog(1002);//連線逾時，嘗試重複連線請玩家稍待
                //向Server重送要求
                SendGetStrengthenQuest();
            }
            else
            {
                WaitCB_GetStrengthen = false;//設定為false代表不接受回傳了
                if (ShowLoading) CaseTableData.ShowPopLog(7); ;//連線逾時，請檢查網路是否正常
                PopupUI.HideLoading();//隱藏Loading
            }
        }
    }
}
