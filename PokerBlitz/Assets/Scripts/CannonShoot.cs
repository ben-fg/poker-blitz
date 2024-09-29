using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class CannonShoot : MonoBehaviour
{
    [SerializeField] private GameObject cannonBall;
    [SerializeField] private AudioClip[] gunSounds = new AudioClip[3];
    private float speed;
    private int damage;
    private int owner;
    private string type;

    public int health;
    public int maxHealth;
    public int kills;
    private int deaths;
    [SerializeField] Transform[] randSpawnPos = new Transform[10];
    [SerializeField] AudioSource landedShot;
    [SerializeField] AudioSource death;
    [SerializeField] public TextMeshProUGUI healthCount;
    [SerializeField] public TextMeshProUGUI killCount;
    [SerializeField] TextMeshProUGUI deathCount;
    PhotonView view;

    void Start()
    {
        view = GetComponent<PhotonView>();
        view.RPC("SetHealth", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void SetHealth()
    {
        if (type == "Gunner")
        {
            maxHealth = 100;
        }
        else if (type == "Tank")
        {
            maxHealth = 300;
        }
        else if (type == "Chainsaw")
        {
            maxHealth = 125;
        }
        else if (type == "Rookie")
        {
            maxHealth = 100;
        }
        health = maxHealth;
        Debug.Log(maxHealth);
    }

    void Update()
    {
        if (view.IsMine)
        {
            healthCount.text = health.ToString();
            killCount.text = "Kills: " + kills.ToString();
            deathCount.text = "Deaths: " + deaths.ToString();
            if (health <= 0)
            {
                //Respawn();
            }
        }
    }

    public void SetCannonProperties(float speed, int damage, int owner, string type)
    {
        this.speed = speed;
        this.damage = damage;
        this.owner = owner;
        this.type = type;
    }

    public void Shoot(Vector2 direction)
    {
        if (view.IsMine)
        {
            float[] directionRaw = { direction.x, direction.y };
            GameObject currentCannonBall = PhotonNetwork.Instantiate(cannonBall.name, transform.position, Quaternion.identity);
            view.RPC("ShootForAll", RpcTarget.All, directionRaw, damage, owner, currentCannonBall.GetComponent<PhotonView>().ViewID);
        }
    }

    [PunRPC]
    public void ShootForAll(float[] direction, int damage, int owner, int viewID)
    {
        GameObject currentCannonBall = PhotonView.Find(viewID).gameObject;
        CannonBall cannonBallScript = currentCannonBall.GetComponent<CannonBall>();
        if (type == "Gunner")
        {
            currentCannonBall.GetComponent<AudioSource>().clip = gunSounds[0];
        }
        else if (type == "Tank")
        {
            currentCannonBall.GetComponent<AudioSource>().clip = gunSounds[1];
            currentCannonBall.transform.localScale = new Vector2(2.5f, 2.5f);
        }
        else if (type == "Rookie")
        {
            currentCannonBall.GetComponent<AudioSource>().clip = gunSounds[2];
        }
        currentCannonBall.GetComponent<AudioSource>().Play();
        cannonBallScript.speed = speed;
        cannonBallScript.damage = damage;
        cannonBallScript.owner = owner;
        //Debug.Log("@" + owner);
        cannonBallScript.direction = new Vector2(direction[0], direction[1]);
        StartCoroutine(DestroyCannonBall(currentCannonBall));
    }

    private IEnumerator DestroyCannonBall(GameObject cannonBall)
    {
        yield return new WaitForSeconds(3);
        Destroy(cannonBall);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("CannonBall"))
        {
            CannonBall cannonBallProperties = collision.GetComponent<CannonBall>();
            PhotonView targetPhotonView = PhotonView.Find(cannonBallProperties.owner);
            //Player targetPlayer = null;
            /*
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.ActorNumber == cannonBallProperties.owner)
                {
                    targetPlayer = p;
                    break;
                }
            }
            */
            //Debug.Log(view);
            //Debug.Log(targetPlayer);
            if (view.IsMine && targetPhotonView.ViewID != view.ViewID)
            {
                if (health - cannonBallProperties.damage > 0)
                {
                    health -= cannonBallProperties.damage;
                    view.RPC("Impact", RpcTarget.All, cannonBallProperties.owner, "HIT");
                }
                else
                {
                    health -= cannonBallProperties.damage;
                    view.RPC("Impact", RpcTarget.All, cannonBallProperties.owner, "DEATH");
                    Respawn();
                }
            }
        }
    }

    [PunRPC]
    public void Impact(int shotOwner, string hitOrDeath)
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName+": "+ view.ViewID);
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + ": " + shotOwner);
        //if (view.ViewID == shotOwner)
        if (true)
        {
            Debug.Log(PhotonNetwork.LocalPlayer);
            //Player shotOwnerPlayer = PhotonNetwork.LocalPlayer;
            if (hitOrDeath == "HIT")
            {
                landedShot.Play();
                Debug.Log("Hit");
            }
            else if (hitOrDeath == "DEATH")
            {
                PhotonView shooterPhotonView = PhotonView.Find(shotOwner);
                CannonShoot shooter = shooterPhotonView.gameObject.GetComponent<CannonShoot>();
                shooter.kills++;
                shooter.health = shooter.maxHealth; //Restores health on a kill
                Debug.Log(shooter.kills +" + "+shooter.maxHealth);
                shooter.healthCount.text = shooter.health.ToString();
                shooter.killCount.text = "Kills: " + shooter.kills.ToString();
                /*
                Debug.Log(shotOwnerPlayer);
                //Debug.Log(shotOwnerPlayer.GetComponent<CannonShoot>());
                shotOwnerPlayer.gameObject.GetComponent<CannonShoot>().Kill();
                */
                Debug.Log("Death");
                death.Play();
            }
            else
            {
                Debug.LogError("Every impact must result in a HIT or DEATH");
            }
        }
    }

    private void Respawn()
    {
        gameObject.transform.position = randSpawnPos[Random.Range(0, randSpawnPos.Length)].position;
        health = maxHealth;
        deaths++;
    }

    [PunRPC]
    public void Kill(int shotOwnerAN)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == shotOwnerAN)
        {
            kills++;
            health = maxHealth; //Restores health on a kill
        }
    }
}
