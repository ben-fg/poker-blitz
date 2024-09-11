using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    public Vector2 spawnPos;

    private void Start()
    {
        Player localPlayer = PhotonNetwork.LocalPlayer;
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);
    }
}
