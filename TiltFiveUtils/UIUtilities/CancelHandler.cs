using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CancelHandler : MonoBehaviour, ICancelHandler
{
    [SerializeField]
    private UnityEngine.Events.UnityEvent onCancel;

    public void OnCancel(BaseEventData eventData)
    {
        onCancel.Invoke();
    }
}
