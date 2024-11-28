using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "DataSourceSO", menuName = "Scriptable Objects/DataSourceSO")]
public class DataSourceSO : ScriptableObject
{
    public float ApparentSize;
    public float HorizontalAxis;
    public float VerticalAxis;
}
