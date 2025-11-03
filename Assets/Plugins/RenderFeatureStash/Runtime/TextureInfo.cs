using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace SamsBackpack.RenderFeatureStash
{
    [CreateAssetMenu(menuName = "Rendering/Texture Info")]
    public class TextureInfo : ScriptableObject
    {
        public Source source;
        public bool enableRandomWrite;
        public bool clear;
        public Color clearColor;
        public TextureWrapMode wrapMode = TextureWrapMode.Clamp;
        public FilterMode filterMode = FilterMode.Bilinear;
        public bool customGraphicFormat;
        public GraphicsFormat graphicFormat = GraphicsFormat.R32G32B32A32_SFloat;
        public StashSizeMode sizeMode;
        public float screenRatio = 0.5f;
        public Vector2Int fixedSize = new Vector2Int(512, 512);

        public ScaleFunc ScaleFunc
        {
            get
            {
                switch (sizeMode)
                {
                    default: return ScreenSizeScaleFunc;
                    case StashSizeMode.ScreenRatio: return ScreenRatioScaleFunc;
                    case StashSizeMode.FixedSize: return FixedSizeScaleFunc;
                }
            }
        }

        public Vector2Int ScreenSizeScaleFunc(Vector2Int screenSize)
        {
            return screenSize;
        }

        public Vector2Int ScreenRatioScaleFunc(Vector2Int screenSize)
        {
            return new Vector2Int(Mathf.RoundToInt(screenSize.x * screenRatio), Mathf.RoundToInt(screenSize.y * screenRatio));
        }

        public Vector2Int FixedSizeScaleFunc(Vector2Int screenSize)
        {
            return fixedSize;
        }
    }

    public enum Source
    {
        CameraColor,
        Custom
    }

    public enum StashSizeMode
    {
        Screen,
        ScreenRatio,
        FixedSize
    }
}