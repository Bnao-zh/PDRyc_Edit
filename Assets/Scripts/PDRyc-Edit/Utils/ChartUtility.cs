using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Data;
using System;
using System.Linq;
using System.Text;

namespace PDRO.Utils
{
    public static class ChartUtility
    {
        //获取自带一个空轨道的ChartData
        public static ChartData GetNewChartData()
        {
            ChartData chart = new();

            chart.MetaData = new();
            chart.MetaData.MusicID = 0;

            chart.Camera = new();
            chart.Camera.Events = new();
            chart.Camera.Events.Add(new CameraEvents
            {
                MoveZEvents = GetDefaultEvents(-10f, 0f),
                FovEvents = GetDefaultEvents(60f, 0f),

                //下面的就是弄成空避免报错了
                MoveXEvents = new(),
                MoveYEvents = new(),

                RotateXEvents = new(),
                RotateYEvents = new(),
                RotateZEvents = new()
            });

            chart.Tracks = new();
            chart.Tracks.Add(GetNewTrack());

            return chart;
        }

        //获取一个能看到的空轨道
        public static TrackData GetNewTrack()
        {
            var temp = new TrackData
            {
                Remarks = "新轨道",
                Events = new()
            };
            temp.Events.Add(new TrackEvents
            {
                UpLengthEvents = GetDefaultEvents(10f, 0f),

                NoteSpeedEvents = GetDefaultEvents(10f, DataManager.Instance.GetAudioClipByID(EditManager.Instance.EditingChart.MetaData.MusicID).length),

                UpSurfaceColorEvents = GetDefaultEvents(new Color(0f, 1f, 1f, 1f), 0f),

                //下面的就是弄成空避免报错了
                MoveXEvents = new(),
                MoveYEvents = new(),
                MoveZEvents = new(),
                RotateXEvents = new(),
                RotateYEvents = new(),
                RotateZEvents = new()
            });

            temp.Notes = new();
            temp.Lines = new();

            return temp;
        }

        //获取一个一直出一个值的值事件
        public static List<NumEventData> GetDefaultEvents(float value, float duration)
        {
            List<NumEventData> ev = new();

            ev.Add(new NumEventData
            {
                StartValue = value,
                EndValue = value,
                StartTime = 0f,
                EndTime = duration,
                EasingType = EaseUtility.Ease.Linear,
                StartEasingRange = 0f,
                EndEasingRange = 1f
            });

            return ev;
        }

        //获取一个一直出一个颜色的颜色事件
        public static List<ColorEventData> GetDefaultEvents(Color value, float duration)
        {
            List<ColorEventData> ev = new();

            ev.Add(new ColorEventData
            {
                StartValue = value,
                EndValue = value,
                StartTime = 0f,
                EndTime = duration,
                EasingType = EaseUtility.Ease.Linear,
                StartEasingRange = 0f,
                EndEasingRange = 1f
            });

            return ev;
        }

        //谱面转换为string
        public static string ToString(this ChartData chart)
        {
            //开一个sb
            var sb = new StringBuilder();

            //首先是MetaData
            var meta = chart.MetaData;
            AddLine($"MusicID:{meta.MusicID}");
            AddLine($"AspectRatio:{meta.TargetAspectRatio}");
            AddLine($"LevelName:{meta.LevelName}");
            AddLine($"CharterName:{meta.CharterName}");
            AddLine($"DifficultyName:{meta.DifficultyName}");

            //MetaData分隔符
            AddLine("-");

            //Camera放在最前面
            AddLine($"camera:{chart.Camera.FatherTrackIndex}");

            //Camera事件层
            for (var i = 0; i < chart.Camera.Events.Count; i++)
            {
                if (i > 0)
                {
                    AddLine("addeventlayer:");
                }

                var layer = chart.Camera.Events[i];

                AddNumEvents("moveX", layer.MoveXEvents);
                AddNumEvents("moveY", layer.MoveYEvents);
                AddNumEvents("moveZ", layer.MoveZEvents);

                AddNumEvents("rotateX", layer.RotateXEvents);
                AddNumEvents("rotateY", layer.RotateYEvents);
                AddNumEvents("rotateZ", layer.RotateZEvents);

                AddNumEvents("fov", layer.FovEvents);
            }

            //然后是轨道们
            for (var i = 0; i < chart.Tracks.Count; i++)
            {
                AddTrack(chart.Tracks[i]);
            }

            //最后会多一个换行，删一下，感觉用Remove比Trim省内存
            return sb.Remove(sb.Length - 1, 1).ToString();
            //return sb.ToString().Trim();

            //下面就是方法/函数了

            void AddTrack(TrackData data)
            {
                AddLine($"track:{data.FatherTrackIndex},{data.Compensate3D},{data.CustomImageIndex},{data.Remarks}");

                //先处理note
                for (var i = 0; i < data.Notes.Count; i++)
                {
                    var note = data.Notes[i];
                    if (note.NoteType is NoteType.Hold)
                    {
                        //Hold的
                        //note:hold,下落方向,判定时间（毫秒）,按住时间,X坐标,真假性
                        AddLine($"note:hold,{note.FallDirection},{note.HitTime * 1000f},{note.HoldTime * 1000f},{note.PosX},{!note.IsFake},{note.LengthX}");
                    }
                    else
                    {
                        //其他的
                        //note:判定类型,下落方向,判定时间（毫秒）,X坐标,真假性
                        AddLine($"note:{GetNoteTypeString(note.NoteType)},{note.FallDirection},{note.HitTime * 1000f},{note.PosX},{!note.IsFake},{note.LengthX}");
                    }
                }

                //处理线
                for (var i = 0; i < data.Lines.Count; i++)
                {
                    var line = data.Lines[i];
                    //line:下落方向,判定时间,X相对位置,目标轨道id,目标下落方向,目标判定时间,目标X坐标,线颜色
                    AddLine($"line:{line.FallDirection},{line.HitTime * 1000f},{line.PosX},{line.TargetTrackIndex},{line.TargetFallDirection},{line.TargetHitTime * 1000f},{line.TargetPosX},{Color2INO(line.StartColor)},{Color2INO(line.EndColor)}");
                }

                //事件层
                for (var i = 0; i < data.Events.Count; i++)
                {
                    if (i > 0)
                    {
                        AddLine("addeventlayer:");
                    }

                    var layer = data.Events[i];

                    AddNumEvents("moveX", layer.MoveXEvents);
                    AddNumEvents("moveY", layer.MoveYEvents);
                    AddNumEvents("moveZ", layer.MoveZEvents);

                    AddNumEvents("rotateX", layer.RotateXEvents);
                    AddNumEvents("rotateY", layer.RotateYEvents);
                    AddNumEvents("rotateZ", layer.RotateZEvents);

                    AddNumEvents("lengthUp", layer.UpLengthEvents);

                    AddNumEvents("noteSpeed", layer.NoteSpeedEvents);

                    AddColorEvents("colorUp", layer.UpSurfaceColorEvents);
                }
            }

            string GetNoteTypeString(NoteType type)
            {
                switch (type)
                {
                    case NoteType.Tap:
                        return "tap";
                    case NoteType.Hold:
                        return "hold";
                    case NoteType.Drag:
                        return "drag";
                    case NoteType.Flick:
                        return "flick";
                    default:
                        throw new Exception("sb检查器");
                }
            }

            void AddNumEvents(string key, List<NumEventData> events)
            {
                for (var i = 0; i < events.Count; i++)
                {
                    var ev = events[i];
                    AddLine($"event:{key},{ev.StartTime * 1000f},{ev.EndTime * 1000f},{ev.StartValue},{ev.EndValue},{(int)ev.EasingType},{ev.StartEasingRange},{ev.EndEasingRange}");
                }
            }

            void AddColorEvents(string key, List<ColorEventData> events)
            {
                for (var i = 0; i < events.Count; i++)
                {
                    var ev = events[i];
                    AddLine($"event:{key},{ev.StartTime * 1000f},{ev.EndTime * 1000f},{Color2INO(ev.StartValue)},{Color2INO(ev.EndValue)},{(int)ev.EasingType},{ev.StartEasingRange},{ev.EndEasingRange}");
                }
            }

            string Color2INO(Color color)
            {
                return $"[{color.r * 255f};{color.g * 255f};{color.b * 255f};{color.a * 255f}]";
            }

            void AddLine(string add)
            {
                sb.Append(add);
                sb.Append("\n");
            }
        }

        //从谱面string反序列化ChartData
        public static ChartData ReadChart(string chartStr)
        {
            var chart = new ChartData();
            var lines = chartStr.Split('\n');

            int splitIndex = -1;

            // 首先，查找MetaData分隔符
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("-"))
                {
                    splitIndex = i;
                    break;
                }
            }

            if (splitIndex == -1)
            {
                throw new InvalidOperationException("未找到MataData分隔符");
            }

            // 然后读MetaData
            chart.MetaData = new ChartMetaData();

            for (int i = 0; i < splitIndex; i++)
            {
                try
                {
                    var line = lines[i].Trim();

                    //整行注释
                    if (line.StartsWith("//")) continue;
                    //后补注释
                    if (line.Contains("//")) line = line.Substring(0, line.IndexOf("//"));

                    int index = line.IndexOf(':');
                    if (index == -1)
                    {
                        Debug.LogWarning($"第{i + 1}行不为注释且未找到key/value分隔符（英文冒号），该行已被跳过");
                        continue;
                    }

                    var key = line.Substring(0, index);
                    var value = line.Substring(index + 1);
                    switch (key)
                    {
                        case "MusicID":
                            chart.MetaData.MusicID = int.Parse(value);
                            break;
                        case "AspectRatio":
                            if (value.Contains(','))
                            {
                                var points = value.Split(',');
                                chart.MetaData.TargetAspectRatio = 1f * int.Parse(points[0]) / int.Parse(points[1]);
                            }
                            else
                            {
                                chart.MetaData.TargetAspectRatio = float.Parse(value);
                            }
                            break;
                        case "LevelName":
                            chart.MetaData.LevelName = value;
                            break;
                        case "CharterName":
                            chart.MetaData.CharterName = value;
                            break;
                        case "DifficultyName":
                            chart.MetaData.DifficultyName = value;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"谱面解码问题发生在第{i + 1}行，详细内容：" + ex.Message);
                }
            }

            var TrackList = new List<TrackData>();

            TrackData LastTrack = new();

            var NoteList = new List<NoteData>();

            var LineList = new List<LineData>();

            var NowReadType = ReadType.None;

            var CameraEventLayerList = new List<CameraEvents>();
            var TrackEventLayerList = new List<TrackEvents>();

            var EventDict = new Dictionary<string, List<TextEventData>>();

            var HasReadCamera = false;

            // 读谱面的数据
            for (int i = splitIndex + 1; i < lines.Length; i++)
            {
                try
                {
                    var line = lines[i].Trim();

                    //整行注释
                    if (line.StartsWith("//")) continue;
                    //后补注释
                    if (line.Contains("//")) line = line.Substring(0, line.IndexOf("//")).Trim();

                    int index = line.IndexOf(':');
                    if (index == -1)
                    {
                        Debug.LogWarning($"第{i + 1}行不为注释且未找到key/value分隔符（英文冒号），该行已被跳过");
                        continue;
                    }

                    var key = line.Substring(0, index);
                    var value = line.Substring(index + 1);
                    switch (key)
                    {
                        case "track":
                            AddTrack(value);
                            break;
                        case "camera":
                            AddCamera(value);
                            break;
                        case "note":
                            AddNote(value);
                            break;
                        case "line":
                            AddLine(value);
                            break;
                        case "event":
                            AddEvent(value, i + 1);
                            break;
                        case "addeventlayer":
                            OnFinishEventLayer();
                            break;

                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"谱面解码问题发生在第{i + 1}行，详细内容：" + ex.Message);
                }
            }

            if (!HasReadCamera) throw new Exception("你怎么不写卡姆rua啊");
            OnFinishObject();

            chart.Tracks = TrackList;

            return chart;


            void AddTrack(string value)
            {
                OnFinishObject();

                NowReadType = ReadType.Track;

                LastTrack = new();
                //如果是新版的多参数Track
                if (value.Contains(","))
                {
                    var points = value.Split(',');
                    LastTrack.FatherTrackIndex = int.Parse(points[0]);
                    LastTrack.Compensate3D = float.Parse(points[1]);
                    LastTrack.CustomImageIndex = int.Parse(points[2]);

                    //如果注释里有英文逗号，，，
                    if (points.Length > 4)
                    {
                        var sb = new StringBuilder();

                        for (var i = 3; i < points.Length; i++)
                        {
                            sb.Append(points[i]);

                            if (i != points.Length - 1)
                            {
                                sb.Append(",");
                            }
                        }
                    }
                    else
                    {
                        LastTrack.Remarks = points[3];
                    }
                }
                else
                {
                    LastTrack.FatherTrackIndex = int.Parse(value);
                }
            }

            void AddCamera(string value)
            {
                OnFinishObject();

                if (HasReadCamera)
                {
                    throw new Exception("只能有一个卡姆rua哦");
                }
                else
                {
                    HasReadCamera = true;
                    chart.Camera = new CameraData();
                    chart.Camera.FatherTrackIndex = int.Parse(value);
                    NowReadType = ReadType.Camera;
                }
            }

            void OnFinishEventLayer()
            {
                if (NowReadType is ReadType.None) return;

                if (NowReadType is ReadType.Track)
                {
                    var tempLayer = new TrackEvents();

                    GetNumEventsFromDict(ref tempLayer.MoveXEvents, "moveX");
                    GetNumEventsFromDict(ref tempLayer.MoveYEvents, "moveY");
                    GetNumEventsFromDict(ref tempLayer.MoveZEvents, "moveZ");

                    GetNumEventsFromDict(ref tempLayer.RotateXEvents, "rotateX");
                    GetNumEventsFromDict(ref tempLayer.RotateYEvents, "rotateY");
                    GetNumEventsFromDict(ref tempLayer.RotateZEvents, "rotateZ");

                    GetNumEventsFromDict(ref tempLayer.UpLengthEvents, "lengthUp");

                    GetNumEventsFromDict(ref tempLayer.NoteSpeedEvents, "noteSpeed");

                    GetColorEventsFromDict(ref tempLayer.UpSurfaceColorEvents, "colorUp");

                    TrackEventLayerList.Add(tempLayer);

                    EventDict.Clear();
                }
                else
                {
                    var tempLayer = new CameraEvents();

                    GetNumEventsFromDict(ref tempLayer.MoveXEvents, "moveX");
                    GetNumEventsFromDict(ref tempLayer.MoveYEvents, "moveY");
                    GetNumEventsFromDict(ref tempLayer.MoveZEvents, "moveZ");

                    GetNumEventsFromDict(ref tempLayer.RotateXEvents, "rotateX");
                    GetNumEventsFromDict(ref tempLayer.RotateYEvents, "rotateY");
                    GetNumEventsFromDict(ref tempLayer.RotateZEvents, "rotateZ");

                    GetNumEventsFromDict(ref tempLayer.FovEvents, "fov");

                    CameraEventLayerList.Add(tempLayer);

                    EventDict.Clear();
                }
            }

            void OnFinishObject()
            {
                if (NowReadType is ReadType.None) return;

                OnFinishEventLayer();

                if (NowReadType is ReadType.Track)
                {
                    LastTrack.Notes = NoteList.OrderBy(x => x.HitTime).ToList();
                    LastTrack.Lines = LineList.OrderBy(x => x.HitTime).ThenBy(x => x.TargetHitTime).ToList();
                    LastTrack.Events = TrackEventLayerList.ToList();

                    TrackEventLayerList.Clear();
                    NoteList.Clear();
                    LineList.Clear();

                    TrackList.Add(LastTrack);
                }
                else
                {
                    chart.Camera.Events = CameraEventLayerList.ToList();
                    CameraEventLayerList.Clear();
                }
            }

            void GetNumEventsFromDict(ref List<NumEventData> events, string key)
            {
                if (EventDict.ContainsKey(key))
                {
                    events = EventDict[key].OrderBy(x => x.StartTime).ToList().ToNum();
                }
                else
                {
                    events = new List<NumEventData>();
                }
            }

            void GetColorEventsFromDict(ref List<ColorEventData> events, string key)
            {
                if (EventDict.ContainsKey(key))
                {
                    events = EventDict[key].OrderBy(x => x.StartTime).ToList().ToColor();
                }
                else
                {
                    events = new List<ColorEventData>();
                }
            }

            void AddLine(string value)
            {
                if (NowReadType is not ReadType.Track)
                {
                    throw new Exception("Line不能脱离Track存在啊喂");
                }
                else
                {
                    var points = value.Split(',');

                    LineList.Add(new LineData
                    {
                        FallDirection = ParseFallingDirection(points[0]),
                        HitTime = float.Parse(points[1]) * 0.001f,
                        PosX = float.Parse(points[2]),

                        TargetTrackIndex = int.Parse(points[3]),

                        TargetFallDirection = ParseFallingDirection(points[4]),
                        TargetHitTime = float.Parse(points[5]) * 0.001f,
                        TargetPosX = float.Parse(points[6]),

                        StartColor = ParseInokanaColor(points[7]),
                        EndColor = ParseInokanaColor(points[8])
                    });
                }
            }

            void AddNote(string value)
            {
                if (NowReadType is not ReadType.Track)
                {
                    throw new Exception("Note不能脱离Track存在啊喂");
                }
                else
                {
                    var points = value.Split(',');

                    NoteType tempType;
                    switch (points[0])
                    {
                        case "tap":
                            tempType = NoteType.Tap;
                            break;
                        case "hold":
                            tempType = NoteType.Hold;
                            break;
                        case "drag":
                            tempType = NoteType.Drag;
                            break;
                        case "flick":
                            tempType = NoteType.Flick;
                            break;
                        default:
                            throw new Exception("你创造了什么新的Note类型吗");
                    }

                    if (tempType is NoteType.Hold)
                    {
                        NoteList.Add(new NoteData
                        {
                            NoteType = NoteType.Hold,
                            FallDirection = ParseFallingDirection(points[1]),
                            HitTime = float.Parse(points[2]) * 0.001f,
                            HoldTime = float.Parse(points[3]) * 0.001f,
                            PosX = float.Parse(points[4]),
                            IsFake = ParseTrueOrFake(points[5]),
                            LengthX = points.Length > 6 ? float.Parse(points[6]) : 1f
                        });
                    }
                    else
                    {
                        NoteList.Add(new NoteData
                        {
                            NoteType = tempType,
                            FallDirection = ParseFallingDirection(points[1]),
                            HitTime = float.Parse(points[2]) * 0.001f,
                            PosX = float.Parse(points[3]),
                            IsFake = ParseTrueOrFake(points[4]),
                            LengthX = points.Length > 5 ? float.Parse(points[5]) : 1f
                        });
                    }
                }

                bool ParseTrueOrFake(string raw)
                {
                    switch (raw)
                    {
                        case "True":
                        case "true":
                        case "real":
                            return false;
                        case "False":
                        case "false":
                        case "fake":
                            return true;
                        default:
                            throw new Exception("你要进入量子领域吗（出现了意料之外的真假类型）");
                    }
                }
            }


            FallingDirection ParseFallingDirection(string raw)
            {
                switch (raw)
                {
                    case "Up":
                    case "up":
                    case "1":
                        return FallingDirection.Up;
                    case "Down":
                    case "down":
                    case "-1":
                        return FallingDirection.Down;
                    default:
                        throw new Exception("你还想怎么落（出现了意料之外的下落方向）");
                }
            }

            void AddEvent(string value, int lineIndex)
            {
                if (NowReadType is ReadType.None)
                {
                    throw new Exception("事件，总得有个家（事件需要添加到Camera或Track上）");
                }
                else
                {
                    try
                    {
                        var points = value.Split(',');

                        // 如果没有这种类就新建
                        if (!EventDict.ContainsKey(points[0]))
                        {
                            EventDict.Add(points[0], new List<TextEventData>());
                        }

                        if (points.Length == 2)//简略默认值语法
                        {
                            EventDict[points[0]].Add(new TextEventData
                            {
                                StartTime = 0f,
                                EndTime = 0f,
                                StartValue = points[1],
                                EndValue = points[1],
                                EasingType = EaseUtility.Ease.Linear,
                                StartEasingRange = 0f,
                                EndEasingRange = 1f
                            });
                        }
                        else
                        {
                            EventDict[points[0]].Add(new TextEventData
                            {
                                StartTime = float.Parse(points[1]) * 0.001f,
                                EndTime = float.Parse(points[2]) * 0.001f,
                                StartValue = points[3],
                                EndValue = points[4],
                                EasingType = points.Length > 5 ? (EaseUtility.Ease)int.Parse(points[5]) : EaseUtility.Ease.Linear,
                                StartEasingRange = points.Length > 6 ? float.Parse(points[6]) : 0f,
                                EndEasingRange = points.Length > 7 ? float.Parse(points[7]) : 1f
                            });
                        }
                    }
                    catch
                    {
                        throw new Exception("事件除始末值以外的信息解析失败，请检查格式");
                    }
                }
            }
        }

        public static List<ColorEventData> ToColor(this List<TextEventData> t)
        {
            var n = new ColorEventData[t.Count];

            for (var i = 0; i < t.Count; i++)
            {
                var temp = t[i];

                try
                {
                    n[i] = new ColorEventData
                    {
                        StartTime = temp.StartTime,
                        EndTime = temp.EndTime,
                        StartValue = ParseInokanaColor(temp.StartValue),
                        EndValue = ParseInokanaColor(temp.EndValue),
                        EasingType = temp.EasingType,
                        StartEasingRange = temp.StartEasingRange,
                        EndEasingRange = temp.EndEasingRange
                    };
                }
                catch
                {
                    throw new Exception($"存在无法解析始末值的事件，请检查格式");
                }
            }

            return n.ToList();
        }

        //be like [12;42;51;51]
        public static Color ParseInokanaColor(string str)
        {
            str = str.Replace("[", "").Replace("]", "");
            var points = str.Split(';');
            return new Color(float.Parse(points[0]) / 255f, float.Parse(points[1]) / 255f, float.Parse(points[2]) / 255f, float.Parse(points[3]) / 255f);
        }

        public static List<NumEventData> ToNum(this List<TextEventData> t)
        {
            var n = new NumEventData[t.Count];


            for (var i = 0; i < t.Count; i++)
            {
                var temp = t[i];

                try
                {
                    n[i] = new NumEventData
                    {
                        StartTime = temp.StartTime,
                        EndTime = temp.EndTime,
                        StartValue = float.Parse(temp.StartValue),
                        EndValue = float.Parse(temp.EndValue),
                        EasingType = temp.EasingType,
                        StartEasingRange = temp.StartEasingRange,
                        EndEasingRange = temp.EndEasingRange
                    };
                }
                catch
                {
                    throw new Exception($"存在无法解析始末值的事件，请检查格式");
                }
            }


            return n.ToList();
        }

        public static ChartData InitObjectIndex(this ChartData c)
        {
            //赋值物件的index
            for (var i = 0; i < c.Tracks.Count; i++)
            {
                c.Tracks[i].TrackIndex = i;

                var upIndex = 0;
                var downIndex = 0;

                for (var j = 0; j < c.Tracks[i].Notes.Count; j++)
                {
                    var note = c.Tracks[i].Notes[j];

                    if (note.FallDirection is FallingDirection.Up)
                    {
                        note.NoteIndex = upIndex;
                        upIndex++;
                    }
                    else
                    {
                        note.NoteIndex = downIndex;
                        downIndex++;
                    }
                }
            }

            return c;
        }

        public static ChartData InitNoteHighlight(this ChartData c)
        {
            //if (GlobalSettings.CurrentSettings.DoubleClickHighlight)
            {
                //首先去除所有高光
                foreach (var t in c.Tracks)
                {
                    foreach (var n in t.Notes)
                    {
                        n.IsHighlight = false;
                    }
                }

                //正常使用字典标注
                var noteDict = new Dictionary<int, NoteData>();

                for (var i = 0; i < c.Tracks.Count; i++)
                {
                    var notes = c.Tracks[i].Notes;
                    for (var j = 0; j < notes.Count; j++)
                    {
                        var note = notes[j];
                        var time = (int)(note.HitTime * 100f);// 乘100然后舍小数正好控制在10ms

                        if (!noteDict.TryGetValue(time, out var oldNote))
                        {
                            noteDict.Add(time, note);
                            continue;
                        }

                        oldNote.IsHighlight = true;
                        note.IsHighlight = true;
                    }
                }
            }

            return c;
        }

        public static ChartData InitGlobalSpeed(this ChartData c)
        {
            //var globalSpeed = GlobalSettings.CurrentSettings.GlobalNoteSpeed;
            var globalSpeed = 1f;

            foreach (var thisTrack in c.Tracks)
            {
                foreach (var eventslayer in thisTrack.Events)
                {
                    foreach (var ev in eventslayer.NoteSpeedEvents)
                    {
                        ev.StartValue *= globalSpeed;
                        ev.EndValue *= globalSpeed;
                    }
                }
            }

            return c;
        }

        public static ChartData InitSpeedEventsLinear(this ChartData c)
        {
            foreach (var thisTrack in c.Tracks)
            {
                foreach (var eventslayer in thisTrack.Events)
                {
                    var duration = 0f;

                    foreach (var ev in eventslayer.NoteSpeedEvents)
                    {
                        //是线性就不用切那么碎了
                        if (ev.EasingType is EaseUtility.Ease.Linear)
                        {
                            if (ReferenceEquals(ev.Duration, null) || ev.Duration.Length != 1)
                            {
                                ev.Duration = new float[2];
                                ev.LinearEaseValue = null;
                            }

                            ev.Duration[0] = duration;
                            if (ev.StartValue == ev.EndValue)
                            {
                                duration += ev.StartValue * ev.DurationTime;
                            }
                            else
                            {
                                duration += (ev.StartValue + ev.EndValue) * ev.DurationTime * 0.5f;
                            }
                            ev.Duration[1] = duration;
                        }
                        else
                        {
                            //为空才初始化
                            if (ReferenceEquals(ev.LinearEaseValue, null))
                            {
                                ev.LinearEaseValue = new float[64];
                            }

                            //先赋值
                            for (var i = 0; i < 64; i++)
                            {
                                ev.LinearEaseValue[i] = GameplayUtility.GetValueFromTimeAndValue(i, 0, 63, ev.StartValue, ev.EndValue, ev.EasingType, ev.StartEasingRange, ev.EndEasingRange);
                            }

                            if (ReferenceEquals(ev.Duration, null) || ev.Duration.Length != 64)
                            {
                                ev.Duration = new float[64];
                            }

                            //然后算面积
                            var segTime = ev.DurationTime / 63;

                            for (var i = 0; i < 64; i++)
                            {
                                ev.Duration[i] = duration;

                                if (i == 63) break;

                                duration += (ev.LinearEaseValue[i] + ev.LinearEaseValue[i + 1]) * segTime * 0.5f;
                            }
                        }
                    }
                }
            }

            return c;
        }

        public enum ReadType
        {
            Track = 0, Camera = 1, None = 2
        }
    }
}