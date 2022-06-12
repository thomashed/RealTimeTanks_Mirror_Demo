using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : NetworkBehaviour
{

    [SerializeField] private GameObject buildingPreview = null;
    [SerializeField] private Sprite icon = null; // icon, as a building is something shown in UI that you can drop into scene
    [SerializeField] private int  price = 100;
    [SerializeField] private int id = -1; // we'll use id to spawn it over the network at other player's instances

    public static event Action<Building> ServerOnBuildingSpawned;
    public static event Action<Building> ServerOnBuildingDespawned;
    public static event Action<Building> AuthorityOnBuildingSpawned;
    public static event Action<Building> AuthorityOnBuildingDespawned;

    public GameObject BuildingPreview { get { return buildingPreview; } }
    public int Id { get { return id; } }
    public int Price { get { return price; } }
    public Sprite Icon { get { return icon; } }

    #region Server
    public override void OnStartServer()
    {
        ServerOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBuildingDespawned?.Invoke(this);
    }
    #endregion

    #region Client
    public override void OnStartAuthority()
    {
        AuthorityOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!hasAuthority) return;
        AuthorityOnBuildingDespawned?.Invoke(this);
    }
    #endregion

}
