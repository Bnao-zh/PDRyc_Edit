using UnityEngine;

namespace PDRO.GameUI.Global.Components
{
    [ExecuteInEditMode]
    public class AutoBackGround : MonoBehaviour
    {
        [SerializeField] private float factor;


        private void Awake()
        {
            Match();
        }

        private void Update() => Match();

        private void Match()
        {
            float size = (float)Screen.width / Screen.height / (16f / 9) * factor;
            if (size < factor) size = factor;
            transform.localScale = new Vector3(size, size, 1f);
        }
    }
}