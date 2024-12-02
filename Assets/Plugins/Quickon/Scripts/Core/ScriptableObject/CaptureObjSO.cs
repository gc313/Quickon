using System.Collections.Generic;
using UnityEngine;
using System;

namespace Quickon.Core
{
    [CreateAssetMenu(fileName = "CaptureObjSO", menuName = "Scriptable Objects/CaptureObjSO")]
    internal class CaptureObjSO : ScriptableObject
    {
        [SerializeField] internal List<CaptureObject> CaptureObjects = new List<CaptureObject>();
    }
}

namespace Quickon.Core
{
    [Serializable]
    internal class CaptureObject
    {
        [SerializeField] internal GameObject gameObject;
        [SerializeField] internal ProjectionType projectionType;
        [SerializeField] internal float horizontalAxis;
        [SerializeField] internal float verticalAxis;
        [SerializeField] internal float fieldOfView;
        [SerializeField] internal float orthographicSize;
    }
}

namespace Quickon.Core
{
    [Serializable]
    internal enum ProjectionType
    {
        None,
        Perspective,
        Orthographic
    }
}