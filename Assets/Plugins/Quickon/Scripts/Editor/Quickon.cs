using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Quickon.Core;
namespace Quickon.Editor
{
    public class Quickon : EditorWindow
    {
        private CaptureHelper captureHelper;
        public TextField picWeight;
        public TextField picHeight;


        [MenuItem("Window/Quickon")]
        public static void ShowExample()
        {
            Quickon wnd = GetWindow<Quickon>();
            wnd.titleContent = new GUIContent("Quickon");
        }

        private void OnEnable()
        {
            // 初始化 captureManager
            captureHelper = new CaptureHelper();
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

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

            Button captureButton = new Button(captureHelper.CapturePicture);
            captureButton.text = "Capture Picture";

            root.Add(picWeight);
            root.Add(picHeight);
            root.Add(captureButton);

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

    }
}