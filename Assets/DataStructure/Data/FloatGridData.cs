using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SimDataStructure.Data
{
    public class FloatGridData : AbstractGridData
    {
        private float value;

        public FloatGridData(float initialValue)
        {
            this.value = initialValue;
        }

        /**
        <summary>
            Sets the target to be the value of this data
        </summary>
        **/
        public override void GetData(ref object target)
        {
            target = this.value;
        }

        /**
        <summary>
            Sets this value to the given one
        </summary>
        **/
        public override void SetData(object source)
        {
            if (source == null)
            {
                Debug.LogError("FloatGridData.SetData: source is null");
                return;
            }

            if (source is float)
                this.value = (float)source;
        }

        public override void Dispose()
        {

        }
    }
}