using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    Grid grid;

    void Awake() { grid = GetComponent<Grid>(); }
    public void FindPath(PathRequest request, Action<PathResult> callback) {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = Grid.GetNodeFromWorldPosition(request.pathStart);
        Node targetNode = Grid.GetNodeFromWorldPosition(request.pathEnd);

        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0) {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode == targetNode) { pathSuccess = true; break; }
            
            foreach (Node neighbour in Grid.GetNeighbours(currentNode)) {
                if (closedSet.Contains(neighbour)) continue;

                int newMovCostToNeigh = currentNode.gCost + GetManhattanDistance(currentNode, neighbour) + neighbour.movementPenalty;
                if (newMovCostToNeigh < neighbour.gCost || !openSet.Contains(neighbour)) {
                    neighbour.gCost = newMovCostToNeigh;
                    neighbour.hCost = GetManhattanDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;
                    if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
                    else openSet.UpdateItem(neighbour);
                }
            }
        }
        
        if (pathSuccess) {
            waypoints = RetracePath(startNode, targetNode);
            pathSuccess = waypoints != null;
        }
        callback(new PathResult(waypoints, pathSuccess, request.callback, request.ID));
    }
    Vector3[] RetracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();
        Node currentNode = endNode.parent;
        while (currentNode != startNode) {
            if (currentNode.walkable) path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = null;
        if (path.Count > 0) waypoints = SimplifyPath(path);
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path) { 
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;
        for (int i = path.Count - 1; i > 0; i--) {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionOld != directionNew) waypoints.Add(path[i].worldPosition);
            directionOld = directionNew;
        }
        waypoints.Add(path[0].worldPosition);
        return waypoints.ToArray();

    }
    int GetManhattanDistance(Node A, Node B) {

        // diagonal distance = 14
        // horizontal or vertical distance = 10

        int dstX = Mathf.Abs(A.gridX - B.gridX);
        int dstY = Mathf.Abs(A.gridY - B.gridY);

        if (dstX > dstY) return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
