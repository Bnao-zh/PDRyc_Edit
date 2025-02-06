using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PDRO.Data;

public class EditLineControl : MonoBehaviour
{

    public RectTransform NoteRect;
    public Button CurrentButton;
    public LineData CurrentData;

    public Text StartText, EndText;


    void Awake()
    {
        CurrentButton.onClick.AddListener(TryShowPanel);
    }

    public void Init(LineData data)
    {
        CurrentData = data;

        StartText.text = EditManager.Instance.EditChartObjectIndex.ToString();
        EndText.text = CurrentData.TargetTrackIndex.ToString();

        StartText.color = CurrentData.StartColor;
        EndText.color = CurrentData.EndColor;
    }

    void Update()
    {
        if (EditLinePanel.Instance.CurrentData == CurrentData)
        {
            CurrentButton.image.color = Color.black;
        }
        else
        {
            CurrentButton.image.color = new Color(1f, 1f, 1f, 0.7f);
        }
    }


    void TryShowPanel()
    {
        if (Input.GetMouseButton(1))
        {
            EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Lines.Remove(CurrentData);

            //毕竟删除东西是不影响排序的
            EditManager.Instance.Reload(false);
        }
        else
        {
            EditLinePanel.Instance.ShowPanel(CurrentData);
        }
    }
}
