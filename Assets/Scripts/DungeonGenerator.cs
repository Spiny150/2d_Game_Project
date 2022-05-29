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

    List<Room> rooms = new List<Room>();


    private void FillWalls()
    {
        BoundsInt bounds = groundMap.cellBounds;
        
        for (int xMap = bounds.xMin - 10; xMap <= bounds.xMax + 10; xMap++)
        {
            for (int yMap = bounds.yMin - 10; yMap <= bounds.yMax + 10; yMap++)
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


    public void GenerateRoutes(List<Room> listRoom)
    {

        for (int i = 0; i < listRoom.Count; i++)
        {
            for (int j = 0; j < listRoom[i].connectedRooms.Count; j++)
            {
                Room R1 = listRoom[i];
                Room R2 = listRoom[i].connectedRooms[j];

                Vector2Int midPoint = (R1.position + R2.position) / 2;

                //check for y axis (vertical) corridor
                if ((R1.position.x - R1.size.x + 1 <= midPoint.x && midPoint.x <= R1.position.x + R1.size.x - 1) && (R2.position.x - R2.size.x + 1 <= midPoint.x && midPoint.x <= R2.position.x + R2.size.x - 1))
                {
                    int dir = Mathf.Clamp(R2.position.y - R1.position.y, -1, 1);
                    for (int height = 0; height < Mathf.Abs(R1.position.y - R2.position.y); height++)
                    {
                        GenerateSquare(midPoint.x, R1.position.y + dir * height, new Vector2Int(1, 1));
                    }
                    R1.removeConnection(R2);
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
                    R1.removeConnection(R2);
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
                R1.removeConnection(R2);

                
            }
        }
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


    public void ClearMap()
    {
        rooms.Clear();
        BoundsInt bounds = groundMap.cellBounds;
        
        for (int xMap = bounds.xMin - 10; xMap <= bounds.xMax + 10; xMap++)
        {
            for (int yMap = bounds.yMin - 10; yMap <= bounds.yMax + 10; yMap++)
            {
                Vector3Int pos = new Vector3Int(xMap, yMap, 0);
                
                groundMap.SetTile(pos, null);
                wallMap.SetTile(pos, null);
            }
        }
    }

    // void OnDrawGizmos() {

    //     foreach (Room room in rooms)
    //     {
    //         Handles.color = Color.red; 
    //         Handles.Label(new Vector2(room.position.x, room.position.y + 20), room.ID.ToString());
    //     }
        
    // }


    public void RandomRooms()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2Int size = new Vector2Int(Random.Range(6, 15), Random.Range(6, 15));

            Vector2Int randomPos = new Vector2Int(Random.Range(-80, 80), Random.Range(-80, 80));
            if (rooms.Count == 0)
            {
                rooms.Add(new Room(_position:randomPos, _size:size, _ID:i+1));
                GenerateSquare(randomPos.x, randomPos.y, size);
                continue;
            }

            int iteration = 0;

            bool checkForRoomPos = true;
            
            while(checkForRoomPos)
            {
                restart:
                iteration++;
                randomPos = new Vector2Int(Random.Range(-80, 80), Random.Range(-80, 80));

                
                if (iteration >= 1000) goto noMoreRoom;


                foreach(Room room in rooms)
                {
                    int xDist = Mathf.Abs(randomPos.x - room.position.x);
                    int yDist = Mathf.Abs(randomPos.y - room.position.y);

                    if (xDist - size.x - room.size.x <= 5 && yDist - size.y - room.size.y <= 5 ) goto restart;

                }

                rooms.Add(new Room(_position:randomPos, _size:size, _ID:i+1));
                GenerateSquare(randomPos.x, randomPos.y, size);
                break;
            }
            
        }
        noMoreRoom:

        List<Vertex2> vertices = new List<Vertex2>();
        foreach (Room room in rooms)
        {
            vertices.Add(new Vertex2(room.position.x, room.position.y));
        }

        delaunay = new DelaunayTriangulation2();
        delaunay.Generate(vertices);

        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = 0; j < delaunay.Cells.Count; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    if (rooms[i].position == new Vector2Int((int) delaunay.Cells[j].Simplex.Vertices[k].X, (int) delaunay.Cells[j].Simplex.Vertices[k].Y))
                    {

                        Vector2Int otherRoomCo1 = new Vector2Int((int) delaunay.Cells[j].Simplex.Vertices[Loop(k + 1, 0, 2)].X, (int) delaunay.Cells[j].Simplex.Vertices[Loop(k + 1, 0, 2)].Y);
                        Vector2Int otherRoomCo2 = new Vector2Int((int) delaunay.Cells[j].Simplex.Vertices[Loop(k + 2, 0, 2)].X, (int) delaunay.Cells[j].Simplex.Vertices[Loop(k + 2, 0, 2)].Y);

                        for (int l = 0; l < rooms.Count; l++)
                        {
                            if (rooms[l].position == otherRoomCo1 || rooms[l].position == otherRoomCo2)
                            {
                                if (!rooms[i].connectedRooms.Contains(rooms[l])) 
                                {
                                    rooms[i].createConnection(rooms[l]);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void Update() {
        if (delaunay != null)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                for (int j = 0; j < rooms[i].connectedRooms.Count; j++)
                {
                    Debug.DrawLine((Vector3Int) rooms[i].position, (Vector3Int) rooms[i].connectedRooms[j].position, color:Color.red);                                
                }
            }
        }
    }


    public int Loop(int value, int minValue, int maxValue)
    {
        while (!(minValue <= value && value <= maxValue))
        {
            if (value > maxValue) value -= maxValue + 1;
            if (value < minValue) value += maxValue + 1;
        }
        return value;
    }

    public void GenerateMinimumSpanningTree()
    {
        bool foundTree = false;

        List<Connection> deletedRooms = new List<Connection>();
        while (!foundTree)
        {

            int? maxRoomIndex = null;
            int? connectedRoomsIndex = null;
            float maxDistance = 0;

            for (int i = 0; i < rooms.Count; i++)
            {
                for (int j = 0; j < rooms[i].connectedRooms.Count; j++)
                {
                    if (rooms[i].canAccess(rooms[i].connectedRooms[j]) && rooms[i].connectionDistance[j] > maxDistance)
                    {
                        maxDistance = rooms[i].connectionDistance[j];
                        maxRoomIndex = i;
                        connectedRoomsIndex = j;
                    }
                }                
            }

            if (maxRoomIndex == null)
            {
                foreach (Connection connection in deletedRooms)
                {
                    if (Random.Range(0, 100) < 10)
                    connection.room1.createConnection(connection.room2);
                }
                foundTree = true;
                break;
            }

            //deletedRooms.Add(new Connection(_room1:rooms[(int) maxRoomIndex], _room2:rooms[(int) maxRoomIndex].connectedRooms[(int) connectedRoomsIndex]));
            rooms[(int) maxRoomIndex].removeConnection(rooms[(int) maxRoomIndex].connectedRooms[(int) connectedRoomsIndex]);


        }

        GenerateRoutes(rooms);
        GenerateRoutes(rooms);
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