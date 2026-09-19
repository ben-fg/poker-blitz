using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameSelector : MonoBehaviour
{
    //PhotonView view;
    public static List<PokerPlayer> rankedPlayers;
    [SerializeField] private TextMeshProUGUI[] playersText = new TextMeshProUGUI[4];
    [SerializeField] private TextMeshProUGUI gameText;
    private bool test;

    // Start is called before the first frame update
    void Start()
    {
        test = true;
        //view = GetComponent<PhotonView>();
        MinigameSelection.gameNumber = 0;

        UpdateText();
        gameText.text = "Host is picking...";

        if (!test)
        {
            for (int i = 0; i < 4; i++)
            {
                playersText[i].text = i + ". " + rankedPlayers[i].GetName().ToString();
            }
        }
    }

    private void UpdateText()
    {
        if (MinigameSelection.gameNumber == 1)
        {
            gameText.text = "Tower Ascent";
        }
        else if (MinigameSelection.gameNumber == 2)
        {
            gameText.text = "Cannons";
        }
        else if (MinigameSelection.gameNumber == 4)
        {
            gameText.text = "Cash Grabbers";
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void HostContinue()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            if (MinigameSelection.gameNumber != 0)
            {
                PhotonNetwork.LoadLevel("PreGame");
            }
            else
            {
                gameText.text = "ROLL THE DICE";
            }
            UpdateText();
        }
    }

    public void Reroll()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int number = PickRandomGameNumber();
            MinigameSelection.gameNumber = number;
            //view.RPC("SetGameNumber", RpcTarget.AllBuffered, number);
            UpdateText();
        }
    }

    private int PickRandomGameNumber()
    {
        int number = Random.Range(1, 4);
        if (number == 3)
        {
            number = 4; //High noon (code 3) is unfinished and Cash grabbers (code 4) is finished.
        }
        return number;
    }


    //Not in use unless we change the game selector to preview the mini game to all players
    [PunRPC]
    public void SetGameNumber(int number)
    {
        MinigameSelection.gameNumber = number;
    }
}
