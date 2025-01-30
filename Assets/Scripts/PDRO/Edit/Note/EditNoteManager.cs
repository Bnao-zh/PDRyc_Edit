using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Data;
using System.Linq;
using UnityEngine.UI;
using PDRO.Utils.Singleton;

public class EditNoteManager : MonoSingleton<EditNoteManager>
{
    public RectTransform EditNoteTransform;
    public RectTransform TrackWidthIndicator;
    public Image TrackWidthIndicatorImage;
    public InputField IntClampInput;
    private float ClampFactor = 1f;

    //X坐标细分的竖线
    public LineControl PosXLinePrefab;
    public List<LineControl> PosXLines = new();
    private int GetPosXLineIndex;
    LineControl GetPosXLine()
    {
        GetPosXLineIndex++;

        if (GetPosXLineIndex > PosXLines.Count)
        {
            PosXLines.Add(Instantiate(PosXLinePrefab, EditNoteTransform));
        }
        return PosXLines[GetPosXLineIndex - 1];
    }

    //谱面内的连线
    public EditLineControl LinePrefab;
    public List<EditLineControl> Lines = new();
    private int GetLineIndex;
    EditLineControl GetLine()
    {
        GetLineIndex++;

        if (GetLineIndex > Lines.Count)
        {
            Lines.Add(Instantiate(LinePrefab, EditNoteTransform));
        }
        return Lines[GetLineIndex - 1];
    }

    //Note
    public EditNoteControl EditNotePrefab;
    public List<EditNoteControl> EditNotes = new();
    private int GetNoteIndex;
    EditNoteControl GetEditNote()
    {
        GetNoteIndex++;

        if (GetNoteIndex > EditNotes.Count)
        {
            EditNotes.Add(Instantiate(EditNotePrefab, EditNoteTransform));
        }

        return EditNotes[GetNoteIndex - 1];
    }

    protected override void OnAwake()
    {
        IntClampInput.text = ClampFactor.ToString();
        IntClampInput.onEndEdit.AddListener(OnEndEditClamp);
    }

    //设为0则不限制吧
    void OnEndEditClamp(string value)
    {
        if (float.TryParse(value, out float temp))
        {
            if (temp < 0)
            {
                Debug.LogWarning("负数是无效的哦");
            }
            else
            {
                ClampFactor = temp;
            }
        }
        IntClampInput.text = ClampFactor.ToString();
    }

    public void OnUpdate()
    {
        if (EditManager.Instance.CurrentEditType is EditManager.EditType.None or EditManager.EditType.Events)
        {
            EditNoteTransform.gameObject.SetActive(false);
            return;
        }
        else
        {
            EditNoteTransform.gameObject.SetActive(true);
        }

        //清一下试试？
        if (thisFrameHasSortAndReloadNotes)
        {
            EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes.Sort();
            EditManager.Instance.Reload(false);
            thisFrameHasSortAndReloadNotes = false;
        }

        //更新轨道粗细指示器
        TrackWidthIndicator.sizeDelta = new Vector2(Mathf.Abs(2f * EditManager.Instance.Tracks[EditManager.Instance.EditChartObjectIndex].ScaleX / EditManager.Instance.EditNotePosXFactor), 5000f);
        TrackWidthIndicatorImage.color = EditManager.Instance.CurrentEditType is EditManager.EditType.UpNotes ? new Color(0f, 1f, 1f, 0.3f) : new Color(1f, 0f, 0f, 0.3f);

        GetPosXLineIndex = 0;
        //画Note可能位置的竖线
        if (ClampFactor > 0.001f)
        {
            var temp = 0;
            while (true)
            {
                var posX = ClampFactor * temp / EditManager.Instance.EditNotePosXFactor;
                if (posX > 250f) break;

                var ins = GetPosXLine();
                ins.gameObject.SetActive(true);
                ins.transform.localPosition = new Vector3(posX, 0f, 0f);
                ins.beatLineImage.color = temp % 2 == 0 ? new Color(1f, 1f, 1f, 0.6f) : new Color(0f, 0f, 0f, 0.6f);

                temp++;
            }
            temp = 1;
            while (true)
            {
                var posX = -ClampFactor * temp / EditManager.Instance.EditNotePosXFactor;
                if (posX < -250f) break;

                var ins = GetPosXLine();
                ins.gameObject.SetActive(true);
                ins.transform.localPosition = new Vector3(posX, 0f, 0f);
                ins.beatLineImage.color = temp % 2 == 0 ? new Color(1f, 1f, 1f, 0.6f) : new Color(0f, 0f, 0f, 0.6f);

                temp++;
            }
        }

        //更新Note
        GetNoteIndex = 0;
        var notes = EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes;

        for (var i = 0; i < notes.Count; i++)
        {
            var note = notes[i];

            //不是这面的Note就先略过
            if (EditManager.Instance.CurrentEditType is EditManager.EditType.UpNotes)
            {
                if (note.FallDirection is not FallingDirection.Up) continue;
            }
            else
            {
                if (note.FallDirection is not FallingDirection.Down) continue;
            }

            var beat = EditManager.Instance.Time2Beat(note.HitTime);

            var dis = (beat - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;
            var endDis = 0f;
            if (note.NoteType is PDRO.Data.NoteType.Hold)
            {
                endDis = (EditManager.Instance.Time2Beat(note.HitTime + note.HoldTime) - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;
                if (endDis < -400) continue;
            }
            else
            {
                if (dis < -400f) continue;
            }
            if (dis > 1200f) break;

            RenderEditNote(note, dis, endDis);
        }

        //更新连线
        GetLineIndex = 0;
        var lines = EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Lines;
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            //不是这面的Note就先略过
            if (EditManager.Instance.CurrentEditType is EditManager.EditType.UpNotes)
            {
                if (line.FallDirection is not FallingDirection.Up) continue;
            }
            else
            {
                if (line.FallDirection is not FallingDirection.Down) continue;
            }

            var beat = EditManager.Instance.Time2Beat(line.HitTime);

            var dis = (beat - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;
            var endDis = (EditManager.Instance.Time2Beat(line.TargetHitTime) - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;

            if (endDis < -400f) continue;
            if (dis > 1200f) break;

            RenderEditLine(line, dis, endDis);
        }


        //关闭不用的
        for (var i = GetLineIndex; i < Lines.Count; i++)
        {
            Lines[i].gameObject.SetActive(false);
        }

        for (var i = GetNoteIndex; i < EditNotes.Count; i++)
        {
            EditNotes[i].gameObject.SetActive(false);
        }

        for (var i = GetPosXLineIndex; i < PosXLines.Count; i++)
        {
            PosXLines[i].gameObject.SetActive(false);
        }

        UpdateInput();
    }

    void RenderNoteByNoteData(NoteData note)
    {
        var dis = (EditManager.Instance.Time2Beat(note.HitTime) - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;
        var endDis = (EditManager.Instance.Time2Beat(note.HitTime + note.HoldTime) - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;

        RenderEditNote(note, dis, endDis);
    }

    void RenderEditNote(NoteData note, float dis, float endDis)
    {
        var ins = GetEditNote();
        ins.Init(note);
        ins.gameObject.SetActive(true);

        if (note.NoteType is PDRO.Data.NoteType.Hold)
        {
            if (dis < -400f) dis = -400f;
            if (endDis > 1200f) endDis = 1200f;

            ins.transform.localPosition = new Vector3(note.PosX / EditManager.Instance.EditNotePosXFactor, (dis + endDis) * 0.5f, 0f);
            ins.NoteRect.sizeDelta = new Vector2(2f * note.LengthX / EditManager.Instance.EditNotePosXFactor, Mathf.Max(endDis - dis, 40f));
        }
        else
        {
            ins.transform.localPosition = new Vector3(note.PosX / EditManager.Instance.EditNotePosXFactor, dis, 0f);
            ins.NoteRect.sizeDelta = new Vector2(2f * note.LengthX / EditManager.Instance.EditNotePosXFactor, 40f);
        }
    }

    void RenderEditLinByLineData(LineData line)
    {
        var dis = (EditManager.Instance.Time2Beat(line.HitTime) - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;
        var endDis = (EditManager.Instance.Time2Beat(line.TargetHitTime) - EditManager.Instance.NowBeat) * EditManager.Instance.EditFallSpeed;

        RenderEditLine(line, dis, endDis);
    }

    void RenderEditLine(LineData line, float dis, float endDis)
    {
        var ins = GetLine();
        ins.Init(line);
        ins.gameObject.SetActive(true);

        ins.transform.localPosition = new Vector3(line.PosX / EditManager.Instance.EditNotePosXFactor, (dis + endDis) * 0.5f, 0f);
        ins.NoteRect.sizeDelta = new Vector2(40f, Mathf.Max(endDis - dis, 40f));
    }


    private void UpdateInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            AddNote(NoteType.Tap, Input.mousePosition);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            AddNote(NoteType.Drag, Input.mousePosition);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddNote(NoteType.Flick, Input.mousePosition);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            AddNote(NoteType.Hold, Input.mousePosition);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            AddLine(Input.mousePosition);
        }
    }

    void AddLine(Vector2 mousePos)
    {
        var beatLine = BeatLineManager.Instance.CurrentBeatLines
                    .OrderBy(x => Mathf.Abs(x.transform.position.y - mousePos.y)).First();

        var posX = 0f;

        //接近0则为不限制
        if (ClampFactor > 0.001f)
        {
            var step = ClampFactor / EditManager.Instance.EditNotePosXFactor;
            var temp = ((mousePos.x - EditNoteTransform.position.x) / (EditNoteTransform.position.x / 400f)) / step;

            posX = Mathf.RoundToInt(temp) * step;
        }
        else
        {
            posX = (mousePos.x - EditNoteTransform.position.x) / (EditNoteTransform.position.x / 400f);
        }

        if (Mathf.Abs(posX) > 350f) return;

        StartCoroutine(PutLine(new LineData
        {
            FallDirection = EditManager.Instance.CurrentEditType is EditManager.EditType.UpNotes ? FallingDirection.Up : FallingDirection.Down,
            TargetFallDirection = EditManager.Instance.CurrentEditType is EditManager.EditType.UpNotes ? FallingDirection.Up : FallingDirection.Down,
            HitTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat,
            TargetHitTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat,
            PosX = posX * EditManager.Instance.EditNotePosXFactor,
            TargetPosX = posX * EditManager.Instance.EditNotePosXFactor,

            TargetTrackIndex = EditManager.Instance.EditChartObjectIndex,
            StartColor = Color.white,
            EndColor = Color.white
        }));
    }

    IEnumerator PutLine(LineData data)
    {
        while (!Input.GetKeyUp(KeyCode.L))
        {
            var beatLine = BeatLineManager.Instance.CurrentBeatLines
    .OrderBy(x => Mathf.Abs(x.transform.position.y - Input.mousePosition.y)).First();

            var endTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat;

            data.TargetHitTime = Mathf.Max(endTime, data.HitTime);

            RenderEditLinByLineData(data);

            yield return null;
        }

        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Lines.Add(data);
        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Lines.Sort();
        EditManager.Instance.Reload(false);
    }

    void AddNote(NoteType type, Vector2 mousePos)
    {
        var beatLine = BeatLineManager.Instance.CurrentBeatLines
            .OrderBy(x => Mathf.Abs(x.transform.position.y - mousePos.y)).First();

        var posX = 0f;

        //接近0则为不限制
        if (ClampFactor > 0.001f)
        {
            var step = ClampFactor / EditManager.Instance.EditNotePosXFactor;
            var temp = ((mousePos.x - EditNoteTransform.position.x) / (EditNoteTransform.position.x / 400f)) / step;

            posX = Mathf.RoundToInt(temp) * step;
        }
        else
        {
            posX = (mousePos.x - EditNoteTransform.position.x) / (EditNoteTransform.position.x / 400f);
        }

        if (Mathf.Abs(posX) > 275f) return;

        if (type is NoteType.Hold)
        {
            StartCoroutine(PutHold(new NoteData
            {
                NoteType = type,
                HitTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat,
                HoldTime = 1f,
                FallDirection = EditManager.Instance.CurrentEditType is EditManager.EditType.UpNotes ? FallingDirection.Up : FallingDirection.Down,
                PosX = posX * EditManager.Instance.EditNotePosXFactor
            }));
        }
        else
        {
            EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes.Add(new NoteData
            {
                NoteType = type,
                HitTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat,
                HoldTime = 0f,
                FallDirection = EditManager.Instance.CurrentEditType is EditManager.EditType.UpNotes ? FallingDirection.Up : FallingDirection.Down,
                PosX = posX * EditManager.Instance.EditNotePosXFactor
            });

            EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes.Sort();
            EditManager.Instance.Reload(false);
        }
    }

    IEnumerator PutHold(NoteData data)
    {
        while (!Input.GetKeyUp(KeyCode.R))
        {
            var beatLine = BeatLineManager.Instance.CurrentBeatLines
    .OrderBy(x => Mathf.Abs(x.transform.position.y - Input.mousePosition.y)).First();

            var endTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat;

            data.HoldTime = Mathf.Max(endTime - data.HitTime, 0f);

            RenderNoteByNoteData(data);

            yield return null;
        }

        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes.Add(data);

        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes.Sort();
        EditManager.Instance.Reload(false);
    }

    //给外部用的
    public void DragStartTime(NoteData data)
    {
        StartCoroutine(DragStartTimeCoroutine(data));
    }

    public void DragStartTime(LineData data)
    {
        StartCoroutine(DragStartTimeCoroutine(data));
    }

    public void DragEndTime(NoteData data)
    {
        StartCoroutine(DragEndTimeCoroutine(data));
    }


    public void DragEndTime(LineData data)
    {
        StartCoroutine(DragEndTimeCoroutine(data));
    }

    private bool thisFrameHasSortAndReloadNotes;

    //为的是如果一次拖动多个只需要撤销一次
    void SortAndReloadNotes()
    {
        if (thisFrameHasSortAndReloadNotes) return;
        thisFrameHasSortAndReloadNotes = true;
    }

    //拖动开始时间
    IEnumerator DragStartTimeCoroutine(NoteData data)
    {
        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes.Remove(data);

        var endTime = data.HitTime + data.HoldTime;

        while (!Input.GetKeyUp(KeyCode.S))
        {
            var beatLine = BeatLineManager.Instance.CurrentBeatLines
    .OrderBy(x => Mathf.Abs(x.transform.position.y - Input.mousePosition.y)).First();

            data.HitTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat;
            data.HoldTime = Mathf.Max(endTime - data.HitTime, 0f);

            RenderNoteByNoteData(data);

            yield return null;
        }

        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes.Add(data);

        SortAndReloadNotes();
    }

    IEnumerator DragStartTimeCoroutine(LineData data)
    {
        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Lines.Remove(data);

        while (!Input.GetKeyUp(KeyCode.S))
        {
            var beatLine = BeatLineManager.Instance.CurrentBeatLines
    .OrderBy(x => Mathf.Abs(x.transform.position.y - Input.mousePosition.y)).First();

            data.HitTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat;

            if (data.HitTime > data.TargetHitTime)
            {
                data.TargetHitTime = data.HitTime;
            }

            RenderEditLinByLineData(data);

            yield return null;
        }

        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Lines.Add(data);

        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Lines.Sort();
        EditManager.Instance.Reload(false);
    }

    IEnumerator DragEndTimeCoroutine(NoteData data)
    {
        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes.Remove(data);

        while (!Input.GetKeyUp(KeyCode.D))
        {
            var beatLine = BeatLineManager.Instance.CurrentBeatLines
    .OrderBy(x => Mathf.Abs(x.transform.position.y - Input.mousePosition.y)).First();

            var endTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat;

            if (endTime < data.HitTime)
            {
                data.HitTime = endTime;
            }

            data.HoldTime = Mathf.Max(endTime - data.HitTime, 0f);

            RenderNoteByNoteData(data);

            yield return null;
        }

        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes.Add(data);

        SortAndReloadNotes();
    }

    IEnumerator DragEndTimeCoroutine(LineData data)
    {
        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Lines.Remove(data);

        while (!Input.GetKeyUp(KeyCode.D))
        {
            var beatLine = BeatLineManager.Instance.CurrentBeatLines
    .OrderBy(x => Mathf.Abs(x.transform.position.y - Input.mousePosition.y)).First();

            data.TargetHitTime = beatLine.CurrentBeat * EditManager.Instance.PerBeat;

            if (data.TargetHitTime < data.HitTime)
            {
                data.HitTime = data.TargetHitTime;
            }

            RenderEditLinByLineData(data);

            yield return null;
        }

        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Lines.Add(data);

        EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Lines.Sort();
        EditManager.Instance.Reload(false);
    }

    public void PasteNote()
    {
        StartCoroutine(Co_PasteNote());
    }

    //写个粘贴用的
    IEnumerator Co_PasteNote()
    {
        //首先创建一份深拷贝出来的
        var cb = EditManager.Instance.EditCopyBuffer.CopyNotes;
        var temp = cb.DeepClone();

        while (!Input.GetKeyUp(KeyCode.V))
        {
            var beatLine = BeatLineManager.Instance.CurrentBeatLines
    .OrderBy(x => Mathf.Abs(x.transform.position.y - Input.mousePosition.y)).First();

            var tp = beatLine.CurrentBeat * EditManager.Instance.PerBeat;

            var posX = 0f;
            var mousePos = Input.mousePosition;
            //接近0则为不限制
            if (ClampFactor > 0.001f)
            {
                var step = ClampFactor / EditManager.Instance.EditNotePosXFactor;
                var t = ((mousePos.x - EditNoteTransform.position.x) / (EditNoteTransform.position.x / 400f)) / step;
                posX = Mathf.RoundToInt(t) * step;
            }
            else
            {
                posX = (mousePos.x - EditNoteTransform.position.x) / (EditNoteTransform.position.x / 400f);
            }

            posX *= EditManager.Instance.EditNotePosXFactor;

            for (var i = 0; i < temp.Count; i++)
            {
                temp[i].HitTime = cb[i].HitTime + tp;
                temp[i].PosX = cb[i].PosX + posX;

                RenderNoteByNoteData(temp[i]);
            }

            yield return null;
        }

        NoteEditPanelControl.Instance.CurrentData.Clear();

        for (var i = 0; i < temp.Count; i++)
        {
            EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes.Add(temp[i]);
            NoteEditPanelControl.Instance.CurrentData.Add(temp[i]);
        }

        SortAndReloadNotes();
    }

}
