using System;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;

public class PathRequestManager : MonoBehaviour {

    Queue<PathResult> results = new Queue<PathResult>();

    static PathRequestManager instance;
    Pathfinding pathfinding;

    void Awake() { instance = this; pathfinding = GetComponent<Pathfinding>(); }
    void Update() {
        if (results.Count > 0) {
            int itemsInQueue = results.Count;
            lock (results) {
                for (int i = 0; i < itemsInQueue; i++) {
                    PathResult result = results.Dequeue();
                    result.callback(result.path, result.success, result.ID);
                }
            }
        }
    }
    public static void RequestPath(PathRequest request) {
        ThreadStart threadStart = delegate { instance.pathfinding.FindPath(request, instance.FinishedProcessingPath); };
        threadStart.Invoke();
    }

    public void FinishedProcessingPath(PathResult result) { lock (results) { results.Enqueue(result); } }
}

public struct PathRequest
{
    public Vector3 pathStart;
    public Vector3 pathEnd;
    public Action<Vector3[], bool, string> callback;
    public string ID;

    public PathRequest(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool, string> callback, string ID)
    {
        this.pathStart = pathStart;
        this.pathEnd = pathEnd;
        this.callback = callback;
        this.ID = ID;
    }
}

public struct PathResult
{
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool, string> callback;
    public string ID;
    public PathResult(Vector3[] path, bool success, Action<Vector3[], bool, string> callback, string ID)
    {
        this.path = path;
        this.success = success;
        this.callback = callback;
        this.ID = ID;
    }
}