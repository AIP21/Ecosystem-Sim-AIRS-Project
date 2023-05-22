using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using SimDataStructure.Data;

namespace Weather
{
    [System.Serializable]
    public class WeatherGridData : AbstractGridData
    {
        #region Primary variables
        [Header("Primary variables")]
        public float BaseTemp = 0;
        public float ActualTemp = 0;

        //[Range(0, 100)]
        public float BaseHumidity = 0; //Percent
                                       //[Range(0, 100)]
        public float ActualHumidity = 0; //Percent
        #endregion

        #region Secondary variables
        [Header("Secondary variables")]
        public float WindSpeed = 0;
        public float WindDir = 0;
        //[Range(0, 100)]
        public float PrecipChance = 0; //Percent
                                       //[Range(0, 100)]
        public float PrecipIntensity = 0; //Percent
        public float PrecipTemp = 0;
        public float PrecipLength = 0;
        //[Range(0, 100)]
        public float Fogginess = 0; //Percent
                                    //[Range(0, 100)]
        public float CloudCover = 0; //Percent

        public bool isRaining = false;
        public bool isThundering = false;
        public bool isSnowing = false;
        #endregion

        #region Methods
        public void Reset()
        {
            BaseTemp = 0;
            ActualTemp = 0;
            BaseHumidity = 0;
            ActualHumidity = 0;
        }
        public void ResetEditor()
        {
            BaseTemp = 0;
            ActualTemp = 0;
            BaseHumidity = 0;
            ActualHumidity = 0;
        }
        #endregion

        public WeatherGridData()
        {

        }

        /**
        <summary>
            Copy all the data from this data to the target
        </summary>
        **/
        public override void GetData(ref object target)
        {
            if (target is WeatherGridData)
            {
                WeatherGridData targetData = (WeatherGridData)target;

                targetData.BaseTemp = this.BaseTemp;
                targetData.ActualTemp = this.ActualTemp;
                targetData.BaseHumidity = this.BaseHumidity;
                targetData.ActualHumidity = this.ActualHumidity;
                targetData.WindSpeed = this.WindSpeed;
                targetData.WindDir = this.WindDir;
                targetData.PrecipChance = this.PrecipChance;
                targetData.PrecipIntensity = this.PrecipIntensity;
                targetData.PrecipTemp = this.PrecipTemp;
                targetData.PrecipLength = this.PrecipLength;
                targetData.Fogginess = this.Fogginess;
                targetData.CloudCover = this.CloudCover;
                targetData.isRaining = this.isRaining;
                targetData.isThundering = this.isThundering;
                targetData.isSnowing = this.isSnowing;
            }
        }

        /**
        <summary>
            Copy all the data from the source to this data
        </summary>
        **/
        public override void SetData(object source)
        {
            if (source == null)
            {
                Debug.LogError("WeatherGridData.SetData: source is null");
                return;
            }

            if (source is WeatherGridData)
            {
                WeatherGridData sourceData = (WeatherGridData)source;

                this.BaseTemp = sourceData.BaseTemp;
                this.ActualTemp = sourceData.ActualTemp;
                this.BaseHumidity = sourceData.BaseHumidity;
                this.ActualHumidity = sourceData.ActualHumidity;
                this.WindSpeed = sourceData.WindSpeed;
                this.WindDir = sourceData.WindDir;
                this.PrecipChance = sourceData.PrecipChance;
                this.PrecipIntensity = sourceData.PrecipIntensity;
                this.PrecipTemp = sourceData.PrecipTemp;
                this.PrecipLength = sourceData.PrecipLength;
                this.Fogginess = sourceData.Fogginess;
                this.CloudCover = sourceData.CloudCover;
                this.isRaining = sourceData.isRaining;
                this.isThundering = sourceData.isThundering;
                this.isSnowing = sourceData.isSnowing;
            }
        }

        public void CopyTo(WeatherGridData other)
        {
            other.BaseTemp = this.BaseTemp;
            other.ActualTemp = this.ActualTemp;
            other.BaseHumidity = this.BaseHumidity;
            other.ActualHumidity = this.ActualHumidity;
            other.WindSpeed = this.WindSpeed;
            other.WindDir = this.WindDir;
            other.PrecipChance = this.PrecipChance;
            other.PrecipIntensity = this.PrecipIntensity;
            other.PrecipTemp = this.PrecipTemp;
            other.PrecipLength = this.PrecipLength;
            other.Fogginess = this.Fogginess;
            other.CloudCover = this.CloudCover;
            other.isRaining = this.isRaining;
            other.isThundering = this.isThundering;
            other.isSnowing = this.isSnowing;
        }

        public override void Dispose()
        {

        }
    }
}