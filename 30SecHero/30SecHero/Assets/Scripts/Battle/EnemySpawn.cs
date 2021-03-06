﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattleManage
{
    bool CheckSelfDestinationCloseToOtherEnemys(Vector2 _pos)
    {
        if (EnemyList.Count == 0)
            return false;
        bool result = false;
        for (int i = 0; i < EnemyList.Count; i++)
        {
            if (!EnemyList[i].isActiveAndEnabled)
                continue;
            AIRoleMove aiMove=EnemyList[i].GetComponent<AIRoleMove>();
            float dist = float.MaxValue;
            if(aiMove==null)
            {
                dist = Vector2.Distance(_pos, EnemyList[i].transform.position);
            }
            else
            {
                dist = Vector2.Distance(_pos, aiMove.Destination);
            }
            if (dist < EnemyDistance)
                return true;
        }
        return result;
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
    void SpawnDemogorgon(int _type)
    {
        if(_type==1)
        {
            if(NextDemonGergons!=null)
            {
                EnemyRole er = Instantiate(NextDemonGergons, Vector3.zero, Quaternion.identity) as EnemyRole;
                er.SetEnemyData(GameDictionary.EnemyDic[NextDemonGergons.ID]);
                //Set SpawnPos
                er.transform.SetParent(EnemyParent);
                er.transform.position = GetSpawnPos(new Vector2(600, 0));
                er.SetStemFromFloor(Floor);
                AddEnemy(er);

                NextDemonGergons = EnemyData.GetNextDemogorgon(Floor + 1, out NextDemogorgonFloor);
            }
        }
        else if(_type==2)
        {
            if(PreviousDemonGergons!=null)
            {
                EnemyRole er = Instantiate(PreviousDemonGergons, Vector3.zero, Quaternion.identity) as EnemyRole;
                er.SetEnemyData(GameDictionary.EnemyDic[PreviousDemonGergons.ID]);
                //Set SpawnPos
                er.transform.SetParent(EnemyParent);
                er.transform.position = GetSpawnPos(new Vector2(-600, 0));
                er.SetStemFromFloor(Floor);
                AddEnemy(er);

                PreviousDemonGergons = EnemyData.GetPreviousDemogorgon(Floor - 1, out PreviousDemogorgonFloor);
            }
        }
        //播放BOSS出場警告動畫與背景音樂
        if (WarningAni != null)
        {
            AudioPlayer.FadeOutMusic("Battle", 1f);
            AudioPlayer.FadeInMusic(GameManager.GM.BossFightMusic, "BossFight", 2f, BattleManage.BM.MyPlayer.GetBGMVolume());
            WarningAni.gameObject.SetActive(true);
            WarningAni.Play(0);
        }
        //Debug.Log("NextDemogorgonFloor=" + NextDemogorgonFloor);
        IsDemogorgonFloor = 0;
    }
    void SpanwEnemy()
    {
        if (IsPause)
            return;
        SpawnForeAndBackEnemy();//預產螢幕外的怪物
        if (CheckActiveEnemyReachLimit())
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
        CurSpawnCount++;
        er.transform.SetParent(EnemyParent);
        er.SetStemFromFloor(Floor);
        AIRoleMove ai = er.GetComponent<AIRoleMove>();
        Vector2 offset = Vector2.zero;
        for (int i = 0; i < MaxRefindTimes; i++)
        {
            offset = ai.SetRandDestination();
            if (!CheckSelfDestinationCloseToOtherEnemys(ai.Destination))
                break;
        }
        Vector2 spawnPos = GetSpawnPos(offset);
        er.transform.position = spawnPos;
        if (CurSpawnCount < GetRandomEnemySpawnfCount())
            StartCoroutine(WaitToSpawnEnemy());
        else
        {
            CurSpawnCount = 0;
            SpawnEnemyTimer.StartRunTimer = true;
            UpdateSpawnEnmeyTimer();
        }
        AddEnemy(er);
    }
    void SpawnForeAndBackEnemy()
    {
        int needForeEnemy = CheckForeEnemyNeedPawnCount();
        int needBackEnemy = CheckBackEnemyNeedPawnCount();
        //Debug.Log("needForeEnemy=" + needForeEnemy);
        //Debug.Log("needBackEnemy=" + needBackEnemy);
        SpawnOutSideEnemy(true, needForeEnemy);
        SpawnOutSideEnemy(false, needBackEnemy);
    }
    void SpawnOutSideEnemy(bool _fore, int _count)
    {
        for (int j = 0; j < _count; j++)
        {
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
            er.transform.SetParent(EnemyParent);
            AIRoleMove ai = er.GetComponent<AIRoleMove>();
            Vector2 offset = Vector2.zero;
            for (int i = 0; i < MaxRefindTimes; i++)
            {
                ai.SetRandOutSideDestination(_fore);
                if (!CheckSelfDestinationCloseToOtherEnemys(ai.Destination))
                    break;
            }
            er.transform.position = ai.Destination;
            AddEnemy(er);
        }
    }
    Vector2 GetSpawnPos(Vector2 _offset)
    {
        Vector2 pos1 = new Vector2(_offset.x, ScreenSize.y / 2);
        Vector2 pos2 = new Vector2(_offset.x, -ScreenSize.y / 2);
        Vector2 pos3 = new Vector2(ScreenSize.x / 2, _offset.y);
        Vector2 pos4 = new Vector2(-ScreenSize.x / 2, _offset.y);
        List<Vector2> posList = new List<Vector2>();
        posList.Add(pos1);
        posList.Add(pos2);
        posList.Add(pos3);
        posList.Add(pos4);
        Vector3 resultPos = Vector2.zero;
        float curDist = float.MaxValue;
        //Debug.Log("_offset=" + _offset);
        for (int i = 0; i < posList.Count; i++)
        {
            //Debug.Log("i=" + i + "=" + posList[i]);
            if (Vector2.Distance(_offset, posList[i]) < curDist)
            {
                curDist = Vector2.Distance(_offset, posList[i]);
                resultPos = posList[i];
            }
        }
        //Debug.Log("resultPos="+resultPos);
        resultPos += MyCameraControler.transform.position;
        return resultPos;
    }
    void UpdateSpawnEnmeyTimer()
    {
        //每次出怪後重新確認出怪時間
        if (IsFirstHalf)
            SpawnEnemyTimer.ResetMaxTime(EnemyFirstHalfInterval);
        else
            SpawnEnemyTimer.ResetMaxTime(EnemySecondHalfInterval);
    }
    bool CheckActiveEnemyReachLimit()
    {
        if (MaxEnemy == 0)
            return true;
        int count = 0;
        for (int i = 0; i < EnemyList.Count; i++)
        {
            if (EnemyList[i]!=null)
            {
                if (EnemyList[i].isActiveAndEnabled)
                    count++;
            }
        }
        if (count < MaxEnemy)
            return false;
        return true;
    }
    int CheckForeEnemyNeedPawnCount()
    {
        if (!MyPlayer)
            return 0;
        if (MaxForeEnemy == 0)
            return 0;
        int count = 0;
        for (int i = 0; i < EnemyList.Count; i++)
        {
            if (!EnemyList[i].isActiveAndEnabled && EnemyList[i].transform.position.x > MyPlayer.transform.position.x)
                count++;
        }
        if (count < MaxForeEnemy)
            return MaxForeEnemy - count;
        return 0;
    }
    int CheckBackEnemyNeedPawnCount()
    {
        if (!MyPlayer)
            return 0;
        if (MaxBackEnemy == 0)
            return 0;
        int count = 0;
        for (int i = 0; i < EnemyList.Count; i++)
        {
            if (!EnemyList[i].isActiveAndEnabled && EnemyList[i].transform.position.x < MyPlayer.transform.position.x)
                count++;
        }
        if (count < MaxBackEnemy)
            return MaxBackEnemy - count;
        return 0;
    }
    IEnumerator WaitToSpawnEnemy()
    {
        yield return new WaitForSeconds(EnemySpawnInterval);
        SpanwEnemy();
    }
}
