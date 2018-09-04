﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectEmitter : MonoBehaviour
{
    static Transform MySelf;
    void Awake()
    {
        MySelf = transform;
    }
    public static void EmitParticle(string _effectName, Vector3 _pos, Vector3 _dir, Transform _parent)
    {
        GameObject particlePrefab = Resources.Load<ParticleSystem>(string.Format("Particles/{0}/{0}", _effectName)).gameObject;
        if (particlePrefab == null)
        {
            Debug.LogWarning("No particle prefab are assigned:" + string.Format("Particles/{0}/{0}", _effectName));
            return;
        }
        GameObject particleGo = Instantiate(particlePrefab.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
        if (_parent)
            particleGo.transform.SetParent(_parent);
        else
            particleGo.transform.SetParent(MySelf);

        particleGo.transform.localPosition = _pos;
        particleGo.transform.localRotation = Quaternion.Euler(_dir);
    }
    public static void EmitParticle(ParticleSystem _particle, Vector3 _pos, Vector3 _dir, Transform _parent)
    {
        GameObject particleGo = Instantiate(_particle.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
        if (_parent)
            particleGo.transform.SetParent(_parent);
        else
            particleGo.transform.SetParent(MySelf);

        particleGo.transform.localPosition = _pos;
        particleGo.transform.localRotation = Quaternion.Euler(_dir);
    }

}
