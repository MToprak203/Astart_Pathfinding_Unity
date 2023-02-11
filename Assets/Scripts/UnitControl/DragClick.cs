using UnityEngine;

public class DragClick : MonoBehaviour
{
    Camera cam;
    [SerializeField]
    RectTransform boxVisual;

    Rect selectionBox;

    Vector2 startPos;
    Vector2 endPos;

    void Start() {
        cam = Camera.main;
        startPos = Vector2.zero;
        endPos = Vector2.zero;
        DrawVisual();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) { 
            startPos = Input.mousePosition;
            selectionBox = new Rect();
        }

        if (Input.GetMouseButton(0)) { 
            endPos = Input.mousePosition;
            DrawVisual();
            DrawSelection();
        }

        if (Input.GetMouseButtonUp(0)) {
            SelectUnits();
            startPos = Vector2.zero;
            endPos = Vector2.zero;
            DrawVisual();
        }
    }

    void DrawVisual() {
        Vector2 boxStart = startPos;
        Vector2 boxEnd = endPos;

        Vector2 boxCenter = (boxStart + boxEnd) / 2;
        boxVisual.position = boxCenter;

        Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));

        boxVisual.sizeDelta = boxSize;
    }

    void DrawSelection() { 
    
        if (Input.mousePosition.x < startPos.x) {
            // Dragging left
            selectionBox.xMin = Input.mousePosition.x;
            selectionBox.xMax = startPos.x;
        }
        else {
            // Dragging right
            selectionBox.xMin = startPos.x;
            selectionBox.xMax = Input.mousePosition.x;
        }

        if (Input.mousePosition.y < startPos.y) {
            // Dragging down
            selectionBox.yMin = Input.mousePosition.y;
            selectionBox.yMax = startPos.y;
        }
        else {
            // Dragging up
            selectionBox.yMin = startPos.y;
            selectionBox.yMax = Input.mousePosition.y;
        }
    }

    void SelectUnits() { 
        foreach (Unit unit in UnitSelections.Instance.unitList) 
            if (selectionBox.Contains(cam.WorldToScreenPoint(unit.transform.position))) 
                UnitSelections.Instance.DragSelect(unit); 
    }
}
