using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Data;
using UnityEngine.UI;

public class EditNoteControl : MonoBehaviour
{
    public RectTransform NoteRect;
    public Text NoteTypeText;
    public Button CurrentButton;

    public NoteData CurrentData;

    void Awake()
    {
        CurrentButton.onClick.AddListener(TryShowPanel);
    }
    public void Init(NoteData data)
    {
        CurrentData = data;

        NoteTypeText.text = data.NoteType.ToString();

        CurrentButton.image.color = data.NoteType switch
        {
            NoteType.Tap => new Color(0.69f, 0.96f, 0.937f, 0.75f),
            NoteType.Drag => new Color(0.953f, 0.866f, 0.49f, 0.75f),
            NoteType.Hold => new Color(0.69f, 0.96f, 0.063f, 0.75f),
            NoteType.Flick => new Color(1f, 0.384f, 0.3f, 0.75f),
            _ => throw new System.Exception("aaaaaaaaaaa")
        };
    }

    void Update()
    {
        if (NoteEditPanelControl.Instance.CurrentData.Contains(CurrentData))
        {
            CurrentButton.image.color = Color.black;
        }
        else
        {
            CurrentButton.image.color = CurrentData.NoteType switch
            {
                NoteType.Tap => new Color(0.69f, 0.96f, 0.937f, 0.75f),
                NoteType.Drag => new Color(0.953f, 0.866f, 0.49f, 0.75f),
                NoteType.Hold => new Color(0.69f, 0.96f, 0.063f, 0.75f),
                NoteType.Flick => new Color(1f, 0.384f, 0.3f, 0.75f),
                _ => throw new System.Exception("aaaaaaaaaaa")
            };
        }
    }

    void TryShowPanel()
    {
        if (Input.GetMouseButton(1))
        {
            EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes.Remove(CurrentData);

            //毕竟删除东西是不影响排序的
            EditManager.Instance.Reload(false);
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                NoteEditPanelControl.Instance.TryAddNoteToEdit(CurrentData, true);
            }
            else
            {
                NoteEditPanelControl.Instance.TryAddNoteToEdit(CurrentData, false);
            }
        }
    }

}
