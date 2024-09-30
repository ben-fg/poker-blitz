using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    public Vector2 spawnPos;
    public Vector2[] multiSpawnPos = new Vector2[4];
    public bool isMultiSpawnPos;

    private void Start()
    {
        if (isMultiSpawnPos)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, multiSpawnPos[Random.Range(0, multiSpawnPos.Length)], Quaternion.identity);
        }
        else
        {
            PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);
        }
    }
}
