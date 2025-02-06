using UnityEngine;

namespace PDRO.Data.ScriptableObject
{
    [CreateAssetMenu(menuName = "PDRO/Music Data", fileName = "New Music Data")]
    public class MusicDataObject : UnityEngine.ScriptableObject
    {
        [SerializeField] private int ID;
        [SerializeField] private float BPM;
        [SerializeField] private string MusicName;
        [SerializeField] private string ComposerName;
        public MusicData CurrentData => new(ID, BPM, MusicName, ComposerName);
    }
}