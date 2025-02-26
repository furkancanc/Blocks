using UnityEngine;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    public Difficulty difficulty = Difficulty.Easy;
    public float cellSize = 1f;
    public Transform piecesParent;

    void Start()
    {
        LevelData levelData = LevelGenerator.GenerateLevel(difficulty);
        SpawnPieces(levelData);
    }

    void SpawnPieces(LevelData levelData)
    {
        foreach (var pieceData in levelData.pieces)
        {
            GameObject pieceObj = new GameObject("Piece_" + pieceData.id);
            pieceObj.transform.parent = piecesParent;

            MeshFilter mf = pieceObj.AddComponent<MeshFilter>();
            MeshRenderer mr = pieceObj.AddComponent<MeshRenderer>();
            mf.mesh = ProceduralMeshGenerator.GeneratePieceMesh(pieceData, cellSize);

            Material mat = new Material(Shader.Find("Standard"));
            if (ColorUtility.TryParseHtmlString(pieceData.color, out Color col))
                mat.color = col;
            mr.material = mat;

            MeshCollider mc = pieceObj.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.mesh;

            pieceObj.AddComponent<Draggable>();

            pieceObj.transform.position = new Vector3(Random.Range(0, levelData.boardSize), Random.Range(levelData.boardSize + 1, levelData.boardSize + 3), 0);
        }
    }
}
