using System.IO;
using UnityEngine;

namespace Quickon.Core
{
    internal static class QuickonConfig
    {
        private static string _imgOutputPath;
        internal static bool IsPreview;
        internal static bool IsTransparent = true;

        internal const string Orthographic = "Orthographic";
        internal const string Perspective = "Perspective";


        static QuickonConfig()
        {
            _imgOutputPath = Path.Combine(Application.dataPath, "Quickon_Output/");
            // 确保路径存在
            Directory.CreateDirectory(_imgOutputPath);
        }

        internal static string ImgOutputPath
        {
            get => _imgOutputPath;
            set
            {
                _imgOutputPath = value;
                Directory.CreateDirectory(_imgOutputPath);
            }
        }
    }
}
