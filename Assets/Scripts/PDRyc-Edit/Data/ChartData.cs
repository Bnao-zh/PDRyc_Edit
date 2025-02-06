using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PDRO.Utils;

namespace PDRO.Data
{
    public static class DeepCloneUtils
    {
        public static List<T> DeepClone<T>(this List<T> oldList) where T : IDeepCloneable<T>
        {
            var newList = new List<T>();

            foreach (var each in oldList)
            {
                newList.Add(each.DeepClone());
            }

            return newList;
        }
    }

    [Serializable]
    public class ChartData : IDeepCloneable<ChartData>
    {
        public ChartMetaData MetaData;

        public List<TrackData> Tracks;
        public CameraData Camera;
        public ChartData DeepClone()
        {
            return new ChartData
            {
                MetaData = this.MetaData.DeepClone(),
                Tracks = this.Tracks.DeepClone(),
                Camera = this.Camera.DeepClone()
            };
        }
    }

    [Serializable]
    public class ChartMetaData : IDeepCloneable<ChartMetaData>
    {
        // 一会儿再想想放点啥
        public int MusicID;

        public float TargetAspectRatio = 1.777778f;

        public string LevelName = "未指定的关卡名";
        public string CharterName = "未指定的谱师名";
        public string DifficultyName = "未指定的难度名";

        public ChartMetaData DeepClone()
        {
            return new ChartMetaData
            {
                MusicID = this.MusicID,
                TargetAspectRatio = this.TargetAspectRatio,
                LevelName = this.LevelName,
                CharterName = this.CharterName,
                DifficultyName = this.DifficultyName
            };
        }
    }

    [Serializable]
    public class CameraData : IDeepCloneable<CameraData>
    {
        public int FatherTrackIndex = -1;// 父轨道，-1为无
        public List<CameraEvents> Events;

        public CameraData DeepClone()
        {
            return new CameraData
            {
                FatherTrackIndex = this.FatherTrackIndex,
                Events = this.Events.DeepClone(),
            };
        }
    }

    [Serializable]
    public class CameraEvents : TransformEvents, IDeepCloneable<CameraEvents>
    {
        public List<NumEventData> FovEvents;

        public CameraEvents DeepClone()
        {
            return new CameraEvents
            {
                MoveXEvents = this.MoveXEvents.DeepClone(),
                MoveYEvents = this.MoveYEvents.DeepClone(),
                MoveZEvents = this.MoveZEvents.DeepClone(),
                RotateXEvents = this.RotateXEvents.DeepClone(),
                RotateYEvents = this.RotateYEvents.DeepClone(),
                RotateZEvents = this.RotateZEvents.DeepClone(),

                FovEvents = this.FovEvents.DeepClone()
            };
        }
    }

    [Serializable]
    public class TrackData : IDeepCloneable<TrackData>
    {
        public int TrackIndex;

        public string Remarks = "Unknow";
        public float Compensate3D = 0f;
        public int CustomImageIndex = -1;
        public int FatherTrackIndex = -1;// 父轨道，-1为无
        public List<TrackEvents> Events;
        public List<NoteData> Notes;
        public List<LineData> Lines;

        public TrackData DeepClone()
        {
            return new TrackData
            {
                TrackIndex = this.TrackIndex,
                Remarks = this.Remarks,
                Compensate3D = this.Compensate3D,
                CustomImageIndex = this.CustomImageIndex,
                FatherTrackIndex = this.FatherTrackIndex,
                Events = this.Events.DeepClone(),
                Notes = this.Notes.DeepClone(),
                Lines = this.Lines.DeepClone()
            };
        }
    }

    public class TransformEvents
    {
        public List<NumEventData> MoveXEvents;
        public List<NumEventData> MoveYEvents;
        public List<NumEventData> MoveZEvents;

        public List<NumEventData> RotateXEvents;
        public List<NumEventData> RotateYEvents;
        public List<NumEventData> RotateZEvents;
    }

    [Serializable]
    public class TrackEvents : TransformEvents, IDeepCloneable<TrackEvents>
    {
        public List<ColorEventData> UpSurfaceColorEvents;
        public List<NumEventData> UpLengthEvents;
        public List<NumEventData> NoteSpeedEvents;

        public TrackEvents DeepClone()
        {
            return new TrackEvents
            {
                MoveXEvents = this.MoveXEvents.DeepClone(),
                MoveYEvents = this.MoveYEvents.DeepClone(),
                MoveZEvents = this.MoveZEvents.DeepClone(),
                RotateXEvents = this.RotateXEvents.DeepClone(),
                RotateYEvents = this.RotateYEvents.DeepClone(),
                RotateZEvents = this.RotateZEvents.DeepClone(),

                UpSurfaceColorEvents = this.UpSurfaceColorEvents.DeepClone(),
                UpLengthEvents = this.UpLengthEvents.DeepClone(),
                NoteSpeedEvents = this.NoteSpeedEvents.DeepClone()
            };
        }
    }

    [Serializable]
    public class LineData : BaseObjectData, IDeepCloneable<LineData>
    {
        public int TargetTrackIndex;
        public float TargetHitTime;
        public FallingDirection TargetFallDirection = FallingDirection.Up;
        public float TargetPosX;

        public Color StartColor;
        public Color EndColor;

        public LineData DeepClone()
        {
            return new LineData
            {
                HitTime = this.HitTime,
                FallDirection = this.FallDirection,
                PosX = this.PosX,
                TargetTrackIndex = this.TargetTrackIndex,
                TargetHitTime = this.TargetHitTime,
                TargetFallDirection = this.TargetFallDirection,
                TargetPosX = this.TargetPosX,
                StartColor = this.StartColor,
                EndColor = this.EndColor
            };
        }
    }

    [Serializable]
    public class NoteData : BaseObjectData, IDeepCloneable<NoteData>
    {
        public NoteType NoteType;
        public float LengthX = 1f;
        public float HoldTime;
        public bool IsFake;

        //这两个是开始时预计算的
        public int NoteIndex;
        public bool IsHighlight;

        public NoteData DeepClone()
        {
            return new NoteData
            {
                HitTime = this.HitTime,
                FallDirection = this.FallDirection,
                PosX = this.PosX,
                NoteType = this.NoteType,
                LengthX = this.LengthX,
                HoldTime = this.HoldTime,
                IsFake = this.IsFake
            };
        }
    }

    [Serializable]
    public class BaseObjectData : IComparable<BaseObjectData>
    {
        public float HitTime;
        public FallingDirection FallDirection = FallingDirection.Up;
        public float PosX;

        //哇！先进科技！
        public int CompareTo(BaseObjectData data)
        {
            if (this.HitTime == data.HitTime)
            {
                return 0;
            }
            else if (this.HitTime > data.HitTime)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }

    public enum FallingDirection
    {
        Up = 1, Down = -1
    }

    public enum NoteType
    {
        Tap = 0, Drag = 1, Hold = 2, Flick = 3
    }

    //插值方式 0缓动，1表达式，2自定义曲线
    public enum EventInterpolationType
    {
        Ease = 0, Expression = 1, AnimationCurve = 2
    }

    [Serializable]
    public class BaseEventData : IComparable<BaseEventData>
    {
        public EventInterpolationType InterpolationType;

        //表达式
        public string Expression;
        //缓动
        public EaseUtility.Ease EasingType;
        public float StartEasingRange, EndEasingRange;

        public float StartTime, EndTime;
        public float DurationTime => EndTime - StartTime;

        //线性计算优化用的
        public float[] LinearEaseValue;

        public int CompareTo(BaseEventData data)
        {
            if (this.StartTime == data.StartTime)
            {
                return 0;
            }
            else if (this.StartTime > data.StartTime)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }

    [Serializable]
    public class TextEventData : BaseEventData, IDeepCloneable<TextEventData>
    {
        public string StartValue, EndValue;

        public TextEventData DeepClone()
        {
            return new TextEventData
            {
                InterpolationType = this.InterpolationType,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                StartValue = this.StartValue,
                EndValue = this.EndValue,
                EasingType = this.EasingType,
                StartEasingRange = this.StartEasingRange,
                EndEasingRange = this.EndEasingRange,
                Expression = this.Expression
            };
        }
    }

    [Serializable]
    public class NumEventData : BaseEventData, IDeepCloneable<NumEventData>
    {
        public float StartValue, EndValue;

        public float ValueChangeAmount => EndValue - StartValue;

        //速度用的
        public float[] Duration;

        public NumEventData DeepClone()
        {
            return new NumEventData
            {
                InterpolationType = this.InterpolationType,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                StartValue = this.StartValue,
                EndValue = this.EndValue,
                EasingType = this.EasingType,
                StartEasingRange = this.StartEasingRange,
                EndEasingRange = this.EndEasingRange,
                Expression = this.Expression
            };
        }
    }

    [Serializable]
    public class ColorEventData : BaseEventData, IDeepCloneable<ColorEventData>
    {
        public Color StartValue, EndValue;

        public ColorEventData DeepClone()
        {
            return new ColorEventData
            {
                InterpolationType = this.InterpolationType,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                StartValue = this.StartValue,
                EndValue = this.EndValue,
                EasingType = this.EasingType,
                StartEasingRange = this.StartEasingRange,
                EndEasingRange = this.EndEasingRange,
                Expression = this.Expression
            };
        }
    }
}
