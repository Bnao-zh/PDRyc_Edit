using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PDRO.Data
{
    [Serializable]
    public class MusicData
    {
        public int ID;
        public float BPM;
        public string MusicName;
        public string ComposerName;

        public MusicData(int id, float bpm, string musicName, string composerName)
        {
            ID = id;
            BPM = bpm;
            MusicName = musicName;
            ComposerName = composerName;
        }
    }
}