using UnityEngine;
using UnityEngine.UI;
using PDRO.Data;
using PDRO.Utils.Singleton;
using PDRO.Utils;

public class EditLinePanel : MonoSingleton<EditLinePanel>
{
    public LineData CurrentData;

    public InputField TargetTrackIndex;
    public InputField HitTimeInput, TargetHitTimeInput, PosXInput, TargetPosXInput;
    public InputField SR, SG, SB, SA, ER, EG, EB, EA;
    public Dropdown FallDropdown, TargetFallDropdown;

    public Button DeleteButton, ClosePanelButton;

    protected override void OnAwake()
    {
        DeleteButton.onClick.AddListener(Delete);
        ClosePanelButton.onClick.AddListener(ClosePanel);

        TargetTrackIndex.onEndEdit.AddListener(OnEndEditTargetTrackID);

        HitTimeInput.onEndEdit.AddListener(OnEndEditHitTime);
        TargetHitTimeInput.onEndEdit.AddListener(OnEndEditTargetHitTime);

        PosXInput.onEndEdit.AddListener(OnEndEditPosX);
        TargetPosXInput.onEndEdit.AddListener(OnEndEditTargetPosX);

        SR.onEndEdit.AddListener(OnEndEditSR);
        SG.onEndEdit.AddListener(OnEndEditSG);
        SB.onEndEdit.AddListener(OnEndEditSB);
        SA.onEndEdit.AddListener(OnEndEditSA);

        ER.onEndEdit.AddListener(OnEndEditER);
        EG.onEndEdit.AddListener(OnEndEditEG);
        EB.onEndEdit.AddListener(OnEndEditEB);
        EA.onEndEdit.AddListener(OnEndEditEA);

        FallDropdown.onValueChanged.AddListener(OnEndEditFallDown);
        TargetFallDropdown.onValueChanged.AddListener(OnEndEditTargetFallDown);

    }
    public void OnEndEditSR(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            CurrentData.StartColor.r = to01;
        }
        SR.text = (Mathf.RoundToInt(CurrentData.StartColor.r * 255f)).ToString();
        OnEditValue();
    }
    public void OnEndEditSG(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            CurrentData.StartColor.g = to01;
        }
        SG.text = (Mathf.RoundToInt(CurrentData.StartColor.g * 255f)).ToString();
        OnEditValue();
    }
    public void OnEndEditSB(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            CurrentData.StartColor.b = to01;
        }
        SB.text = (Mathf.RoundToInt(CurrentData.StartColor.b * 255f)).ToString();
        OnEditValue();
    }
    public void OnEndEditSA(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            CurrentData.StartColor.a = to01;
        }
        SA.text = (Mathf.RoundToInt(CurrentData.StartColor.a * 255f)).ToString();
        OnEditValue();
    }
    public void OnEndEditER(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            CurrentData.EndColor.r = to01;
        }
        ER.text = (Mathf.RoundToInt(CurrentData.EndColor.r * 255f)).ToString();
        OnEditValue();
    }
    public void OnEndEditEG(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            CurrentData.EndColor.g = to01;
        }
        EG.text = (Mathf.RoundToInt(CurrentData.EndColor.g * 255f)).ToString();
        OnEditValue();
    }
    public void OnEndEditEB(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            CurrentData.EndColor.b = to01;
        }
        EB.text = (Mathf.RoundToInt(CurrentData.EndColor.b * 255f)).ToString();
        OnEditValue();
    }
    public void OnEndEditEA(string value)
    {
        if (float.TryParse(value, out float color))
        {
            var to01 = Mathf.Clamp01(color / 255f);
            CurrentData.EndColor.a = to01;
        }
        EA.text = (Mathf.RoundToInt(CurrentData.EndColor.a * 255f)).ToString();
        OnEditValue();
    }

    public void OnEndEditHitTime(string value)
    {
        if (int.TryParse(value, out int time))
        {
            CurrentData.HitTime = time * 0.001f;
        }
        HitTimeInput.text = (Mathf.RoundToInt(CurrentData.HitTime * 1000f)).ToString();
        OnEditValue(true);
    }

    public void OnEndEditTargetHitTime(string value)
    {
        if (int.TryParse(value, out int time))
        {
            CurrentData.TargetHitTime = Mathf.Max(time * 0.001f, CurrentData.HitTime);
        }
        TargetHitTimeInput.text = (Mathf.RoundToInt(CurrentData.TargetHitTime * 1000f)).ToString();
        OnEditValue(false);
    }

    public void OnEndEditPosX(string value)
    {
        if (float.TryParse(value, out float posX))
        {
            CurrentData.PosX = posX;
        }
        PosXInput.text = CurrentData.PosX.ToString();
        OnEditValue();
    }
    public void OnEndEditTargetPosX(string value)
    {
        if (float.TryParse(value, out float posX))
        {
            CurrentData.TargetPosX = posX;
        }
        TargetPosXInput.text = CurrentData.TargetPosX.ToString();
        OnEditValue();
    }

    public void OnEndEditFallDown(int value)
    {
        CurrentData.FallDirection = value == 0 ? FallingDirection.Up : FallingDirection.Down;
        OnEditValue();
    }

    public void OnEndEditTargetFallDown(int value)
    {
        CurrentData.TargetFallDirection = value == 0 ? FallingDirection.Up : FallingDirection.Down;
        OnEditValue();
    }

    public void OnEndEditTargetTrackID(string value)
    {
        if (int.TryParse(value, out int id) && id > -1 && id < EditManager.Instance.Tracks.Length)
        {
            CurrentData.TargetTrackIndex = id;
        }
        TargetTrackIndex.text = CurrentData.TargetTrackIndex.ToString();
        OnEditValue();
    }

    public void ShowPanel(LineData data)
    {
        CurrentData = data;

        TargetTrackIndex.text = CurrentData.TargetTrackIndex.ToString();

        HitTimeInput.text = (Mathf.RoundToInt(CurrentData.HitTime * 1000f)).ToString();
        TargetHitTimeInput.text = (Mathf.RoundToInt(CurrentData.TargetHitTime * 1000f)).ToString();

        PosXInput.text = CurrentData.PosX.ToString();
        TargetPosXInput.text = CurrentData.TargetPosX.ToString();

        SR.text = (Mathf.RoundToInt(CurrentData.StartColor.r * 255f)).ToString();
        SG.text = (Mathf.RoundToInt(CurrentData.StartColor.g * 255f)).ToString();
        SB.text = (Mathf.RoundToInt(CurrentData.StartColor.b * 255f)).ToString();
        SA.text = (Mathf.RoundToInt(CurrentData.StartColor.a * 255f)).ToString();

        ER.text = (Mathf.RoundToInt(CurrentData.EndColor.r * 255f)).ToString();
        EG.text = (Mathf.RoundToInt(CurrentData.EndColor.g * 255f)).ToString();
        EB.text = (Mathf.RoundToInt(CurrentData.EndColor.b * 255f)).ToString();
        EA.text = (Mathf.RoundToInt(CurrentData.EndColor.a * 255f)).ToString();

        FallDropdown.value = CurrentData.FallDirection is FallingDirection.Up ? 0 : 1;
        TargetFallDropdown.value = CurrentData.TargetFallDirection is FallingDirection.Up ? 0 : 1;

        this.gameObject.SetActive(true);

        NoteEditPanelControl.Instance.ClosePanel();
    }


    void Delete()
    {
        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Lines.Remove(CurrentData);

        OnEditValue();
    }


    public void OnEditValue(bool sort = false)
    {
        if (sort)
        {
            EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Lines.Sort();
        }

        EditManager.Instance.Reload(false);
    }

    public void ClosePanel()
    {
        CurrentData = null;
        this.gameObject.SetActive(false);
    }

    void Update()
    {
        //按下S拖动开始时间
        if (Input.GetKeyDown(KeyCode.S))
        {
            EditNoteManager.Instance.DragStartTime(CurrentData);
        }
        //按下D拖动结束时间
        else if (Input.GetKeyDown(KeyCode.D))
        {
            EditNoteManager.Instance.DragEndTime(CurrentData);
        }

        if (!EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Lines.Contains(CurrentData)) ClosePanel();
    }
}
