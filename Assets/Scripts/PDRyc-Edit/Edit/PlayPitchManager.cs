using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayPitchManager : MonoBehaviour
{
    public Text NowPitch;
    public Button a, aa, aaa, aaaa, aaaaa, aaaaaa;

    void Start()
    {
        ReadPlayPitch();

        a.onClick.AddListener(() => SetPitch(0.25f));
        aa.onClick.AddListener(() => SetPitch(0.5f));
        aaa.onClick.AddListener(() => SetPitch(0.75f));
        aaaa.onClick.AddListener(() => SetPitch(1f));
        aaaaa.onClick.AddListener(() => SetPitch(2f));
        aaaaaa.onClick.AddListener(() => SetPitch(4f));
    }

    void ReadPlayPitch()
    {
        var pitch = EditManager.Instance.EditAudioSource.pitch;
        NowPitch.text = $"当前播放速度：{pitch:F2}x";
    }


    void SetPitch(float pitch)
    {
        EditManager.Instance.EditAudioSource.pitch = pitch;

        ReadPlayPitch();
    }
}
