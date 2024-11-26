using UnityEngine;
using System.IO;
using System.Collections.Generic;
namespace Quickon.Core
{
    public class CaptureHelper
    {
        private Camera m_Camera;
        private int m_CaptureCount;

        /// <summary>
        /// 放置对象并拍照
        /// </summary>
        /// <param name="objects"></param>
        public void PlaceObjects(List<CaptureObject> objects)
        {
            foreach (var item in objects)
            {
                // 实例化预制体并添加到当前场景中
                GameObject instance = Object.Instantiate(item.gameObject);
                if (instance != null)
                {
                    instance.transform.position = item.gameObject.transform.position; // 可以设置位置
                    instance.transform.rotation = item.gameObject.transform.rotation; // 可以设置旋转
                    instance.transform.localScale = item.gameObject.transform.localScale; // 可以设置缩放
                }
                else
                {
                    Debug.LogError("Failed to instantiate object: " + item.gameObject.name);
                }

                CapturePicture(item.gameObject.name);
                Object.DestroyImmediate(instance);
            }
            Debug.Log("Capture Done!");
        }

        /// <summary>
        /// 拍照
        /// </summary>
        /// <param name="name"></param>
        public void CapturePicture(string name)
        {
            if (m_Camera == null)
            {
                m_Camera = Camera.main; // 尝试在编辑器模式下获取主摄像机
                if (m_Camera == null)
                {
                    return;
                }
            }

            int width = Config.ImgWeight;
            int height = Config.ImgHeight;

            // 创建一个RenderTexture
            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);

            // 设置摄像机的目标纹理
            m_Camera.targetTexture = rt;

            // 渲染摄像机
            m_Camera.Render();
            RenderTexture.active = rt;

            // 创建一个Texture2D对象来读取像素
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // 确保读取区域不超过RenderTexture的边界
            Rect readRect = new Rect(0, 0, width, height);

            texture.ReadPixels(readRect, 0, 0);
            texture.Apply();

            // 将Texture2D对象转换为PNG格式的字节数组
            byte[] bytes = texture.EncodeToPNG();

            // 保存到文件
            m_CaptureCount++;
            string path = $"E:/{name}{m_CaptureCount}.png";
            File.WriteAllBytes(path, bytes);

            // 清理
            m_Camera.targetTexture = null;
            RenderTexture.active = null;

            // 使用DestroyImmediate来销毁对象
            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(rt);
        }
    }
}
