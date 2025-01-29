using System;
using System.Threading.Tasks;
using PDRO.Gameplay.Managers;
using UnityEngine;
using PDRO.Utils.Pool;

namespace PDRO.Gameplay.Controls
{
    public class HitEffectControl : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _thisParticle;
        private ObjectPoolQueue<HitEffectControl> _thisParticlePool;

        public void Play(ObjectPoolQueue<HitEffectControl> pool)
        {
            _thisParticlePool = pool;
            _thisParticle.Play();
        }

        //试试回调回收
        private void OnParticleSystemStopped()
        {
            _thisParticlePool.ReturnObject(this);
        }
    }
}
