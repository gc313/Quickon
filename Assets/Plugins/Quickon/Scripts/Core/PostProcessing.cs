using System;
using UnityEngine;

namespace Quickon.Core
{
    internal class PostProcessing
    {
        RenderTexture renderTexture;
        Camera mainCamera;
        Rect readRect;
        int width, height;

        internal void Stack(RenderTexture renderTexture, Camera mainCamera, Rect rect, int width, int height)
        {
            this.renderTexture = renderTexture;
            this.mainCamera = mainCamera;
            this.width = width;
            this.height = height;
            readRect = rect;

            if (QuickonConfig.IsTransparent) RemoveBackground();
        }

        private void RemoveBackground()
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;

            string materialPath = "Materials/NoneBackground";
            Material transparent = Resources.Load<Material>(materialPath);

            // 创建一个临时的RenderTexture来存储原始图像
            RenderTexture tempRenderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            tempRenderTexture.Create();

            // 将当前渲染结果复制到临时RenderTexture
            Graphics.Blit(renderTexture, tempRenderTexture);

            // 读取临时RenderTexture的像素数据
            Texture2D originTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            RenderTexture.active = tempRenderTexture;
            originTexture.ReadPixels(readRect, 0, 0);
            originTexture.Apply();
            RenderTexture.active = null;

            // 设置材质的纹理参数
            transparent.SetTexture("_MainTex", originTexture);

            // 使用材质进行Blit操作，将处理后的图像写回原始RenderTexture
            Graphics.Blit(tempRenderTexture, renderTexture, transparent);

            // 清理临时资源
            UnityEngine.Object.DestroyImmediate(originTexture);
            UnityEngine.Object.DestroyImmediate(tempRenderTexture);
        }
    }
}
