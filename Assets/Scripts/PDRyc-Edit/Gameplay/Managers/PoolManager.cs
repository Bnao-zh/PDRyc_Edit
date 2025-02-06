using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Utils.Singleton;
using PDRO.Utils.Pool;
using PDRO.Gameplay.Controls;

namespace PDRO.Gameplay.Managers
{
    public class PoolManager : MonoSingleton<PoolManager>
    {
        protected override void OnAwake()
        {
            InitPool();
        }

        public void InitPool()
        {
            for (var i = 0; i < LevelTransform.childCount; i++)
            {
                Destroy(LevelTransform.GetChild(i).gameObject);
            }

            OnNotePoolAwake();
            OnHoldPoolAwake();
        }

        [SerializeField] private Transform LevelTransform;

        // Note池子
        [SerializeField] private NoteMonoBehaviour NoteMono;
        private ObjectPoolQueue<NoteMonoBehaviour> NotePool;
        void OnNotePoolAwake()
        {
            NotePool = new ObjectPoolQueue<NoteMonoBehaviour>(NoteMono, 64, LevelTransform);
        }
        public NoteMonoBehaviour GetNote() => NotePool.PrepareObject();
        public void ReturnNote(NoteMonoBehaviour mono)
        {
            NotePool.ReturnObject(mono);
        }

        // Hold池子
        [SerializeField] private HoldMonoBehaviour HoldMono;
        private ObjectPoolQueue<HoldMonoBehaviour> HoldPool;
        void OnHoldPoolAwake()
        {
            HoldPool = new ObjectPoolQueue<HoldMonoBehaviour>(HoldMono, 16, LevelTransform);
        }
        public HoldMonoBehaviour GetHoldNote() => HoldPool.PrepareObject();
        public void ReturnHoldNote(HoldMonoBehaviour mono)
        {
            HoldPool.ReturnObject(mono);
        }

    }
}