using UnityEngine;

public class Path {
    public readonly Vector3[] lookPoints;
    public readonly Line[] turnBoundaries;
    public readonly int finishLineIndex;
    public readonly int slowDownIndex;
    public readonly string ID;

    public Path(Vector3[] wayPoints, Vector3 startPos, float turnDst, float stoppingDst, string ID) { 
        lookPoints = wayPoints;
        turnBoundaries = new Line[lookPoints.Length];
        finishLineIndex = turnBoundaries.Length - 1;

        Vector2 previousPoint = V3ToV2(startPos);
        for (int i = 0; i < lookPoints.Length; i++) {
            Vector2 currentPoint = V3ToV2(lookPoints[i]);
            Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
            Vector2 turnBoundaryPoint = (i == finishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDst;
            turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDst);
            previousPoint = turnBoundaryPoint;
        }

        float dstFromEndPoint = 0;
        for (int i = lookPoints.Length - 1; i > 0; i--) {
            dstFromEndPoint += Vector3.Distance(lookPoints[i], lookPoints[i - 1]);
            if (dstFromEndPoint > stoppingDst) { slowDownIndex = i; break; }
        }

        this.ID = ID;
    }
    Vector2 V3ToV2(Vector3 v3) => new Vector2(v3.x, v3.z);

    public void DrawWithGizmos(Vector3 target) {
        Gizmos.color = Color.black;
        foreach (Vector3 p in lookPoints) Gizmos.DrawCube(p, Vector3.one * 0.1f);
        Gizmos.color = Color.blue; 
        foreach (Line l in turnBoundaries) l.DrawWithGizmos(2);
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(target, Vector3.one * 0.1f);
    }
}
