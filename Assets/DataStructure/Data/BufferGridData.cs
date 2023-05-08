using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SimDataStructure.Data
{
    public class BufferGridData : AbstractGridData
    {
        private int width;
        private int height;

        private int contentSize;

        private ComputeBuffer computeBuffer;

        public BufferGridData(int width, int height, int contentSize = sizeof(float) * 4)
        {
            this.width = width;
            this.height = height;
            this.contentSize = contentSize;

            this.computeBuffer = new ComputeBuffer(width * height, contentSize, ComputeBufferType.Default, ComputeBufferMode.Immutable);
        }

        // Return a copy of the buffer
        public ComputeBuffer GetData()
        {
            ComputeBuffer copy = new ComputeBuffer(width * height, contentSize, ComputeBufferType.Default, ComputeBufferMode.Immutable);

            ComputeBuffer.CopyCount(this.computeBuffer, copy, 0);

            return copy;
        }

        public void SetData(ComputeBuffer toSet)
        {
            this.computeBuffer = toSet;
        }

        public override void Release()
        {
            if (this.computeBuffer != null)
            {
                this.computeBuffer.Release();
                this.computeBuffer = null;
            }
        }

        public int GetWidth()
        {
            return this.width;
        }
        
        public int GetHeight()
        {
            return this.height;
        }

        public int GetContentSize()
        {
            return this.contentSize;
        }
    }
}