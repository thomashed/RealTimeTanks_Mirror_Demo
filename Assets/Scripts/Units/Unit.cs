using Assets.Scripts.Movement;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Unit : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private int resourceCost = 0;
    public int ResourceCost { get { return resourceCost; } }

    [SerializeField] private UnitMovement unitMovement = null;
    public UnitMovement UnitMovement { get { return unitMovement; } private set { unitMovement = value; }}

    [SerializeField] private Targeter targeter = null;
    public Targeter Targeter { get { return targeter; } }

    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;

    [SerializeField] private Health health = null;

    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;

    #region Server

    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        ServerOnUnitDespawned?.Invoke(this);
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server] 
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client
    [ClientCallback]
    private void Update()
    {
        if (!hasAuthority) return;
        // if we click this object 
    }

    [Client]
    public void Select()
    {
        if (!hasAuthority) return; // we dont wanna call units that isn't this player's
        onSelected?.Invoke(); // call invoke if onSelected is not null
    }

    [Client]
    public void Deselect()
    {
        if (!hasAuthority) return;
        onDeselected?.Invoke();
    }

    public override void OnStartAuthority()
    {
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!hasAuthority) return;
        AuthorityOnUnitDespawned?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!hasAuthority) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

    }

    #endregion

}
