# Quickon User Guide

Quickon is a Unity plugin designed to batch-capture prefabs and generate PNG images. 

## Feature Overview

1. **Automatic Capture**: Retrieves prefabs from a list, instantiates them one by one, and captures images.
2. **Manual Capture**: Allows users to place prefabs manually in the scene and capture images.
3. **Camera Control**: Supports switching between orthographic and perspective projections, as well as adjusting horizontal and vertical axes.
4. **Transparent Background**: Users can choose whether the background of the generated images should be transparent.
5. **Preview Functionality**: Supports previewing before batch image generation, saving camera settings during preview.

## Usage

### 1. Install Dependencies

Ensure that the following Unity packages are installed in your project:

- Cinemachine
- ShaderGraph

### 2. Import Quickon Plugin

Import the Quickon plugin into your Unity project.

### 3. Open Quickon Window

1. In the Unity Editor, select `Tools -> Quickon` to open the Quickon window.
2. Enter the `CaptureScene` scene.

### 4. Preview Functionality

1. Activate the `Preview` option to instantiate the first prefab from the list into the scene.
2. Click the `Next` and `Previous` buttons to switch between preview objects; the plugin will save the camera settings during preview.
3. Enable `Gizmo` to preview the image size ratio.

### 5. Automatic Capture

1. In the Quickon window, add the objects you want to capture to the `CaptureObjects` list.
2. Set the camera's projection mode, horizontal axis, vertical axis, and other parameters.
3. Click the `Auto Capture Images` button to automatically instantiate and capture images of the objects in the list. (Make sure the scene is cleared before capturing)

### 6. Manual Capture

1. Place the object you want to capture in the scene.
2. Set the camera's projection mode, horizontal axis, vertical axis, and other parameters.
3. Click the `Manual Capture Image` button to capture the image.

### 7. Output Path

The default output path for images is: `Assets/Quickon_Output/`

### 8. Version History

- 0.1.0: Initial release, including basic automatic and manual capture functionalities.