using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Utils.Singleton;

namespace PDRO.Gameplay.Managers
{
    public class ProgressManager : MonoSingleton<ProgressManager>
    {
        public float NowTime => EditManager.Instance.NowTime;
    }
}