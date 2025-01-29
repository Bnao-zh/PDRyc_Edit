using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChartObjectControl : MonoBehaviour
{
    public Button BaseButton, UpButton, DownButton;
    public InputField IDInput, FatherIDInput, RemarksInput;
    public int ObjectID;

    void Awake()
    {
        BaseButton.onClick.AddListener(StartEdit);
        UpButton.onClick.AddListener(UpMove);
        DownButton.onClick.AddListener(DownMove);

        FatherIDInput.onEndEdit.AddListener(OnEndEditFatherID);
        RemarksInput.onEndEdit.AddListener(OnEndEditRemarks);
    }

    void StartEdit()
    {
        EditManager.Instance.StartEdit(ObjectID);
    }

    void UpMove()
    {
        TrackMoveManager.Instance.UpMove(ObjectID);
    }

    void DownMove()
    {
        TrackMoveManager.Instance.DownMove(ObjectID);
    }

    void OnEndEditFatherID(string value)
    {
        if (int.TryParse(value, out int id) && id > -2 && id < EditManager.Instance.EditingChart.Tracks.Count && id != ObjectID)
        {
            SetFatherID(id);
        }
        else if (value != "无")
        {
            FatherIDInput.text = GetFatherID();
            Debug.LogError("输入了不正确的父轨道id，请重新输入");
        }
    }

    void SetFatherID(int FatherID)
    {
        if (ObjectID == -1)
        {
            EditManager.Instance.EditingChart.Camera.FatherTrackIndex = FatherID;
        }
        else
        {
            EditManager.Instance.EditingChart.Tracks[ObjectID].FatherTrackIndex = FatherID;
        }

        EditManager.Instance.Reload(false);
    }

    void OnEndEditRemarks(string value)
    {
        EditManager.Instance.EditingChart.Tracks[ObjectID].Remarks = value;
    }

    public void Init(int id)
    {
        this.gameObject.SetActive(true);
        
        ObjectID = id;
        IDInput.text = ObjectID.ToString();

        FatherIDInput.text = GetFatherID();

        if (id != -1)
        {
            RemarksInput.text = GetRemarks();
        }
        else
        {
            RemarksInput.text = "我是唯一的光（摄像机）";
            RemarksInput.interactable = false;
        }
    }

    void Update()
    {
        if (EditManager.Instance.CurrentEditType is not EditManager.EditType.None && EditManager.Instance.EditChartObjectIndex == ObjectID)
        {
            BaseButton.image.color = Color.black;
        }
        else
        {
            BaseButton.image.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        }
    }

    string GetFatherID()
    {
        if (ObjectID == -1)
        {
            return EditManager.Instance.EditingChart.Camera.FatherTrackIndex == -1 ? "无" : EditManager.Instance.EditingChart.Camera.FatherTrackIndex.ToString();
        }
        else
        {
            return EditManager.Instance.EditingChart.Tracks[ObjectID].FatherTrackIndex == -1 ? "无" : EditManager.Instance.EditingChart.Tracks[ObjectID].FatherTrackIndex.ToString();
        }
    }

    string GetRemarks()
    {
        return EditManager.Instance.EditingChart.Tracks[ObjectID].Remarks;
    }

}
