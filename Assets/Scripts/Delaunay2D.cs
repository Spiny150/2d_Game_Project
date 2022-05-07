using UnityEngine;
using System.Collections.Generic;

using HullDelaunayVoronoi.Delaunay;
using HullDelaunayVoronoi.Primitives;

namespace HullDelaunayVoronoi
{

    public class Delaunay2D : MonoBehaviour
    {

        private DelaunayTriangulation2 delaunay;
        

        public void Draw(List<Vector3Int> rooms)
        {

            List<Vertex2> vertices = new List<Vertex2>();
            foreach (Vector3Int room in rooms)
            {
                vertices.Add(new Vertex2(room.x, room.y));
            }

            delaunay = new DelaunayTriangulation2();
            delaunay.Generate(vertices);

            foreach (DelaunayCell<Vertex2> cell in delaunay.Cells)
            {
                DrawSimplex(cell.Simplex);
            }
        }

        private void DrawSimplex(Simplex<Vertex2> f)
        {

            Debug.DrawLine(new Vector3(f.Vertices[0].X, f.Vertices[0].Y, 0.0f), new Vector3(f.Vertices[1].X, f.Vertices[1].Y, 0.0f), Color.red, duration:1);
            Debug.DrawLine(new Vector3(f.Vertices[0].X, f.Vertices[0].Y, 0.0f), new Vector3(f.Vertices[2].X, f.Vertices[2].Y, 0.0f), Color.red, duration:1);
            Debug.DrawLine(new Vector3(f.Vertices[1].X, f.Vertices[1].Y, 0.0f), new Vector3(f.Vertices[2].X, f.Vertices[2].Y, 0.0f), Color.red, duration:1);
            

        }


    }

}



















