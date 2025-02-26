using System.Collections.Generic;
using UnityEngine;

public enum Difficulty { Easy, Medium, Hard }

public static class LevelGenerator
{
    public static LevelData GenerateLevel(Difficulty difficulty)
    {
        LevelData levelData = new LevelData();
        int boardSize = 0;
        int desiredPieceCount = 0;
        int minPieceSize = 3;
        int maxPieceSize = 6;
        switch (difficulty)
        {
            case Difficulty.Easy:
                boardSize = 4;
                desiredPieceCount = 5;
                break;
            case Difficulty.Medium:
                boardSize = 5;
                desiredPieceCount = 8;
                break;
            case Difficulty.Hard:
                boardSize = 6;
                desiredPieceCount = 12;
                break;
        }
        levelData.boardSize = boardSize;
        levelData.pieces = CreatePiecesBFS(boardSize, desiredPieceCount, minPieceSize, maxPieceSize);
        return levelData;
    }

    private static List<PieceData> CreatePiecesBFS(int boardSize, int desiredPieceCount, int minPieceSize, int maxPieceSize)
    {
        List<PieceData> pieces = new List<PieceData>();
        int[,] board = new int[boardSize, boardSize];

        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                board[x, y] = -1;
            }
        }
        int totalCells = boardSize * boardSize;
        int assignedCells = 0;
        int pieceId = 0;
        while (assignedCells < totalCells && pieces.Count < desiredPieceCount)
        {
            int remainingCells = totalCells - assignedCells;
            int targetSize = Random.Range(minPieceSize, maxPieceSize + 1);
            if (targetSize > remainingCells) targetSize = remainingCells;
            Vector2Int start = FindRandomEmptyCell(board, boardSize);
            if (start.x < 0) break;
            List<Vector2Int> pieceCells = BFSExpand(board, boardSize, start, targetSize);

            foreach (var cell in pieceCells)
            {
                board[cell.x, cell.y] = pieceId;
            }
            assignedCells += pieceCells.Count;
            PieceData pd = new PieceData();
            pd.id = pieceId;
            pd.cells = pieceCells;
            pd.color = "#" + ColorUtility.ToHtmlStringRGB(Random.ColorHSV());
            pieces.Add(pd);
            pieceId++;
        }
        return pieces;
    }

    private static Vector2Int FindRandomEmptyCell(int[,] board, int boardSize)
    {
        List<Vector2Int> empties = new List<Vector2Int>();
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                if (board[x, y] == -1)
                {
                    empties.Add(new Vector2Int(x, y));
                }
            }
        }
        if (empties.Count == 0) return new Vector2Int(-1, -1);
        return empties[Random.Range(0, empties.Count)];
    }

    private static List<Vector2Int> BFSExpand(int[,] board, int boardSize, Vector2Int start, int targetSize)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        List<Vector2Int> result = new List<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(start);
        visited.Add(start);
        while (queue.Count > 0 && result.Count < targetSize)
        {
            Vector2Int current = queue.Dequeue();
            result.Add(current);
            if (result.Count >= targetSize) break;
            List<Vector2Int> neighbors = GetNeighbors(current, boardSize);
            Shuffle(neighbors);
            foreach (var n in neighbors)
            {
                if (!visited.Contains(n) && board[n.x, n.y] == -1)
                {
                    visited.Add(n);
                    queue.Enqueue(n);
                }
            }
        }
        return result;
    }

    private static List<Vector2Int> GetNeighbors(Vector2Int cell, int boardSize)
    {
        List<Vector2Int> neigh = new List<Vector2Int>();
        if (cell.x > 0) neigh.Add(new Vector2Int(cell.x - 1, cell.y));
        if (cell.x < boardSize - 1) neigh.Add(new Vector2Int(cell.x + 1, cell.y));
        if (cell.y > 0) neigh.Add(new Vector2Int(cell.x, cell.y - 1));
        if (cell.y < boardSize - 1) neigh.Add(new Vector2Int(cell.x, cell.y + 1));
        return neigh;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[r];
            list[r] = temp;
        }
    }
}
