using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Data;
using PDRO.Gameplay.Managers;

namespace PDRO.Gameplay.Controls
{
    public class LineControl : MonoBehaviour
    {
        public LineRenderer CurrentLine;

        public TrackControl FatherTrack;
        public TrackControl TargetTrack;

        public LineData CurrentData;

        public float StartDistance, TargetStartDistance;

        public void Init(LineData data, TrackControl fatherTrack)
        {
            CurrentData = data;

            FatherTrack = fatherTrack;
            TargetTrack = EditManager.Instance.Tracks[CurrentData.TargetTrackIndex];

            StartDistance = FatherTrack.GetDistance(CurrentData.HitTime);
            TargetStartDistance = TargetTrack.GetDistance(CurrentData.TargetHitTime);
        }

        private float _otherStartHeight = -1f;

        public void OnUpdate()
        {
            var height = CalculateHeight();

            if (NoteOutOfRenderRange() || ProgressManager.Instance.NowTime > CurrentData.TargetHitTime)
            {
                CurrentLine.enabled = false;
                return;
            }
            else
            {
                CurrentLine.enabled = true;

                var targetHeight = CalculateTargrtHeight();

                var thisPos = FatherTrack.transform.TransformPoint(new Vector3(CurrentData.PosX, CurrentData.FallDirection is FallingDirection.Up ? height : -height, 0f));
                var otherPos = TargetTrack.transform.TransformPoint(new Vector3(CurrentData.TargetPosX, CurrentData.TargetFallDirection is FallingDirection.Up ? targetHeight : -targetHeight, 0f));
                var deltaPos = otherPos - thisPos;
                // 如果另一个在轨道外面对结束点进行插值
                if (TargetOutOfRenderRange())
                {
                    var ns = -2f;

                    if (CurrentData.TargetFallDirection is FallingDirection.Up)
                    {
                        if (targetHeight > TargetTrack.UpLength * 2f)
                        {
                            ns = targetHeight - TargetTrack.UpLength * 2f;
                        }
                    }
                    else
                    {
                        if (targetHeight > TargetTrack.DownLength * 2f)
                        {
                            ns = targetHeight - TargetTrack.DownLength * 2f;
                        }
                    }

                    if (_otherStartHeight < ns)
                    {
                        _otherStartHeight = ns;
                    }



                    if (CurrentData.TargetFallDirection is FallingDirection.Up)
                    {
                        otherPos -= deltaPos * (targetHeight - TargetTrack.UpLength * 2f) / _otherStartHeight;
                    }
                    else
                    {
                        otherPos -= deltaPos * (targetHeight - TargetTrack.DownLength * 2f) / _otherStartHeight;
                    }
                }

                // 如果过了这个的判定时间则对开始点进行插值
                if (ProgressManager.Instance.NowTime > CurrentData.HitTime)
                {
                    // 效果就是当前时间占总时间比例越大开始点越接近结束点
                    thisPos += deltaPos * (ProgressManager.Instance.NowTime - CurrentData.HitTime) / (CurrentData.TargetHitTime - CurrentData.HitTime);
                }

                //var endPos=

                CurrentLine.SetPositions(new[] { thisPos, otherPos });

                CurrentLine.startColor = CurrentData.StartColor;
                CurrentLine.endColor = CurrentData.EndColor;

                bool TargetOutOfRenderRange()
                {
                    if (CurrentData.TargetFallDirection is FallingDirection.Up)
                    {
                        return targetHeight > TargetTrack.UpLength * 2f;
                    }
                    else
                    {
                        return targetHeight > TargetTrack.DownLength * 2f;
                    }
                }


            }

            bool NoteOutOfRenderRange()
            {
                if (CurrentData.FallDirection is FallingDirection.Up)
                {
                    return height > FatherTrack.UpLength * 2f;
                }
                else
                {
                    return height > FatherTrack.DownLength * 2f;
                }
            }

        }





        float CalculateHeight()
        {
            return StartDistance - FatherTrack.NowDistance;
        }

        float CalculateTargrtHeight()
        {
            return TargetStartDistance - TargetTrack.NowDistance;
        }

    }
}