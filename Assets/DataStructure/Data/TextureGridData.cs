using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SimDataStructure.Data
{
    public class TextureGridData : AbstractGridData
    {
        private int resolution;

        private RenderTexture renderTexture;

        public TextureGridData(int resolution, RenderTextureFormat format, FilterMode filterMode = FilterMode.Point)
        {
            this.resolution = resolution;
            this.renderTexture = this.CreateTexture(format, filterMode);
        }

        // Return a copy of the texture
        public RenderTexture GetData()
        {
            RenderTexture copy = CreateTexture(this.renderTexture.format, this.renderTexture.filterMode);

            Graphics.Blit(this.renderTexture, copy);

            return copy;
        }

        public void SetData(RenderTexture toSet)
        {
            this.renderTexture = (RenderTexture)toSet;
        }

        public override void Release()
        {
            if (this.renderTexture != null)
            {
                this.renderTexture.Release();
                this.renderTexture = null;
            }
        }

        public RenderTexture CreateTexture(RenderTextureFormat format, FilterMode filterMode = FilterMode.Point)
        {
            RenderTexture rt = new RenderTexture(resolution, resolution, 24, format);
            rt.filterMode = filterMode;
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.enableRandomWrite = true;
            rt.Create();

            return rt;
        }
    }
}