using UnityEngine;

public class Draggable : MonoBehaviour
{
    private Vector3 offset;
    private bool dragging = false;
    public float cellSize = 1f;

    void OnMouseDown()
    {
        dragging = true;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        offset = transform.position - mouseWorld;
    }

    void OnMouseDrag()
    {
        if (!dragging) return;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        transform.position = mouseWorld + offset;
    }

    void OnMouseUp()
    {
        dragging = false;
        Vector3 pos = transform.position;
        float snapX = Mathf.Round(pos.x / cellSize) * cellSize;
        float snapY = Mathf.Round(pos.y / cellSize) * cellSize;
        transform.position = new Vector3(snapX, snapY, pos.z);
    }
}
