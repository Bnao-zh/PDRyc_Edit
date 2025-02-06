using UnityEngine;

namespace PDRO.GameUI.Global.Components
{
    public class AutoRotate : MonoBehaviour
    {
        [SerializeField] private float trigger, duration;
        [SerializeField] private Vector3 from, to;
        [SerializeField] private AnimationCurve curve;
        [SerializeField] private bool isLocal, isLoop, isSmooth;

        private float _time = 0f;

        private void Awake()
        {
            _time = 0f;
        }

        private void Update()
        {
            _time += isSmooth ? Time.smoothDeltaTime : Time.deltaTime;
            if (_time < trigger || _time > duration + trigger && !isLoop)
            {
                return;
            }
            var progress = curve.Evaluate(_time % duration / duration);
            var trans = transform;
            if (isLocal)
            {
                trans.localEulerAngles = Vector3.Lerp(from, to, progress);
            }
            else
            {
                trans.eulerAngles = Vector3.Lerp(from, to, progress);
            }
        }
    }
}
