﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    public Vector2 ScreenSize;
    [SerializeField]
    PlayerRole Player;       //Public variable to store a reference to the player game object
    [SerializeField]
    bool FaceOffset;
    [SerializeField]
    float LerpFactor;
    static AnimationPlayer MyPlayer;
    Vector3 Offset;         //Private variable to store the offset distance between the player and camera
    [Tooltip("攝影機震動音效")]
    [SerializeField]
    AudioClip ShakeSound;
    [SerializeField]
    List<EffectData> EffectList;

    static AudioClip MyShakeSound;
    static Dictionary<string, EffectData> EffectDic;
    float FaceOffsetX;
    static CameraController MySelf;
    // Use this for initialization
    void Start()
    {
        MySelf = this;
        //Calculate and store the offset value by getting the distance between the player's position and camera's position.
        Offset = transform.position - Player.transform.position;
        FaceOffsetX = Mathf.Abs(Player.transform.position.x);
        MyPlayer = GetComponent<AnimationPlayer>();
        MyShakeSound = ShakeSound;
        EffectDic = new Dictionary<string, EffectData>();
        for (int i = 0; i < EffectList.Count; i++)
        {
            EffectDic.Add(EffectList[i].Name, EffectList[i]);
        }
    }
    // LateUpdate is called after Update each frame
    void FixedUpdate()
    {
        if (!Player)
            return;
        // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
        if (FaceOffset)
        {
            Vector3 faceOffset = new Vector3(Player.FaceLeftOrRight * FaceOffsetX, Offset.y, Offset.z);
            //Debug.Log(FaceOffsetX);
            //Debug.Log(new Vector3(FaceOffsetX, Offset.y, Offset.z));
            transform.position = Vector3.Lerp(transform.position, new Vector3(Player.transform.position.x, 0, 0) + faceOffset, LerpFactor);
        }
        else
            transform.position = Vector3.Lerp(transform.position, new Vector3(Player.transform.position.x, 0, 0) + Offset, LerpFactor);
    }
    public static void PlayMotion(string _motionName)
    {
        if (MyPlayer != null)
            MyPlayer.PlayTrigger(_motionName, 0);
        AudioPlayer.PlaySound(MyShakeSound);
    }
    public static void PlayEffect(string _effectName)
    {
        if (!MySelf)
            return;
        if (EffectDic.ContainsKey(_effectName))
        {
            EffectEmitter.EmitParticle(EffectDic[_effectName].Particle, Vector3.zero, Vector3.zero, MySelf.transform);
        }
    }
}
