using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace SamsBackpack.RenderFeatureStash
{
    public class TextureStash : ContextItem
    {
        public TextureHandle GetTexture(TextureInfo info, RenderGraph renderGraph, TextureHandle cameraColor)
        {
            switch (info.source)
            {
                case Source.CameraColor:
                    return cameraColor;

                case Source.Custom:
                    {
                        if (textures.ContainsKey(info))
                            return textures[info];

                        TextureHandle handle = CreateTexture(info, renderGraph, cameraColor);
                        textures.Add(info, handle);
                        return handle;

                    }
            }

            return TextureHandle.nullHandle;
        }

        public TextureHandle CreateTexture(TextureInfo info, RenderGraph renderGraph, TextureHandle cameraColor)
        {
            TextureDesc cameraDesc = cameraColor.GetDescriptor(renderGraph);

            TextureDesc texDesc = new TextureDesc(info.ScaleFunc)
            {
                name = info.name,
                wrapMode = info.wrapMode,
                clearBuffer = info.clear,
                clearColor = info.clearColor,
                colorFormat = info.customGraphicFormat ? info.graphicFormat : cameraDesc.colorFormat,
                filterMode = info.filterMode,
                enableRandomWrite = info.enableRandomWrite,
            };

            return renderGraph.CreateTexture(texDesc);
        }

        private Dictionary<TextureInfo, TextureHandle> textures = new Dictionary<TextureInfo, TextureHandle>();

        public override void Reset()
        {
            textures.Clear();
        }
    }
}