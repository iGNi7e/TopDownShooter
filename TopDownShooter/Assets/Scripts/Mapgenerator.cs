using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mapgenerator : MonoBehaviour {

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Vector2 mapSize;

    [Range(0,1)]
    public float outlinePercent;
    [Range(0,1)]
    public float obstaclePercent;

    List<Coord> allTileCoords;
    Queue<Coord> shuffleTileCoords;

    public int seed = 10;
    Coord mapCenter;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        allTileCoords = new List<Coord>();

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x,y));
            }
        }

        shuffleTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(),seed));
        mapCenter = new Coord((int)mapSize.x / 2,(int)mapSize.y / 2);

        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x,y);
                Transform newTile = Instantiate(tilePrefab,tilePosition,Quaternion.Euler(Vector3.right*90f)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent);
                newTile.parent = mapHolder;
            }
        }

        bool[,] obstacleMap = new bool[(int)mapSize.x,(int)mapSize.y];

        int obstacleCount = (int)(mapSize.x * mapSize.y * obstaclePercent);
        int currentObstacleCount = 0;
        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();

            obstacleMap[randomCoord.x,randomCoord.y] = true;
            currentObstacleCount++;
            if (randomCoord.x != mapCenter.x && randomCoord.y != mapCenter.y && MapisFullyAccessible(obstacleMap,currentObstacleCount))
            {
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x,randomCoord.y);
                Transform newObstacle = Instantiate(obstaclePrefab,obstaclePosition + Vector3.up * 0.5f,Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder;
            }
            else
            {
                obstacleMap[randomCoord.x,randomCoord.y] = false;
                currentObstacleCount--;
            }

        }
    }

    bool MapisFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0),obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(mapCenter);
        mapFlags[mapCenter.x,mapCenter.y] = true;

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
        int targetAccessibleTileCount = (int)(mapSize.x * mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-mapSize.x / 2 + 1f * x,0,-mapSize.y / 2 + 1f * y);
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffleTileCoords.Dequeue();
        shuffleTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

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
}
