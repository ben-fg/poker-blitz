using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private readonly float followSpeed = 10f;
    [SerializeField] private float yOffset;
    private Transform target;
    [SerializeField] private bool followOnX;
    [SerializeField] private bool followOnY;

    private void Start()
    {
        target = transform.parent;
    }

    void Update()
    {
        if (followOnX)
        {
            //Allows the camera to follow the player horizontally
            Vector3 newPos = new Vector3(target.position.x, yOffset, -10f);
            transform.position = Vector3.Slerp(transform.position, newPos, followSpeed * Time.deltaTime);
        }
        else if (followOnY)
        {
            //Allows the camera to follow the player vertically
            Vector3 newPos = new Vector3(0, target.position.y + yOffset, -10f);
            transform.position = Vector3.Slerp(transform.position, newPos, followSpeed * Time.deltaTime);
            transform.position = new Vector3(0, transform.position.y, -10f);
        }
        else
        {
            //Allows the camera to follow the player horizontally and vertically
            Vector3 newPos = new Vector3(target.position.x, target.position.y + yOffset, -10f);
            transform.position = Vector3.Slerp(transform.position, newPos, followSpeed * Time.deltaTime);
        }
    }
}
