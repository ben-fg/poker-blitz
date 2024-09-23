using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PreGame : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] preGameText = new TextMeshProUGUI[9];
    [SerializeField] private Image[] preGameImages = new Image[4];
    [SerializeField] private Sprite[] preGameSprites = new Sprite[4]; //Turn into 2D array when more than one game is added
    PhotonView view;

    void Start()
    {
        GameMaster.gameNumber = 2; //For testing purposes

        view = GetComponent<PhotonView>();
        if (GameMaster.gameNumber == 1)
        {
            preGameText[0].text = "Jump across the platforms to climb the tower. Be the first player to reach the top.";
            preGameText[1].text = "A - Move left\nD - Move right\nSpace or W (Hold) - Jump\nShift - Use ability";
            preGameText[2].text = "Boxing Glove";
            preGameText[3].text = "Knock an opponent off the platform. [Cooldown: 7s]";
            preGameText[4].text = "Magnet";
            preGameText[5].text = "Pull all nearby players towards you. [Cooldown: 12s]";
            preGameText[6].text = "Double Jump";
            preGameText[7].text = "Jump again in mid-air. [Cooldown: 4s]";
            preGameText[8].text = "Tower Ascent";
        }
        for (int i = 0; i < preGameImages.Length; i++)
        {
            preGameImages[i].sprite = preGameSprites[i];
        }
    }

    void Update()
    {
        
    }

    public void HostStart()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Game"+GameMaster.gameNumber);
        }
    }
}
