using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mapgenerator : MonoBehaviour {

    public Map[] maps;
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;
    //public Vector2 mapSize;
    public Vector2 maxMapSize;

    [Range(0,1)]
    public float outlinePercent;
    //[Range(0,1)]
    //public float obstaclePercent;

    public float tileSize;

    List<Coord> allTileCoords;
    Queue<Coord> shuffleTileCoords; //перетасованная очередь
    Queue<Coord> shuffleOpenTileCoords;
    Transform[,] tileMap;

    //public int seed = 10;
    //Coord mapCenter;

    Map currentMap;

    private void Awake()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
        //GenerateMap();
    }

    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }

    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x,currentMap.mapSize.y];
        System.Random prng = new System.Random(currentMap.seed);
        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize,0.05f,currentMap.mapSize.y * tileSize);

        //Генерация координат
        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x,y)); //присвоение всех координат карты
            }
        }

        shuffleTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(),currentMap.seed)); //перетасованная очередь
        //mapCenter = new Coord((int)currentMap.mapSize.x / 2,(int)currentMap.mapSize.y / 2); //определние центра карты

        //Группировка созданных платформ
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            try
            {
                DestroyImmediate(transform.Find(holderName).gameObject);
            }
            catch (System.Exception)
            {
                Destroy(transform.Find(holderName).gameObject);

                throw;
            } 
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        //Спавн тайлов
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x,y);
                Transform newTile = Instantiate(tilePrefab,tilePosition,Quaternion.Euler(Vector3.right*90f)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize; //изменение размера тайлов
                newTile.parent = mapHolder;
                tileMap[x,y] = newTile;
            }
        }

        //Спавн препятствий
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x,(int)currentMap.mapSize.y]; //массив препятствий

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent); //количество препятствий на карте
        int currentObstacleCount = 0; //текущее количество препятствий
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();

            obstacleMap[randomCoord.x,randomCoord.y] = true;
            currentObstacleCount++;
            if (randomCoord != currentMap.mapCenter && MapisFullyAccessible(obstacleMap,currentObstacleCount)) //randomCoord.x != mapCenter.x && randomCoord.y != mapCenter.y
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight,currentMap.maxObstacleHeight,(float)prng.NextDouble());
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x,randomCoord.y);
                Transform newObstacle = Instantiate(obstaclePrefab,obstaclePosition + Vector3.up * obstacleHeight/2,Quaternion.identity) as Transform;
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize,obstacleHeight,(1 - outlinePercent) * tileSize);
                newObstacle.parent = mapHolder;

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colourPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColour,currentMap.backgroundColour,colourPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoord);
            }
            else
            {
                obstacleMap[randomCoord.x,randomCoord.y] = false;
                currentObstacleCount--;
            }

        }

        shuffleOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(),currentMap.seed));

        //Создание mavmeshmask
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x)/4f * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f,1,currentMap.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab,Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize,Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f,1,currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab,Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize,Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x,1,(maxMapSize.y - currentMap.mapSize.y)/2f) * tileSize;

        Transform maskBot = Instantiate(navmeshMaskPrefab,Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize,Quaternion.identity) as Transform;
        maskBot.parent = mapHolder;
        maskBot.localScale = new Vector3(maxMapSize.x,1,(maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        navmeshFloor.localScale = new Vector3(maxMapSize.x,maxMapSize.y) * tileSize;
    }

    bool MapisFullyAccessible(bool[,] obstacleMap, int currentObstacleCount) //определение возможности поставки препятствия на карте, алгоритм заливки Flood fild
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0),obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x,currentMap.mapCenter.y] = true;

        int accessibleTileCount = 1;

        while(queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if(x==0 || y == 0)
                    {
                        if(neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            if(!mapFlags[neighbourX,neighbourY] && !obstacleMap[neighbourX,neighbourY])
                            {
                                mapFlags[neighbourX,neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX,neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }
        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    //Преобразование координат для тайлов
    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x,0,-currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
    }

    public Transform GetTileFromPos(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        x = Mathf.Clamp(x,0,tileMap.GetLength(0) -1);
        y = Mathf.Clamp(y,0,tileMap.GetLength(1) -1);
        return tileMap[x,y];
    }

    //Выборка координат первого в очереди элемента
    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffleTileCoords.Dequeue();
        shuffleTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffleOpenTileCoords.Dequeue();
        shuffleOpenTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x,randomCoord.y];
    }

    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1,Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1,Coord c2)
        {
            return !(c1==c2);
        }
    }

    [System.Serializable]
    public class Map
    {
        public Coord mapSize;
        [Range(0,1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColour;
        public Color backgroundColour;

        public Coord mapCenter
        {
            get { return new Coord((int)mapSize.x / 2,(int)mapSize.y / 2); }
        }
    }
}
