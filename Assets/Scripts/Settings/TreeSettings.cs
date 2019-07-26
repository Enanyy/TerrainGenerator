using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TreeSettings : UpdatableData
{
    [Serializable]
    public class TreeLayer
    {
        [Range(1, 10)]
        public int distance = 4;
        public GameObject tree;
        [Range(0,100)]
        public float minHeight;
        [Range(0, 100)]
        public float maxHeight;

        public int seed = 0;
    }
    
    public TreeLayer[] trees;
}

