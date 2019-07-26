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
        public float range = 2;

        private Queue<GameObject> mCacheTrees = new Queue<GameObject>();
        public GameObject InstantiateTree()
        {
            GameObject go = null;
            if(mCacheTrees.Count > 0)
            {
                go = mCacheTrees.Dequeue();
            }else
            {
                go = Instantiate(tree);
            }
            go.SetActive(true);
            return go;
        }
        public void ReturnTree(GameObject go)
        {
            go.transform.SetParent(null);
            go.SetActive(false);
            mCacheTrees.Enqueue(go);
        }
    }
    
    public TreeLayer[] trees;
}

