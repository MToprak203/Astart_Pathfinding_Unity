using UnityEngine;

public struct Line {
    const float verticalLineGradient = 1e5f;
    float gradient;
    float y_intecept;
    Vector2 pointOnLine_1;
    Vector2 pointOnLine_2;

    float gradientPerpendicular;

    bool approachSide;

    public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine) {
        float dx = pointOnLine.x - pointPerpendicularToLine.x;
        float dy = pointOnLine.y - pointPerpendicularToLine.y;

        if (dx == 0) gradientPerpendicular = verticalLineGradient;
        else gradientPerpendicular = dy / dx;

        if (gradientPerpendicular == 0) gradient = verticalLineGradient;
        else gradient = -1 / gradientPerpendicular;

        y_intecept = pointOnLine.y - gradient * pointOnLine.x;
        pointOnLine_1 = pointOnLine;
        pointOnLine_2 = pointOnLine + new Vector2(1, gradient);

        approachSide = false;
        approachSide = GetSide(pointPerpendicularToLine);
    }
    public float DistanceFromPoint(Vector2 point) {
        float yInterceptPerpendicular = point.y - gradientPerpendicular * point.x;
        float intersectX = (yInterceptPerpendicular - y_intecept) / (gradient - gradientPerpendicular);
        float intersectY = gradient * intersectX + y_intecept;
        return Vector2.Distance(point, new Vector2(intersectX, intersectY));
    }
    bool GetSide(Vector2 point) => (point.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) > (point.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);
    public bool HasCrossedLine(Vector2 point) => GetSide(point) != approachSide;
    public void DrawWithGizmos(float length) {
        Vector3 lineDir = new Vector3(1, 0, gradient).normalized;
        Vector3 lineCentre = new Vector3(pointOnLine_1.x, 0, pointOnLine_1.y);
        Gizmos.DrawLine(lineCentre - lineDir * length / 2f, lineCentre + lineDir * length / 2f);
    }
}
