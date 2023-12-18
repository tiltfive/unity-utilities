using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBillboard : MonoBehaviour
{

    public TiltFive.PlayerIndex currentPlayer = TiltFive.PlayerIndex.None;

    public GameObject objectToRotate;
    // Start is called before the first frame update
    void Start()
    {
        foreach(TiltFive.PlayerIndex index in Enum.GetValues(typeof(TiltFive.PlayerIndex)))
        {
            if(TiltFive.Glasses.GetPoseRoot(index) == transform.parent.gameObject)
            {
                currentPlayer = index;
            }
        }
    }

    private void OnPreRender()
    {
        if(currentPlayer != TiltFive.PlayerIndex.None)
        {
            if(objectToRotate != null)
            {
                //Instead of averaging the two up vectors, either one can be used independently as well depending on user preference.
                objectToRotate.transform.LookAt(TiltFive.Glasses.GetPoseRoot(currentPlayer).transform, Vector3.Lerp(TiltFive.Glasses.GetPoseRoot(currentPlayer).transform.up, Vector3.up, 0.5f));
            }
        }
    }
}
