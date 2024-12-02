using System.Collections.Generic;
using Quickon.Core;
using UnityEngine;

[CreateAssetMenu(fileName = "CaptureObjSO", menuName = "Scriptable Objects/CaptureObjSO")]
public class CaptureObjSO : ScriptableObject
{
    public List<CaptureObject> CaptureObjects = new List<CaptureObject>();
}

[System.Serializable]
public class CaptureObject
{
    public GameObject gameObject;
    public ProjectionType projectionType;
    public float horizontalAxis;
    public float verticalAxis;
    public float fieldOfView;
    public float orthographicSize;
}


public enum ProjectionType
{
    None,
    Perspective,
    Orthographic
}