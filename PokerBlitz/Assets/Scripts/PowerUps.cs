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
    private bool sceneIsLoaded;
   
    void Start()
    {
        view = GetComponent<PhotonView>();
        playerProperties["PowerUp"] = 4;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        for (int i = 0; i < 3; i++)
        {
            powerUpButtons[i].GetComponent<Image>().sprite = GetSprite(GameMaster.gameNumber - 1, i);
        }

        Debug.Log("Setting clear 0");
        currentPowerUpIcon.color = Color.clear;

        Zapper propertyZapper = GetComponent<Zapper>();
        //Create an order for mini game testing purposes
        propertyZapper.OrderForTesting();
    }

    void Update()
    {
        if (!sceneIsLoaded && PhotonNetwork.IsMasterClient && currentTurn == 4)
        {
            PhotonNetwork.LoadLevel("Game" + GameMaster.gameNumber);
            sceneIsLoaded = true;
        }
        //Debug.Log("View: " + view.IsMine);
        //Debug.Log("Order: " + ((int)PhotonNetwork.LocalPlayer.CustomProperties["Order"] == currentTurn));
        Player currentPlayer = GetPlayerWithProperty("Order", currentTurn);
        if (currentPlayer == null)
        {
            yourTurn = false;
            ChangeButtonColours(Color.HSVToRGB(0.2f, 0.2f, 0.2f));
            view.RPC("ChangeText", RpcTarget.AllBuffered, "Odin");
        }
        else
        {
            if (currentPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                yourTurn = true;
                ChangeButtonColours(Color.white);
                view.RPC("ChangeText", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.NickName);
            }
            else
            {
                yourTurn = false;
                ChangeButtonColours(Color.HSVToRGB(0.2f, 0.2f, 0.2f));
            }
        }
        /*
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
        */
        
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            timer = 10;
            currentTurn++;
            Debug.Log(currentTurn);
        }

        timerText.text = (timer + 0.5).ToString("0");
    }

    [PunRPC]
    public void ChangeText(string nickname)
    {
        playerNameText.text = nickname + " select your powerup.";
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

    public Player GetPlayerWithProperty(string property, object propertyValue)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey(property))
            {
                if ((int)player.CustomProperties[property] == (int)propertyValue)
                {
                    return player;
                }
            }
        }
        return null;
    }

    public void PowerUp1()
    {
        //Debug.Log((int)PhotonNetwork.LocalPlayer.CustomProperties["Order"]);
        //Debug.Log(currentTurn);
        if (view.IsMine && yourTurn)
        {
            Debug.Log("Setting white 1");
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
            Debug.Log("Setting white 2");
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
            Debug.Log("Setting white 3");
            playerProperties["PowerUp"] = 3;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
            currentPowerUpIcon.color = Color.white;
            currentPowerUpIcon.sprite = GetSprite(GameMaster.gameNumber - 1, 2);
        }
    }
}
