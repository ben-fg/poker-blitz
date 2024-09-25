using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class WaitingLobby : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerCount;
    [SerializeField] private Button startButton;
    private int requiredPlayers = 2;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Update()
    {
        int currentPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        playerCount.text = currentPlayerCount + "/4 Players";

        if (currentPlayerCount != requiredPlayers)
        {
            startButton.GetComponent<Image>().color = Color.grey;
            startButton.GetComponentInChildren<TextMeshProUGUI>().text = "Waiting...";
        }
        else
        {
            startButton.GetComponent<Image>().color = Color.green;
            startButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start (Host)";
        }
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == requiredPlayers)
        {
            PhotonNetwork.LoadLevel("PreGame");
        }
    }

    public void ForceStart()
    {
        PhotonNetwork.LoadLevel("PreGame");
    }
}
