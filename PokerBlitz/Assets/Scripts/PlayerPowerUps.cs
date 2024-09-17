using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerPowerUps : MonoBehaviour
{
    private float[] powerUpCooldowns = new float[3];
    private PlayerMovement playerMovement;
    PhotonView view;
    private GameObject myPowerUp = null;
    private int powerUpNum;
    private float displacement = 0;
    [SerializeField] private Image abilityIcon;
    [SerializeField] private Sprite[] powerUpSprites = new Sprite[3];
    [SerializeField] private TextMeshProUGUI cooldown;
    public static bool selectionEnd = false;

    void Start()
    {
        PlayerMovement.isFrozen = true;
        view = GetComponent<PhotonView>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (GameMaster.gameNumber == 1)
        {
            //Debug.Log("num "+powerUpNum);
            //Debug.Log("cooldown " + powerUpCooldowns[2]);
            if (selectionEnd)
            {
                //Selects the appropriate powerup
                powerUpNum = (int)PhotonNetwork.LocalPlayer.CustomProperties["PowerUp"];
                if (powerUpNum == 1)
                {
                    myPowerUp = GetChildByTag(GetComponent<Transform>(), "PU1");
                    displacement = 1.666667f;
                }
                else if (powerUpNum == 2)
                {
                    myPowerUp = GetChildByTag(GetComponent<Transform>(), "PU2");
                }

                abilityIcon.sprite = powerUpSprites[powerUpNum - 1];
                abilityIcon.color = Color.white;

                PlayerMovement.isFrozen = false;
                selectionEnd = false;
            }

            if (powerUpNum != 0)
            {
                //Counts down any ability on cooldown
                if (powerUpCooldowns[powerUpNum - 1] > 0)
                {
                    cooldown.color = Color.white;
                    cooldown.text = (powerUpCooldowns[powerUpNum - 1] + 0.5).ToString("0");
                    abilityIcon.color = Color.black;
                    powerUpCooldowns[powerUpNum - 1] -= Time.deltaTime;
                }
                else
                {
                    if (powerUpNum != 0)
                    {
                        cooldown.color = Color.clear;
                        abilityIcon.color = Color.white;
                    }
                }

                //Makes sure the boxing glove faces the right direction
                if (powerUpNum == 1)
                {
                    view.RPC("FlipItemRPC", RpcTarget.All, "PU1", Input.GetAxis("Horizontal"));
                }

                //Applies the selected powerups ability
                if (view.IsMine && Input.GetKeyDown(KeyCode.LeftShift))
                {
                    if (powerUpNum == 1 && powerUpCooldowns[0] <= 0)
                    {
                        Debug.Log("Box");
                        StartCoroutine(TogglePowerUp(0.5f, "PU1"));
                        powerUpCooldowns[0] = 6;
                        myPowerUp.GetComponent<AudioSource>().Play();
                    }
                    else if (powerUpNum == 2 && powerUpCooldowns[1] <= 0)
                    {
                        Debug.Log("Magnet");
                        StartCoroutine(TogglePowerUp(4f, "PU2"));
                        powerUpCooldowns[1] = 12;
                        myPowerUp.GetComponent<AudioSource>().Play();
                    }
                    else if (powerUpNum == 3 && powerUpCooldowns[2] <= 0)
                    {
                        Debug.Log("Jump");
                        playerMovement.DoubleJump();
                        powerUpCooldowns[2] = 3;
                    }
                }
            }
        }
    }

    public IEnumerator TogglePowerUp(float duration, string powerUpName)
    {
        view.RPC("ActivateTriggerRPC", RpcTarget.All, powerUpName);
        yield return new WaitForSeconds(duration);
        view.RPC("DeactivateTriggerRPC", RpcTarget.All, powerUpName);
    }

    [PunRPC]
    public void ActivateTriggerRPC(string powerUpName)
    {
        //Set the trigger to active for all clients
        myPowerUp = GetChildByTag(GetComponent<Transform>(), powerUpName);
        myPowerUp.SetActive(true);
        Debug.Log("Trigger activated on all clients");
    }

    [PunRPC]
    public void DeactivateTriggerRPC(string powerUpName)
    {
        //Set the trigger to inactive for all clients
        myPowerUp = GetChildByTag(GetComponent<Transform>(), powerUpName);
        myPowerUp.SetActive(false);
        Debug.Log("Trigger activated on all clients");
    }

    [PunRPC]
    public void FlipItemRPC(string powerUpName, float hMov)
    {
        displacement = 1.666667f;
        myPowerUp = GetChildByTag(GetComponent<Transform>(), powerUpName);
        if (hMov > 0)
        {
            myPowerUp.GetComponent<SpriteRenderer>().flipX = false;
            myPowerUp.transform.position = new Vector2(transform.position.x + displacement, transform.position.y);
        }
        else if (hMov < 0)
        {
            myPowerUp.GetComponent<SpriteRenderer>().flipX = true;
            myPowerUp.transform.position = new Vector2(transform.position.x - displacement, transform.position.y);
        }
    }

    private static GameObject GetChildByTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }
        }
        return null;
    }
}
