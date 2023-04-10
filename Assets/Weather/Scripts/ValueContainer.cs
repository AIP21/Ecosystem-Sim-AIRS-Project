using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

[System.Serializable]
public class ValueContainer
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
}