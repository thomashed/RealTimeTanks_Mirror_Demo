using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerCustom : NetworkManager
{
    [SerializeField] public GameObject _unitSpawnerPrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);
        var playerNetworkBehaviour = conn.identity.GetComponent<NetworkBehaviourCustom>();
        var player = conn.identity.GetComponent<RTSPlayer>();
        player.SetTeamColor(
            new Color(
                UnityEngine.Random.Range(0f, 1f), 
                UnityEngine.Random.Range(0f, 1f), 
                UnityEngine.Random.Range(0f, 1f)
                )
            );
        
        // instantiate a spawner unique for this player
        var unitSpawnerInstance = Instantiate(_unitSpawnerPrefab, conn.identity.transform.position, conn.identity.transform.rotation);
        NetworkServer.Spawn(unitSpawnerInstance, conn);

        playerNetworkBehaviour.SetDisplayName($"Player_{numPlayers}");
        print($"Client joined: {playerNetworkBehaviour.DisplayName}, address: {conn.address}");
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map")) // TODO: oh no, that string of yours
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
        }

    }
}
