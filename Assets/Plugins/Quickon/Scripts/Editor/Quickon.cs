using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class Quickon : EditorWindow
{
    private ObjectField cameraSelector;
    public Camera m_Camera;
    public TextField picWeight;
    public TextField picHeight;


    [MenuItem("Window/Quickon")]
    public static void ShowExample()
    {
        Quickon wnd = GetWindow<Quickon>();
        wnd.titleContent = new GUIContent("Quickon");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        cameraSelector = new ObjectField
        {
            objectType = typeof(Camera),
            label = "Select Camera"
        };

        picWeight = new TextField
        {
            label = "Picture Width",
            value = $"{Config.PicWeight}"
        };

        picHeight = new TextField
        {
            label = "Picture Height",
            value = $"{Config.PicHeight}"
        };



        Button captureButton = new Button(CapturePicture);
        captureButton.text = "Capture Picture";

        root.Add(cameraSelector);
        root.Add(picWeight);
        root.Add(picHeight);
        root.Add(captureButton);

        // 初始化摄像机
        if (cameraSelector.value != null)
        {
            m_Camera = cameraSelector.value as Camera;
        }

        // 监听摄像机选择变化
        cameraSelector.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
        {
            m_Camera = e.newValue as Camera;
        });

        // 监听图片宽度变化
        picWeight.RegisterCallback<ChangeEvent<string>>(e =>
        {
            if (int.TryParse(e.newValue, out int newPicWeight))
            {
                Config.PicWeight = newPicWeight;
            }
        });

        // 监听图片高度变化
        picHeight.RegisterCallback<ChangeEvent<string>>(e =>
        {
            if (int.TryParse(e.newValue, out int newPicHeight))
            {
                Config.PicHeight = newPicHeight;
            }
        });
    }

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
        DestroyImmediate(texture);
        DestroyImmediate(rt);

        Debug.Log("Capture Success!");
    }
}