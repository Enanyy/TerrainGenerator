using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (TerrainPreview))]
public class TerrainPreviewEditor : Editor {

	public override void OnInspectorGUI() {
		TerrainPreview mapPreview = (TerrainPreview)target;

		if (DrawDefaultInspector ()) {
			if (mapPreview.autoUpdate) {
				mapPreview.DrawMapInEditor ();
			}
		}

		if (GUILayout.Button ("Generate")) {
			mapPreview.DrawMapInEditor ();
		}

        if (GUILayout.Button("Generate Tree"))
        {
            mapPreview.GenerateTree();
        }
	}
}
