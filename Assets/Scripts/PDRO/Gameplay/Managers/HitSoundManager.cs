using System.Collections.Generic;
using PDRO.Global;
using PDRO.Utils.Singleton;
using UnityEngine;

namespace PDRO.Gameplay.Managers
{
    public class HitSoundManager : MonoSingleton<HitSoundManager>
    {
        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private int[] hitSoundsLength;

        private static Dictionary<int, AudioSource[]> _unityAudios;
        private static Dictionary<int, int> _audioIndexes;
        private static float _hitSoundVolume = 1f;
        private static int _hitSoundMode = 1;

        protected override void OnAwake()
        {
            InitUnityAudio();
        }

        public static void Init()
        {
            var settings = GlobalSettings.CurrentSettings;
            _hitSoundVolume = settings.HitSoundVolume * settings.MainVolume;

            if (settings.DSPBufferSize > 1024) _hitSoundVolume = 0f;

            for (var i = 0; i < Instance.hitSounds.Length; i++)
            {
                for (var j = 0; j < Instance.hitSoundsLength[i]; j++)
                {
                    _unityAudios[i][j].volume = _hitSoundVolume;
                }
            }

            Debug.Log($"初始化Unity音频系统，模式{_hitSoundMode}，音量{_hitSoundVolume}");

        }

        private void InitUnityAudio()
        {
            _unityAudios = new Dictionary<int, AudioSource[]>();
            _audioIndexes = new Dictionary<int, int>();

            for (var i = 0; i < hitSounds.Length; i++)
            {
                _unityAudios.Add(i, new AudioSource[hitSoundsLength[i]]);
                _audioIndexes.Add(i, 0);
                for (var j = 0; j < hitSoundsLength[i]; j++)
                {
                    var obj = new GameObject("Unity Audio - HitSound");
                    obj.transform.SetParent(transform);
                    var comp = obj.AddComponent<AudioSource>();
                    comp.loop = false;
                    comp.playOnAwake = false;
                    comp.clip = hitSounds[i];
                    _unityAudios[i][j] = comp;
                }
            }
        }

        public void Play(int soundIndex)
        {
            //没有在播放则返回
            if (!EditManager.Instance.EditAudioSource.isPlaying) return;

            PlayByUnityAudio(soundIndex);
        }

        private void PlayByUnityAudio(int soundIndex)
        {
            if (_hitSoundVolume <= 0.01f) return;

            var index = _audioIndexes[soundIndex] + 1;
            if (index >= hitSoundsLength[soundIndex]) index = 0;
            var source = _unityAudios[soundIndex][index];
            _audioIndexes[soundIndex] = index;

            if (_hitSoundMode == 0)
            {
                source.PlayOneShot(source.clip);
            }
            else
            {
                if (source.isPlaying)
                {
                    if (source.time > 0.03f)
                    {
                        source.Stop();
                    }
                    else
                    {
                        return;
                    }
                }

                if (_hitSoundMode == 1)
                {
                    source.PlayScheduled(float.MinValue);
                }
                else
                {
                    source.Play();
                }
            }
        }
    }
}