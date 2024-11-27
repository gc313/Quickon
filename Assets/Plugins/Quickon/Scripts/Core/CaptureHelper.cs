using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using Unity.Cinemachine;


namespace Quickon.Core
{
    public class CaptureHelper
    {
        private Camera mainCamera;
        private CinemachineOrbitalFollow orbitalFollow;
        private CinemachineCamera camera;
        private int captureCount;
        private int currentPreviewIndex;
        private GameObject previewObject;

        /// <summary>
        /// 初始化摄像机
        /// </summary>
        public void InitializeCamera(CinemachineCamera camera)
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main; // 尝试在编辑器模式下获取主摄像机
                if (mainCamera == null)
                {
                    Debug.LogError("Main camera not found!");
                }
            }
            // orbitalFollow = mainCamera.GetComponent<CinemachineOrbitalFollow>();
            // if (orbitalFollow == null)
            // {
            //     Debug.LogError("CinemachineOrbitalFollow component not found!");
            // }
            // camera = mainCamera.GetComponent<CinemachineCamera>();
            // if (camera == null)
            // {
            //     Debug.LogError("CinemachineCamera component not found!");
            // }
            this.camera = camera;

        }

        /// <summary>
        /// 放置对象并拍照
        /// </summary>
        /// <param name="captureObjects">要拍摄的对象列表</param>
        public void PlaceObjectsAndCapture(CinemachineCamera camera, List<CaptureObject> captureObjects)
        {
            InitializeCamera(camera);
            foreach (var item in captureObjects)
            {
                if (item.gameObject == null)
                {
                    continue;
                }
                else
                {
                    InstantiateObjectToScene(item.gameObject);
                    CaptureImage(item.gameObject.name);
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
        public void ToggleObjectPreview(CinemachineCamera camera, List<CaptureObject> captureObjects, bool isPreview)
        {
            if (captureObjects == null || captureObjects.Count == 0) return;

            if (isPreview)
            {
                InitializeCamera(camera);
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
            previewObject.transform.position = obj.transform.position; // 设置位置
            previewObject.transform.rotation = obj.transform.rotation; // 设置旋转
            previewObject.transform.localScale = obj.transform.localScale; // 设置缩放

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
            camera.Follow = targetTransform;
        }

        /// <summary>
        /// 拍照
        /// </summary>
        /// <param name="name">图片名称</param>
        public void CaptureImage(string name)
        {
            int width = Config.ImgWeight;
            int height = Config.ImgHeight;

            // 创建一个RenderTexture
            RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);

            // 设置摄像机的目标纹理
            mainCamera.targetTexture = renderTexture;

            // 渲染摄像机
            mainCamera.Render();
            RenderTexture.active = renderTexture;

            // 创建一个Texture2D对象来读取像素
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // 确保读取区域不超过RenderTexture的边界
            Rect readRect = new Rect(0, 0, width, height);

            texture.ReadPixels(readRect, 0, 0);
            texture.Apply();

            // 将Texture2D对象转换为PNG格式的字节数组
            byte[] bytes = texture.EncodeToPNG();

            // 保存到文件
            captureCount++;
            string path = $"E:/{name}{captureCount}.png";
            File.WriteAllBytes(path, bytes);

            // 清理
            mainCamera.targetTexture = null;
            RenderTexture.active = null;

            // 使用DestroyImmediate来销毁对象
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }
    }
}