using System.Collections.Generic;
using UnityEngine;

public static class ProceduralMeshGenerator
{
    public static Mesh GeneratePieceMesh(PieceData pieceData, float cellSize = 1f)
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        foreach (var cell in pieceData.cells)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.y < minY) minY = cell.y;
        }
        List<Vector2Int> normalizedCells = new List<Vector2Int>();
        foreach (var cell in pieceData.cells)
        {
            normalizedCells.Add(new Vector2Int(cell.x - minX, cell.y - minY));
        }

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int vertCount = 0;
        foreach (var cell in normalizedCells)
        {

            Vector3 v0 = new Vector3(cell.x * cellSize, cell.y * cellSize, 0);
            Vector3 v1 = new Vector3((cell.x + 1) * cellSize, cell.y * cellSize, 0);
            Vector3 v2 = new Vector3((cell.x + 1) * cellSize, (cell.y + 1) * cellSize, 0);
            Vector3 v3 = new Vector3(cell.x * cellSize, (cell.y + 1) * cellSize, 0);
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            triangles.Add(vertCount + 0);
            triangles.Add(vertCount + 1);
            triangles.Add(vertCount + 2);
            triangles.Add(vertCount + 0);
            triangles.Add(vertCount + 2);
            triangles.Add(vertCount + 3);

            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));
            vertCount += 4;
        }
        Mesh mesh = new Mesh();
        mesh.name = "PieceMesh_" + pieceData.id;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
