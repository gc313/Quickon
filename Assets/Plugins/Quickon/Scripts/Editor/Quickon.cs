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
        private VisualTreeAsset visualTreeAsset;
        private VisualTreeAsset cameraPanel;
        private VisualTreeAsset postProcessingPanel;
        private DataSourceSO dataSourceSO;

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
            dataSourceSO = Resources.Load<DataSourceSO>("DataSource/DataSource");
            visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/Quickon");
            cameraPanel = Resources.Load<VisualTreeAsset>("UI/CameraPanel");
            postProcessingPanel = Resources.Load<VisualTreeAsset>("UI/PostProcessingPanel");

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

        /// <summary>
        /// 创建UI的主方法
        /// </summary>
        internal void CreateGUI()
        {
            // 使用折叠面板包装每个区域
            root.Add(WithFoldout("Camera Settings", DrawCameraField));
            root.Add(WithFoldout("Image Size Settings", DrawImageSizeField));
            root.Add(WithFoldout("Post-Processing Settings", DrawPostPrecessingPanel));
            root.Add(WithFoldout("Capture Object List", DrawCaptureObjectList));
        }

        #region Camera Settings
        private VisualElement DrawCameraField()
        {
            var container = new VisualElement();

            cameraField = new ObjectField("Camera")
            {
                value = SetCaptureCamera()
            };
            container.Add(cameraField);

            cameraField.RegisterCallback<ChangeEvent<GameObject>>(e => { RegisterCameraEvent(e.newValue); });

            container.Add(DrawCameraPanel());
            return container;
        }

        private VisualElement DrawCameraPanel()
        {
            // 创建一个容器来承载相机面板
            var container = new VisualElement();

            isCameraOrthographic = Camera.main.orthographic;
            cameraPanelElement = cameraPanel.Instantiate();
            cameraProjection = cameraPanelElement.Q<DropdownField>("CameraProjection");
            cameraProjection.RegisterCallback<ChangeEvent<string>>(e => { CameraProjectionChoice(e.newValue); });
            perspectiveField = cameraPanelElement.Q<VisualElement>("Perspective");
            orthographicField = cameraPanelElement.Q<VisualElement>("Orthographic");
            CameraProjectionChoice();

            // 将面板添加到当前容器中
            container.Add(cameraPanelElement);

            UpdateUIFromCamera();

            return container;
        }

        /// <summary>
        /// 设置捕获相机
        /// </summary>
        private GameObject SetCaptureCamera()
        {
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
                newCamera.AddComponent<OnDrawGizmo>();
            }
            RegisterCameraEvent(newCamera);
            return newCamera;
        }

        /// <summary>
        /// 注册相机事件
        /// </summary>
        private void RegisterCameraEvent(GameObject cameraObj)
        {
            cinemachineCamera = cameraObj.GetComponent<CinemachineCamera>();
            orbitalFollow = cameraObj.GetComponent<CinemachineOrbitalFollow>();
            rotationComposer = cameraObj.GetComponent<CinemachineRotationComposer>();
            // 相机每次变化时重新注册更新事件
            EditorApplication.update -= UpdateCameraFromDataSource;
            EditorApplication.update += UpdateCameraFromDataSource;
            captureHelper.InitializeHelper(cameraObj, dataSourceSO);
        }

        /// <summary>
        /// 更新相机UI
        /// </summary>
        private void UpdateUIFromCamera()
        {
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

        /// <summary>
        /// 处理相机投影选择
        /// </summary>
        private string CameraProjectionChoice(string arg)
        {
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

        /// <summary>
        /// 更新相机面板
        /// </summary>
        private void UpdateCameraPanel()
        {
            isCameraOrthographic = Camera.main.orthographic;
            CameraProjectionChoice();
            cameraPanelElement.MarkDirtyRepaint();
        }

        /// <summary>
        /// 根据相机投影选择显示不同的UI元素
        /// </summary>
        private void CameraProjectionChoice()
        {
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

        /// <summary>
        /// 从数据源更新相机设置
        /// </summary>
        private void UpdateCameraFromDataSource()
        {
            if (cinemachineCamera == null || orbitalFollow == null || dataSourceSO == null) return;
            cinemachineCamera.Lens.OrthographicSize = dataSourceSO.OrthographicSize;
            cinemachineCamera.Lens.FieldOfView = dataSourceSO.FieldOfView;
            orbitalFollow.HorizontalAxis.Value = dataSourceSO.HorizontalAxis;
            orbitalFollow.VerticalAxis.Value = dataSourceSO.VerticalAxis;
            rotationComposer.TargetOffset = dataSourceSO.TargetOffset;
            EditorUtility.SetDirty(cinemachineCamera);
            EditorUtility.SetDirty(orbitalFollow);
            EditorUtility.SetDirty(rotationComposer);
        }
        #endregion

        #region Image Size Settings
        private VisualElement DrawImageSizeField()
        {
            var container = new VisualElement();

            mainElement = visualTreeAsset.Instantiate();
            container.Add(mainElement);

            previewToggle = mainElement.Q<Toggle>("Preview_Toggle");
            previousPreviewButton = mainElement.Q<Button>("Previous_Preview_Button");
            nextPreviewButton = mainElement.Q<Button>("Next_Preview_Button");
            autoCaptureButton = mainElement.Q<Button>("AutoCapture_Button");
            manualCaptureButton = mainElement.Q<Button>("ManualCapture_Button");

            previewToggle.RegisterCallback<ChangeEvent<bool>>(e =>
            {
                Core.QuickonConfig.IsPreview = e.newValue;
                captureHelper.ToggleObjectPreview(captureObject.CaptureObjects, Core.QuickonConfig.IsPreview);
            });
            previousPreviewButton.clicked += () => { captureHelper.PreviousObjectPreview(captureObject.CaptureObjects, Core.QuickonConfig.IsPreview); };
            nextPreviewButton.clicked += () => { captureHelper.NextObjectPreview(captureObject.CaptureObjects, Core.QuickonConfig.IsPreview); };
            autoCaptureButton.clicked += () => { captureHelper.PlaceObjectsAndCapture(captureObject.CaptureObjects); };
            manualCaptureButton.clicked += () => { captureHelper.CaptureImage(false); };

            return container;
        }
        #endregion

        #region Post-Processing Settings
        private VisualElement DrawPostPrecessingPanel()
        {
            var container = new VisualElement();

            postPanelElement = postProcessingPanel.Instantiate();
            container.Add(postPanelElement);

            transparentToggle = postPanelElement.Q<Toggle>("RemoveBackground_Toggle");
            transparentToggle.RegisterCallback<ChangeEvent<bool>>(e => { Core.QuickonConfig.IsTransparent = e.newValue; });

            return container;
        }
        #endregion

        #region Capture Object List
        private VisualElement DrawCaptureObjectList()
        {
            var container = new VisualElement();

            captureObject = CreateInstance<CaptureObjSO>();
            var inspectorElement = new InspectorElement();
            var serializedObject = new SerializedObject(captureObject);
            inspectorElement.Bind(serializedObject);
            container.Add(inspectorElement);

            return container;
        }
        #endregion

        /// <summary>
        /// 创建折叠面板
        /// </summary>
        /// <param name="title"></param>
        /// <param name="contentGenerator"></param>
        /// <returns></returns>
        private VisualElement WithFoldout(string title, Func<VisualElement> contentGenerator)
        {
            var foldout = new Foldout { text = title, value = true };
            foldout.Add(contentGenerator());
            return foldout;
        }
    }


}