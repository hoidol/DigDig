using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Range(0f, 1f)]
    public float[] fillPercents;
    public Color[] fillColors;

    public Texture2D DrawNoiseMap(float[,] noiseMap, float[,] gradientMap, bool useColorMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        Texture2D noiseTex = new Texture2D(width, height);
        noiseTex.filterMode = FilterMode.Point;
        Color[] colorMap = new Color[width * height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                colorMap[x * height + y] = CalcColor(noiseMap[x, y], gradientMap[x, y], useColorMap);
            }
        }
        noiseTex.SetPixels(colorMap);
        noiseTex.Apply();
        // spriteRenderer.sprite = Sprite.Create(noiseTex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        return noiseTex;

    }

    private Color CalcColor(float noiseValue, float gradientValue, bool useColorMap)
    {
        float value = noiseValue + gradientValue;
        value = Mathf.InverseLerp(0, 2, value); //노이즈 맵과 그라디언트 맵을 더한 값을 0~1사이의 값으로 변환
        Color color = Color.Lerp(Color.black, Color.white, value); //변환된 값에 해당하는 색상을 그레이스케일로 저장
        if (useColorMap)
        {
            for (int i = 0; i < fillPercents.Length; i++)
            {
                if (color.grayscale < fillPercents[i])
                {
                    color = fillColors[i]; //미리 설정한 색상 범위에 따라 색상 변환
                    break;
                }
            }
        }
        return color;
    }

    public int GetIdx(Color color)
    {
        // for (int i = 0; i < fillColors.Length; i++)
        // {
        //     if (fillColors[i].Equals(color))
        //     {
        //         return i;
        //     }
        // }
        // 색상을 정확히 비교하는 대신, 유사한 색상(오차 범위 내의 색상)을 찾도록 구현
        float colorThreshold = 0.05f; // 오차 허용치 (0 ~ 1 사이 값, 필요에 따라 조절)
        for (int i = 0; i < fillColors.Length; i++)
        {
            Color target = fillColors[i];
            // RGB 채널 각각의 차이가 threshold 이하인지 확인 (각 채널의 차이)
            float diffR = Mathf.Abs(target.r - color.r);
            float diffG = Mathf.Abs(target.g - color.g);
            float diffB = Mathf.Abs(target.b - color.b);

            // 또는 전체 색상 벡터 거리로 확인 (아래 중 하나만 사용)
            float diff = Mathf.Sqrt(
                Mathf.Pow(target.r - color.r, 2) +
                Mathf.Pow(target.g - color.g, 2) +
                Mathf.Pow(target.b - color.b, 2)
            );

            // 1. 각 RGB가 모두 threshold 이하면 일치로 간주
            if (diffR < colorThreshold && diffG < colorThreshold && diffB < colorThreshold)
            {
                return fillColors.Length - 1 - i; //i;
            }
            // 2. 또는 전체 컬러 벡터 거리가 threshold 보다 작으면 일치로 간주 (더 엄격)
            // if (diff < colorThreshold) return i;
        }
        // Debug.Log("GetIdx color " + color.ToString());
        // for (int i = 0; i < fillColors.Length; i++)
        // {
        //     Debug.Log($"fillColors[i] {fillColors[i]} ");
        //     if (fillColors[i].Equals(color))
        //     {
        //         return fillColors.Length - 1 - i;
        //     }
        // }
        return 0;
    }
}