using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{

    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private float destroyAfterSeconds = 5f;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private int damageToDeal = 20;


    void Start()
    {
        rb.velocity = transform.forward * launchForce;
         
    }

    void Update()
    {
        
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destroyAfterSeconds);
    }

    private void OnTriggerEnter(Collider other) // we only want this to run on server, client shouldn't care about collision 
    {
        if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity)) // does whatever we collided with belong to us?
        {
            if (networkIdentity.connectionToClient == connectionToClient) return;
        }

        // does what we hit have a health component? If so, we wanna deal damage
        if (other.TryGetComponent<Health>(out var health))
        {
            health.DealDamage(damageToDeal);
        }

        DestroySelf();
    }

    [Server]
    private void DestroySelf() 
    {
        NetworkServer.Destroy(gameObject);
    }

}
