using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Quickon.Core;
using Unity.Cinemachine;
using Unity.VisualScripting;
using Unity.Properties;

namespace Quickon.Editor
{
    public class Quickon : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset = default; // 可视化树资产
        [SerializeField] private VisualTreeAsset cameraPanel = default;
        private VisualElement root, element, cameraPanelElement;

        private CaptureObjSO captureObject;
        private CinemachineCamera camera;
        private CinemachineOrbitalFollow orbitalFollow;
        private ObjectField cameraField;

        private FloatField apparentSizeField, horizontalAxisField, verticalAxisField;
        private Slider apparentSizeSlider, horizontalAxisSlider, verticalAxisSlider;

        private CaptureHelper captureHelper;
        private IntegerField imageWidthField, imageHeightField;
        private Toggle previewToggle;
        private Button previousPreviewButton, nextPreviewButton, autoCaptureButton, manualCaptureButton;

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
            root = rootVisualElement; // 获取根视觉元素

            DrawCameraField();
            DrawImageSizeField();
            DrawCaptureObjectList();
        }

        private void DrawImageSizeField()
        {
            element = visualTreeAsset.Instantiate();
            root.Add(element);

            imageWidthField = element.Q<IntegerField>("Image_Weight");
            imageHeightField = element.Q<IntegerField>("Image_Height");

            previewToggle = element.Q<Toggle>("Preview_Toggle");
            previousPreviewButton = element.Q<Button>("Previous_Preview_Button");
            nextPreviewButton = element.Q<Button>("Next_Preview_Button");
            autoCaptureButton = element.Q<Button>("AutoCapture_Button");
            manualCaptureButton = element.Q<Button>("ManualCapture_Button");

            // 监听图片宽度变化
            imageWidthField.RegisterCallback<ChangeEvent<int>>(e => { Config.ImgWeight = e.newValue; });
            // 监听图片高度变化
            imageHeightField.RegisterCallback<ChangeEvent<int>>(e => { Config.ImgHeight = e.newValue; });

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
            autoCaptureButton.clicked += () => { captureHelper.PlaceObjectsAndCapture(captureObject.CaptureObjects); };
            manualCaptureButton.clicked += () => { captureHelper.CaptureImage(); };
        }

        private void DrawCameraField()
        {
            cameraField = new ObjectField("Camera");
            root.Add(cameraField);

            cameraPanelElement = cameraPanel.Instantiate();
            root.Add(cameraPanelElement);

            apparentSizeField = cameraPanelElement.Q<FloatField>("ApparentSize_Value");
            horizontalAxisField = cameraPanelElement.Q<FloatField>("HorizontalAxis_Value");
            verticalAxisField = cameraPanelElement.Q<FloatField>("VerticalAxis_Value");
            apparentSizeSlider = cameraPanelElement.Q<Slider>("ApparentSize_Slider");
            horizontalAxisSlider = cameraPanelElement.Q<Slider>("HorizontalAxis_Slider");
            verticalAxisSlider = cameraPanelElement.Q<Slider>("VerticalAxis_Slider");

            // 监听相机字段变化
            cameraField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                camera = e.newValue.GetComponent<CinemachineCamera>();
                // cameraPanelElement.dataSource = camera.Lens;

                // apparentSizeField.SetBinding("value", new DataBinding
                // {
                //     dataSourcePath = new PropertyPath(nameof(camera.Lens.OrthographicSize)),
                //     bindingMode = BindingMode.ToSource
                // });

                // apparentSizeSlider.SetBinding("value", new DataBinding
                // {
                //     dataSourcePath = new PropertyPath(nameof(camera.Lens.OrthographicSize)),
                //     bindingMode = BindingMode.ToSource
                // });

                // // 添加事件监听器以更新数据源
                // apparentSizeField.RegisterCallback<ChangeEvent<float>>(e =>
                // {
                //     camera.Lens.OrthographicSize = e.newValue;
                // });

                // apparentSizeSlider.RegisterCallback<ChangeEvent<float>>(e =>
                // {
                //     camera.Lens.OrthographicSize = e.newValue;
                // });

                orbitalFollow = e.newValue.GetComponent<CinemachineOrbitalFollow>();

                // horizontalAxisField.SetBinding("value", new DataBinding
                // {
                //     dataSource = orbitalFollow.HorizontalAxis,
                //     dataSourcePath = new PropertyPath(nameof(orbitalFollow.HorizontalAxis.Value)),
                //     bindingMode = BindingMode.ToSource
                // });
                // horizontalAxisSlider.SetBinding("value", new DataBinding
                // {
                //     dataSource = orbitalFollow.HorizontalAxis,
                //     dataSourcePath = new PropertyPath(nameof(orbitalFollow.HorizontalAxis.Value)),
                //     bindingMode = BindingMode.ToSource
                // });

                // verticalAxisField.value = orbitalFollow.VerticalAxis.Value;
                // verticalAxisSlider.value = orbitalFollow.VerticalAxis.Value;

                captureHelper.InitializeCamera(camera);
            });
        }

        private void DrawCaptureObjectList()
        {
            captureObject = CreateInstance<CaptureObjSO>(); // 创建捕获对象实例
            var inspectorElement = new InspectorElement(); // 创建检查器元素
            var serializedObject = new SerializedObject(captureObject); // 序列化捕获对象
            inspectorElement.Bind(serializedObject); // 绑定序列化对象到检查器元素
            root.Add(inspectorElement);
        }
    }
}