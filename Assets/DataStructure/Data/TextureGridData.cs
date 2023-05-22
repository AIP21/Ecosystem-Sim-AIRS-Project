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
            this.renderTexture = this.createTexture(format, filterMode);
        }

        /**
        <summary>
            Copies the data from this TextureGridData to the given RenderTexture.
            This just does Graphics.Blit() to copy over the data.
        </summary>
        **/
        public override void GetData(ref object target)
        {
            if (this.renderTexture == null)
            {
                Debug.LogError("TextureGridData.SetData: RenderTexture is null");
                return;
            }

            if (target != null && target is RenderTexture)
            {
                Graphics.Blit(this.renderTexture, (RenderTexture)target);
            }
            else
            {
                Debug.LogError("TextureGridData.GetData: target is null or not a RenderTexture");
            }
        }

        public void CopyData(RenderTexture other){
            Graphics.Blit(this.renderTexture, other);
        }

        /**
        <summary>
            Copies the data from the given RenderTexture to this TextureGridData.
            This just does Graphics.Blit() to copy over the data.
        </summary>
        **/
        public override void SetData(object source)
        {
            if (this.renderTexture == null)
            {
                Debug.LogError("TextureGridData.SetData: RenderTexture is null");
                return;
            }

            if (source != null && source is RenderTexture)
            {
                Graphics.Blit((RenderTexture)source, this.renderTexture);
            }
        }

        public override void Dispose()
        {
            if (this.renderTexture != null)
            {
                this.renderTexture.Release();
                this.renderTexture = null;
            }
        }

        private RenderTexture createTexture(RenderTextureFormat format, FilterMode filterMode = FilterMode.Point)
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