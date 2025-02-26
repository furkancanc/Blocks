using System.Collections.Generic;
using UnityEngine;

public static class ProceduralMeshGenerator
{
    /// <summary>
    /// Parçanın hücrelerini kullanarak dış konturu çıkarır ve ear clipping yöntemiyle mesh oluşturur.
    /// </summary>
    public static Mesh GeneratePieceMeshFromCells(PieceData pieceData, float cellSize = 1f)
    {
        // 1. Hücrelerin dış konturunu (outline) çıkarıyoruz.
        List<Vector2> outline = GetOutline(pieceData.cells);
        if (outline.Count < 3)
        {
            Debug.LogWarning("Outline yeterince nokta içermiyor!");
            return new Mesh();
        }

        // 2. CellSize ölçeğinde çeviriyoruz (örneğin grid hücreleri 1 birimse, cellSize ile çarpıyoruz)
        for (int i = 0; i < outline.Count; i++)
        {
            outline[i] *= cellSize;
        }

        // 3. Outline'ı triangüle ediyoruz
        List<int> triangles = TriangulatePolygon(outline);

        // 4. Mesh için vertex listesi oluşturuyoruz (z = 0)
        Vector3[] vertices = new Vector3[outline.Count];
        for (int i = 0; i < outline.Count; i++)
        {
            vertices[i] = new Vector3(outline[i].x, outline[i].y, 0);
        }

        Mesh mesh = new Mesh();
        mesh.name = "PieceMesh_" + pieceData.id;
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    #region Outline Extraction

    // Küçük bir segment yapısı: her segment iki nokta içerir.
    private class Segment
    {
        public Vector2 start;
        public Vector2 end;
        public Segment(Vector2 s, Vector2 e)
        {
            start = s;
            end = e;
        }
    }

    /// <summary>
    /// Verilen hücre grubundan dış kontur (outline) çıkarır.
    /// Her hücre, (x,y) noktasından (x+1,y+1) noktasına bir kareyi temsil eder.
    /// Hücre kenarı, komşu hücre yoksa outline’a eklenir.
    /// </summary>
    private static List<Vector2> GetOutline(List<Vector2Int> cells)
    {
        HashSet<Vector2Int> cellSet = new HashSet<Vector2Int>(cells);
        List<Segment> segments = new List<Segment>();

        foreach (var cell in cellSet)
        {
            int x = cell.x, y = cell.y;
            // Üst kenar: (x, y+1) -> (x+1, y+1) (eğer yukarıdaki hücre yoksa)
            if (!cellSet.Contains(new Vector2Int(x, y + 1)))
            {
                segments.Add(new Segment(new Vector2(x, y + 1), new Vector2(x + 1, y + 1)));
            }
            // Sağ kenar: (x+1, y+1) -> (x+1, y) (eğer sağdaki hücre yoksa)
            if (!cellSet.Contains(new Vector2Int(x + 1, y)))
            {
                segments.Add(new Segment(new Vector2(x + 1, y + 1), new Vector2(x + 1, y)));
            }
            // Alt kenar: (x+1, y) -> (x, y) (eğer alttaki hücre yoksa)
            if (!cellSet.Contains(new Vector2Int(x, y - 1)))
            {
                segments.Add(new Segment(new Vector2(x + 1, y), new Vector2(x, y)));
            }
            // Sol kenar: (x, y) -> (x, y+1) (eğer soldaki hücre yoksa)
            if (!cellSet.Contains(new Vector2Int(x - 1, y)))
            {
                segments.Add(new Segment(new Vector2(x, y), new Vector2(x, y + 1)));
            }
        }

        // Şimdi segmentleri sıralı bir outline oluşturacak şekilde birleştiriyoruz.
        List<Vector2> outline = OrderSegments(segments);
        return outline;
    }

    /// <summary>
    /// Segment listesini, bitişik noktaları eşleştirerek sıralı bir outline (kapalı poligon) haline getirir.
    /// </summary>
    private static List<Vector2> OrderSegments(List<Segment> segments)
    {
        List<Vector2> ordered = new List<Vector2>();
        if (segments.Count == 0)
            return ordered;

        // İlk segmenti seçelim.
        Segment current = segments[0];
        ordered.Add(current.start);
        ordered.Add(current.end);
        segments.RemoveAt(0);

        // Kalan segmentleri, son eklenen noktaya bağlanana kadar ekle.
        while (segments.Count > 0)
        {
            bool found = false;
            for (int i = 0; i < segments.Count; i++)
            {
                Segment seg = segments[i];
                if (ApproximatelyEqual(seg.start, ordered[ordered.Count - 1]))
                {
                    ordered.Add(seg.end);
                    segments.RemoveAt(i);
                    found = true;
                    break;
                }
                else if (ApproximatelyEqual(seg.end, ordered[ordered.Count - 1]))
                {
                    ordered.Add(seg.start);
                    segments.RemoveAt(i);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                // Eğer bağlı segment bulunamazsa, outline tamamlanmamış olabilir.
                break;
            }
        }
        return ordered;
    }

    private static bool ApproximatelyEqual(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b) < 0.01f;
    }

    #endregion

    #region Triangulation (Ear Clipping)

    /// <summary>
    /// Basit ear clipping yöntemiyle verilen poligonun triangülasyonunu yapar.
    /// </summary>
    private static List<int> TriangulatePolygon(List<Vector2> vertices)
    {
        List<int> indices = new List<int>();
        int n = vertices.Count;
        if (n < 3)
            return indices;

        // Poligonun köşe indekslerini içeren liste (varsayıyoruz ki outline sıralıdır)
        List<int> V = new List<int>();
        for (int i = 0; i < n; i++)
            V.Add(i);

        int count = 0;
        while (V.Count > 3)
        {
            bool earFound = false;
            for (int i = 0; i < V.Count; i++)
            {
                int prev = V[(i - 1 + V.Count) % V.Count];
                int curr = V[i];
                int next = V[(i + 1) % V.Count];

                if (IsEar(prev, curr, next, vertices, V))
                {
                    indices.Add(prev);
                    indices.Add(curr);
                    indices.Add(next);
                    V.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }
            if (!earFound)
                break; // Poligon hata içeriyor olabilir
            count++;
            if (count > 1000)
                break;
        }
        if (V.Count == 3)
        {
            indices.Add(V[0]);
            indices.Add(V[1]);
            indices.Add(V[2]);
        }
        return indices;
    }

    private static bool IsEar(int i, int j, int k, List<Vector2> vertices, List<int> V)
    {
        Vector2 a = vertices[i], b = vertices[j], c = vertices[k];
        if (Area(a, b, c) >= 0) // Saat yönünün tersinde olmalı (veya sizin poligonunuzun yönüne göre ayarlayın)
            return false;
        // Diğer noktaların, a,b,c üçgeni içinde olup olmadığını kontrol et
        for (int p = 0; p < V.Count; p++)
        {
            int vi = V[p];
            if (vi == i || vi == j || vi == k)
                continue;
            if (PointInTriangle(vertices[vi], a, b, c))
                return false;
        }
        return true;
    }

    private static float Area(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = Mathf.Abs(Area(a, b, c));
        float area1 = Mathf.Abs(Area(p, a, b));
        float area2 = Mathf.Abs(Area(p, b, c));
        float area3 = Mathf.Abs(Area(p, c, a));
        return Mathf.Abs(area - (area1 + area2 + area3)) < 0.01f;
    }

    #endregion
}
