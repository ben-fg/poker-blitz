using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CannonShoot : MonoBehaviour
{
    [SerializeField] private GameObject cannonBall;
    [SerializeField] private AudioClip[] gunSounds = new AudioClip[3];
    private float speed;
    private int damage;
    private int owner;
    private string type;
    PhotonView view;

    void Start()
    {
        view = GetComponent<PhotonView>();
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
            view.RPC("ShootForAll", RpcTarget.All, directionRaw, damage);
        }
    }

    [PunRPC]
    public void ShootForAll(float[] direction, int damage)
    {
        if (view.IsMine)
        {
            GameObject currentCannonBall = PhotonNetwork.Instantiate(cannonBall.name, transform.position, Quaternion.identity);
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
            cannonBallScript.direction = new Vector2(direction[0], direction[1]);
            StartCoroutine(DestroyCannonBall(currentCannonBall));
        }
    }

    private IEnumerator DestroyCannonBall(GameObject cannonBall)
    {
        yield return new WaitForSeconds(3);
        Destroy(cannonBall);
    }
}
