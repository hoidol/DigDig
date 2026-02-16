using UnityEngine;

public class MapManager : MonoBehaviour
{

    Texture2D tex2D;
    IslandGeneratorByPerlinNoise islandGeneratorByPerlinNoise;
    [SerializeField] private MapDisplay mapDisplay;

    public string seed;
    public bool useRandomSeed;

    public OreStone oreStonePrefab;

    private void Awake()
    {
        islandGeneratorByPerlinNoise = GetComponent<IslandGeneratorByPerlinNoise>();
        mapDisplay = GetComponent<MapDisplay>();
    }

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        if (useRandomSeed) seed = Time.time.ToString(); //시드
        System.Random pseudoRandom = new System.Random(seed.GetHashCode()); //의사 난수
        islandGeneratorByPerlinNoise.xOrg = pseudoRandom.Next(0, 99999); //의사 난수로 부터 랜덤 값 추출
        islandGeneratorByPerlinNoise.yOrg = pseudoRandom.Next(0, 99999);
        tex2D = islandGeneratorByPerlinNoise.GenerateMap();
        int maxX = islandGeneratorByPerlinNoise.width;
        int maxY = islandGeneratorByPerlinNoise.width;

        float initX = -maxX / 2;
        float initY = -maxY / 2;

        for (int y = 0; y < maxY; y++)
        {
            for (int x = 0; x < maxX; x++)
            {
                Vector3 pos = new Vector3(initX + x, initY + y, 0);

                //0,0 기준 반지름 3이내에 있는 점은 continue
                if (Vector2.Distance(new Vector2(0, 0), pos) <= 3) continue;
                Color color = tex2D.GetPixel(x, y);
                int idx = mapDisplay.GetIdx(color);
                OreStone oreStone = Instantiate(oreStonePrefab, pos, Quaternion.identity);
                oreStone.transform.parent = transform;
                oreStone.Init(idx, color);
            }
        }
    }


    private void SaveTexture(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();

#if UNITY_EDITOR
        var dirPath = Application.dataPath + "/RenderOutput";
#else
        var dirPath = Application.persistentDataPath + "/RenderOutput";
#endif
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }

        string fileName = "R_" + Random.Range(0, 100000);

        System.IO.File.WriteAllBytes(dirPath + $"/{fileName}" + ".png", bytes);
        Debug.Log(bytes.Length / 1024 + $"Kb {fileName} was saved as: " + dirPath);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
