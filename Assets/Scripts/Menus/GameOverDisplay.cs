using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverDisplay : MonoBehaviour // UI is completely ClientSide, so we'll leave it as MonoBehaviour
{
    [SerializeField] private GameObject gameOverDisplayParent = null; // to set the UI display active or not
    [SerializeField] private TMP_Text winnerNameText = null;

    [SerializeField] private Button buttonLeaveGame = null;

    private void Start()
    {
        buttonLeaveGame.onClick.AddListener(LeaveGame);
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }
    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    // TODO: add onclick event listener to leaveButton

    public void LeaveGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected) // if we're both server AND client
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }

    private void ClientHandleGameOver(string winner)
    {
        gameOverDisplayParent.SetActive(true);
        winnerNameText.text = $"{winner} has won!";
    }
}
