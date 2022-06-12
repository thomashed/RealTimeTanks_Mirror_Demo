using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    // keep track of player's own units
    // add them to a list when they spawn --> subscribe to units spawn/deSpan events 

    [SerializeField] private Building[] buildings = new Building[0];
    [SyncVar(hook = nameof(OnClientHandleResourcesUpdated))] private int resources = 500;
    public int Resources { get { return resources; } }
    [SerializeField] private LayerMask buildingLayerMask = new LayerMask();
    [SerializeField] private float builingRangeLimit = 5.0f;
    public event Action<int> ClientOnResourcesUpdated;
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();
    private Color teamColor = new Color();
    public Color TeamColor { get { return teamColor; } }

    [SerializeField] private Transform cameraTransform = null;
    public Transform CameraTransform { get { return cameraTransform; } }

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    public List<Building> MyBuildings { get { return myBuildings; }}

    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
    {
        // check if we collide with anything
        if (Physics.CheckBox(point, buildingCollider.size / 2, Quaternion.identity, buildingLayerMask))
        {
            return false;
        }

        // check for range before allowing placement
        foreach (var building in MyBuildings)
        {
            // basically Pythagoras, but excpluding the squareRoot operation in the end
            if ((point - building.transform.position).sqrMagnitude <= Mathf.Pow(builingRangeLimit, 2))
            {
                return true;
            }
        }

        return false;
    }

    #region Server
    public override void OnStartServer()
    {
        if (!isServer) return;
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    public override void OnStopServer()
    {
        if (!isServer) return; // TODO: this check seems silly at best
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId,  Vector3 point)
    {
        // what building to spawn
        // where to spawn building
        //Instantiate(building, transform);
        Building buildingToPlace = null;

        foreach (Building building in buildings)
        {
            if (building.Id == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        }

        if (buildingToPlace == null) return;
        if (resources < buildingToPlace.Price) return;

        // check if building collides with other elements
        var buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

        if (!CanPlaceBuilding(buildingCollider, point)) return; // still too far away for building placement

        var buildingInstance = Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation); // we do it this way so we can pass ownership
        NetworkServer.Spawn(buildingInstance, connectionToClient); // give ownership to the player, and spawn it on the network so all players can see it
        UpdateResources(resources - buildingToPlace.Price);

    }

    [Server]
    public void UpdateResources(int updatedResources)
    {
        resources = updatedResources;
    }

    [Server]
    public void SetTeamColor(Color newTeamColor)
    {
        teamColor = newTeamColor;
    }

    private void ServerHandleUnitSpawned(Unit unit)
    {
        if (connectionToClient is null) return;
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myUnits.Add(unit);
    }

    private void ServerHandleUnitDespawned(Unit unit)
    {
        if (connectionToClient is null) return;
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myUnits.Remove(unit);
    }

    private void ServerHandleBuildingSpawned(Building building)
    {
        if (connectionToClient is null) return;
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myBuildings.Add(building);
    }

    private void ServerHandleBuildingDespawned(Building building)
    {
        if (connectionToClient is null) return;
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myBuildings.Remove(building);
    }

    #endregion


    #region Client
    public override void OnStartAuthority()
    {
        if (NetworkServer.active) return; // we don't wanna subsribe if we're the server 
        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
    }

    public override void OnStopClient()
    {
        if (!hasAuthority || !isClientOnly) return;
        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
    }

    private void OnClientHandleResourcesUpdated(int oldResources, int newResources)
    {
        ClientOnResourcesUpdated?.Invoke(newResources);
    }

    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    private void AuthorityHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);   
    }

    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }

    #endregion

}
