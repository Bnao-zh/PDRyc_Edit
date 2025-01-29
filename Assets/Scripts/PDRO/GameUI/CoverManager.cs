using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//让我们来想一下一个任意比例的遮罩应该怎么实现

public class CoverManager : MonoBehaviour
{
    private float AspectRatioDelta;
    private float ScreenAspectRatio;

    public RectTransform MaskTransform;

    void UpdateAspectRatio()
    {
        ScreenAspectRatio = 1f * Screen.width / Screen.height;
        AspectRatioDelta = Mathf.Clamp01(ScreenAspectRatio / EditManager.Instance.EditingChart.MetaData.TargetAspectRatio);
    }

    void Update()
    {
        UpdateAspectRatio();

        MaskTransform.sizeDelta = new Vector2(1080f * EditManager.Instance.EditingChart.MetaData.TargetAspectRatio, 1080f * AspectRatioDelta);
    }


}
