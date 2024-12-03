using UnityEngine;

namespace Quickon.Core
{
    internal class OnDrawGizmo : MonoBehaviour
    {
        [SerializeField] DataSourceSO dataSourceSO;
        private Camera m_Camera;

        private void OnDrawGizmos()
        {
            if (m_Camera == null)
            {
                m_Camera = Camera.main;
                if (m_Camera == null)
                {
                    return;
                }
            }

            int width = dataSourceSO.ImgWeight;
            int height = dataSourceSO.ImgHeight;

            /// 对于不同宽高比的图像，我们需要计算其在固定高度比例为1的情况下，线框的宽度比例。
            /// 以常见的16:9比例为例，其宽高比约为1.7777。在这种情况下，高度比例为1的长度相当于宽度比例为0.5625的长度（即1 / 1.7777）。
            /// 因此，线框的两侧边距总占比为(1 - 0.5625) = 0.4375，两边边距所占比例分别为 0.4375 / 2 = 0.21875。这意味着左侧边距占比为0.21875，右侧边距占比为(1 - 0.21875)。
            /// 这是在图像尺寸为1:1的情况下的基础情况,有公式：
            /// leftMargin = (1 - 1 / 分辨率比例) * 0.5,
            ///
            /// 对于其他宽高比，我们可以根据以下情况推算：
            /// - 当宽高比为2:1时，左侧边距占比 leftMargin = 0.21875 - (0.5625 / 2)
            /// - 当宽高比为1:2时，左侧边距占比 leftMargin = 0.21875 + (0.5625 / 4)
            ///
            /// 综合上述情况，再引入图像宽高比picAspect，线框宽度占比随picAspect变化，两边间距总的占比为：1 - 1 / 分辨率比例 * 图像宽高比
            /// 得出一个通用公式来计算边距：
            /// margin = (1 - 1 / 分辨率比例 * 图像宽高比) * 0.5

            float picAspect = (float)width / (float)height;
            float screenAspect = m_Camera.aspect;  // 16:9 的比例为 1.777778，4:3 的比例为 1.333333

            float leftMargin, rightMargin;
            leftMargin = (1 - 1 / screenAspect * picAspect) * 0.5f;
            rightMargin = 1 - leftMargin;

            float nearClipPlane = m_Camera.nearClipPlane + 0.01f;

            // 计算边界点
            Vector3 bottomLeft = m_Camera.ViewportToWorldPoint(new Vector3(leftMargin, 0.001f, nearClipPlane));
            Vector3 topRight = m_Camera.ViewportToWorldPoint(new Vector3(rightMargin, 0.999f, nearClipPlane));
            Vector3 bottomRight = m_Camera.ViewportToWorldPoint(new Vector3(rightMargin, 0.001f, nearClipPlane));
            Vector3 topLeft = m_Camera.ViewportToWorldPoint(new Vector3(leftMargin, 0.999f, nearClipPlane));

            // 绘制方框
            Gizmos.color = Color.green;
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }
    }
}