using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Utils.Singleton;
using System.Linq;

public class BeatLineManager : MonoSingleton<BeatLineManager>
{
    public BeatLineControl BeatLinePrefab;

    public RectTransform BeatLineTransform;

    public List<BeatLineControl> BeatLines = new();
    public List<BeatLineControl> CurrentBeatLines => BeatLines.Where(x => x.gameObject.activeSelf).ToList();

    public int MaxBeat;

    public void SetBeatLineTime(int X分音, float bpm, float TotalLength)
    {
        var per = (60 / bpm);
        MaxBeat = (int)(TotalLength / per + 1f);

        return;
    }


    public void OnUpdate()
    {
        if (EditManager.Instance.CurrentEditType is EditManager.EditType.None) return;

        GetIndex = 0;

        for (var i = 0; i < MaxBeat; i++)
        {
            for (var j = 0; j < EditManager.Instance.X分音; j++)
            {
                var beat = i + 1f * j / EditManager.Instance.X分音;
                //求出差
                var dis = (beat - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;
                if (dis < -400) continue;
                if (dis > 1200f) break;

                var ins = GetBeatLine();
                ins.Init(i, EditManager.Instance.X分音, j);
                ins.gameObject.SetActive(true);
                ins.transform.localPosition = new Vector3(0f, dis, 0f);
            }
        }


        for (var i = GetIndex; i < BeatLines.Count; i++)
        {
            BeatLines[i].gameObject.SetActive(false);
        }
    }

    private int GetIndex;

    BeatLineControl GetBeatLine()
    {
        GetIndex++;

        if (GetIndex > BeatLines.Count)
        {
            var ins = Instantiate(BeatLinePrefab, BeatLineTransform);
            BeatLines.Add(ins);
        }

        return BeatLines[GetIndex - 1];
    }
}
