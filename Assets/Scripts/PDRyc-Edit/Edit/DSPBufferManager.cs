using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DSPBufferManager : MonoBehaviour
{
    public Text NowDSPBuffer;
    public Button a, aa, aaa, aaaa, aaaaa, aaaaaa;

    void Awake()
    {
        ReadDSPBufferSize();
        SetDspBuffer(384);

        a.onClick.AddListener(() => SetDspBuffer(256));
        aa.onClick.AddListener(() => SetDspBuffer(384));
        aaa.onClick.AddListener(() => SetDspBuffer(512));
        aaaa.onClick.AddListener(() => SetDspBuffer(768));
        aaaaa.onClick.AddListener(() => SetDspBuffer(1024));
        aaaaaa.onClick.AddListener(() => SetDspBuffer(2048));
    }

    void ReadDSPBufferSize()
    {
        AudioSettings.GetDSPBufferSize(out var dspbuffer, out _);
        NowDSPBuffer.text = $"当前DSP Buffer：{dspbuffer}";
    }


    void SetDspBuffer(int dsp)
    {
        var config = AudioSettings.GetConfiguration();
        config.dspBufferSize = dsp;
        AudioSettings.Reset(config);

        ReadDSPBufferSize();
    }
}
