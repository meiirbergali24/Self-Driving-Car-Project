using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSetter : MonoBehaviour
{
    [SerializeField] Transform target;

    public void SetPosition(Vector3 position)
    {
        target.position = position;
    }

    public void SetRotation(Transform from)
    {
        target.rotation = from.rotation;
    }

    public void SetTransfom(Transform from)
    {
        target.position = from.position;
        target.rotation = from.rotation;
    }
}
