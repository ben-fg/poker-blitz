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
    [SerializeField] private Sprite[] preGameSprites = new Sprite[3 * GameMaster.maxGames];
    [SerializeField] private Sprite gameplaySprite;
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
        if (GameMaster.gameNumber == 2)
        {
            preGameText[0].text = "Shoot other players to eliminate them. First player to 20 eliminations wins.";
            preGameText[1].text = "W - Move Up\nS - Move Down\nA - Move left\nD - Move right\nMouse - Aim\nLeft Click - Shoot";
            preGameText[2].text = "Gunner";
            preGameText[3].text = "Your shots fire rapidly at reduced damage";
            preGameText[4].text = "Tank";
            preGameText[5].text = "Gain extra health with high damage shots but at reduced fire rate.";
            preGameText[6].text = "Sniper";
            preGameText[7].text = "Your shots travel very fast but you have less health.";
            preGameText[8].text = "Cannons";
        }
        preGameImages[3].sprite = gameplaySprite;
        for (int i = 0; i < preGameImages.Length - 1; i++)
        {
            preGameImages[i].sprite = GetSprite(GameMaster.gameNumber - 1, i);
        }
    }

    void Update()
    {
        
    }

    private Sprite GetSprite(int row, int column)
    {
        return preGameSprites[(row * 3) + column];
    }

    public void HostStart()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("PowerUps");
        }
    }
}
