using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PDRO.Gameplay.Managers;
using System.IO;
using PDRO.Gameplay.Controls;
using PDRO.Data;
using PDRO.Utils;
using PDRO.Utils.Singleton;
using UnityEngine.UI;
using DG.Tweening;

public enum CopyType
{
    Empty, Note, Event, Line
}

public class CopyBuffer
{
    public CopyType Type = CopyType.Empty;

    public List<NoteData> CopyNotes;
    public List<LineData> CopyLines;
    public List<NumEventData> NumEvents;
    public List<ColorEventData> ColorEvents;
    public List<TextEventData> TextEvents;
}

public class EditManager : MonoSingleton<EditManager>
{
    [Header("音频相关")]
    public Text MusicText;
    public Text CompText;
    public Text BPMText;
    public Button playButton, pauseButton, backToPauseButton;
    public InputField MusicIDInput;
    public Text musicTimeText, secondText, nowBeatText;
    public Slider musicTimeSlider;
    public RawImage MusicWaveformRawImg;

    public Toggle ShowWaveform, UpdateWaveform;

    public int ChartIndex = -1;
    public int MaxUnReLength;
    public List<ChartData> ChartHistory = new();

    public Text CanRe, CanUn;
    void UpdateUnReState()
    {
        CanRe.text = $"可重做步数：{ChartHistory.Count - 1 - ChartIndex}";
        CanUn.text = $"可撤销步数：{ChartIndex}";
    }

    public void PutChart(ChartData Chart)
    {
        var nextIndex = ChartIndex + 1;

        if (nextIndex > MaxUnReLength)
        {
            ChartHistory.RemoveAt(0);
        }
        else if (nextIndex < ChartHistory.Count)
        {
            ChartHistory.RemoveRange(nextIndex, ChartHistory.Count - nextIndex);
        }

        ChartHistory.Add(Chart.DeepClone());
        ChartIndex = Math.Min(nextIndex, MaxUnReLength);

        UpdateUnReState();
    }

    public void RedoChart() // Ctrl + Y
    {
        int nextIndex = ChartIndex + 1;

        ChartIndex = Math.Min(ChartHistory.Count - 1, nextIndex);

        Reload(false, true);

        UpdateUnReState();
    }

    public void UndoChart() // Ctrl + Z
    {
        int lastIndex = ChartIndex - 1;

        ChartIndex = Math.Max(0, lastIndex);

        //这里放重新加载
        Reload(false, true);

        UpdateUnReState();
    }



    [Header("编辑相关")]
    public float EditFallSpeed = 500f;
    public float EditNotePosXFactor = 0.05f;
    public CopyBuffer EditCopyBuffer = new();
    public EditType CurrentEditType;
    public enum EditType
    {
        None = 0, UpNotes = 1, DownNotes = 2, Events = 3
    }
    public int EditChartObjectIndex;
    public AudioSource EditAudioSource;

    public bool IsAuto = true;
    public bool ShowUI = true;
    public Transform[] P2Switch;

    public Transform[] EditSomeThing;

    public Transform LevelTransform;
    public TrackControl TrackPrefab;
    public TrackControl[] Tracks;
    public Button SaveButton, DeleteButton, AddTrackButton;
    public float NowTime;
    public float TotalLength, lastPauseTime, NowBeat, PerBeat, BPM;
    public int X分音;

    public Text NowEditText;
    public ChartData EditingChart;

    public Button ReloadButton, CopyButton, SettingsButton, SaveSettingsButton, EndEditButton, SwitchEditModeButton;

    public InputField ChartPathInput, XifenInput;

    public InputField Compensate3DInput;

    protected override void OnAwake()
    {
        Application.targetFrameRate = 60;

        ReloadButton.onClick.AddListener(ReloadForButton);

        CopyButton.onClick.AddListener(CopySecond);

        void CopySecond()
        {
            UnityEngine.GUIUtility.systemCopyBuffer = $"{Mathf.RoundToInt(NowTime * 1000f)}";
        }

        playButton.onClick.AddListener(PlayMusic);
        pauseButton.onClick.AddListener(PauseMusic);
        backToPauseButton.onClick.AddListener(BackToLastPause);

        DeleteButton.onClick.AddListener(DeleteObject);
        EndEditButton.onClick.AddListener(EndEdit);

        AddTrackButton.onClick.AddListener(AddNewTrack);

        SaveButton.onClick.AddListener(SaveChart);

        SwitchEditModeButton.onClick.AddListener(SwitchEditMode);

        musicTimeSlider.onValueChanged.AddListener((value) =>
        {
            if (Mathf.Abs(value * TotalLength - NowTime) > 3f && EditAudioSource.isPlaying)
            {
                PauseMusic();
            }
        });

        ChartPathInput.text = PlayerPrefs.GetString("Chart_Path", "0.txt");
        ChartPathInput.onEndEdit.AddListener(OnEndEditPath);

        XifenInput.text = (X分音 = 4).ToString();
        XifenInput.onEndEdit.AddListener(OnEndEditXiFen);

        MusicIDInput.onEndEdit.AddListener((value) => ChangeMusic());

        Compensate3DInput.onEndEdit.AddListener(OnEndEditCompensate3D);
    }

    void OnEndEditCompensate3D(string value)
    {
        if (float.TryParse(value, out float com) && EditChartObjectIndex != -1)
        {
            EditingChart.Tracks[EditChartObjectIndex].Compensate3D = com;
            Reload(false);
        }
        else
        {
            Compensate3DInput.text = EditChartObjectIndex == -1 ? "233" : EditingChart.Tracks[EditChartObjectIndex].Compensate3D.ToString();
        }
    }

    void OnEndEditPath(string value)
    {
        if (value.Trim() != null)
        {
            value = value.Trim();
            if (value.EndsWith(".txt"))
            {
                if (value.Contains("\"") || value.Contains("*") || value.Contains("<") || value.Contains(">") || value.Contains("?") || value.Contains("\\") || value.Contains("/") || value.Contains("|") || value.Contains(":"))
                {
                    Debug.LogError("文件名不可包含特殊字符");
                }
                else
                {
                    PlayerPrefs.SetString("Chart_Path", value);
                }
            }
            else
            {
                Debug.LogError("文件名需要包括扩展名");
            }
        }
        else
        {
            Debug.LogError("路径不可为空");
        }
        ChartPathInput.text = PlayerPrefs.GetString("Chart_Path", "0.txt");
    }

    void OnEndEditXiFen(string value)
    {
        if (int.TryParse(value, out int xifen) && xifen > 0)
        {
            X分音 = xifen;
        }
        else
        {
            XifenInput.text = X分音.ToString();
        }
    }


    void SwitchEditMode()
    {
        if (CurrentEditType is EditType.UpNotes or EditType.DownNotes)
        {
            CurrentEditType = EditType.Events;
        }
        else if (CurrentEditType is EditType.Events && EditChartObjectIndex != -1)
        {
            CurrentEditType = EditType.UpNotes;
        }

        NowEditText.text = EditChartObjectIndex == -1 ? $"正在编辑：Camera - {CurrentEditType}" : $"正在编辑：Track[{EditChartObjectIndex}] - {CurrentEditType}";
    }

    void Start()
    {
        Application.targetFrameRate = -1;
        Reload(true);
    }

    void ChangeMusic()
    {
        if (int.TryParse(MusicIDInput.text, out int id) && DataManager.Instance.HasMusicID(id))
        {
            EditingChart.MetaData.MusicID = id;
            Reload(false);
        }
        else
        {
            MusicIDInput.text = EditingChart.MetaData.MusicID.ToString();
            Debug.LogError("输入了不正确的曲目id，请重新输入");
        }
    }

    public void StartEdit(int id)
    {
        if (id > -2 && id < Tracks.Length)
        {
            EditChartObjectIndex = id;

            if (id != -1)
            {
                CurrentEditType = EditType.UpNotes;
                Compensate3DInput.text = EditingChart.Tracks[EditChartObjectIndex].Compensate3D.ToString();
            }
            else
            {
                CurrentEditType = EditType.Events;
            }

            NowEditText.text = id == -1 ? $"正在编辑：Camera - {CurrentEditType}" : $"正在编辑：Track[{id}] - {CurrentEditType}";


            for (var i = 0; i < EditSomeThing.Length; i++)
            {
                EditSomeThing[i].gameObject.SetActive(true);
            }
        }
        else
        {
            throw new Exception("id所对应的物体不存在");
        }
    }

    public void DeleteObject()
    {
        if (CurrentEditType is EditType.None)
        {
            Debug.LogError("请先选中要删除的轨道");
            return;
        }

        var id = EditChartObjectIndex;

        if (id == -1) throw new Exception("不可以删除摄像机哦");

        if (id > -1 && id < Tracks.Length)
        {
            EditingChart.Tracks.RemoveAt(id);

            //检查父轨道ID

            //摄像机的
            if (EditingChart.Camera.FatherTrackIndex == id)
            {
                Debug.LogWarning("摄像机以删除的轨道为父轨道，摄像机的父轨道已自动设为无");
            }
            else if (EditingChart.Camera.FatherTrackIndex > id)
            {
                EditingChart.Camera.FatherTrackIndex = EditingChart.Camera.FatherTrackIndex - 1;
            }

            //轨道的
            for (var i = 0; i < EditingChart.Tracks.Count; i++)
            {
                var t = EditingChart.Tracks[i];

                if (t.FatherTrackIndex == id)
                {
                    t.FatherTrackIndex = -1;
                    Debug.LogWarning($"轨道{i}以删除的轨道为父轨道，该轨道的父轨道已自动设为无");
                }
                else if (t.FatherTrackIndex > id)
                {
                    t.FatherTrackIndex = t.FatherTrackIndex - 1;
                }
            }

            //连线的
            for (var i = 0; i < EditingChart.Tracks.Count; i++)
            {
                var ls = EditingChart.Tracks[i].Lines;

                for (var j = 0; j < ls.Count; j++)
                {
                    var l = ls[j];

                    if (l.TargetTrackIndex == id)
                    {
                        ls.Remove(l);
                        Debug.LogWarning($"连线（位于轨道{i}-{j}）以删除的轨道为目标轨道，该连线已被移除");
                    }
                    else if (l.TargetTrackIndex > id)
                    {
                        l.TargetTrackIndex = l.TargetTrackIndex - 1;
                    }
                }
            }


            EndEdit();
            Reload(false);
        }
        else
        {
            throw new Exception("id所对应的物体不存在");
        }
    }

    void SaveChart()
    {
        var path = Application.dataPath + "/" + PlayerPrefs.GetString("Chart_Path", "0.txt");

        File.WriteAllText(path, ChartUtility.ToString(EditingChart));
        Debug.Log("保存成功！");
    }

    void EndEdit()
    {
        CurrentEditType = EditType.None;

        for (var i = 0; i < EditSomeThing.Length; i++)
        {
            EditSomeThing[i].gameObject.SetActive(false);
        }
    }

    void AddNewTrack()
    {
        if (CurrentEditType is EditType.None)
        {
            EditingChart.Tracks.Add(ChartUtility.GetNewTrack());
            Reload(false);
        }
        else//如果有选中的物件那这个物件的id就是他的下一个
        {
            EditingChart.Tracks.Add(ChartUtility.GetNewTrack());

            var newID = EditingChart.Tracks.Count - 1;
            var targetID = EditChartObjectIndex + 1;

            var num = newID - targetID;

            for (var i = 0; i < num; i++)
            {
                TrackMoveManager.Instance.UpMove(newID - i, false);
            }
            Reload(false);
        }
    }

    void ReloadForButton()
    {
        Reload(true);
    }

    public void Reload(bool isLoadLocal, bool isUnRe = false)
    {
        //相机跑到最外面防止被一块杀了
        CameraControl.Instance.Run();

        //杀轨道
        //就算我用了DestroyImmediate，Transform.childCount实时更新就没有一点错吗？（逃
        for (var i = 0; i < LevelTransform.childCount; i++)
        {
            Destroy(LevelTransform.GetChild(i).gameObject);
        }

        Tracks = Array.Empty<TrackControl>();
        //重启Note池子
        PoolManager.Instance.InitPool();

        if (isLoadLocal)
        {
            //读取新的谱面
            var path = Application.dataPath + "/" + PlayerPrefs.GetString("Chart_Path", "0.txt");

            string str;
            try
            {
                str = File.ReadAllText(path);
            }
            catch (FileNotFoundException ex)
            {
                //throw new Exception($"无法找到文件{path}，请检查路径");
                Debug.LogWarning($"无法找到文件{path}，已自动创建并打开新谱面({ex.Message})");

                str = ChartUtility.ToString(ChartUtility.GetNewChartData());

                File.WriteAllText(path, str);
            }
            Debug.Log($"路径{path}读取成功");

            EditingChart = ChartUtility.ReadChart(str);

            Debug.Log($"谱面解码成功");
        }

        if (isUnRe)//如果是撤回重做的
        {
            EditingChart = ChartHistory[ChartIndex].DeepClone();

            //防止撤回重做UI出问题
            Compensate3DInput.text = EditChartObjectIndex == -1 ? "233" : EditingChart.Tracks[EditChartObjectIndex].Compensate3D.ToString();
        }
        else//不是就把当前的谱存下
        {
            PutChart(EditingChart);
        }

        EditingChart.InitObjectIndex().InitNoteHighlight().InitSpeedEventsLinear();

        //设置要用的音乐
        EditAudioSource.clip = DataManager.Instance.GetAudioClipByID(EditingChart.MetaData.MusicID);
        TotalLength = EditAudioSource.clip.length - 0.001f;//AudioSource是不是有病啊 实际达不到最后面只能减掉一点点

        //更新曲目信息
        var musicData = DataManager.Instance.GetNowMusicData();

        MusicIDInput.text = musicData.ID.ToString();
        MusicText.text = $"Name：{musicData.MusicName}";
        CompText.text = $"Artists：{musicData.ComposerName}";
        BPMText.text = $"BPM：{musicData.BPM}";

        BPM = musicData.BPM;
        PerBeat = (60 / BPM);

        //更新Track总数
        //TrackNumText.text = $"当前轨道总数：{EditingChart.Tracks.Count}";

        if (EditChartObjectIndex >= EditingChart.Tracks.Count)
        {
            EditChartObjectIndex = EditingChart.Tracks.Count - 1;
            Debug.LogWarning("切换谱面导致编辑索引超界，已自动修复");
        }
        NowEditText.text = EditChartObjectIndex == -1 ? $"正在编辑：Camera - {CurrentEditType}" : $"正在编辑：Track[{EditChartObjectIndex}] - {CurrentEditType}";

        //初始化物件显示
        ChartObjectManager.Instance.Init();

        //重新实例化
        InitChart();

        Debug.Log($"谱面实例化成功");

        //避免Note再响
        for (var i = 0; i < Tracks.Length; i++)
        {
            for (var j = 0; j < Tracks[i].Notes.Length; j++)
            {
                var note = Tracks[i].Notes[j];
                if (NowTime > note.HitTime)
                {
                    note.IsJudged = true;
                }
            }
        }

        //小节线
        BeatLineManager.Instance.SetBeatLineTime(X分音, BPM, TotalLength);
    }

    public void UpdateAudioWaveform()
    {
        if (CurrentEditType is EditType.None) return;

        if (ShowWaveform.isOn)
        {
            MusicWaveformRawImg.gameObject.SetActive(true);
            if (UpdateWaveform.isOn)
            {
                //防止内存泄露，销毁上一张
                Destroy(MusicWaveformRawImg.texture);
                //更新波形图
                MusicWaveformRawImg.texture = Utility.GetAudioWaveformT2D(EditAudioSource.clip, NowTime, (900 / EditFallSpeed + NowBeat) * PerBeat);
            }
        }
        else
        {
            MusicWaveformRawImg.gameObject.SetActive(false);
        }
    }

    void InitChart()
    {
        InitTracks();
        CameraControl.Instance.Init(EditingChart.Camera);
    }

    //秒转换Beat，用于Note和小节线那些
    public float Time2Beat(float time)
    {
        return time / PerBeat;
    }

    void InitTracks()
    {
        // 先实例化出来
        var TrackDatas = EditingChart.Tracks;
        var TrackList = new List<TrackControl>();

        for (var i = 0; i < TrackDatas.Count; i++)
        {
            var temp = Instantiate(TrackPrefab, LevelTransform);
            temp.gameObject.name = $"Track - [{i}]";

            temp.Init(TrackDatas[i]);
            TrackList.Add(temp);
        }

        Tracks = TrackList.ToArray();

        // 再设父轨道
        for (var i = 0; i < TrackDatas.Count; i++)
        {
            var track = Tracks[i];

            track.SecondInit();

            var index = TrackDatas[i].FatherTrackIndex;
            if (index != -1 && index != i)// -1是没有父轨道，i是不能自交（逃
            {
                track.transform.SetParent(Tracks[index].transform);
            }
        }
    }

    void Update()
    {
        if (!EditAudioSource.isPlaying)
        {
            NowTime = TotalLength * musicTimeSlider.value;
            EditAudioSource.time = NowTime;
        }
        else
        {
            NowTime = EditAudioSource.time;
            musicTimeSlider.value = TotalLength > 0 ? (NowTime / TotalLength) : 0;
        }

        musicTimeText.text = $"{Seconds2Times(NowTime)}/{Seconds2Times(TotalLength)}";

        NowBeat = NowTime / PerBeat;

        secondText.text = $"毫秒：{Mathf.RoundToInt(NowTime * 1000f)}ms";
        nowBeatText.text = $"Beat: {NowBeat:F2}";

        UpdateKeyBordInput();
        UpdateGameplay();
        UpdateAudioWaveform();

        //编辑相关
        BeatLineManager.Instance.OnUpdate();
        EditEventManager.Instance.OnUpdate();
        EditNoteManager.Instance.OnUpdate();
    }

    void UpdateKeyBordInput()
    {
        //按下A开关Auto
        if (Input.GetKeyDown(KeyCode.A))
        {
            IsAuto = !IsAuto;
        }

        //按下P开关UI
        if (Input.GetKeyDown(KeyCode.P))
        {
            ShowUI = !ShowUI;
            for (var i = 0; i < P2Switch.Length; i++)
            {
                P2Switch[i].gameObject.SetActive(ShowUI);
            }

            Reload(false);
        }

        //编辑Note的时按下F更换下落面
        if (Input.GetKeyDown(KeyCode.F))
        {
            CurrentEditType = EditType.UpNotes;
            NowEditText.text = $"正在编辑：Track[{EditChartObjectIndex}] - {CurrentEditType}";
        }

        // 按下空格就播放/暂停
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (EditAudioSource.isPlaying)
            {
                PauseMusic();
            }
            else
            {
                PlayMusic();
            }
        }

        //Ctrl+Z撤回Ctrl+Y重做
        if (Input.GetKeyDown(KeyCode.Z) && GetControlKey())
        {
            UndoChart();
        }
        else if (Input.GetKeyDown(KeyCode.Y) && GetControlKey())
        {
            RedoChart();
        }






        //粘贴！！！！
        if (Input.GetKeyDown(KeyCode.V) && GetControlKey())
        {
            if (EditCopyBuffer.Type is CopyType.Empty)
            {
                Debug.LogWarning("请先复制再粘贴");
            }
            else if (EditCopyBuffer.Type is CopyType.Note)
            {
                //粘贴Note
                if (CurrentEditType is not EditType.UpNotes or EditType.DownNotes)
                {
                    Debug.LogWarning("Note只能粘贴到Note层哦");
                }
                else
                {
                    EditNoteManager.Instance.PasteNote();
                }

            }
            else
            {
                //略
            }


        }

        //下面是和鼠标滚轮有关的操作
        var mouseScroll = Input.GetAxis("Mouse ScrollWheel");

        // 如果Ctrl+鼠标滚轮就改变小节线距离(而且不在粘贴)
        if (GetControlKey() && !Input.GetKey(KeyCode.V))
        {
            EditFallSpeed += mouseScroll * 200f * EditFallSpeed / 500f;
        }
        // 如果X+鼠标滚轮就改变NoteX缩放
        else if (Input.GetKey(KeyCode.X))
        {
            EditNotePosXFactor += mouseScroll * 0.02f * EditNotePosXFactor / 0.05f;
        }
        // 如果只有鼠标滚轮就改变时间
        else if (Mathf.Abs(mouseScroll) > 0)
        {
            //播放期间滚滚轮暂停
            if (EditAudioSource.isPlaying)
            {
                PauseMusic();
            }

            float targetBeat = Mathf.Round(NowBeat * X分音) / X分音 +
                               mouseScroll / 0.1f / X分音; // 正着走
            SetNowBeat(targetBeat);
        }

        bool GetControlKey()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }
    }

    void UpdateGameplay()
    {
        UpdateTracks();
        CameraControl.Instance.OnUpdate();

        void UpdateTracks()
        {
            for (var i = 0; i < Tracks.Length; i++)
            {
                Tracks[i].OnUpdate();
            }
        }
    }

    public void PlayMusic()
    {
        if (EditAudioSource.isPlaying) return;
        lastPauseTime = NowTime;
        EditAudioSource.PlayScheduled(0f);
    }

    public void PauseMusic()
    {
        if (!EditAudioSource.isPlaying) return;

        EditAudioSource.Pause();
        lastPauseTime = NowTime;
    }

    public void BackToLastPause()
    {
        if (!EditAudioSource.isPlaying) return;

        EditAudioSource.time = lastPauseTime;
    }

    private string Seconds2Times(double seconds)
    {
        int minutes = Mathf.FloorToInt(Mathf.Abs((float)seconds) / 60);
        float newSeconds = Mathf.Abs((float)seconds) % 60;
        string str = seconds >= 0 ? "" : "-";
        return $"{str}{minutes}:{newSeconds:00.00}";
    }

    public void SetNowBeat(float beat)
    {
        float targetTime = beat * PerBeat;


        if (targetTime < 0) targetTime = 0;
        if (targetTime > TotalLength) targetTime = TotalLength;

        if (!EditAudioSource.isPlaying)
        {
            musicTimeSlider.value = targetTime / TotalLength;
        }
        else
        {
            EditAudioSource.time = targetTime;
        }

        musicTimeText.text = $"{Seconds2Times(NowTime)}/{Seconds2Times(TotalLength)}";
        nowBeatText.text = $"Beat: {NowBeat:F2}";
    }


    public class BpmGroup : IComparable<BpmGroup>
    {
        // 改变后的BPM
        public float TargetBpm;
        // 哪拍改变
        public float ChangeBeat;

        // 在这次更改前的时间积分，秒
        public float Duration;

        // 每拍的秒数
        public float PerBeat => 60f / TargetBpm;


        // 哇！先进科技！
        public int CompareTo(BpmGroup data)
        {
            if (this.ChangeBeat == data.ChangeBeat)
            {
                return 0;
            }
            else if (this.ChangeBeat > data.ChangeBeat)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }
}
