using UnityEngine;

namespace PDRO.GameUI.Global.Components
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class AutoChangeScale : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 originalScale;
        [SerializeField] private bool withWideScreen, withPadScreen = true, inverse = false, inverse2 = false;

        private void Awake() => Match();

        private void Update() => Match();

        private void Match()
        {
            if (!target) target = transform;
            var delta = (float)Screen.width / Screen.height / (16f / 9);
            if (!withWideScreen) delta = Mathf.Min(delta, 1f);
            if (!withPadScreen) delta = Mathf.Max(delta, 1f);
            if (inverse) delta = 1f / delta;
            if (inverse && inverse2 && delta < 1f) delta = 1f / delta;
            target.localScale = originalScale * delta;
        }
    }
}