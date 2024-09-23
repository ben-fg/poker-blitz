using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Camera myCamera;
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private float yOffset;
    private Transform target;
    [SerializeField] private bool followOnX;
    [SerializeField] private bool followOnY;
    PhotonView view;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        target = transform;
    }

    void Update()
    {
        if (view.IsMine)
        {
            if (followOnX && !followOnY)
            {
                //Allows the camera to follow the player horizontally
                Vector3 newPos = new Vector3(target.position.x, yOffset, -10f);
                myCamera.transform.position = Vector3.Slerp(transform.position, newPos, followSpeed * Time.deltaTime);
            }
            else if (followOnY && !followOnX)
            {
                //Allows the camera to follow the player vertically
                Vector3 newPos = new Vector3(0, target.position.y + yOffset, -10f);
                myCamera.transform.position = Vector3.Slerp(transform.position, newPos, followSpeed * Time.deltaTime);
                myCamera.transform.position = new Vector3(0, transform.position.y, -10f);
            }
            else
            {
                //Allows the camera to follow the player horizontally and vertically
                Vector3 newPos = new Vector3(target.position.x, target.position.y + yOffset, -10f);
                myCamera.transform.position = newPos;
                //myCamera.transform.position = Vector3.Slerp(transform.position, newPos, followSpeed * Time.deltaTime);
            }
        }
        else
        {
            myCamera.gameObject.SetActive(false);
        }
    }
}
