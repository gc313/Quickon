using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Quickon.Core;
using System.Collections.Generic;
namespace Quickon.Editor
{
    public class Quickon : EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
        private CaptureHelper captureHelper;
        public IntegerField imgWeight;
        public IntegerField imgHeight;
        public Button captureButton;


        [MenuItem("Window/Quickon")]
        public static void ShowExample()
        {
            Quickon wnd = GetWindow<Quickon>();
            wnd.titleContent = new GUIContent("Quickon");
        }

        private void OnEnable()
        {
            captureHelper = new CaptureHelper();
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            VisualElement element = m_VisualTreeAsset.Instantiate();

            CaptureObjSO captureObject = CreateInstance<CaptureObjSO>();
            var inspectorElement = new InspectorElement();
            var serializedObject = new SerializedObject(captureObject);
            inspectorElement.Bind(serializedObject);

            var box = new Box();
            root.Add(box);
            box.Add(inspectorElement);
            box.Add(element);

            imgWeight = element.Q<IntegerField>("Image_Weight");
            imgHeight = element.Q<IntegerField>("Image_Height");
            captureButton = element.Q<Button>("Capture_Image");

            // 监听图片宽度变化
            imgWeight.RegisterCallback<ChangeEvent<int>>(e =>
            {
                Config.ImgWeight = e.newValue;
            });

            // 监听图片高度变化
            imgHeight.RegisterCallback<ChangeEvent<int>>(e =>
            {
                Config.ImgHeight = e.newValue;
            });

            // 监听截图按钮点击事件
            captureButton.clicked += () => { captureHelper.PlaceObjects(captureObject.CaptureObjects); };
        }

    }
}