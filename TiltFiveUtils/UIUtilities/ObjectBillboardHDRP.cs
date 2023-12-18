using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class ObjectBillboardHDRP : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        TiltFive.PlayerIndex currentPlayer = TiltFive.PlayerIndex.None;
        foreach (TiltFive.PlayerIndex index in Enum.GetValues(typeof(TiltFive.PlayerIndex)))
        {
            if (TiltFive.Glasses.GetPoseRoot(index) == camera.transform.parent.gameObject)
            {
                currentPlayer = index;
            }
        }
        if (currentPlayer != TiltFive.PlayerIndex.None)
        {
            //Instead of averaging the two up vectors, either one can be used independently as well depending on user preference.
            transform.LookAt(TiltFive.Glasses.GetPoseRoot(currentPlayer).transform, Vector3.Lerp(TiltFive.Glasses.GetPoseRoot(currentPlayer).transform.up, Vector3.up, 0.5f));
        }
    }

    void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }
}
