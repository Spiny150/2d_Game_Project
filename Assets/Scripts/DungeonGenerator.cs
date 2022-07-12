using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

using HullDelaunayVoronoi.Delaunay;
using HullDelaunayVoronoi.Primitives;

public class DungeonGenerator : MonoBehaviour
{
    private DelaunayTriangulation2 delaunay;
    

    [SerializeField]
    private Tile groundTile;
    [SerializeField]
    private Tile WallTile;



    [SerializeField]
    private Tilemap groundMap;

    [SerializeField]
    private Tilemap wallMap;

    [SerializeField]
    public List<Room> rooms = new List<Room>();

    const int nbOfRoom = 20;

    const int minRoomSize = 5;
    const int maxRoomSize = 10;

    const int minRoomPos = -80;
    const int maxRoomPos = 80;

    const int minRoomSpread = 5;
    const int roomPlacementTryout = 1000; //like timeout but with try

    const int connectionProbability = 10;



#region auto-call function

    void Update() 
    {
        if (delaunay != null)
        {
            for (int roomIdx = 0; roomIdx < rooms.Count; roomIdx++)
            {
                for (int connectedRoomIdx = 0; connectedRoomIdx < rooms[roomIdx].connectedRooms.Count; connectedRoomIdx++)
                {
                    Debug.DrawLine((Vector3Int) rooms[roomIdx].position, (Vector3Int) rooms[roomIdx].connectedRooms[connectedRoomIdx].position, color:Color.red);                                
                }
            }
        }
    }
    void OnDrawGizmos() {

        foreach (Room room in  rooms)
        {
            Handles.color = Color.red; 
            Handles.Label(new Vector2(room.position.x, room.position.y + room.size.y + 5), room.ID.ToString());
        }
    }

#endregion auto-call function



#region private function

    private int Loop(int value, int minValue, int maxValue)
    {
        while (value < minValue || value > maxValue)
        {
            if (value > maxValue) value -= maxValue + 1;
            if (value < minValue) value += maxValue + 1;
        }
        return value;
    }

    private void GenerateSquare(int x, int y, Vector2Int size)
    {
        for (int tileX = x - size.x; tileX <= x + size.x; tileX++)
        {
            for (int tileY = y - size.y; tileY <= y + size.y; tileY++)
            {
                Vector3Int tilePos = new Vector3Int(tileX, tileY, 0);
                groundMap.SetTile(tilePos, groundTile);
            }
        }
    }


    private void FillWalls()
    {
        BoundsInt bounds = groundMap.cellBounds;
        
        for (int xMap = bounds.xMin - 1; xMap <= bounds.xMax + 1; xMap++)
        {
            for (int yMap = bounds.yMin - 1; yMap <= bounds.yMax + 1; yMap++)
            {
                Vector3Int pos = new Vector3Int(xMap, yMap, 0);
                TileBase tile = groundMap.GetTile(pos);

                TileBase tileUp = groundMap.GetTile(pos + Vector3Int.up);
                TileBase tileDown = groundMap.GetTile(pos + Vector3Int.down);
                TileBase tileRight = groundMap.GetTile(pos + Vector3Int.right);
                TileBase tileLeft = groundMap.GetTile(pos + Vector3Int.left);
                                    
                if (tile == null && (tileUp != null || tileDown != null || tileRight != null || tileLeft != null))
                {
                    wallMap.SetTile(pos, WallTile);                    
                }
            }
        }
    }

    private void ClearMap()
    {
        rooms.Clear();
        BoundsInt bounds = groundMap.cellBounds;
        
        for (int xMap = bounds.xMin - 1; xMap <= bounds.xMax + 1; xMap++)
        {
            for (int yMap = bounds.yMin - 1; yMap <= bounds.yMax + 1; yMap++)
            {
                Vector3Int pos = new Vector3Int(xMap, yMap, 0);
                
                groundMap.SetTile(pos, null);
                wallMap.SetTile(pos, null);
            }
        }
    }

#endregion private function



#region fat-ass function

    private void RandomRooms()
    {
        for (int currentRoomIdx = 0; currentRoomIdx < nbOfRoom; currentRoomIdx++)
        {
            Vector2Int size = new Vector2Int(Random.Range(minRoomSize, maxRoomSize), Random.Range(minRoomSize, maxRoomSize));
            Vector2Int randomPos = new Vector2Int(Random.Range(minRoomPos, maxRoomPos), Random.Range(minRoomPos, maxRoomPos));

            int iteration = 0;
            while(true)
            {
                restart:
                foreach(Room room in rooms)
                {
                    int xDist = Mathf.Abs(randomPos.x - room.position.x);
                    int yDist = Mathf.Abs(randomPos.y - room.position.y);

                    if (xDist - (size.x + room.size.x) <= minRoomSpread && yDist - (size.y + room.size.y) <= minRoomSpread)
                    {
                        randomPos = new Vector2Int(Random.Range(minRoomPos, maxRoomPos), Random.Range(minRoomPos, maxRoomPos));
                        iteration++;

                        if (iteration >= roomPlacementTryout) return;
                        goto restart;
                    }
                }

                rooms.Add(new Room(_position:randomPos, _size:size, _ID:currentRoomIdx+1));
                GenerateSquare(randomPos.x, randomPos.y, size);
                break;
            }            
        }
    }

    private void Triangulate()
    {
        List<Vertex2> vertices = new List<Vertex2>();
        foreach (Room room in rooms)
        {
            vertices.Add(new Vertex2(room.position.x, room.position.y));
        }

        delaunay = new DelaunayTriangulation2();
        delaunay.Generate(vertices);

        for (int roomIdx = 0; roomIdx < rooms.Count; roomIdx++)
        {
            for (int cellIdx = 0; cellIdx < delaunay.Cells.Count; cellIdx++)
            {
                for (int verticeIdx = 0; verticeIdx < 2; verticeIdx++)
                {
                    if (rooms[roomIdx].position == new Vector2Int((int) delaunay.Cells[cellIdx].Simplex.Vertices[verticeIdx].X, (int) delaunay.Cells[cellIdx].Simplex.Vertices[verticeIdx].Y))
                    {

                        Vector2Int relatedRoomPos1 = new Vector2Int((int) delaunay.Cells[cellIdx].Simplex.Vertices[Loop(verticeIdx + 1, 0, 2)].X, (int) delaunay.Cells[cellIdx].Simplex.Vertices[Loop(verticeIdx + 1, 0, 2)].Y);
                        Vector2Int relatedRoomPos2 = new Vector2Int((int) delaunay.Cells[cellIdx].Simplex.Vertices[Loop(verticeIdx + 2, 0, 2)].X, (int) delaunay.Cells[cellIdx].Simplex.Vertices[Loop(verticeIdx + 2, 0, 2)].Y);

                        for (int extraRoomIdx = 0; extraRoomIdx < rooms.Count; extraRoomIdx++)
                        {
                            if (rooms[extraRoomIdx].position == relatedRoomPos1 || rooms[extraRoomIdx].position == relatedRoomPos2)
                            {
                                if (!rooms[roomIdx].connectedRooms.Contains(rooms[extraRoomIdx])) 
                                {
                                    rooms[roomIdx].createConnection(rooms[extraRoomIdx]);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void GenerateMinimumSpanningTree()
    {
        bool foundTree = false;

        List<Connection> deletedRooms = new List<Connection>();
        while (!foundTree)
        {

            int? maxRoomIndex = null;
            int? connectedRoomIdx = null;
            float maxDistance = 0;

            for (int roomIdx = 0; roomIdx < rooms.Count; roomIdx++)
            {
                for (int relatedRoomIdx = 0; relatedRoomIdx < rooms[roomIdx].connectedRooms.Count; relatedRoomIdx++)
                {
                    if (rooms[roomIdx].canAccess(rooms[roomIdx].connectedRooms[relatedRoomIdx]) && rooms[roomIdx].connectionDistance[relatedRoomIdx] >= maxDistance)
                    {
                        maxDistance = rooms[roomIdx].connectionDistance[relatedRoomIdx];
                        maxRoomIndex = roomIdx;
                        connectedRoomIdx = relatedRoomIdx;
                    }
                }                
            }

            if (maxRoomIndex == null)
            {
                foreach (Connection connection in deletedRooms)
                {
                    if (Random.Range(0, 100) < connectionProbability)
                    connection.room1.createConnection(connection.room2);
                }
                foundTree = true;
                break;
            }

            deletedRooms.Add(new Connection(_room1:rooms[(int) maxRoomIndex], _room2:rooms[(int) maxRoomIndex].connectedRooms[(int) connectedRoomIdx]));
            rooms[(int) maxRoomIndex].removeConnection(rooms[(int) maxRoomIndex].connectedRooms[(int) connectedRoomIdx]);

        } 
    }

    private void GenerateRoutes()
    {

        for (int roomIdx = 0; roomIdx < rooms.Count; roomIdx++)
        {
            for (int connectedRoomIdx = 0; connectedRoomIdx < rooms[roomIdx].connectedRooms.Count; connectedRoomIdx++)
            {
                Room R1 = rooms[roomIdx];
                Room R2 = rooms[roomIdx].connectedRooms[connectedRoomIdx];

                Vector2Int midPoint = (R1.position + R2.position) / 2;

                //check for y axis (vertical) corridor
                if ((R1.position.x - R1.size.x + 1 <= midPoint.x && midPoint.x <= R1.position.x + R1.size.x - 1) && (R2.position.x - R2.size.x + 1 <= midPoint.x && midPoint.x <= R2.position.x + R2.size.x - 1))
                {
                    int dir = Mathf.Clamp(R2.position.y - R1.position.y, -1, 1);
                    for (int height = 0; height < Mathf.Abs(R1.position.y - R2.position.y); height++)
                    {
                        GenerateSquare(midPoint.x, R1.position.y + dir * height, new Vector2Int(1, 1));
                    }
                    continue;
                }
                
                //check for x axis (horizontal) corridor
                if ((R1.position.y - R1.size.y + 1 <= midPoint.y && midPoint.y <= R1.position.y + R1.size.y - 1) && (R2.position.y - R2.size.y + 1 <= midPoint.y && midPoint.y <= R2.position.y + R2.size.y - 1))
                {
                    int dir = Mathf.Clamp(R2.position.x - R1.position.x, -1, 1);
                    for (int height = 0; height < Mathf.Abs(R1.position.x - R2.position.x); height++)
                    {
                        GenerateSquare(R1.position.x + dir * height, midPoint.y, new Vector2Int(1, 1));
                    }
                    continue;
                }

                //create L shape corridor
                int direction1 = Mathf.Clamp(R2.position.y - R1.position.y, -1, 1);
                for (int height = 0; height < Mathf.Abs(R1.position.y - R2.position.y); height++)
                {
                    GenerateSquare(R1.position.x, R1.position.y + direction1 * height, new Vector2Int(1, 1));
                }
                int direction2 = Mathf.Clamp(R2.position.x - R1.position.x, -1, 1);
                for (int height = 0; height < Mathf.Abs(R1.position.x - R2.position.x); height++)
                {
                    GenerateSquare(R1.position.x + direction2 * height, R2.position.y, new Vector2Int(1, 1));
                }                
            }

            while (rooms[roomIdx].connectedRooms.Count > 0)
            {
                rooms[roomIdx].removeConnection(rooms[roomIdx].connectedRooms[0]);
            }
        }
    }

#endregion fat-ass function

    public void NewDungeon()
    {
        rooms.Clear();
        ClearMap();

        RandomRooms();
        Triangulate();
        GenerateMinimumSpanningTree();
        GenerateRoutes();
        FillWalls();
    }

}



public class Room
{

    public int ID;
    
    public List<Room> connectedRooms = new List<Room>();
    public List<float> connectionDistance = new List<float>();
    public Vector2Int position;

    public Vector2Int size;

    public Room(Vector2Int _position, Vector2Int _size, int _ID)
    {
        this.position = _position;
        this.size = _size;
        this.ID = _ID;
    }

    public bool canAccess(Room toAccess)
    {
        List<Room> testedRooms = new List<Room>();
        List<Room> testingRooms = new List<Room>();
        List<Room> toTestRooms = new List<Room>();

        toTestRooms = connectedRooms;
        testedRooms.Add(this);

        bool firstIteration = true;

        while (true)
        {
            testingRooms = toTestRooms;
            toTestRooms = new List<Room>();

            for (int i = 0; i < testingRooms.Count; i++)
            {
                if (testingRooms[i] == toAccess && !firstIteration) return true;

                else if (testingRooms[i] != toAccess)
                {
                    for (int j = 0; j < testingRooms[i].connectedRooms.Count; j++)
                    {
                        bool isSameRoom = false;
                        for (int k = 0; k < testedRooms.Count; k++)
                        {
                            if (testedRooms[k] == testingRooms[i].connectedRooms[j])
                            {
                                isSameRoom = true;
                            }                            
                        }
                        if (!isSameRoom) toTestRooms.Add(testingRooms[i].connectedRooms[j]);
                    }
                    testedRooms.Add(testingRooms[i]);
                }
            }

            if (toTestRooms.Count == 0) return false;

            firstIteration = false;
        }
    }

    public void createConnection(Room room)
    {
        float distance = Vector2.Distance(position, room.position); 

        connectedRooms.Add(room);
        connectionDistance.Add(distance);

        room.connectedRooms.Add(this);
        room.connectionDistance.Add(distance);
    }

    public void removeConnection(Room room)
    {
        for (int i = 0; i < connectedRooms.Count; i++)
        {
            if (connectedRooms[i] == room) 
            {
                connectedRooms.RemoveAt(i);
                connectionDistance.RemoveAt(i);

                for (int j = 0; j < room.connectedRooms.Count; j++)
                {
                    if (room.connectedRooms[j] == this)
                    {
                        room.connectedRooms.RemoveAt(j);
                        room.connectionDistance.RemoveAt(j);
                    }
                }

                return;
            }
        }
    }
}

public class Connection
{
    public Room room1;
    public Room room2;

    public Connection(Room _room1, Room _room2)
    {
        room1 = _room1;
        room2 = _room2;
    }

}