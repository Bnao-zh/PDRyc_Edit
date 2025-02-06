using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Data;
using PDRO.Utils.Singleton;

namespace PDRO.Gameplay.Managers
{
    public class SkinManager : MonoSingleton<SkinManager>
    {
        [SerializeField] private Sprite Tap, TapHL;
        [SerializeField] private Sprite Drag, DragHL;
        [SerializeField] private Sprite Flick, FlickHL;
        [SerializeField] private Sprite HoldHead, HoldHeadHL, HoldBody, HoldBodyHL, HoldEnd;

        public Sprite GetBaseSprite(NoteType type, bool isHL)
        {
            switch (type)
            {
                case NoteType.Tap:
                    return isHL ? TapHL : Tap;
                case NoteType.Drag:
                    return isHL ? DragHL : Drag;
                case NoteType.Flick:
                    return isHL ? FlickHL : Flick;
                case NoteType.Hold:
                    return isHL ? HoldHeadHL : HoldHead;
            }

            throw new System.Exception("这是什么类型我不造啊");
        }

        public Sprite GetHoldBody(bool isHL)
        {
            return isHL ? HoldBodyHL : HoldBody;
        }

        public Sprite GetHoldEnd(bool isHL)
        {
            return HoldEnd;
        }
    }
}