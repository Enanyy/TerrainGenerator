using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class MeshSettings : UpdatableData {

	public const int numSupportedLODs = 5;
	public const int numSupportedChunkSizes = 9;
	
	public static readonly int[] supportedChunkSizes = {48,72,96,120,144,168,192,216,240};
	
	public float meshScale = 2.5f;

	[Range(0,numSupportedChunkSizes-1)]
	public int chunkSizeIndex;
	

	// num verts per line of mesh rendered at LOD = 0. Includes the 2 extra verts that are excluded from final mesh, but used for calculating normals
	public int numVertsPerLine {
		get {
			return supportedChunkSizes [chunkSizeIndex] + 5;
		}
	}

	public float meshWorldSize {
		get {
			return (numVertsPerLine - 3) * meshScale;
		}
	}


}
