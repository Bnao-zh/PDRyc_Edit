using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIToggleMove : MonoBehaviour
{
    public Vector3 EnablePos;
    public Vector3 DisablePos;
    public Button CurrentButton;
    public RectTransform RectTrans => this.gameObject.GetComponent<RectTransform>();
    public float Duration;
    void Awake()
    {
        CurrentButton.onClick.AddListener(Execute);
    }

    public bool IsEnable;
    void Execute()
    {
        IsEnable = !IsEnable;

        RectTrans.DOKill();
        DOTween.To(() => IsEnable ? DisablePos : EnablePos, x => RectTrans.anchoredPosition = x, IsEnable ? EnablePos : DisablePos, Duration);
        //Rect.DOLocalMove(IsEnable ? EnablePos : DisablePos, Duration);
    }
}
