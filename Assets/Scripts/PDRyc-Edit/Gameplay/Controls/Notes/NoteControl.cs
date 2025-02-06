using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using PDRO.Data;
using PDRO.Gameplay.Managers;

namespace PDRO.Gameplay.Controls
{
    public abstract class NoteControl
    {
        public NoteMonoBehaviour NoteInstance;
        public HoldMonoBehaviour HoldInstance;

        public NoteData CurrentData;
        public TrackControl FatherTrack;

        public float HitTime => CurrentData.HitTime;
        public float HoldTime => CurrentData.HoldTime;

        public float StartDistance, EndDistance;

        public bool IsJudged;

        protected NoteControl(NoteData data, TrackControl track)
        {
            CurrentData = data;
            FatherTrack = track;

            StartDistance = FatherTrack.GetDistance(HitTime);
            if (CurrentData.NoteType is NoteType.Hold)
            {
                EndDistance = FatherTrack.GetDistance(HitTime + HoldTime);
            }
        }


        public void OnUpdate()
        {
            var height = CalculateNoteHeight();

            //Note半高
            var NoteSpHeight = 0.25f;
            if (CurrentData.NoteType is not NoteType.Hold)
            {
                NoteSpHeight = 0.25f * (1f + FatherTrack.CurrentData.Compensate3D * Mathf.Clamp01(CalculateNoteHeight() / (CurrentData.FallDirection is FallingDirection.Up ? FatherTrack.UpLength * 2f : FatherTrack.DownLength * 2f)));
            }

            // 如果不该出现
            if (NoteOutOfRenderRange() || IsJudged && CurrentData.NoteType != NoteType.Hold || ProgressManager.Instance.NowTime > HitTime + HoldTime)
            {
                // 如果有实例则返回实例到对象池
                if (!System.Object.ReferenceEquals(NoteInstance, null))
                {
                    ReturnNoteInstance();
                }
            }
            // 如果要出现了
            else
            {
                // 如果无实例则从对象池里面拿一个实例
                if (System.Object.ReferenceEquals(NoteInstance, null))
                {
                    GetNoteInstance();
                }

                // 如果实例存在，就执行下面的方法 (如果还没出现或者超过判定时间不能执行)
                UpdateNoteTransform();
                //UpdateNoteRender();
                UpdateNoteOthers();

            }
            //
            UpdateAuto();
            //UpdateNoteRegister();
            //UpdateNoteJudge();
            //UpdateNoteMiss();
            UpdateHoldEffect();

            void UpdateAuto()
            {
                if (ProgressManager.Instance.NowTime - 0.001f > HitTime)
                {
                    JudgeNote(JudgeResult.PERFECT);
                }
                else
                {
                    IsJudged = false;
                }
            }

            bool NoteOutOfRenderRange()
            {
                if (CurrentData.FallDirection is FallingDirection.Up)
                {
                    return height > Length2Transform(FatherTrack.UpLength, NoteSpHeight) || height < -Length2Transform(FatherTrack.DownLength, NoteSpHeight);
                }
                else
                {
                    return height > Length2Transform(FatherTrack.DownLength, NoteSpHeight) || height < -Length2Transform(FatherTrack.UpLength, NoteSpHeight);
                }
            }

            void GetNoteInstance()
            {

                if (CurrentData.NoteType is NoteType.Hold)
                {
                    NoteInstance = PoolManager.Instance.GetHoldNote();
                    HoldInstance = NoteInstance as HoldMonoBehaviour;

                    HoldInstance.HoldBodySpriteRenderer.sprite = SkinManager.Instance.GetHoldBody(CurrentData.IsHighlight);
                    HoldInstance.HoldEndSpriteRenderer.sprite = SkinManager.Instance.GetHoldEnd(CurrentData.IsHighlight);
                }
                else
                {
                    NoteInstance = PoolManager.Instance.GetNote();
                }

                if (EditManager.Instance.ShowUI)
                {
                    NoteInstance.TipText.text =
                    $"{FatherTrack.CurrentData.TrackIndex} + {CurrentData.NoteIndex}";
                }
                else
                {
                    NoteInstance.TipText.text = null;
                }

                NoteInstance.NoteSpriteRenderer.sprite = SkinManager.Instance.GetBaseSprite(CurrentData.NoteType, CurrentData.IsHighlight);
                NoteInstance.transform.SetParent(FatherTrack.transform, false);
                NoteInstance.transform.localEulerAngles = CurrentData.FallDirection == FallingDirection.Up ? Vector3.zero : new Vector3(0f, 0f, 180f);
            }

            void ReturnNoteInstance()
            {
                if (CurrentData.NoteType is NoteType.Hold)
                {
                    PoolManager.Instance.ReturnHoldNote(HoldInstance);

                    HoldInstance = null;
                }
                else
                {
                    PoolManager.Instance.ReturnNote(NoteInstance);
                }

                NoteInstance = null;
            }
        }

        //计算hold特效间隔用的
        private float delta;

        void UpdateHoldEffect()
        {
            if (CurrentData.IsFake || CurrentData.NoteType is not NoteType.Hold) return;

            if (EditManager.Instance.NowTime - 0.001f > HitTime && EditManager.Instance.NowTime + 0.001f < HitTime + HoldTime)
            {
                delta += Time.unscaledDeltaTime;

                if (delta > 0.1f)
                {
                    delta = 0f;
                    HitEffectManager.Instance.ShowHitEffect(JudgeResult.PERFECT, CurrentData.LengthX, FatherTrack.transform.TransformPoint(0f, 0f, 0f), FatherTrack.transform.rotation);
                }
            }
            else
            {
                delta = 0f;
            }
        }

        public void JudgeNote(JudgeResult accuracy)
        {
            if (IsJudged || CurrentData.IsFake) return;

            IsJudged = true;

            HitEffectManager.Instance.ShowHitEffect(accuracy, CurrentData.LengthX, FatherTrack.transform.TransformPoint(0f, 0f, 0f), FatherTrack.transform.rotation);

            if (accuracy != JudgeResult.Bad && accuracy != JudgeResult.Miss)
            {
                HitSoundManager.Instance.Play((int)CurrentData.NoteType);
            }
        }

        protected virtual void UpdateNoteOthers() { }

        protected float Length2Transform(float length, float noteSpHeight = 0.25f)
        {
            return Mathf.Max(2f * length - noteSpHeight, noteSpHeight);
        }

        void UpdateNoteTransform()
        {
            NoteInstance.transform.localPosition = new Vector3(0f, CalculateNoteHeight() * (int)CurrentData.FallDirection, 0f);
            NoteInstance.transform.localScale = new Vector3(1.25f, 1.25f, 1f);

            if (CurrentData.NoteType is NoteType.Hold)
            {
                NoteInstance.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
            }
            else
            {
                NoteInstance.transform.localScale = new Vector3(1.25f, 1.25f + FatherTrack.CurrentData.Compensate3D * Mathf.Clamp01(CalculateNoteHeight() / (CurrentData.FallDirection is FallingDirection.Up ? Length2Transform(FatherTrack.UpLength) : Length2Transform(FatherTrack.DownLength))), 1f);
            }

            /*
            if (CurrentData.NoteType is NoteType.Hold)
            {
                //如果是Hold
                NoteInstance.transform.localScale = new Vector3(CurrentData.LengthX * LevelManager.Instance.NoteScale, 1f, 1f);
            }
            else
            {
                //不是Hold的厚度补正
                NoteInstance.transform.localScale = LevelManager.Instance.NoteScale * new Vector3(CurrentData.LengthX, 1f + Mathf.Clamp01(NoteHeight / (CurrentData.FallDirection is FallingDirection.Up ? Length2Transform(FatherTrack.UpLength) : Length2Transform(FatherTrack.DownLength))), 1f);
            }*/
        }


        protected float CalculateNoteHeight()
        {
            if (ProgressManager.Instance.NowTime >= HitTime && CurrentData.NoteType is NoteType.Hold)
            {
                return 0f;
            }
            else
            {
                return StartDistance - FatherTrack.NowDistance;
            }
        }
    }
}