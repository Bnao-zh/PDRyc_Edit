using System;
using System.Threading.Tasks;
using PDRO.Gameplay.Controls;
using PDRO.Utils.Pool;
using PDRO.Utils.Singleton;
using UnityEngine;

namespace PDRO.Gameplay.Managers
{
    public class HitEffectManager : MonoSingleton<HitEffectManager>
    {
        [SerializeField] private HitEffectControl[] hitEffects;
        [SerializeField] private int[] hitEffectsLength;
        private ObjectPoolQueue<HitEffectControl>[] _hitEffectPools;

        public const int MaxLengthOfPool = 768;

        public float HitFxSize = 0.7f;

        protected override void OnAwake()
        {
            InitHitEffects();
        }

        private void InitHitEffects()
        {
            _hitEffectPools = new ObjectPoolQueue<HitEffectControl>[hitEffects.Length];
            for (int i = 0; i < hitEffects.Length; i++)
            {
                _hitEffectPools[i] = new ObjectPoolQueue<HitEffectControl>(hitEffects[i], hitEffectsLength[i], transform);
            }
        }

        public void ShowHitEffect(JudgeResult accuracy, float notewidth, Vector3 pos, Quaternion rot)
        {
            //没有在播放则返回
            if (!EditManager.Instance.EditAudioSource.isPlaying) return;

            ObjectPoolQueue<HitEffectControl> hitEffectPool;

            if (accuracy == JudgeResult.PERFECT)
            {
                hitEffectPool = _hitEffectPools[0];
            }
            else
            {
                hitEffectPool = _hitEffectPools[(int)accuracy - 1]; // 取出对象池
            }

            if (hitEffectPool.Length >= MaxLengthOfPool && hitEffectPool.Count <= 0) return;// 对象池取完了就别取了喵

            var player = hitEffectPool.PrepareObject(); // 从对象池里取一个Fx

            player.Play(hitEffectPool);

            var playerTransform = player.transform;
            playerTransform.position = pos;
            playerTransform.rotation = rot;
            playerTransform.localScale = HitFxSize * notewidth * Vector3.one;
        }
    }
}

