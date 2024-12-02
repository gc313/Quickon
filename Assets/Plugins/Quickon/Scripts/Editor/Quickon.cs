using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Quickon.Core;
using Unity.Cinemachine;
using Unity.VisualScripting;
using System.Collections.Generic;

namespace Quickon.Editor
{
    public class Quickon : EditorWindow
    {
        // 字段
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        [SerializeField] private VisualTreeAsset cameraPanel;
        [SerializeField] private VisualTreeAsset postProcessingPanel;
        [SerializeField] private DataSourceSO dataSourceSO;

        private CaptureHelper captureHelper;
        private CaptureObjSO captureObject;
        private CinemachineCamera camera;
        private CinemachineOrbitalFollow orbitalFollow;
        private bool isCameraOrthographic;

        private VisualElement root, mainElement, cameraPanelElement, postPanelElement, perspectiveField, orthographicField;
        private ObjectField cameraField;
        private DropdownField cameraProjection;
        private FloatField orthographicSizeField, fieldOfViewField, horizontalAxisField, verticalAxisField;
        private Slider orthographicSlider, fieldOfViewSlider, horizontalAxisSlider, verticalAxisSlider;
        private Toggle previewToggle, transparentToggle;
        private Button previousPreviewButton, nextPreviewButton, autoCaptureButton, manualCaptureButton;

        // 构造函数
        public Quickon()
        {
        }

        // 方法
        [MenuItem("Window/Quickon")]
        public static void ShowExample()
        {
            // 显示Quickon窗口
            Quickon wnd = GetWindow<Quickon>();
            wnd.titleContent = new GUIContent("Quickon");
        }

        private void OnEnable()
        {
            // 启用窗口时初始化
            captureHelper = new CaptureHelper();
            EditorApplication.update += UpdateCameraFromDataSource;
            EditorApplication.update += UpdateCameraPanel;
        }

        private void OnDisable()
        {
            // 禁用窗口时清理
            EditorApplication.update -= UpdateCameraFromDataSource;
            EditorApplication.update -= UpdateCameraPanel;
        }

        public void CreateGUI()
        {
            // 创建GUI界面
            root = rootVisualElement;

            DrawCameraField();
            DrawImageSizeField();
            DrawPostPrecessingPanel();
            DrawCaptureObjectList();
        }

        private void DrawPostPrecessingPanel()
        {
            // 绘制后期处理面板
            postPanelElement = postProcessingPanel.Instantiate();
            root.Add(postPanelElement);

            transparentToggle = postPanelElement.Q<Toggle>("RemoveBackground_Toggle");
            transparentToggle.RegisterCallback<ChangeEvent<bool>>(e => { Config.IsTransparent = e.newValue; });
        }

        private void DrawImageSizeField()
        {
            // 绘制图像大小字段
            mainElement = visualTreeAsset.Instantiate();
            root.Add(mainElement);

            previewToggle = mainElement.Q<Toggle>("Preview_Toggle");
            previousPreviewButton = mainElement.Q<Button>("Previous_Preview_Button");
            nextPreviewButton = mainElement.Q<Button>("Next_Preview_Button");
            autoCaptureButton = mainElement.Q<Button>("AutoCapture_Button");
            manualCaptureButton = mainElement.Q<Button>("ManualCapture_Button");

            // 监听控件值变化事件
            previewToggle.RegisterCallback<ChangeEvent<bool>>(e =>
            {
                Config.IsPreview = e.newValue;
                captureHelper.ToggleObjectPreview(captureObject.CaptureObjects, Config.IsPreview);
            });
            previousPreviewButton.clicked += () => { captureHelper.PreviousObjectPreview(captureObject.CaptureObjects, Config.IsPreview); };
            nextPreviewButton.clicked += () => { captureHelper.NextObjectPreview(captureObject.CaptureObjects, Config.IsPreview); };
            autoCaptureButton.clicked += () => { captureHelper.PlaceObjectsAndCapture(captureObject.CaptureObjects); };
            manualCaptureButton.clicked += () => { captureHelper.CaptureImage(false); };
        }

        private void DrawCameraField()
        {
            // 绘制相机字段
            cameraField = new ObjectField("Camera")
            {
                value = SetCaptureCamera()
            };
            root.Add(cameraField);

            // 监听相机字段变化
            cameraField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e => { RegisterCameraEvent(e.newValue); });

            DrawCameraPanel();
        }

        private UnityEngine.Object SetCaptureCamera()
        {
            // 设置捕获相机
            var newCamera = GameObject.FindWithTag("CaptureCamera");
            if (newCamera == null)
            {
                newCamera = new GameObject("CaptureCamera");
                newCamera.tag = "CaptureCamera";
                newCamera.AddComponent<CinemachineCamera>();
                newCamera.AddComponent<CinemachineOrbitalFollow>();
                newCamera.AddComponent<CinemachineRotationComposer>();
                newCamera.AddComponent<CinemachineFreeLookModifier>();
                newCamera.AddComponent<CinemachineInputAxisController>();
            }
            RegisterCameraEvent(newCamera);
            return newCamera;
        }

        private void RegisterCameraEvent(UnityEngine.Object cameraObj)
        {
            // 注册相机事件
            camera = cameraObj.GetComponent<CinemachineCamera>();
            orbitalFollow = cameraObj.GetComponent<CinemachineOrbitalFollow>();
            // 相机每次变化时重新注册更新事件
            EditorApplication.update -= UpdateCameraFromDataSource;
            EditorApplication.update += UpdateCameraFromDataSource;
            captureHelper.InitializeHelper(cameraObj, dataSourceSO);
        }

        private void UpdateUIFromCamera()
        {
            // 从相机更新UI
            orthographicSizeField = cameraPanelElement.Q<FloatField>("OrthographicSize_Value");
            orthographicSlider = cameraPanelElement.Q<Slider>("OrthographicSize_Slider");

            fieldOfViewField = cameraPanelElement.Q<FloatField>("FieldOfView_Value");
            fieldOfViewSlider = cameraPanelElement.Q<Slider>("FieldOfView_Slider");

            horizontalAxisField = cameraPanelElement.Q<FloatField>("HorizontalAxis_Value");
            horizontalAxisSlider = cameraPanelElement.Q<Slider>("HorizontalAxis_Slider");

            verticalAxisField = cameraPanelElement.Q<FloatField>("VerticalAxis_Value");
            verticalAxisSlider = cameraPanelElement.Q<Slider>("VerticalAxis_Slider");

            orthographicSizeField.SetValueWithoutNotify(camera.Lens.OrthographicSize);
            orthographicSlider.SetValueWithoutNotify(camera.Lens.OrthographicSize);
            fieldOfViewField.SetValueWithoutNotify(camera.Lens.FieldOfView);
            fieldOfViewSlider.SetValueWithoutNotify(camera.Lens.FieldOfView);
            horizontalAxisField.SetValueWithoutNotify(orbitalFollow.HorizontalAxis.Value);
            horizontalAxisSlider.SetValueWithoutNotify(orbitalFollow.HorizontalAxis.Value);
            verticalAxisField.SetValueWithoutNotify(orbitalFollow.VerticalAxis.Value);
            verticalAxisSlider.SetValueWithoutNotify(orbitalFollow.VerticalAxis.Value);
        }

        private void DrawCameraPanel()
        {
            // 绘制相机面板
            isCameraOrthographic = Camera.main.orthographic;
            cameraPanelElement = cameraPanel.Instantiate();
            cameraProjection = cameraPanelElement.Q<DropdownField>("CameraProjection");
            cameraProjection.RegisterCallback<ChangeEvent<string>>(e => { CameraProjectionChoice(e.newValue); });
            perspectiveField = cameraPanelElement.Q<VisualElement>("Perspective");
            orthographicField = cameraPanelElement.Q<VisualElement>("Orthographic");
            CameraProjectionChoice();
            root.Add(cameraPanelElement);

            UpdateUIFromCamera();
        }

        private string CameraProjectionChoice(string arg)
        {
            // 处理相机投影选择
            switch (arg)
            {
                case Config.Orthographic:
                    Camera.main.orthographic = true;
                    return Config.Orthographic;
                case Config.Perspective:
                    Camera.main.orthographic = false;
                    return Config.Perspective;
            }
            return "";
        }

        private void UpdateCameraPanel()
        {
            // 更新相机面板
            if (isCameraOrthographic == Camera.main.orthographic) return;
            isCameraOrthographic = Camera.main.orthographic;
            CameraProjectionChoice();
            cameraPanelElement.MarkDirtyRepaint();
        }

        private void CameraProjectionChoice()
        {
            // 根据相机投影选择显示不同的UI元素
            if (camera == null || orbitalFollow == null) return;
            if (isCameraOrthographic)
            {
                cameraProjection.SetValueWithoutNotify(Config.Orthographic);
                orthographicField.style.display = DisplayStyle.Flex;
                perspectiveField.style.display = DisplayStyle.None;
            }
            else
            {
                cameraProjection.SetValueWithoutNotify(Config.Perspective);
                perspectiveField.style.display = DisplayStyle.Flex;
                orthographicField.style.display = DisplayStyle.None;
            }
        }

        private void UpdateCameraFromDataSource()
        {
            // 从数据源更新相机设置
            if (camera == null || orbitalFollow == null) return;
            camera.Lens.OrthographicSize = dataSourceSO.OrthographicSize;
            camera.Lens.FieldOfView = dataSourceSO.FieldOfView;
            orbitalFollow.HorizontalAxis.Value = dataSourceSO.HorizontalAxis;
            orbitalFollow.VerticalAxis.Value = dataSourceSO.VerticalAxis;
            EditorUtility.SetDirty(camera);
            EditorUtility.SetDirty(orbitalFollow);
        }

        private void DrawCaptureObjectList()
        {
            // 绘制捕获对象列表
            captureObject = CreateInstance<CaptureObjSO>();
            var inspectorElement = new InspectorElement();
            var serializedObject = new SerializedObject(captureObject);
            inspectorElement.Bind(serializedObject);
            root.Add(inspectorElement);
        }
    }
}