using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PowerUps : MonoBehaviour
{
    Hashtable playerProperties = new Hashtable();
    [SerializeField] GameObject[] powerUpButtons = new GameObject[3];
    [SerializeField] Sprite[,] powerUpSprites = new Sprite[2,3];
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] TextMeshProUGUI timerText;
    private float timer = 10;
    private int currentTurn = 1;
    private bool yourTurn;
    PhotonView view;
   
    void Start()
    {
        view = GetComponent<PhotonView>();
        playerProperties["PowerUp"] = 4;

        //Create an order for mini game testing purposes
        OrderForTesting();
    }

    void Update()
    {
        if (currentTurn == 4)
        {
            PhotonNetwork.LoadLevel("Game" + GameMaster.gameNumber);
        }

        if (view.IsMine && (int)PhotonNetwork.LocalPlayer.CustomProperties["Order"] == currentTurn)
        {
            yourTurn = true;
            ChangeButtonColours(Color.grey);
            playerNameText.text = view.Owner.NickName + " select your powerup.";
        }
        else
        {
            yourTurn = false;
            ChangeButtonColours(Color.white);
        }
        
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            timer = 5;
            currentTurn++;
        }

        timerText.text = timer.ToString("0");
    }

    private void ChangeButtonColours(Color colour)
    {
        for (int i = 0; i < 3; i++)
        {
            powerUpButtons[i].GetComponent<Image>().color = colour;
        }
    }

    public void PowerUp1()
    {
        if (view.IsMine && yourTurn)
        {
            playerProperties["PowerUp"] = 1;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
            PlayerPowerUps.selectionEnd = true;
        }
    }

    public void PowerUp2()
    {
        if (view.IsMine && yourTurn)
        {
            playerProperties["PowerUp"] = 2;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
            PlayerPowerUps.selectionEnd = true;
        }
    }

    public void PowerUp3()
    {
        if (view.IsMine && yourTurn)
        {
            playerProperties["PowerUp"] = 3;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
            PlayerPowerUps.selectionEnd = true;
        }
    }

    private void OrderForTesting()
    {
        for (int i = 1; i < 4; i++)
        {
            if (view.IsMine && view.Owner.ActorNumber == i)
            {
                Debug.Log(i);
                playerProperties["Order"] = i;
            }
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }
}
