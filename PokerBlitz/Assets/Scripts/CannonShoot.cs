using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CannonShoot : MonoBehaviour
{
    [SerializeField] private GameObject cannonBall;
    private float speed;
    private int damage;
    private int owner;
    PhotonView view;

    void Start()
    {
        view = GetComponent<PhotonView>();
    }

    public void SetCannonProperties(float speed, int damage, int owner)
    {
        this.speed = speed;
        this.damage = damage;
        this.owner = owner;
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
            if (damage == 100)
            {
                currentCannonBall.transform.localScale = new Vector2(2.5f, 2.5f);
            }
            cannonBallScript.speed = speed;
            cannonBallScript.damage = damage;
            cannonBallScript.owner = owner;
            cannonBallScript.direction = new Vector2(direction[0], direction[1]);
            StartCoroutine(DestroyCannonBall(currentCannonBall));
        }
    }

    private IEnumerator DestroyCannonBall(GameObject cannonBall)
    {
        yield return new WaitForSeconds(5);
        Destroy(cannonBall);
    }
}
