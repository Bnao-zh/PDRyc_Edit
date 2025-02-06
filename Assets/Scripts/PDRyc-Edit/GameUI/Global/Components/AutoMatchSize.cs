using UnityEngine;
using UnityEngine.UI;

namespace PDRO.GameUI.Global.Components
{
    [ExecuteInEditMode, RequireComponent(typeof(CanvasScaler))]
    public class AutoMatchSize : MonoBehaviour
    {
        [SerializeField] private float max, min;

        private CanvasScaler _scaler;

        private void Awake()
        {
            _scaler = GetComponent<CanvasScaler>();
            Match();
        }

        private void Update() => Match();

        private void Match()
        {
            _scaler.matchWidthOrHeight = (float)Screen.width / Screen.height > 16f / 9 ? max : min;
        }
    }
}
