using UnityEngine;
using System.Collections;

public class PerlinNoiseAlgo : MonoBehaviour {

	// Hash lookup table as defined by Ken Perlin. Random 0-255 inclusive
	private static int[] permutation = { 151,160,137,91,90,15,
		131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
		190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
		88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
		77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
		102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
		135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
		5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
		223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
		129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
		251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
		49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
		138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
	};

	private static int[] p;

	static void DoublePerlinTable() {
		p = new int [512];
		for (int i = 0; i < 512; i++) {
			p[i] = permutation[i % 256];
		}
	}

	//Smooths output; easing towards integral values
	public static float fade(float t) {
		return 6 * Mathf.Pow (t, 5) - 15 * Mathf.Pow (t, 4) + 10 * Mathf.Pow (t, 3);
	}

	// Source: http://riven8192.blogspot.com/2010/08/calculate-perlinnoise-twice-as-fast.html
	public static float gradient(int hash, float x, float y, float z) {
		switch(hash & 0xF) {
		case 0x0: return  x + y;
		case 0x1: return -x + y;
		case 0x2: return  x - y;
		case 0x3: return -x - y;
		case 0x4: return  x + z;
		case 0x5: return -x + z;
		case 0x6: return  x - z;
		case 0x7: return -x - z;
		case 0x8: return  y + z;
		case 0x9: return -y + z;
		case 0xA: return  y - z;
		case 0xB: return -y - z;
		case 0xC: return  y + x;
		case 0xD: return -y + z;
		case 0xE: return  y - x;
		case 0xF: return -y - z;
		default: return 0; // never happens
		}
	}

	//Perlin Values in 3D
	public static float GeneratePerlinValue(float x, float y, float z) {
		DoublePerlinTable ();

		//Determine unit cube coordinates
		int x0 = (int)x & 255;
		int x1 = x0 + 1;

		int y0 = (int)y & 255;
		int y1 = y0 + 1;

		int z0 = (int)z & 255;
		int z1 = z0 + 1;

		//Get input point in unit cube space
		float xFade = x - (int)x;
		float yFade = y - (int)y;
		float zFade = z - (int)z;

		//Determine fade curves for each point
		float unitCubeX = fade (xFade);
		float unitCubeY = fade (yFade);
		float unitCubeZ = fade (zFade);

		//Hash coordinates of unit cube vertices
		int pointOne   = p[p[p[x0] + y0] + z0];
		int pointTwo   = p[p[p[x0] + y1] + z0];
		int pointThree = p[p[p[x0] + y0] + z1];
		int pointFour  = p[p[p[x0] + y1] + z1];
		int pointFive  = p[p[p[x1] + y0] + z0];
		int pointSix   = p[p[p[x1] + y1] + z0];
		int pointSeven = p[p[p[x1] + y0] + z1];
		int pointEight = p[p[p[x1] + y1] + z1];

		float x1Lerp = Mathf.Lerp (gradient (pointOne, xFade, yFade, zFade), gradient (pointFive, xFade - 1, yFade, zFade), unitCubeX);
		float x2Lerp = Mathf.Lerp (gradient (pointTwo, xFade, yFade - 1, zFade), gradient (pointSix, xFade - 1, yFade - 1, zFade), unitCubeX);

		float x3Lerp = Mathf.Lerp (gradient (pointThree, xFade, yFade, zFade - 1), gradient (pointSeven, xFade - 1, yFade, zFade - 1), unitCubeX);
		float x4Lerp = Mathf.Lerp (gradient (pointFour, xFade, yFade - 1, zFade - 1), gradient (pointEight, xFade - 1, yFade - 1, zFade - 1), unitCubeX);

		float y1Lerp = Mathf.Lerp (x1Lerp, x2Lerp, unitCubeY);
		float y2Lerp = Mathf.Lerp (x3Lerp, x4Lerp, unitCubeY);

		return (Mathf.Lerp (y1Lerp, y2Lerp, unitCubeZ) + 1) / 2.0f;
	}
}
