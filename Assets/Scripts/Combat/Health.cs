using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : NetworkBehaviour
{

    [SerializeField] private int maxHealth = 100;
    [SyncVar(hook=nameof(HandleHealthUpdated))] private int currentHealth;

    public event Action ServerOnDie;
    public event Action<int, int> ClientOnHealthUpdated;

    #region Server
    public override void OnStartServer()
    {
        currentHealth = maxHealth;
        UnitBase.ServerOnPlayerDie += ServerHandleOnPlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandleOnPlayerDie;
    }

    [Server]
    public void DealDamage(int damageAmount) 
    {
        if (currentHealth == 0) return;
        currentHealth = Mathf.Max((currentHealth - damageAmount), 0);
        if (currentHealth != 0) return;
        ServerOnDie?.Invoke();
    }

    [Server]
    private void ServerHandleOnPlayerDie(int playerId)
    {
        // if id matches the owner of this object, destroy it
        if (connectionToClient.connectionId != playerId) return;
        DealDamage(currentHealth);

    }

    #endregion

    #region Client
    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }

    #endregion

}
