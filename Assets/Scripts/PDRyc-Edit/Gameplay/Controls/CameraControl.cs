using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDRO.Data;
using PDRO.Utils.Singleton;
using PDRO.Utils;
using PDRO.Gameplay.Managers;

namespace PDRO.Gameplay.Controls
{
    public class CameraControl : MonoSingleton<CameraControl>
    {
        public Transform cameraTransform;
        public CameraData CurrentData;

        public Camera GameCamera;

        public float TargetFov;

        public float AspectRatioDelta;
        public float ScreenAspectRatio;
        void Start()
        {
            // 初始化cameraTransform为当前Transform
            cameraTransform = transform;
        }

        public void Init(CameraData data)
        {
            CurrentData = data;

            var index = CurrentData.FatherTrackIndex;
            if (index != -1)
            {
                this.transform.SetParent(EditManager.Instance.Tracks[index].transform);
            }
            else
            {
                this.transform.SetParent(EditManager.Instance.LevelTransform);
            }

            UpdateAspectRatio();
        }

        void UpdateAspectRatio()
        {
            ScreenAspectRatio = 1f * Screen.width / Screen.height;
            AspectRatioDelta = Mathf.Clamp01(ScreenAspectRatio / EditManager.Instance.EditingChart.MetaData.TargetAspectRatio);
        }

        public void Run()
        {
            this.transform.parent = null;
        }

        public void OnUpdate()
        {
            UpdateAspectRatio();

            UpdateEvents();
            UpdateFov();
        }

        void UpdateEvents()
        {
            var nowTime = ProgressManager.Instance.NowTime;

            float posX = 0f, posY = 0f, posZ = 0f,
                    rotX = 0f, rotY = 0f, rotZ = 0f,
                    fov = 0f;

            for (var i = 0; i < CurrentData.Events.Count; i++)
            {
                var nowLayer = CurrentData.Events[i];

                posX += GameplayUtility.GetValueFromTrackEvent(nowTime, nowLayer.MoveXEvents);
                posY += GameplayUtility.GetValueFromTrackEvent(nowTime, nowLayer.MoveYEvents);
                posZ += GameplayUtility.GetValueFromTrackEvent(nowTime, nowLayer.MoveZEvents);

                rotX += GameplayUtility.GetValueFromTrackEvent(nowTime, nowLayer.RotateXEvents);
                rotY += GameplayUtility.GetValueFromTrackEvent(nowTime, nowLayer.RotateYEvents);
                rotZ += GameplayUtility.GetValueFromTrackEvent(nowTime, nowLayer.RotateZEvents);

                fov += GameplayUtility.GetValueFromTrackEvent(nowTime, nowLayer.FovEvents);
            }

            this.transform.localPosition = new Vector3(posX, posY, posZ);
            this.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);
            TargetFov = fov;
        }

        void UpdateFov()
        {
            if (AspectRatioDelta < 1f)
            {
                var horizontalFov = Camera.VerticalToHorizontalFieldOfView(TargetFov, EditManager.Instance.EditingChart.MetaData.TargetAspectRatio);
                TargetFov = Camera.HorizontalToVerticalFieldOfView(horizontalFov, ScreenAspectRatio);
            }

            GameCamera.fieldOfView = TargetFov;
        }
    }
}