using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarFollowingCamera : MonoBehaviour
{
    [SerializeField]
    private Transform car;

    [SerializeField]
    private float speed;
    [SerializeField]
    private Vector3 offset;
    private new Transform transform;

    public Vector3 Offset
    {
        get
        {
            return car.right * offset.x + car.up * offset.y + car.forward * offset.z;
        }
        set
        {
            offset = value;
        }
    }

    private void Awake()
    {
        transform = gameObject.transform;
    }
    
    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, car.position + Offset, Time.deltaTime * speed);
        transform.LookAt(car);
    }
}
