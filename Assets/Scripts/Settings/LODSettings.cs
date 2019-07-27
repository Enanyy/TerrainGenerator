using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu()]
public class LODSettings : UpdatableData
{
    public const int numSupportedLODs = 5;

    public float minDistance = 50;
    public float maxDistance = 1000;


    public LODInfo[] detailLevels;

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        base.OnValidate();
        if(detailLevels!= null && detailLevels.Length >numSupportedLODs)
        {
            var array = new LODInfo[numSupportedLODs];
            Array.Copy(detailLevels, 0, array, 0, numSupportedLODs);
            detailLevels = array;
        }
        
    }
#endif
}
[System.Serializable]
public struct LODInfo
{
    [Range(0, LODSettings.numSupportedLODs - 1)]
    public int lod;
    public float distance;
}