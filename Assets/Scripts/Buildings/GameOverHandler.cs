using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action ServerOnGameOver;
    public static event Action<string> ClientOnGameOver;
    private List<UnitBase> bases = new List<UnitBase>();

    #region Server
    public override void OnStartServer()
    {
        base.OnStartServer();
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
    }

    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }

    [Server]
    private void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        bases.Remove(unitBase);
        if (bases.Count != 1) return;

        int winnerId = bases[0].connectionToClient.connectionId; // the remaining base owner is the winner

        RpcGameOver($"Player {winnerId}");
        ServerOnGameOver?.Invoke();
    }

    #endregion

    #region Client
    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        print("RpcGameOver");
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}
