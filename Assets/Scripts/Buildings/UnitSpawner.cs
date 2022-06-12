using Assets.Scripts.Movement;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{

    [SerializeField] private Health health = null;
    [SerializeField] private Unit unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;

    [SerializeField] private TMP_Text remainingUnitsText = null;
    [SerializeField] private Image unitsProgressImage = null;
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 7f; // so units won't stack on top of each other
    [SerializeField] private float unitSpawnDuration = 5f; // so units won't stack on top of each other

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits = 0;

    [SyncVar]
    private float unitTimer = 0; 

    private float progressImageVelocity;

    // this Update contains logic for both Server and Client, thus we divide it like below, instead of going with the usual [ServerCallback]
    private void Update() 
    {
        if (isServer)
        {
            ProduceUnits();
        }
        if (isClient)
        {
            UpdateTimerDisplay();
        }
    }

    #region Server

    public override void OnStartServer()
    {
        base.OnStartServer();
        health.ServerOnDie += ServerHandleDie;
    }
    
    public override void OnStopServer()
    {
        base.OnStopServer();
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CmdSpawnUnit()
    {

        if (queuedUnits >= maxUnitQueue) return;
        

        var player = connectionToClient.identity.GetComponent<RTSPlayer>();
        
        if (player.Resources < unitPrefab.ResourceCost) return;

        queuedUnits++;
        
        player.UpdateResources(player.Resources - unitPrefab.ResourceCost);
    }

    [Server]
    private void ProduceUnits()
    {
        if (queuedUnits == 0) return;
        unitTimer += Time.deltaTime;
        if (unitTimer < unitSpawnDuration) return;
        
        var unitInstance = Instantiate(unitPrefab, unitSpawnPoint.position, unitSpawnPoint.rotation);
        NetworkServer.Spawn(unitInstance.gameObject, connectionToClient); // otherwise it was only spawned on the server 

        Vector3 spawnOffset = UnityEngine.Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = unitSpawnPoint.position.y;

        var unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

        unitTimer = 0;
        queuedUnits--;
    }

    #endregion

    #region Client
    public void OnPointerClick(PointerEventData eventData)
    {
        
        if (!hasAuthority) return;
        if (eventData.button != PointerEventData.InputButton.Right) return;
        CmdSpawnUnit();
    }

    private void ClientHandleQueuedUnitsUpdated(int oldQueue, int newQueue)
    {
        remainingUnitsText.text = $"{newQueue}";
    }

    // updates the fillAmount of the unitProduction timer 
    private void UpdateTimerDisplay()
    {
        var newProgress = unitTimer / unitSpawnDuration;

        if (newProgress < unitsProgressImage.fillAmount)
        {
            unitsProgressImage.fillAmount = newProgress;
        }
        else
        {
            // the server will only update us every few frames, so to avoid jittery, we smooth the progress using below functionatlity(in case of network latency)
            unitsProgressImage.fillAmount = Mathf.SmoothDamp(unitsProgressImage.fillAmount, newProgress, ref progressImageVelocity, 0.1f);
        }
    }

    #endregion

}
