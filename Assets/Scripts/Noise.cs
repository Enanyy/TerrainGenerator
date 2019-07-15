using UnityEngine;
using System.Collections;

public static class Noise {

	public enum NormalizeMode {Local, Global};

	public static float[,] GenerateNoiseData(int mapWidth, int mapHeight, NoiseSettings noiseSettings, Vector2 center) {
		float[,] noiseMap = new float[mapWidth,mapHeight];

		System.Random prng = new System.Random (noiseSettings.seed);
		Vector2[] octaveOffsets = new Vector2[noiseSettings.octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

		for (int i = 0; i < noiseSettings.octaves; i++) {
			float offsetX = prng.Next (-100000, 100000) + noiseSettings.offset.x + center.x;
			float offsetY = prng.Next (-100000, 100000) - noiseSettings.offset.y - center.y;
			octaveOffsets [i] = new Vector2 (offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= noiseSettings.persistance;
		}

		if (noiseSettings.noiseScale <= 0) {
            noiseSettings.noiseScale = 0.0001f;
		}

		float maxLocalNoiseHeight = float.MinValue;
		float minLocalNoiseHeight = float.MaxValue;

		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;


		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {

				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

				for (int i = 0; i < noiseSettings.octaves; i++) {
					float sampleX = (x-halfWidth + octaveOffsets[i].x) / noiseSettings.noiseScale * frequency;
					float sampleY = (y-halfHeight + octaveOffsets[i].y) / noiseSettings.noiseScale * frequency;

					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;

					amplitude *= noiseSettings.persistance;
					frequency *= noiseSettings.lacunarity;
				}

				if (noiseHeight > maxLocalNoiseHeight) {
					maxLocalNoiseHeight = noiseHeight;
				} else if (noiseHeight < minLocalNoiseHeight) {
					minLocalNoiseHeight = noiseHeight;
				}
				noiseMap [x, y] = noiseHeight;
			}
		}

		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				if (noiseSettings.mode == NormalizeMode.Local) {
					noiseMap [x, y] = Mathf.InverseLerp (minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap [x, y]);
				} else {
					float normalizedHeight = (noiseMap [x, y] + 1) / (maxPossibleHeight/0.9f);
					noiseMap [x, y] = Mathf.Clamp(normalizedHeight,0, int.MaxValue);
				}
			}
		}

		return noiseMap;
	}

}

