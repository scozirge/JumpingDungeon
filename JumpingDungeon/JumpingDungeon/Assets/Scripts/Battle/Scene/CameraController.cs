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
    Vector3 Offset;         //Private variable to store the offset distance between the player and camera
    float FaceOffsetX;
    // Use this for initialization
    void Start()
    {
        //Calculate and store the offset value by getting the distance between the player's position and camera's position.
        Offset = transform.position - Player.transform.position;
        FaceOffsetX = Mathf.Abs(Player.transform.position.x);
    }
    // LateUpdate is called after Update each frame
    void FixedUpdate()
    {
        if (!Player)
            return;
        // Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
        if (FaceOffset)
        {
            Vector3 faceOffset = new Vector3(Player.FaceDir * FaceOffsetX, Offset.y, Offset.z);
            //Debug.Log(FaceOffsetX);
            //Debug.Log(new Vector3(FaceOffsetX, Offset.y, Offset.z));
            transform.position = Vector3.Lerp(transform.position, new Vector3(Player.transform.position.x, 0, 0) + faceOffset, LerpFactor);
        }
        else
            transform.position = Vector3.Lerp(transform.position, new Vector3(Player.transform.position.x, 0, 0) + Offset, LerpFactor);
    }

}
