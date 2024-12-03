using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Quickon.Core;
using Unity.Cinemachine;

namespace Quickon.Editor
{
    internal class Quickon : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        [SerializeField] private VisualTreeAsset cameraPanel;
        [SerializeField] private VisualTreeAsset postProcessingPanel;
        [SerializeField] private DataSourceSO dataSourceSO;

        private InstallRequiredPackages installRequired;
        private CaptureHelper captureHelper;
        private CaptureObjSO captureObject;
        private CinemachineCamera cinemachineCamera;
        private CinemachineOrbitalFollow orbitalFollow;
        private CinemachineRotationComposer rotationComposer;
        private bool isCameraOrthographic;

        private VisualElement root, mainElement, cameraPanelElement, postPanelElement, perspectiveField, orthographicField;
        private ObjectField cameraField;
        private DropdownField cameraProjection;
        private FloatField orthographicSizeField, fieldOfViewField, horizontalAxisField, verticalAxisField;
        private Slider orthographicSlider, fieldOfViewSlider, horizontalAxisSlider, verticalAxisSlider;
        private Toggle previewToggle, transparentToggle;
        private Button previousPreviewButton, nextPreviewButton, autoCaptureButton, manualCaptureButton;
        private Vector3Field targetOffsetField;

        private VisualElement loadingPanel;
        private Label loadingLabel;

        [MenuItem("Tools/Quickon")]
        internal static void ShowWindow()
        {
            Quickon wnd = GetWindow<Quickon>();
            wnd.titleContent = new GUIContent("Quickon");
        }

        private async void OnEnable()
        {
            root = rootVisualElement;

            captureHelper = new CaptureHelper();
            installRequired = new InstallRequiredPackages();
            CreateLoadingPanel();

            await installRequired.InstallPackages();
            HideLoadingPanel();

            EditorApplication.update += UpdateCameraFromDataSource;
            EditorApplication.update += UpdateCameraPanel;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateCameraFromDataSource;
            EditorApplication.update -= UpdateCameraPanel;
        }

        private void CreateLoadingPanel()
        {
            loadingPanel = new VisualElement();
            loadingLabel = new Label("Checking dependencies...");
            loadingPanel.Add(loadingLabel);
            root.Add(loadingPanel);
        }

        private void HideLoadingPanel()
        {
            loadingPanel.RemoveFromHierarchy();
        }

        internal void CreateGUI()
        {
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
            transparentToggle.RegisterCallback<ChangeEvent<bool>>(e => { Core.QuickonConfig.IsTransparent = e.newValue; });
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
                Core.QuickonConfig.IsPreview = e.newValue;
                captureHelper.ToggleObjectPreview(captureObject.CaptureObjects, Core.QuickonConfig.IsPreview);
            });
            previousPreviewButton.clicked += () => { captureHelper.PreviousObjectPreview(captureObject.CaptureObjects, Core.QuickonConfig.IsPreview); };
            nextPreviewButton.clicked += () => { captureHelper.NextObjectPreview(captureObject.CaptureObjects, Core.QuickonConfig.IsPreview); };
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
            cameraField.RegisterCallback<ChangeEvent<GameObject>>(e => { RegisterCameraEvent(e.newValue); });

            DrawCameraPanel();
        }

        private GameObject SetCaptureCamera()
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

        private void RegisterCameraEvent(GameObject cameraObj)
        {
            // 注册相机事件
            cinemachineCamera = cameraObj.GetComponent<CinemachineCamera>();
            orbitalFollow = cameraObj.GetComponent<CinemachineOrbitalFollow>();
            rotationComposer = cameraObj.GetComponent<CinemachineRotationComposer>();
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

            targetOffsetField = cameraPanelElement.Q<Vector3Field>("TargetOffset_Vector3");

            orthographicSizeField.SetValueWithoutNotify(cinemachineCamera.Lens.OrthographicSize);
            orthographicSlider.SetValueWithoutNotify(cinemachineCamera.Lens.OrthographicSize);
            fieldOfViewField.SetValueWithoutNotify(cinemachineCamera.Lens.FieldOfView);
            fieldOfViewSlider.SetValueWithoutNotify(cinemachineCamera.Lens.FieldOfView);
            horizontalAxisField.SetValueWithoutNotify(orbitalFollow.HorizontalAxis.Value);
            horizontalAxisSlider.SetValueWithoutNotify(orbitalFollow.HorizontalAxis.Value);
            verticalAxisField.SetValueWithoutNotify(orbitalFollow.VerticalAxis.Value);
            verticalAxisSlider.SetValueWithoutNotify(orbitalFollow.VerticalAxis.Value);
            targetOffsetField.SetValueWithoutNotify(rotationComposer.TargetOffset);
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
                case Core.QuickonConfig.Orthographic:
                    Camera.main.orthographic = true;
                    return Core.QuickonConfig.Orthographic;
                case Core.QuickonConfig.Perspective:
                    Camera.main.orthographic = false;
                    return Core.QuickonConfig.Perspective;
            }
            return "";
        }

        private void UpdateCameraPanel()
        {
            // 更新相机面板
            isCameraOrthographic = Camera.main.orthographic;
            CameraProjectionChoice();
            cameraPanelElement.MarkDirtyRepaint();
        }

        private void CameraProjectionChoice()
        {
            // 根据相机投影选择显示不同的UI元素
            if (cinemachineCamera == null || orbitalFollow == null) return;
            if (isCameraOrthographic)
            {
                cameraProjection.SetValueWithoutNotify(Core.QuickonConfig.Orthographic);
                orthographicField.style.display = DisplayStyle.Flex;
                perspectiveField.style.display = DisplayStyle.None;
            }
            else
            {
                cameraProjection.SetValueWithoutNotify(Core.QuickonConfig.Perspective);
                perspectiveField.style.display = DisplayStyle.Flex;
                orthographicField.style.display = DisplayStyle.None;
            }
        }

        private void UpdateCameraFromDataSource()
        {
            // 从数据源更新相机设置
            if (cinemachineCamera == null || orbitalFollow == null) return;
            cinemachineCamera.Lens.OrthographicSize = dataSourceSO.OrthographicSize;
            cinemachineCamera.Lens.FieldOfView = dataSourceSO.FieldOfView;
            orbitalFollow.HorizontalAxis.Value = dataSourceSO.HorizontalAxis;
            orbitalFollow.VerticalAxis.Value = dataSourceSO.VerticalAxis;
            rotationComposer.TargetOffset = dataSourceSO.TargetOffset;
            EditorUtility.SetDirty(cinemachineCamera);
            EditorUtility.SetDirty(orbitalFollow);
            EditorUtility.SetDirty(rotationComposer);
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