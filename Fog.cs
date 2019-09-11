using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.Experimental.Rendering.HDPipeline;

public class Fog : MonoBehaviour
{

    [SerializeField] bool SRP;
    [SerializeField] Material CookieCreator;
    [SerializeField] Material CookieBlur;
    [SerializeField] List<Transform> Center;
    [SerializeField] int CastResolution = 32;
    [SerializeField] int TextureResolution = 128;
    [SerializeField] float CastPointHeight = 1.5f;
    [Range(0, 12)]
    [SerializeField] int PastTexCount = 12;
    [SerializeField] float Radius = 3;

    private RenderTexture cookieMask, cookieBlurred;
    private Projector projector;
    //private DecalProjectorComponent decal;
    private RaycastHit hit;
    private float[] data, data_sec;
    private float projectorSize;
    private float d_angle;
    private RenderTexture[] pastMasks;
    private int c_pastMask = 0;

    void Start()
    {
        cookieMask = new RenderTexture(TextureResolution, TextureResolution, 0);
        cookieBlurred = new RenderTexture(TextureResolution, TextureResolution, 0);
        //PastTexCount = CookieBlur.GetInt("_PastTexCount");
        pastMasks = new RenderTexture[PastTexCount];
        for (int i = 0; i < PastTexCount; i++)
        {
            pastMasks[i] = new RenderTexture(TextureResolution / 2, TextureResolution / 2, 0);
            CookieBlur.SetTexture("_PastTex" + i.ToString(), pastMasks[i]);
        }
        /*if (SRP)
        {
            decal = GetComponent<DecalProjectorComponent>();
            decal.m_Material.SetTexture("_BaseColorMap", cookieBlurred);
            projectorSize = decal.m_Size.x;
        }
        else
        {*/
            projector = GetComponent<Projector>();
            projector.material.SetTexture("_ShadowTex", cookieBlurred);
            projectorSize = projector.orthographicSize * 2;
        //}

        data = new float[CastResolution * 2 + 3];
        data[0] = CastResolution;
        d_angle = 360 * Mathf.Deg2Rad / CastResolution;
    }

    public void SetCookie() {
        CookieBlur.SetInt("_PastTexCount", PastTexCount);
        float totalProjectorSize = projectorSize + 0.5f;

        for (int k = 0; k < Center.Count; k++) {
            Vector3 castPoint = Center[k].position;
            //castPoint.y = CastPointHeight;
            for (int i = 0; i < CastResolution; i++) {
                Vector3 dir = Vector3.one;
                dir.x *= Mathf.Cos(d_angle * i);
                dir.z *= Mathf.Sin(d_angle * i);
                dir.y = 0;

                if (Physics.Raycast(castPoint, dir, out hit, Radius)) {
                    data[3 + i * 2] = (hit.point.x - transform.position.x) / totalProjectorSize;
                    data[4 + i * 2] = (hit.point.z - transform.position.z) / totalProjectorSize;
                } else {
                    Vector3 point = dir.normalized * Radius + castPoint;
                    data[3 + i * 2] = (point.x - transform.position.x) / totalProjectorSize;
                    data[4 + i * 2] = (point.z - transform.position.z) / totalProjectorSize;
                }
            }
            data[1] = (castPoint.x - transform.position.x) / totalProjectorSize;
            data[2] = (castPoint.z - transform.position.z) / totalProjectorSize;

            CookieCreator.SetFloatArray("_Data", data);
            Graphics.Blit(null, cookieMask, CookieCreator);
            Graphics.Blit(cookieMask, cookieBlurred, CookieBlur);
            Graphics.Blit(cookieBlurred, pastMasks[c_pastMask++]);
            if (c_pastMask >= PastTexCount) {
                c_pastMask = 0;
            }
        }
    }

    //Add viewable Center
    public void AddCenter(Transform obj) {
        Center.Add(obj);
    }

    public void RemoveCenter(Transform obj) {
        Center.Remove(obj);
    }
}
