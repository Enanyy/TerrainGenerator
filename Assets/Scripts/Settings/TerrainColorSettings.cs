using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TerrainColorSettings
{
    public Material material;
    public TerrainColor[] colors = new TerrainColor[]
    {
        new TerrainColor{name = "Water Deep", height = 0,color = new Color(42/255f,92/255f,189/255f,0)},
        new TerrainColor{name = "Water Shallow", height = 0.27f,color = new Color(54/255f,103/255f,199/255f,0)},
        new TerrainColor{name = "Sand", height = 0.38f,color = new Color(210/255f,208/255f,125/255f,0)},
        new TerrainColor{name = "Grass", height = 0.46f,color = new Color(86/255f,152/255f,23/255f,0)},
        new TerrainColor{name = "Grass2", height = 0.67f,color = new Color(62/255f,107/255f,18/255f,0)},
        new TerrainColor{name = "Rock", height = 0.76f,color = new Color(90/255f,69/255f,53/255f,0)},
        new TerrainColor{name = "Rock2", height = 0.87f,color = new Color(75/255f,60/255f,53/255f,0)},
        new TerrainColor{name = "Snow", height = 0.91f,color = new Color(255/255f,255/255f,255/255f,0)},
      
    };
}

