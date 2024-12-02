using UnityEngine;
using System;

namespace Quickon.Core
{
    [CreateAssetMenu(fileName = "DataSourceSO", menuName = "Scriptable Objects/DataSourceSO")]
    internal class DataSourceSO : ScriptableObject
    {
        [SerializeField] internal int ImgWeight = 256;
        [SerializeField] internal int ImgHeight = 256;
        [SerializeField] internal float OrthographicSize;
        [SerializeField] internal float FieldOfView;
        [SerializeField] internal float HorizontalAxis;
        [SerializeField] internal float VerticalAxis;
    }
}
