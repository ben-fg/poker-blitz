using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerPowerUps : MonoBehaviour
{
    private float[] powerUpCooldowns = new float[3];
    private PlayerMovement playerMovement;
    PhotonView view;
    public static bool selectionEnd = false;
    private GameObject myPowerUp = null;
    private int powerUpNum;
    private float displacement = 0;

    void Start()
    {
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
                myPowerUp.SetActive(false);

                selectionEnd = false;
            }

            //Counts down any ability on cooldown
            for (int i = 0; i < 3; i++)
            {
                if (powerUpCooldowns[i] > 0)
                {
                    powerUpCooldowns[i] -= Time.deltaTime;
                }
            }
            Debug.Log(powerUpNum);
            //Makes sure the powerup faces the right direction
            if (powerUpNum != 3)
            {
                if (Input.GetAxis("Horizontal") > 0)
                {
                    myPowerUp.GetComponent<SpriteRenderer>().flipX = false;
                    myPowerUp.transform.position = new Vector2(transform.position.x + displacement, transform.position.y);
                }
                else if (Input.GetAxis("Horizontal") < 0)
                {
                    myPowerUp.GetComponent<SpriteRenderer>().flipX = true;
                    myPowerUp.transform.position = new Vector2(transform.position.x - displacement, transform.position.y);
                }
            }

            //Applies the selected powerups ability
            if (view.IsMine && Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (powerUpNum == 1 && powerUpCooldowns[0] <= 0)
                {
                    Debug.Log("Box");
                    StartCoroutine(TogglePowerUp(0.5f));
                    powerUpCooldowns[0] = 6;
                    myPowerUp.GetComponent<AudioSource>().Play();
                }
                else if (powerUpNum == 2 && powerUpCooldowns[1] <= 0)
                {
                    Debug.Log("Magnet");
                    StartCoroutine(TogglePowerUp(4f));
                    powerUpCooldowns[1] = 10;
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

    public IEnumerator TogglePowerUp(float duration)
    {
        myPowerUp.SetActive(true);
        yield return new WaitForSeconds(duration);
        myPowerUp.SetActive(false);
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
