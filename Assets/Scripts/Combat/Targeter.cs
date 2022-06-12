using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeter : NetworkBehaviour
{

    private Targetable target = null;

    public Targetable Target { get { return target; } }

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [Command]
    public void CmdSetTarget(GameObject targetGameObject) // we let server validate that we have a target
    {
        if (!targetGameObject.TryGetComponent<Targetable>(out Targetable validatedTarget)) return;
        target = validatedTarget;
    } 

    [Server]
    public void ClearTarget()
    {
        target = null;
    }

    [Server]
    private void ServerHandleGameOver() 
    {
        ClearTarget();
    }
}
