using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PieceData : MonoBehaviour
{
    public int id;
    public List<Vector2Int> cells;
    public string color;
}

