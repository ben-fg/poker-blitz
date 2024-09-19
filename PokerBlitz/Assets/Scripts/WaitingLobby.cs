using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class WaitingLobby : MonoBehaviour
{
    [SerializeField] private Text playerCount;
    private int requiredPlayers = 2;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Update()
    {
        playerCount.text = PhotonNetwork.CurrentRoom.PlayerCount + "/4";
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
