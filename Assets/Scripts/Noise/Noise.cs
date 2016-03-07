using UnityEngine;
using System.Collections;

public static class Noise {

	public static float[ , , ] GenerateNoiseMap(int mapWidth, int mapHeight, int mapDepth, 
											    float scale, int seed, Vector3 offset, 
											    int octaves, float persistance, float lacunarity) {

		float[ , , ] noiseMap = new float[mapWidth, mapHeight, mapDepth];

		System.Random prng = new System.Random (seed);
		Vector3[] octaveOffsets = new Vector3[octaves];
		for (int i = 0; i < octaves; i++) {
			float offsetX = prng.Next (-100000, 100000) + offset.x; //random number to offset perlinNoise + offset to scroll through noise
			float offsetY = prng.Next (-100000, 100000) + offset.y;
			float offsetZ = prng.Next (-100000, 100000) + offset.z;
			octaveOffsets [i] = new Vector3 (offsetX, offsetY, offsetZ);
		}

		if (scale <= 0) {
			scale = 0.0001f; //choose some small positive value
		}

		float maxNoiseHeight = float.MinValue;
		float minNoiseHeight = float.MaxValue;

		float halfWidth = mapWidth / 2.0f;
		float halfHeight = mapHeight / 2.0f;
		float halfDepth = mapDepth / 2.0f;

		for (int x = 0; x < mapWidth; x++) {
			for (int y = 0; y < mapHeight; y++) {
				for (int z = 0; z < mapDepth; z++) {
					
					float amplitude = 1;
					float frequency = 1;
					float noiseHeight = 0;

					for (int i = 0; i < octaves; i++) {
						float sampleX = octaveOffsets [i].x + frequency * (x - halfWidth) / scale; //sample a random point
						float sampleY = octaveOffsets [i].y + frequency * (y - halfHeight) / scale;
						float sampleZ = octaveOffsets [i].z + frequency * (z - halfDepth) / scale;

						//Map noise from -1 to 1 (valleys to hills)
						float perlinValue = PerlinNoiseAlgo.GeneratePerlinValue (sampleX, sampleY, sampleZ);

						noiseHeight += perlinValue * amplitude; //adjust value by current amplitude
						frequency *= lacunarity; //increase frequency with each octave (more details)
						amplitude *= persistance; //decrease amplitude with each octave (less impact from details)
					}

					//Find min/max values
					if (noiseHeight > maxNoiseHeight) {
						maxNoiseHeight = noiseHeight;
					} else if (noiseHeight < minNoiseHeight) {
						minNoiseHeight = noiseHeight;
					}

					noiseMap [x, y, z] = noiseHeight;
				}
			}
		}

		//Normalize the noise
		for (int x = 0; x < mapWidth; x++) {
			for (int y = 0; y < mapHeight; y++) {
				for (int z = 0; z < mapDepth; z++) {
					noiseMap [x, y, z] = Mathf.InverseLerp (minNoiseHeight, maxNoiseHeight, noiseMap [x, y, z]);
				}
			}
		}
		return noiseMap;
	}
}
