using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NetworkBehaviourCustom : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent _navAgent = null;

    [SyncVar(hook = nameof(HandleSetDisplayName))]
    public string DisplayName = "No Name";

    #region Server
    [Server]
    public void SetDisplayName(string newName)
    {
        DisplayName = newName;
    }
    #endregion

    #region Client
    public override void OnStartAuthority()
    {
        
    }

    void HandleSetDisplayName(string oldValue, string newValue)
    {
        DisplayName = newValue;
    }

    [Command]
    public void CmdSetDisplayName()
    {
        SetDisplayName("newName");
    }

    [ContextMenu("Set Display Name")]
    public void ContextMenuSetDisplayName()
    {
        CmdSetDisplayName();
    }

    #endregion


}
