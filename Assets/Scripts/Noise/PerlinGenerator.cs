using System;

using UnityEngine;

using CoherentNoise;

public class PerlinGenerator : Generator {

	float [ , , ] noiseMap;

	public PerlinGenerator(int mapWidth, int mapHeight, int mapDepth, 
		float scale, int seed, Vector3 offset, 
		int octaves, float persistance, float lacunarity) {
		noiseMap = Noise.GenerateNoiseMap (mapWidth, mapHeight, mapDepth, scale, seed, offset, octaves, persistance, lacunarity);
	}

	public override float GetValue(float x, float y, float z) {
		return noiseMap [(int)x, (int)y, (int)z];
	}

}
