using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamColorSetter : NetworkBehaviour
{

    [SerializeField] private Renderer[] colorRenderers = new Renderer[0];

    [SyncVar(hook=nameof(HandleTeamColorUpdated))]
    private Color teamColor = new Color();

    #region Server
    public override void OnStartServer()
    {
        // get player, so we know their color
        var player = connectionToClient.identity.GetComponent<RTSPlayer>();
        // add that^ color to array, which is synced across all clients
        teamColor = player.TeamColor;
    }

    #endregion

    #region Client
    private void HandleTeamColorUpdated(Color oldColor, Color newColor)
    {
        foreach (var renderer in colorRenderers)
        {
            renderer.material.SetColor("_BaseColor", newColor);
        }
    }

    #endregion

}
