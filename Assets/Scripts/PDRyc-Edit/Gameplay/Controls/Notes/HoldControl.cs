using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Data;
using PDRO.Gameplay.Managers;

namespace PDRO.Gameplay.Controls
{
    public class HoldControl : NoteControl
    {
        public HoldControl(NoteData data, TrackControl track) : base(data, track) { }

        protected override void UpdateNoteOthers()
        {
            UpdateHoldBody();
        }

        void UpdateHoldBody()
        {
            var posDelta = Length2Transform(CurrentData.FallDirection == FallingDirection.Up ? FatherTrack.UpLength : FatherTrack.DownLength) - CalculateNoteHeight();
            var size = Mathf.Min(CalculateHoldSize(), posDelta)*0.8f;
            HoldInstance.HoldScaleTransform.localScale = new Vector3(1f, size * 0.1f, 1f);
            HoldInstance.HoldEndSpriteRenderer.transform.localPosition = new Vector3(0f, size, 0f);
        }

        private float CalculateHoldSize()
        {
            if (ProgressManager.Instance.NowTime > HitTime + HoldTime) return 0f;

            float distance = HitTime >= ProgressManager.Instance.NowTime ? StartDistance : FatherTrack.NowDistance;
            return Mathf.Abs((EndDistance - distance));
        }
    }
}