using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnEnableEvent : MonoBehaviour
{
    [SerializeField] UnityEvent Event;
    private void OnEnable()
    {
        Event.Invoke();
    }
}
