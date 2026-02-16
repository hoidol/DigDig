using UnityEngine;

public class IslandGeneratorByPerlinNoise : MonoBehaviour
{
    public int width = 64;
    public int height = 64;
    public float scale = 1.0f;
    public int octaves = 3;
    public float persistance = 0.5f;
    public float lacunarity = 2;

    [HideInInspector] public float xOrg = 0;
    [HideInInspector] public float yOrg = 0;

    public bool useColorMap;
    public bool useGradientMap;

    [SerializeField] private PerlinNoise perlinNoise;
    [SerializeField] private Gradient gradient;
    [SerializeField] private MapDisplay mapDisplay;

   

    public Texture2D GenerateMap()
    {
        float[,] noiseMap = perlinNoise.GenerateMap(width, height, scale, octaves, persistance, lacunarity, xOrg, yOrg); //노이즈 맵 생성
        float[,] gradientMap = gradient.GenerateMap(width, height); //그라디언트 맵 생성
        if (useGradientMap)
            return mapDisplay.DrawNoiseMap(noiseMap, gradientMap, useColorMap); //노이즈 맵과 그라디언트 맵 결합

        return mapDisplay.DrawNoiseMap(noiseMap, noiseMap, useColorMap);
    }

}