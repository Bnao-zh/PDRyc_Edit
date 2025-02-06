using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Data;
using PDRO.Utils;
using PDRO.Utils.Singleton;
using UnityEngine.UI;

public class NoteEditPanelControl : MonoSingleton<NoteEditPanelControl>
{
    public Text Title;

    public InputField HitTimeInput, HoldTimeInput, PosXInput, XLengthInput;
    public Dropdown TypeDropdown, FakeDropdown;

    public Button DeleteNoteButton, ClosePanelButton;

    public List<NoteData> CurrentData;

    protected override void OnAwake()
    {
        CurrentData = new();

        DeleteNoteButton.onClick.AddListener(DeleteNote);
        ClosePanelButton.onClick.AddListener(ClosePanel);

        HitTimeInput.onEndEdit.AddListener(OnEndEditHitTime);
        HoldTimeInput.onEndEdit.AddListener(OnEndEditHoldTime);
        PosXInput.onEndEdit.AddListener(OnEndEditPosX);
        XLengthInput.onEndEdit.AddListener(OnEndEditXLength);

        TypeDropdown.onValueChanged.AddListener(OnEndEditNoteType);
        FakeDropdown.onValueChanged.AddListener(OnEndEditTorF);
    }

    public void TryAddNoteToEdit(NoteData data, bool isMulti)
    {
        //先打开编辑面板
        this.gameObject.SetActive(true);

        //如果不是多选就再清空一下
        if (!isMulti)
        {
            CurrentData.Clear();

            CurrentData.Add(data);
        }
        else//如果是多选就检测一下这个选中了没有 选中了就不选了
        {
            if (CurrentData.Contains(data))
            {
                CurrentData.Remove(data);
            }
            else
            {
                CurrentData.Add(data);
            }
        }

        if (CurrentData.Count == 0)
        {
            ClosePanel();
            return;
        }

        InitInputValue();
        //如果只有一个
        if (CurrentData.Count == 1)
        {
            Title.text = "单Note数据编辑";
        }
        else
        {
            Title.text = $"多Note数据编辑，目前选中{CurrentData.Count}个";
        }

        EditLinePanel.Instance.ClosePanel();
    }

    void ZeroInputValue()
    {
        var data = CurrentData[0];

        HitTimeInput.text = (Mathf.RoundToInt(data.HitTime * 1000f)).ToString();
        HoldTimeInput.text = (Mathf.RoundToInt(data.HoldTime * 1000f)).ToString();
        PosXInput.text = data.PosX.ToString();
        XLengthInput.text = data.LengthX.ToString();

        TypeDropdown.value = (int)data.NoteType;
        FakeDropdown.value = data.IsFake ? 1 : 0;

        TypeDropdown.captionText.text = data.NoteType.ToString();
        FakeDropdown.captionText.text = data.IsFake ? "假" : "真";
    }

    public void InitInputValue()
    {
        ZeroInputValue();

        if (CurrentData.Count != 1)
        {
            bool hit = true, hold = true, posX = true, xLength = true, type = true, fake = true, fall = true;
            //该检查某个值是否全部相等了呜呜
            for (var i = 1; i < CurrentData.Count; i++)
            {
                var one = CurrentData[0];
                var data = CurrentData[i];

                //判断
                if (hit && one.HitTime != data.HitTime)
                {
                    hit = false;
                }

                if (hold && one.HoldTime != data.HoldTime)
                {
                    hold = false;
                }

                if (posX && one.PosX != data.PosX)
                {
                    posX = false;
                }

                if (xLength && one.LengthX != data.LengthX)
                {
                    xLength = false;
                }

                if (type && one.NoteType != data.NoteType)
                {
                    type = false;
                }

                if (fake && one.IsFake != data.IsFake)
                {
                    fake = false;
                }

                if (fall && one.FallDirection != data.FallDirection)
                {
                    fall = false;
                }
            }

            if (!hit)
            {
                HitTimeInput.text = "-";
            }

            if (!hold)
            {
                HoldTimeInput.text = "-";
            }

            if (!posX)
            {
                PosXInput.text = "-";
            }

            if (!xLength)
            {
                XLengthInput.text = "-";
            }

            if (!type)
            {
                TypeDropdown.captionText.text = "-";
            }

            if (!fake)
            {
                FakeDropdown.captionText.text = "-";
            }
        }
    }



    public void OnEndEditNoteType(int value)
    {
        CurrentData.ForEach(x => x.NoteType = (NoteType)value);
        OnEditValue();
    }

    public void OnEndEditTorF(int value)
    {
        CurrentData.ForEach(x => x.IsFake = value == 0 ? false : true);
        OnEditValue();
    }

    public void OnEndEditXLength(string value)
    {
        //如果是用-开始
        if (value.StartsWith("-"))
        {
            value = value.Substring(1);

            if (value.StartsWith("+") || value.StartsWith("-"))
            {
                //全体，偏移部分
                if (float.TryParse(value, out float xl))
                {
                    CurrentData.ForEach(x => x.LengthX += xl);
                }
            }
            else//负数，全部设置
            {
                if (float.TryParse(value, out float xl))
                {
                    CurrentData.ForEach(x => x.LengthX = -xl);
                }
            }
        }
        else
        {
            //正数，全部设置
            if (float.TryParse(value, out float xl))
            {
                CurrentData.ForEach(x => x.LengthX = xl);
            }
        }

        InitInputValue();
        OnEditValue();
    }

    public void OnEndEditPosX(string value)
    {
        //如果是用-开始
        if (value.StartsWith("-"))
        {
            value = value.Substring(1);

            if (value.StartsWith("+") || value.StartsWith("-"))
            {
                //全体，偏移部分
                if (float.TryParse(value, out float posX))
                {
                    CurrentData.ForEach(x => x.PosX += posX);
                }
            }
            else//负数，全部设置
            {
                if (float.TryParse(value, out float posX))
                {
                    CurrentData.ForEach(x => x.PosX = -posX);
                }
            }
        }
        else
        {
            //正数，全部设置
            if (float.TryParse(value, out float posX))
            {
                CurrentData.ForEach(x => x.PosX = posX);
            }
        }

        InitInputValue();
        OnEditValue();
    }

    public void OnEndEditHitTime(string value)
    {
        //如果是用-开始
        if (value.StartsWith("-"))
        {
            value = value.Substring(1);

            if (value.StartsWith("+") || value.StartsWith("-"))
            {
                //全体，偏移部分
                if (float.TryParse(value, out float time))
                {
                    var t = time * 0.001f;
                    CurrentData.ForEach(x => x.HitTime += t);
                }
            }
            else//负数，全部设置
            {
                if (float.TryParse(value, out float time))
                {
                    var t = time * 0.001f;
                    CurrentData.ForEach(x => x.HitTime = -t);
                }
            }
        }
        else
        {
            //正数，全部设置
            if (float.TryParse(value, out float time))
            {
                var t = time * 0.001f;
                CurrentData.ForEach(x => x.HitTime = t);
            }
        }

        InitInputValue();
        OnEditValue(true);
    }

    public void OnEndEditHoldTime(string value)
    {
        //如果是用-开始
        if (value.StartsWith("-"))
        {
            value = value.Substring(1);

            if (value.StartsWith("+") || value.StartsWith("-"))
            {
                //全体，偏移部分
                if (float.TryParse(value, out float time))
                {
                    var t = time * 0.001f;
                    foreach (var data in CurrentData)
                    {
                        if (data.NoteType is NoteType.Hold)
                        {
                            data.HoldTime += t;
                        }
                    }
                }
            }
            else//负数，全部设置
            {
                if (float.TryParse(value, out float time))
                {
                    if (time >= 0)
                    {
                        var t = time * 0.001f;
                        foreach (var data in CurrentData)
                        {
                            if (data.NoteType is NoteType.Hold)
                            {
                                data.HoldTime = -t;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            //正数，全部设置
            if (float.TryParse(value, out float time))
            {
                if (time >= 0)
                {
                    var t = time * 0.001f;
                    foreach (var data in CurrentData)
                    {
                        if (data.NoteType is NoteType.Hold)
                        {
                            data.HoldTime = t;
                        }
                    }
                }
            }
        }

        InitInputValue();
        OnEditValue();
    }

    public void ClosePanel()
    {
        CurrentData.Clear();
        this.gameObject.SetActive(false);
    }

    void DeleteNote()
    {
        //全删了！
        for (var i = 0; i < CurrentData.Count; i++)
        {
            var data = CurrentData[i];
            EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes.Remove(data);
        }

        OnEditValue();
    }


    public void OnEditValue(bool sort = false)
    {
        //如果修改HitTime则需要排序
        if (sort)
        {
            EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes.Sort();
        }

        EditManager.Instance.Reload(false);
    }



    void Update()
    {
        //按下S拖动开始时间
        if (Input.GetKeyDown(KeyCode.S))
        {
            foreach (var data in CurrentData)
            {
                EditNoteManager.Instance.DragStartTime(data);
            }
        }
        //按下D拖动结束时间
        else if (Input.GetKeyDown(KeyCode.D))
        {
            foreach (var data in CurrentData)
            {
                if (data.NoteType is NoteType.Hold)
                {
                    EditNoteManager.Instance.DragEndTime(data);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.C) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            //复制了！
            var cb = EditManager.Instance.EditCopyBuffer;

            cb.Type = CopyType.Note;

            var t = CurrentData.DeepClone();
            t.Sort();

            //预处理一下这些Note
            var st = t[0].HitTime;
            var sp = t[0].PosX;

            for (var i = 0; i < t.Count; i++)
            {
                t[i].HitTime = t[i].HitTime - st;
                t[i].PosX = t[i].PosX - sp;
            }

            cb.CopyNotes = t;

            Debug.Log($"当前剪切板内容为{t.Count}个Note");
        }

        //如果因为其他原因被删除了就去掉
        for (var i = 0; i < CurrentData.Count; i++)
        {
            var data = CurrentData[i];
            if (!EditManager.Instance.EditingChart.Tracks[EditManager.Instance.EditChartObjectIndex].Notes.Contains(data)) CurrentData.Remove(data);
        }

        if (CurrentData.Count == 0) ClosePanel();
    }
}
