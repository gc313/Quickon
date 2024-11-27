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
        [SerializeField] private VisualTreeAsset visualTreeAsset = default; // 可视化树资产
        private CaptureHelper captureHelper; // 截图辅助类实例
        private IntegerField imageWidthField; // 图片宽度输入框
        private IntegerField imageHeightField; // 图片高度输入框
        private Toggle previewToggle; // 预览开关
        private Button previousPreviewButton; // 上一个预览按钮
        private Button nextPreviewButton; // 下一个预览按钮
        private Button captureButton; // 截图按钮

        [MenuItem("Window/Quickon")]
        public static void ShowExample()
        {
            Quickon wnd = GetWindow<Quickon>();
            wnd.titleContent = new GUIContent("Quickon");
        }

        private void OnEnable()
        {
            captureHelper = new CaptureHelper(); // 初始化截图辅助类
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement; // 获取根视觉元素
            VisualElement element = visualTreeAsset.Instantiate(); // 实例化可视化树资产

            CaptureObjSO captureObject = CreateInstance<CaptureObjSO>(); // 创建捕获对象实例
            var inspectorElement = new InspectorElement(); // 创建检查器元素
            var serializedObject = new SerializedObject(captureObject); // 序列化捕获对象
            inspectorElement.Bind(serializedObject); // 绑定序列化对象到检查器元素

            var box = new Box(); // 创建一个盒子容器
            root.Add(box); // 将盒子容器添加到根视觉元素
            box.Add(inspectorElement); // 将检查器元素添加到盒子容器
            box.Add(element); // 将实例化的可视化树资产添加到盒子容器

            imageWidthField = element.Q<IntegerField>("Image_Weight"); // 获取图片宽度输入框
            imageHeightField = element.Q<IntegerField>("Image_Height"); // 获取图片高度输入框

            previewToggle = element.Q<Toggle>("Preview_Toggle"); // 获取预览开关
            previousPreviewButton = element.Q<Button>("Previous_Preview_Button"); // 获取上一个预览按钮
            nextPreviewButton = element.Q<Button>("Next_Preview_Button"); // 获取下一个预览按钮
            captureButton = element.Q<Button>("Capture_Button"); // 获取截图按钮

            // 监听图片宽度变化
            imageWidthField.RegisterCallback<ChangeEvent<int>>(e =>
            {
                Config.ImgWeight = e.newValue; // 更新配置中的图片宽度
            });

            // 监听图片高度变化
            imageHeightField.RegisterCallback<ChangeEvent<int>>(e =>
            {
                Config.ImgHeight = e.newValue; // 更新配置中的图片高度
            });

            // 监听预览开关变化
            previewToggle.RegisterCallback<ChangeEvent<bool>>(e =>
            {
                Config.IsPreview = e.newValue; // 更新配置中的预览状态
                captureHelper.ToggleObjectPreview(captureObject.CaptureObjects, Config.IsPreview); // 切换预览状态
            });

            // 监听上一个、下一个按钮点击事件
            previousPreviewButton.clicked += () => { captureHelper.PreviousObjectPreview(captureObject.CaptureObjects, Config.IsPreview); }; // 切换到上一个预览对象
            nextPreviewButton.clicked += () => { captureHelper.NextObjectPreview(captureObject.CaptureObjects, Config.IsPreview); }; // 切换到下一个预览对象

            // 监听截图按钮点击事件
            captureButton.clicked += () => { captureHelper.PlaceObjectsAndCapture(captureObject.CaptureObjects); }; // 捕获当前对象并截图
        }
    }
}