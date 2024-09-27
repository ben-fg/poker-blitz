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
    [SerializeField] Sprite[] powerUpSprites = new Sprite[GameMaster.maxGames * 3];
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] Image currentPowerUpIcon;
    private float timer = 5;
    private int currentTurn = 1;
    private bool yourTurn;
    PhotonView view;
   
    void Start()
    {
        view = GetComponent<PhotonView>();
        playerProperties["PowerUp"] = 4;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        for (int i = 0; i < 3; i++)
        {
            powerUpButtons[i].GetComponent<Image>().sprite = GetSprite(GameMaster.gameNumber - 1, i);
        }

        currentPowerUpIcon.color = Color.clear;

        Zapper propertyZapper = GetComponent<Zapper>();
        //Create an order for mini game testing purposes
        propertyZapper.OrderForTesting();
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
            ChangeButtonColours(Color.white);
            playerNameText.text = view.Owner.NickName + " select your powerup.";
        }
        else
        {
            yourTurn = false;
            ChangeButtonColours(Color.HSVToRGB(0.2f, 0.2f, 0.2f));
        }
        
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            timer = 10;
            currentTurn++;
        }

        timerText.text = (timer + 0.5).ToString("0");
    }

    public Sprite GetSprite(int row, int column)
    {
        return powerUpSprites[(row * 3) + column];
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
        Debug.Log((int)PhotonNetwork.LocalPlayer.CustomProperties["Order"]);
        Debug.Log(currentTurn);
        if (view.IsMine && yourTurn)
        {
            playerProperties["PowerUp"] = 1;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
            currentPowerUpIcon.color = Color.white;
            currentPowerUpIcon.sprite = GetSprite(GameMaster.gameNumber - 1, 0);
        }
    }

    public void PowerUp2()
    {
        if (view.IsMine && yourTurn)
        {
            playerProperties["PowerUp"] = 2;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
            currentPowerUpIcon.color = Color.white;
            currentPowerUpIcon.sprite = GetSprite(GameMaster.gameNumber - 1, 1);
        }
    }

    public void PowerUp3()
    {
        if (view.IsMine && yourTurn)
        {
            playerProperties["PowerUp"] = 3;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
            currentPowerUpIcon.color = Color.white;
            currentPowerUpIcon.sprite = GetSprite(GameMaster.gameNumber - 1, 2);
        }
    }
}
