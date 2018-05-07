using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class StreetWalker : MonoBehaviour {

    private Vector3 
        start,
        end;
    private new Transform transform;
    private new Renderer renderer;

    private static Color
        visible = new Color(1, 1, 1, 1),
        invisible = new Color(1, 1, 1, 0);

    [SerializeField]
    private float
        fadeDistance,
        moveSpeed;
    [SerializeField]
    private Transform Hip;

    private Animator animator;

    private bool active;
    public bool Active
    {
        get
        {
            return active;
        }
        private set
        {
            //if (value)
            //{
            //    if (Hip.GetComponent<Rigidbody>())
            //    {
            //        Destroy(Hip.GetComponent<Rigidbody>());
            //    }
            //}
            //else
            //{
            //    if (!Hip.GetComponent<Rigidbody>())
            //    {
            //        Hip.gameObject.AddComponent<Rigidbody>();
            //    }
            //}
            gameObject.SetActive(value);
            animator.enabled = value;
            active = value;
        }
    }

    public void Init(Vector3 start, Vector3 end)
    {
        //cache properties of this game object
        transform = this.GetComponent<Transform>();
        //renderer = this.GetComponent<Renderer>();
        //var currentColor = renderer.material.color;
        //currentColor.a = 0f;
        //renderer.material.color = currentColor;
        animator = this.GetComponent<Animator>();

        this.start = start;
        this.end = end;

        transform.position = start;
        transform.rotation = Quaternion.LookRotation(end - start);
        Active = true;
    }

    public void Render()
    {
        float distance = (transform.position - start).sqrMagnitude;
        float visibility = 1f;

        if (distance > fadeDistance)
        {
            distance = (transform.position - end).sqrMagnitude;
            if (distance > fadeDistance)
            {
                visibility = 1f;
            }
            else
            {
                visibility = distance;
            }
        }
        else
        {
            visibility = distance;
        }
        renderer.material.color = Color.Lerp(visible, invisible, visibility);
    }
    
    public void Walk()
    {
        transform.position += (end - start).normalized * moveSpeed * Time.deltaTime;
        if (Vector3.Distance(transform.position, end) < 1f) Active = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(start,end);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(start,0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(end,0.2f);
    }
}
