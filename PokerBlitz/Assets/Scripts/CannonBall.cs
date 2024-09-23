using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CannonBall : MonoBehaviour
{
    internal float speed;
    internal int damage;
    internal int owner;
    internal Vector2 direction;
    PhotonView view;

    void Start()
    {
        view = GetComponent<PhotonView>();
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
        //view.RPC("MoveForAll", RpcTarget.All);
    }

    [PunRPC]
    public void MoveForAll()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    /*
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            Destroy(this);
        }
    }
    */

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger");
        if (collision.CompareTag("Ground"))
        {
            Debug.Log("Ground");
            Destroy(this);
        }
    }
}
