using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraSetup : MonoBehaviour
{
    public GameObject cameraPrefab;
    PhotonView view;
    void Start()
    {
        view = GetComponent<PhotonView>();
        if (view.IsMine)
        {
            GameObject playerCamera = Instantiate(cameraPrefab, transform.position, Quaternion.identity);
            playerCamera.transform.SetParent(transform);
            //playerCamera.transform.localPosition = new Vector3(0, 1, -5);
        }
    }
}
