using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using System.Threading.Tasks;

namespace Quickon.Core
{
    public class CaptureHelper
    {
        private Camera mainCamera;
        private CinemachineOrbitalFollow orbitalFollow;
        private CinemachineCamera camera;
        private int captureCount;
        private int currentPreviewIndex;
        private CaptureObject previewObject;
        private PostProcessing postProcessing;
        private DataSourceSO dataSourceSO;

        /// <summary>
        /// 初始化摄像机
        /// </summary>
        public void InitializeHelper(UnityEngine.Object cameraObj, DataSourceSO dataSourceSO)
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("Main camera not found!");
                }
            }
            this.dataSourceSO = dataSourceSO;
            camera = cameraObj.GetComponent<CinemachineCamera>();
            orbitalFollow = cameraObj.GetComponent<CinemachineOrbitalFollow>();

            postProcessing = new PostProcessing();
            previewObject = new CaptureObject();

            // Debug.Log("Initialized!");
        }

        /// <summary>
        /// 放置对象并拍照
        /// </summary>
        /// <param name="captureObjects">要拍摄的对象列表</param>
        public async void PlaceObjectsAndCapture(List<CaptureObject> captureObjects)
        {
            if (captureObjects == null || captureObjects.Count == 0) return;
            captureCount = 0;
            foreach (var item in captureObjects)
            {
                if (item.gameObject == null)
                {
                    continue;
                }
                else
                {
                    InstantiateObjectToScene(item);
                    await Task.Delay(500); // 等待500毫秒
                    CaptureImage(true);
                    DestroyObjectFromScene(previewObject.gameObject);
                }
            }
            Debug.Log("Capture Done!");
        }

        private void WaitForEndOfFrame()
        {
            // 这里使用了一个简单的等待一帧的方法
            // 如果需要更复杂的逻辑，可以考虑使用其他方法
            while (!Application.isEditor && !Application.isPlaying)
            {
                // 确保在编辑器模式下也能正常工作
                return;
            }
            while (Time.frameCount == Time.frameCount - 1)
            {
                // 等待当前帧结束
            }
        }

        /// <summary>
        /// 切换预览对象
        /// </summary>
        /// <param name="captureObjects">要预览的对象列表</param>
        /// <param name="isPreview">是否开启预览</param>
        public void ToggleObjectPreview(List<CaptureObject> captureObjects, bool isPreview)
        {
            if (captureObjects == null || captureObjects.Count == 0) return;
            if (isPreview)
            {
                for (int index = 0; index < captureObjects.Count; index++)
                {
                    if (captureObjects[index].gameObject == null)
                    {
                        continue;
                    }
                    else
                    {
                        currentPreviewIndex = index;
                        InstantiateObjectToScene(captureObjects[index]);
                        return;
                    }
                }
            }
            else
            {
                SaveObjectCameraSettings(captureObjects[currentPreviewIndex]);
                DestroyObjectFromScene(previewObject.gameObject);
            }
        }

        /// <summary>
        /// 上一个预览对象
        /// </summary>
        /// <param name="captureObjects">要预览的对象列表</param>
        /// <param name="isPreview">是否开启预览</param>
        public void PreviousObjectPreview(List<CaptureObject> captureObjects, bool isPreview)
        {
            if (!isPreview || captureObjects == null || captureObjects.Count == 0) return;

            SaveObjectCameraSettings(captureObjects[currentPreviewIndex]);
            currentPreviewIndex = Math.Max(0, currentPreviewIndex - 1);
            if (currentPreviewIndex >= 0)
            {
                DestroyObjectFromScene(previewObject.gameObject);
                InstantiateObjectToScene(captureObjects[currentPreviewIndex]);
            }
        }

        /// <summary>
        /// 下一个预览对象
        /// </summary>
        /// <param name="captureObjects">要预览的对象列表</param>
        /// <param name="isPreview">是否开启预览</param>
        public void NextObjectPreview(List<CaptureObject> captureObjects, bool isPreview)
        {
            if (!isPreview || captureObjects == null || captureObjects.Count == 0) return;

            SaveObjectCameraSettings(captureObjects[currentPreviewIndex]);
            currentPreviewIndex = Math.Min(currentPreviewIndex + 1, captureObjects.Count - 1);
            if (currentPreviewIndex < captureObjects.Count)
            {
                DestroyObjectFromScene(previewObject.gameObject);
                InstantiateObjectToScene(captureObjects[currentPreviewIndex]);
            }
        }

        /// <summary>
        /// 在场景中实例化对象
        /// </summary>
        /// <param name="obj">要实例化的对象</param>
        public void InstantiateObjectToScene(CaptureObject obj)
        {
            if (obj == null) return;
            if (obj.projectionType != ProjectionType.None)
            {
                LoadObjectCameraSettings(obj);
            }
            else
            {
                SaveObjectCameraSettings(obj);
            }
            previewObject.gameObject = UnityEngine.Object.Instantiate(obj.gameObject);
            previewObject.projectionType = obj.projectionType;
            previewObject.horizontalAxis = obj.horizontalAxis;
            previewObject.verticalAxis = obj.verticalAxis;
            previewObject.fieldOfView = obj.fieldOfView;
            previewObject.orthographicSize = obj.orthographicSize;
            CameraLookAtTarget(previewObject.gameObject.transform);
        }

        /// <summary>
        /// 从场景中立即销毁对象
        /// </summary>
        /// <param name="obj">要销毁的对象</param>
        public void DestroyObjectFromScene(GameObject obj)
        {
            if (obj == null) return;
            UnityEngine.Object.DestroyImmediate(obj);
        }

        public void CameraLookAtTarget(Transform targetTransform)
        {
            if (targetTransform == null) return;
            camera.Follow = targetTransform;
        }

        /// <summary>
        /// 拍照
        /// </summary>
        public void CaptureImage(bool isAuto)
        {
            if (mainCamera == null || camera == null || orbitalFollow == null) return;
            string imageName;
            if (isAuto)
            {
                if (previewObject == null) return;
                imageName = previewObject.gameObject.name;
            }
            else
            {
                imageName = "PreviewObject";
            }

            int width = dataSourceSO.ImgWeight;
            int height = dataSourceSO.ImgHeight;

            // 创建一个RenderTexture
            RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            mainCamera.targetTexture = renderTexture;
            mainCamera.Render();
            RenderTexture.active = renderTexture;
            // 以RenderTexture的边界创建读取区域
            Rect readRect = new Rect(0, 0, width, height);

            // 应用后期处理
            postProcessing.Stack(renderTexture, mainCamera, readRect, width, height);

            // 读取纹理
            Texture2D finalTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            finalTexture.ReadPixels(readRect, 0, 0);
            finalTexture.Apply();

            // 保存到文件
            SaveImage(imageName, finalTexture);

            // 清理
            mainCamera.targetTexture = null;
            RenderTexture.active = null;

            // 使用DestroyImmediate来销毁对象
            UnityEngine.Object.DestroyImmediate(finalTexture);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }

        private void SaveImage(string imageName, Texture2D finalTexture)
        {
            captureCount++;
            string path = $"{Config.ImgOutputPath}{imageName}{captureCount}.png";
            byte[] bytes = finalTexture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }

        private void SaveObjectCameraSettings(CaptureObject obj)
        {
            obj.projectionType = mainCamera.orthographic ? ProjectionType.Orthographic : ProjectionType.Perspective;
            switch (obj.projectionType)
            {
                case ProjectionType.Perspective:
                    {
                        obj.fieldOfView = dataSourceSO.FieldOfView;
                        break;
                    }
                case ProjectionType.Orthographic:
                    {
                        obj.orthographicSize = dataSourceSO.OrthographicSize;
                        break;
                    }
            }
            obj.horizontalAxis = dataSourceSO.HorizontalAxis;
            obj.verticalAxis = dataSourceSO.VerticalAxis;
        }

        private void LoadObjectCameraSettings(CaptureObject obj)
        {
            switch (obj.projectionType)
            {
                case ProjectionType.Perspective:
                    {
                        mainCamera.orthographic = false;
                        dataSourceSO.FieldOfView = obj.fieldOfView;
                        break;
                    }
                case ProjectionType.Orthographic:
                    {
                        mainCamera.orthographic = true;
                        dataSourceSO.OrthographicSize = obj.orthographicSize;
                        break;
                    }
            }
            dataSourceSO.HorizontalAxis = obj.horizontalAxis;
            dataSourceSO.VerticalAxis = obj.verticalAxis;
        }
    }
}