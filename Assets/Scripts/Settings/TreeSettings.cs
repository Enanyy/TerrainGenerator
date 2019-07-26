using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TreeSettings : UpdatableData
{
    [Serializable]
    public class TreeLayer
    {
        public GameObject tree;
        [Range(0,100)]
        public float minHeight;
        [Range(0, 100)]
        public float maxHeight;
    }
    [Range(1,10)]
    public int distance = 4;
    public TreeLayer[] trees;
}

