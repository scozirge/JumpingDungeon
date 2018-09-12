﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAmmo : Ammo
{
    [SerializeField]
    protected float AmmoSpeed;
    [SerializeField]
    protected float TraceFactor;


    protected Vector3 Ammovelocity;
    public override void Init(Dictionary<string, object> _dic)
    {
        base.Init(_dic);
        Target = ((Role)_dic["Target"]);
        Ammovelocity = (Vector3)(_dic["Direction"]) * AmmoSpeed;
        //transform.LookAt(MyRigi.velocity);

        Launch();
    }
    protected override void OnTriggerEnter2D(Collider2D _col)
    {
        if (IsCausedDamage && AmmoType != ShootAmmoType.Penetration)
            return;
        base.OnTriggerEnter2D(_col);
    }
    protected override void OnTriggerStay2D(Collider2D _col)
    {
        if (IsCausedDamage && AmmoType != ShootAmmoType.Penetration)
            return;
        base.OnTriggerStay2D(_col);
    }
    protected override void TriggerTarget(Role _curTarget)
    {
        base.TriggerTarget(_curTarget);
        Vector2 force = (_curTarget.transform.position - transform.position).normalized * KnockIntensity;
        Dictionary<RoleBuffer, BufferData> condition = new Dictionary<RoleBuffer, BufferData>();
        condition.Add(RoleBuffer.Stun, new BufferData(StunIntensity, 0));
        _curTarget.BeAttack(Damage, force, condition);
        IsCausedDamage = true;
        if (AmmoType != ShootAmmoType.Penetration)
            SelfDestroy();
    }
    protected override void Update()
    {
        base.Update();
        if (TraceFactor > 0)
        {
            Vector2 targetVel = (Target.transform.position - transform.position).normalized * AmmoSpeed;
            MyRigi.velocity = Vector2.Lerp(MyRigi.velocity, targetVel, TraceFactor);
        }
    }
    public override void Launch()
    {
        base.Launch();
        MyRigi.velocity = Ammovelocity;
    }
}
