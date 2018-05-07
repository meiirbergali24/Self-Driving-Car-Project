using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CarController : MonoBehaviour {

    const float speed = 450f;

    [SerializeField]
    private DataCollectionConfig config;
    [SerializeField]
    private CarData data;
    [SerializeField]
    private Rigidbody carBody;
    [SerializeField, Space]
    private Wheel[] wheels;
    [SerializeField]
    private float
        maxAngle = 45f,
        aligningTime = 1f,
        currAlignTime = 0,
        maxTorque = 0f;
    
    private void Update () {
        
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            maxTorque = speed;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            StopMove();
        }

        ///////////////////////////////////////////////////////////////
        if (config.isDataCollection)
        {
            float horizontal = Input.GetAxis("Horizontal") * maxAngle;

            data.angle = horizontal;
        }
        ///////////////////////////////////////////////////////////////

        if (data.angle == -999.0f)
        {
            StopMove();
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.Move(data.angle, maxTorque);
            }
        }
    }

    public void StopMove()
    {
        carBody.AddForce(-carBody.velocity * 3, ForceMode.Acceleration);
    }
    
    [SerializeField]
    private Vector3 startPosition;
    public void Reset()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.physics != null)
            {
                wheel.physics.steerAngle = 0;
                wheel.physics.motorTorque = 0;
            }
        }
        transform.position = startPosition;
    }

    [System.Serializable]
    private class Wheel
    {
        public WheelCollider physics;
        public Transform transform;
        public bool veering;
        public bool pushing;

        public Side side;
        public enum Side
        {
            Right = 1,
            Left = -1
        }

        private Vector3 position;
        private Quaternion rotation;

        public void Move(float angle, float torque)
        {
            if (veering)
            {
                physics.steerAngle = angle;
            }
            if (pushing)
            {
                physics.motorTorque = torque;
            }
            physics.GetWorldPose(out position, out rotation);
            transform.SetPositionAndRotation(position, rotation);
        }

        private void Update()
        {
            physics.GetWorldPose(out position, out rotation);
            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
