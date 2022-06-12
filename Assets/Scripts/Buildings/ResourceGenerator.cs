using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGenerator : NetworkBehaviour
{

    [SerializeField] private Health health = null;
    [SerializeField] private int resourcesPerInterval = 5;
    [SerializeField] private float interval = 2f;

    private float timer;
    private RTSPlayer player;

    public override void OnStartServer()
    {
        timer = interval;
        player = connectionToClient.identity.GetComponent<RTSPlayer>(); // we know player exists, as it'd be player placing a building

        health.ServerOnDie += ServerHandleDie; // we need to stop generating resources if we die
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver; // we need to stop generating resources if we lost
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver; // we need to stop generating resources if we die
    }

    [ServerCallback] // only server should have authority to update the resouceUpTick
    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            // update resources on player
            player.UpdateResources(player.Resources + resourcesPerInterval);
            timer = interval;
        }
    }

    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    private void ServerHandleGameOver()
    {
        enabled = false;
    }

}
