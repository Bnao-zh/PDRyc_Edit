using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PDRO.Data;
using PDRO.Utils;
using PDRO.Gameplay.Managers;

namespace PDRO.Gameplay.Controls
{
    public class TrackControl : MonoBehaviour
    {
        public TextMesh TipText;
        public SpriteRenderer JudgmentLineSpriteRenderer;
        public TrackData CurrentData;
        public Transform ScaleXTransform;
        public NoteControl[] Notes;
        public LineControl[] Lines;

        [SerializeField] private TrackSurfaceControl UpSurface;

        public float NowDistance;
        public float UpLength, DownLength;
        public float ScaleX;

        public void Init(TrackData data)
        {
            CurrentData = data;

            InitNotes();

            if (EditManager.Instance.ShowUI)
            {
                TipText.text = $"{CurrentData.TrackIndex}";
            }
            else
            {
                TipText.text = null;
            }
        }

        //需要全部Track生成好之后进行的设置父子关系和生成Line
        public void SecondInit()
        {
            InitLines();
        }

        public LineControl LinePrefab;

        void InitLines()
        {
            var LineDatas = CurrentData.Lines;
            var LineList = new List<LineControl>();

            for (var i = 0; i < LineDatas.Count; i++)
            {
                var data = LineDatas[i];

                var ctl = Instantiate(LinePrefab, EditManager.Instance.LevelTransform);
                ctl.Init(data, this);

                LineList.Add(ctl);
            }

            Lines = LineList.OrderBy(x => x.CurrentData.HitTime).ToArray();
        }

        void InitNotes()
        {
            var NoteDatas = CurrentData.Notes;
            var NoteList = new List<NoteControl>();

            for (var i = 0; i < NoteDatas.Count; i++)
            {
                var data = NoteDatas[i];
                switch (data.NoteType)
                {
                    case NoteType.Tap:
                        NoteList.Add(new TapControl(data, this));
                        break;
                    case NoteType.Hold:
                        NoteList.Add(new HoldControl(data, this));
                        break;
                    case NoteType.Drag:
                        NoteList.Add(new DragControl(data, this));
                        break;
                    case NoteType.Flick:
                        NoteList.Add(new FlickControl(data, this));
                        break;
                }
            }

            Notes = NoteList.OrderBy(x => x.CurrentData.HitTime).ToArray();
        }

        public void OnUpdate()
        {
            //标出选中的轨道
            if (EditManager.Instance.ShowUI && EditManager.Instance.CurrentEditType is not EditManager.EditType.None && EditManager.Instance.EditChartObjectIndex == CurrentData.TrackIndex)
            {
                TipText.color = new Color(0f, 1f, 0f, 1f);
            }
            else
            {
                TipText.color = new Color(1f, 1f, 1f, 0.7f);
            }


            UpdateEvents();
            UpdateNotes();
            UpdateLines();
        }

        void UpdateEvents()
        {
            var nowTime = ProgressManager.Instance.NowTime;

            float posX = 0f, posY = 0f, posZ = 0f,
                rotX = 0f, rotY = 0f, rotZ = 0f;

            Color up = new Color(0f, 0f, 0f, 0f), line = new Color(0f, 0f, 0f, 0f), down = new Color(0f, 0f, 0f, 0f);

            UpLength = 0f;

            for (var i = 0; i < CurrentData.Events.Count; i++)
            {
                var nowLayer = CurrentData.Events[i];

                posX += GameplayUtility.GetValueFromTrackEvent(nowTime, nowLayer.MoveXEvents);
                posY += GameplayUtility.GetValueFromTrackEvent(nowTime, nowLayer.MoveYEvents);
                posZ += GameplayUtility.GetValueFromTrackEvent(nowTime, nowLayer.MoveZEvents);

                rotX += GameplayUtility.GetValueFromTrackEvent(nowTime, nowLayer.RotateXEvents);
                rotY += GameplayUtility.GetValueFromTrackEvent(nowTime, nowLayer.RotateYEvents);
                rotZ += GameplayUtility.GetValueFromTrackEvent(nowTime, nowLayer.RotateZEvents);

                UpLength += GameplayUtility.GetValueFromTrackEvent(nowTime, nowLayer.UpLengthEvents);

                up += GameplayUtility.GetColorFromColorEvent(nowTime, nowLayer.UpSurfaceColorEvents);
            }
            line = up;
            if (UpLength < 0.0001f){
                line.a = 0f;
            }
            this.transform.localPosition = new Vector3(posX, posY, posZ);
            this.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);
            ScaleXTransform.localScale = new Vector3(1.25f, 1f, 1f);
            UpSurface.SetSurfaceLength(UpLength, true);

            //颜色
            if (line.a < 0.01f)
            {
                JudgmentLineSpriteRenderer.enabled = false;
            }
            else
            {
                JudgmentLineSpriteRenderer.enabled = true;
                JudgmentLineSpriteRenderer.color = line;
            }
            UpSurface.SetColor(up, EditManager.Instance.IsAuto);
            // 速度时间
            NowDistance = GetDistance(nowTime);
        }

        void UpdateNotes()
        {
            for (var i = 0; i < Notes.Length; i++)
            {
                Notes[i].OnUpdate();
            }
        }

        void UpdateLines()
        {
            for (var i = 0; i < Lines.Length; i++)
            {
                Lines[i].OnUpdate();
            }
        }

        public float GetDistance(float time)
        {
            var temp = 0f;

            for (var i = 0; i < CurrentData.Events.Count; i++)
            {
                var SpeedEvents = CurrentData.Events[i].NoteSpeedEvents;

                for (var j = 0; j < SpeedEvents.Count; j++)
                {
                    if (time < SpeedEvents[j].StartTime) break;

                    temp = GameplayUtility.CalculateDistance(time, SpeedEvents[j]);

                }
            }

            return temp;
        }
    }
}