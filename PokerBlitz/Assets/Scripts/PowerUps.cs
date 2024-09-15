using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PowerUps : MonoBehaviour
{
    Hashtable playerProperties = new Hashtable();
    // Start is called before the first frame update
    void Start()
    {
        GameMaster.gameNumber = 1;
        playerProperties["PowerUp"] = 4;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PowerUp1()
    {
        playerProperties["PowerUp"] = 1;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        PlayerPowerUps.selectionEnd = true;
    }

    public void PowerUp2()
    {
        playerProperties["PowerUp"] = 2;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        PlayerPowerUps.selectionEnd = true;
    }

    public void PowerUp3()
    {
        playerProperties["PowerUp"] = 3;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        PlayerPowerUps.selectionEnd = true;
    }
}
