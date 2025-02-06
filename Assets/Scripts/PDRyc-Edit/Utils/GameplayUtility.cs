using System.Collections;
using System.Collections.Generic;
using PDRO.Data;
using UnityEngine;
using System;

namespace PDRO.Utils
{


    public static class GameplayUtility
    {
        /// <summary> 通过时间和SpeedEvent求出一段Distance </summary>
        public static float CalculateDistance(float nowTime, NumEventData speedEvent)
        {
            // 原理：梯形面积公式

            // 当前时间之后的可能已经在上一步排除掉了，故这里不再检查

            // 如果SpeedEvent在当前时间之前，就返回完整的Distance
            if (speedEvent.EndTime <= nowTime)
                return speedEvent.Duration[^1];

            // 如果SpeedEvent在当前时间内，就返回插值的Distance

            var timeOfEvent = nowTime - speedEvent.StartTime;
            var duration = timeOfEvent / speedEvent.DurationTime;

            if (speedEvent.EasingType is EaseUtility.Ease.Linear)
            {
                //正方形的hhh
                if (speedEvent.StartValue == speedEvent.EndValue)
                {
                    return speedEvent.Duration[0] + speedEvent.StartValue * timeOfEvent;
                }
                else//梯形
                {
                    return speedEvent.Duration[0] + (speedEvent.StartValue + speedEvent.ValueChangeAmount * duration + speedEvent.StartValue) * timeOfEvent * 0.5f;
                }
            }
            else
            {
                var segTime = speedEvent.DurationTime / 63;

                var duration2Seg = duration * 63f;
                var duration2SegFloor = (int)duration2Seg;

                var diff = duration2Seg - duration2SegFloor;

                var l = speedEvent.LinearEaseValue[duration2SegFloor];
                var r = (speedEvent.LinearEaseValue[duration2SegFloor + 1] - speedEvent.LinearEaseValue[duration2SegFloor]) * diff + speedEvent.LinearEaseValue[duration2SegFloor];

                return speedEvent.Duration[duration2SegFloor] + (l + r) * (segTime * diff) * 0.5f;
            }
        }

        /// <summary> 获得ColorEvent的当前颜色 </summary>
        public static Color GetColorFromColorEvent(float nowTime, List<ColorEventData> Events)
        {
            if (!Events.IsNullOrEmpty())
            {
                var temp = Events[0].StartValue;
                for (int i = 0; i < Events.Count; i++)
                {
                    var CurrentEvent = Events[i];
                    float st = CurrentEvent.StartTime;
                    if (st <= nowTime)
                    {
                        var et = CurrentEvent.EndTime;
                        var sv = Events[i].StartValue;
                        var ev = Events[i].EndValue;
                        var ease = Events[i].EasingType;
                        var sr = Events[i].StartEasingRange;
                        var er = Events[i].EndEasingRange;

                        var t = GetValueFromTimeAndValue(nowTime, st, et, 0f, 1f, ease, sr, er);
                        temp = Color.LerpUnclamped(sv, ev, t);
                    }
                    else break;
                }
                temp.r = 1f;
                temp.g = 1f;
                temp.b = 1f;
                return temp;
            }
            else
            {
                return new Color(0f, 0f, 0f, 0f);
            }
        }

        /// <summary> 获得TrackEvent的当前值 </summary>
        public static float GetValueFromTrackEvent(float nowTime, List<NumEventData> Events)
        {
            if (!Events.IsNullOrEmpty())
            {
                var temp = Events[0].StartValue;
                for (var i = 0; i < Events.Count; i++)
                {
                    var CurrentEvent = Events[i];
                    var st = Events[i].StartTime;
                    if (st <= nowTime)
                    {
                        var et = CurrentEvent.EndTime;
                        var sv = CurrentEvent.StartValue;
                        var ev = CurrentEvent.EndValue;
                        var ease = CurrentEvent.EasingType;
                        var sr = CurrentEvent.StartEasingRange;
                        var er = CurrentEvent.EndEasingRange;
                        temp = GetValueFromTimeAndValue(nowTime, st, et, sv, ev, ease, sr, er);
                    }
                    else break;
                }
                return temp;
            }
            else
            {
                return 0f;
            }
        }

        /// <summary> 通过时间和首尾数值获得缓动插值 </summary>
        public static float GetValueFromTimeAndValue(float now, float startTime, float endTime, float startValue, float endValue, EaseUtility.Ease ease, float startRange = 0, float endRange = 1)
        {
            if (now < endTime)
            {
                float value = EaseUtility.Evaluate(ease, now - startTime, endTime - startTime, startRange, endRange);
                value = (endValue - startValue) * value + startValue;
                return value;
            }

            return endValue;
        }

        /// <summary> 判断List是否为空 </summary>
        public static bool IsNullOrEmpty<T>(this IList<T> list) => list == null || list is not { Count: > 0 };
    }
}