# Quickon 使用说明

Quickon 是一个 Unity 插件，用于对预制件进行批量拍摄并生成 PNG 图片，集成了自动拍照和手动拍照功能。

## 功能概述

1. **自动拍照**：从列表中获取预制件，逐一实例化并拍照。
2. **手动拍照**：用户可以在场景中自行摆放预制件进行拍照。
3. **相机控制**：支持正交投影和透视投影的切换，以及水平和垂直轴向的调整。
4. **背景透明**：用户可以选择生成的图片背景是否透明。
5. **预览功能**：支持在批量生成图片前进行预览，预览时的相机设置将会被保存。

## 使用方法

### 1. 安装依赖

确保项目中已安装以下 Unity 包：

- Cinemachine
- ShaderGraph

### 2. 导入 Quickon 插件

将 Quickon 插件导入到 Unity 项目中。

### 3. 打开 Quickon 窗口

1. 在 Unity 编辑器中，选择 `Tools -> Quickon` 打开 Quickon 窗口；
2. 进入`CaptureScene` 场景。

### 4. 预览功能

1. 激活 `Preview` 选项，将列表中第一个预制体生成到场景中。
2. 点击 `Next` 按钮和 `Previous` 按钮进行预览对象的切换，插件将保存预览时的相机设置。
3. 打开 `Gizmo` 可以预览图像与图片尺寸比例。

### 5. 自动拍照

1. 在 Quickon 窗口中，将需要拍摄的物体添加到 `CaptureObjects` 列表中。
2. 设置相机的投影模式、水平轴向、垂直轴向等参数。
3. 点击 `Auto Capture Images` 按钮，插件将自动逐一实例化列表中的物体并进行拍摄。（拍摄前请确保场景已清空）

### 6. 手动拍照

1. 在场景中摆放好需要拍摄的物体。
2. 设置相机的投影模式、水平轴向、垂直轴向等参数。
3. 点击 `Manual Capture Image` 按钮进行拍摄。

### 7. 输出路径

图像输出路径默认为: `Assets/Quickon_Output/`

### 8. 版本历史

- v1.0.0: 初始版本，包含基本的自动拍照和手动拍照功能。