using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Quickon.Core;
using Unity.Cinemachine;
using Unity.VisualScripting;

namespace Quickon.Editor
{
    public class Quickon : EditorWindow
    {
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
        private IntegerField imageWidthField, imageHeightField;
        private FloatField orthographicSizeField, fieldOfViewField, horizontalAxisField, verticalAxisField;
        private Slider orthographicSlider, fieldOfViewSlider, horizontalAxisSlider, verticalAxisSlider;
        private Toggle previewToggle, transparentToggle;
        private Button previousPreviewButton, nextPreviewButton, autoCaptureButton, manualCaptureButton;

        [MenuItem("Window/Quickon")]
        public static void ShowExample()
        {
            Quickon wnd = GetWindow<Quickon>();
            wnd.titleContent = new GUIContent("Quickon");
        }

        private void OnEnable()
        {
            captureHelper = new CaptureHelper();
            // 监听 dataSourceSO 的变化
            EditorApplication.update += UpdateCameraFromDataSource;
            EditorApplication.update += UpdateCameraPanel;
        }

        private void OnDisable()
        {
            // 清理监听
            EditorApplication.update -= UpdateCameraFromDataSource;
            EditorApplication.update -= UpdateCameraPanel;
        }

        public void CreateGUI()
        {
            root = rootVisualElement; // 获取根视觉元素

            DrawCameraField();
            DrawImageSizeField();
            DrawPostPrecessingPanel();
            DrawCaptureObjectList();
        }

        private void DrawPostPrecessingPanel()
        {
            postPanelElement = postProcessingPanel.Instantiate();
            root.Add(postPanelElement);

            transparentToggle = postPanelElement.Q<Toggle>("RemoveBackground_Toggle");
            transparentToggle.RegisterCallback<ChangeEvent<bool>>(e => { Config.IsTransparent = e.newValue; });
        }

        private void DrawImageSizeField()
        {
            mainElement = visualTreeAsset.Instantiate();
            root.Add(mainElement);

            imageWidthField = mainElement.Q<IntegerField>("Image_Weight");
            imageHeightField = mainElement.Q<IntegerField>("Image_Height");

            previewToggle = mainElement.Q<Toggle>("Preview_Toggle");
            previousPreviewButton = mainElement.Q<Button>("Previous_Preview_Button");
            nextPreviewButton = mainElement.Q<Button>("Next_Preview_Button");
            autoCaptureButton = mainElement.Q<Button>("AutoCapture_Button");
            manualCaptureButton = mainElement.Q<Button>("ManualCapture_Button");

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
            manualCaptureButton.clicked += () => { captureHelper.CaptureImage(false); };
        }

        private void DrawCameraField()
        {
            cameraField = new ObjectField("Camera")
            {
                value = SetCaptureCamera()
            };
            root.Add(cameraField);

            // 监听相机字段变化
            cameraField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                RegisterCameraEvent(e.newValue);
            });

            DrawCameraPanel();
        }

        private UnityEngine.Object SetCaptureCamera()
        {
            var newCamera = GameObject.FindWithTag("CaptureCamera");
            if (newCamera == null)
            {
                //创建一个
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
            camera = cameraObj.GetComponent<CinemachineCamera>();
            orbitalFollow = cameraObj.GetComponent<CinemachineOrbitalFollow>();
            // 每次选择新的相机时重新注册 dataSourceSO 的监听
            EditorApplication.update -= UpdateCameraFromDataSource;
            EditorApplication.update += UpdateCameraFromDataSource;
            captureHelper.InitializeHelper(cameraObj);

        }

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
            isCameraOrthographic = Camera.main.orthographic;
            cameraPanelElement = cameraPanel.Instantiate();
            perspectiveField = cameraPanelElement.Q<VisualElement>("Perspective");
            orthographicField = cameraPanelElement.Q<VisualElement>("Orthographic");
            CameraProjectionChoice();
            root.Add(cameraPanelElement);

            UpdateUIFromCamera();
        }

        private void UpdateCameraPanel()
        {
            if (isCameraOrthographic == Camera.main.orthographic) return;
            isCameraOrthographic = Camera.main.orthographic;
            CameraProjectionChoice();
            cameraPanelElement.MarkDirtyRepaint();
        }

        private void CameraProjectionChoice()
        {
            if (camera == null || orbitalFollow == null) return;
            if (isCameraOrthographic)
            {
                orthographicField.style.display = DisplayStyle.Flex;
                perspectiveField.style.display = DisplayStyle.None;
            }
            else
            {
                perspectiveField.style.display = DisplayStyle.Flex;
                orthographicField.style.display = DisplayStyle.None;
            }
        }

        private void UpdateCameraFromDataSource()
        {
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
            captureObject = CreateInstance<CaptureObjSO>(); // 创建捕获对象实例
            var inspectorElement = new InspectorElement(); // 创建检查器元素
            var serializedObject = new SerializedObject(captureObject); // 序列化捕获对象
            inspectorElement.Bind(serializedObject); // 绑定序列化对象到检查器元素
            root.Add(inspectorElement);
        }
    }
}