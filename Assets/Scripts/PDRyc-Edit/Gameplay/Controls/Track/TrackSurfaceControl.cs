using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PDRO.Utils;

public class TrackSurfaceControl : MonoBehaviour
{
    [SerializeField] private SpriteRenderer SurfaceRenderer;
    public void SetSurfaceLength(float length, bool isUp)
    {
        this.transform.localScale = new Vector3(0.1f, length * 0.1f, 1f);
        this.transform.localPosition = new Vector3(0f, isUp ? length * 1.024f  : -length, 0f);
    }

    public void SetColor(Color color, bool isTouching)
    {
        if (!isTouching)
        {
            color.a = color.a * 0.8f;
        }

        //关掉节省资源喵
        if (color.a < 0.01f)
        {
            SurfaceRenderer.enabled = false;
        }
        else
        {
            SurfaceRenderer.enabled = true;
            SurfaceRenderer.color = color;
        }
    }
}
