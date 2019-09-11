using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class PreRenderCalls : MonoBehaviour
{
    [SerializeField] bool UsingSRP;
    [SerializeField] Fog _Fog;

    void OnEnable()
    {
        if (UsingSRP)
        {
            RenderPipeline.beginCameraRendering += PreRenderSRP;
        }
    }

    void OnDisable()
    {
        if (UsingSRP)
        {
            RenderPipeline.beginCameraRendering -= PreRenderSRP;
        }
    }


    void PreRenderSRP(Camera obj)
    {
        OnPreRender();
    }

    void OnPreRender()
    {
        // FOG CALL
        _Fog.SetCookie();
    }


}
