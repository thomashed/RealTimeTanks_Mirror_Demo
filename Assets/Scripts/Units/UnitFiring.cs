using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{

    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    private float lastFireTime;

    // we want only server to call the Update
    [ServerCallback]
    private void Update()
    {
        if (targeter.Target is null) return;

        if (!CanFireAtTarget()) return;
        // we're in range of our target
        // now for keep rotating towars our target
        Quaternion targetRotation = Quaternion.LookRotation(targeter.Target.transform.position - transform.position); // will give us a vector pointing towards our target
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        if (Time.time > (1 / fireRate) + lastFireTime)
        {
            Quaternion projectileRotation = Quaternion.LookRotation(targeter.Target.GetAimAtPoint().position - projectileSpawnPoint.position); // control projectile rotation 
            GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation);
            NetworkServer.Spawn(projectileInstance, connectionToClient);
            lastFireTime = Time.time;
        } 
    }

    [Server]
    public bool CanFireAtTarget()
    {
        return (targeter.Target.transform.position - transform.position).sqrMagnitude <= (fireRange * fireRange) ;
    }

}
