using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Utils.Singleton;

public class ChartObjectManager : MonoSingleton<ChartObjectManager>
{
    public Transform ChartObjectViewContent;

    public ChartObjectControl ChartObjectPrefab;

    public List<ChartObjectControl> ChartObjectControls;

    public void Init()
    {
        GetIndex = 0;

        for (var i = -1; i < EditManager.Instance.EditingChart.Tracks.Count; i++)
        {
            var control = GetChartObjectControl();
            control.Init(i);
        }

        for (var i = GetIndex; i < ChartObjectControls.Count; i++)
        {
            ChartObjectControls[i].gameObject.SetActive(false);
        }
    }


    private int GetIndex;

    ChartObjectControl GetChartObjectControl()
    {
        GetIndex++;

        if (GetIndex > ChartObjectControls.Count)
        {
            ChartObjectControls.Add(Instantiate(ChartObjectPrefab, ChartObjectViewContent));
        }
        return ChartObjectControls[GetIndex - 1];
    }
}
