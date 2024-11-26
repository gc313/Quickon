using UnityEngine;
using System.IO;
namespace Quickon.Core
{
    public class CaptureHelper
    {
        private Camera m_Camera;

        public void CapturePicture()
        {
            if (m_Camera == null)
            {
                m_Camera = Camera.main; // 尝试在编辑器模式下获取主摄像机
                if (m_Camera == null)
                {
                    return;
                }
            }

            int width = Config.PicWeight;
            int height = Config.PicHeight;

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
            string path = "E:/Quickon.png";
            File.WriteAllBytes(path, bytes);

            // 清理
            m_Camera.targetTexture = null;
            RenderTexture.active = null;

            // 使用DestroyImmediate来销毁对象
            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(rt);

            Debug.Log("Capture Success!");
        }
    }
}
