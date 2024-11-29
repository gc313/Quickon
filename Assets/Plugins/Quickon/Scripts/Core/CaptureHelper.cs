using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.UI;


namespace Quickon.Core
{
    public class CaptureHelper
    {
        [SerializeField] private Material material;
        private Camera mainCamera;
        private CinemachineOrbitalFollow orbitalFollow;
        private CinemachineCamera camera;
        private int captureCount;
        private int currentPreviewIndex;
        private GameObject previewObject;

        /// <summary>
        /// 初始化摄像机
        /// </summary>
        public void InitializeCamera(UnityEngine.Object cameraObj)
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main; // 尝试在编辑器模式下获取主摄像机
                if (mainCamera == null)
                {
                    Debug.LogError("Main camera not found!");
                }
            }
            camera = cameraObj.GetComponent<CinemachineCamera>();
            orbitalFollow = cameraObj.GetComponent<CinemachineOrbitalFollow>();

            Debug.Log("Camera Initialized!");
        }

        /// <summary>
        /// 放置对象并拍照
        /// </summary>
        /// <param name="captureObjects">要拍摄的对象列表</param>
        public void PlaceObjectsAndCapture(List<CaptureObject> captureObjects)
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
                    InstantiateObjectToScene(item.gameObject);
                    CaptureImage(true);
                    DestroyObjectFromScene(previewObject);
                }
            }
            Debug.Log("Capture Done!");
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
                        InstantiateObjectToScene(captureObjects[index].gameObject);
                        return;
                    }
                }
            }
            else
            {
                DestroyObjectFromScene(previewObject);
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

            currentPreviewIndex = Math.Max(0, currentPreviewIndex - 1);
            if (currentPreviewIndex >= 0)
            {
                DestroyObjectFromScene(previewObject);
                InstantiateObjectToScene(captureObjects[currentPreviewIndex].gameObject);
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

            currentPreviewIndex = Math.Min(currentPreviewIndex + 1, captureObjects.Count - 1);
            if (currentPreviewIndex < captureObjects.Count)
            {
                DestroyObjectFromScene(previewObject);
                InstantiateObjectToScene(captureObjects[currentPreviewIndex].gameObject);
            }
        }

        /// <summary>
        /// 在场景中实例化对象
        /// </summary>
        /// <param name="obj">要实例化的对象</param>
        public void InstantiateObjectToScene(GameObject obj)
        {
            if (obj == null) return;
            previewObject = UnityEngine.Object.Instantiate(obj);
            CameraLookAtTarget(previewObject.transform);
        }

        /// <summary>
        /// 从场景中立即销毁对象
        /// </summary>
        /// <param name="obj">要销毁的对象</param>
        public void DestroyObjectFromScene(GameObject obj)
        {
            if (obj == null) return;
            UnityEngine.Object.DestroyImmediate(obj);
            previewObject = null;
        }

        public void CameraLookAtTarget(Transform targetTransform)
        {
            if (targetTransform == null) return;
            camera.Follow = targetTransform;
        }

        /// <summary>
        /// 拍照
        /// </summary>
        /// <param name="name">图片名称</param>
        public void CaptureImage(bool isAuto)
        {
            if (mainCamera == null || camera == null || orbitalFollow == null) return;
            string imageName;
            if (isAuto)
            {
                if (previewObject == null) return;
                imageName = previewObject.name;
            }
            else
            {
                imageName = "PreviewObject";
            }

            int width = Config.ImgWeight;
            int height = Config.ImgHeight;

            // 创建一个RenderTexture
            RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);

            // 设置摄像机的目标纹理
            mainCamera.targetTexture = renderTexture;

            // 透明背景时使用
            // mainCamera.clearFlags = CameraClearFlags.SolidColor;
            // mainCamera.backgroundColor = Color.black;
            string materialPath = "Shaders/TransparentBackground";
            Material transparent = Resources.Load<Material>(materialPath);
            if (transparent == null)
            {
                Debug.LogError($"Could not find the material at path: {materialPath}. Please check the path and name.");
            }
            else
            {
                Debug.Log($"Loaded material: {transparent.name}");
            }

            // 渲染摄像机
            mainCamera.Render();
            RenderTexture.active = renderTexture;

            // 创建一个Texture2D对象来读取像素
            Texture2D originTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // 确保读取区域不超过RenderTexture的边界
            Rect readRect = new Rect(0, 0, width, height);

            originTexture.ReadPixels(readRect, 0, 0);
            originTexture.Apply();

            transparent.SetTexture("_MainTex", originTexture);
            Graphics.Blit(null, renderTexture, transparent);
            Texture2D finalTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            finalTexture.ReadPixels(readRect, 0, 0);
            finalTexture.Apply();

            // 将Texture2D对象转换为PNG格式的字节数组
            byte[] bytes0 = finalTexture.EncodeToPNG();
            if (bytes0.Length == 0)
            {
                Debug.LogError("Failed to encode Texture2D to PNG.");
            }
            else
            {
                Debug.Log("Texture2D encoded to PNG successfully.");
            }

            // 保存到文件
            captureCount++;
            string path = $"E:/{imageName}{captureCount}.png";
            File.WriteAllBytes(path, bytes0);

            // 清理
            mainCamera.targetTexture = null;
            RenderTexture.active = null;

            // 使用DestroyImmediate来销毁对象
            UnityEngine.Object.DestroyImmediate(originTexture);
            UnityEngine.Object.DestroyImmediate(finalTexture);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }
    }
}