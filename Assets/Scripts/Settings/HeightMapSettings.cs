using System;
using UnityEngine;

[Serializable]
public class HeightMapSettings
{
    public NoiseSettings noiseSettings;

    public float meshHeightMultiplier = 10;
    public AnimationCurve meshHeightCurve;

}