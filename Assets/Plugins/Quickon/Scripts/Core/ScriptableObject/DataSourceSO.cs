using UnityEngine;

[CreateAssetMenu(fileName = "DataSourceSO", menuName = "Scriptable Objects/DataSourceSO")]
public class DataSourceSO : ScriptableObject
{
    public int ImgWeight = 256;
    public int ImgHeight = 256;
    public float OrthographicSize;
    public float FieldOfView;
    public float HorizontalAxis;
    public float VerticalAxis;
}
