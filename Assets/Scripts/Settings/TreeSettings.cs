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
        public Mesh mesh;
        public Material material;
        [Range(0,100)]
        public float minHeight;
        [Range(0, 100)]
        public float maxHeight;

        public int seed = 0;
        public float range = 2;

        public float minScale = 0.3f;
        public float maxScale = 5f;

    }
    
    public TreeLayer[] trees;
}

