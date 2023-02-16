using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public float speed = 1f;
    public float turnSpeed = 3f;
    public float turnDst = 0f;
    public float stoppingDst = 0f;
    Path path;

    public int unitNodePenalty;

    int pathIndex;
    public bool followingPath;
    public LayerMask unitMask;
    public float raycastDistance = .2f;

    Node node;
    Vector3 target;

    bool wait = false;

    void Start() { 
        UnitSelections.Instance.unitList.Add(this);
        AddPenaltyToCurrentNode(Grid.GetNodeFromWorldPosition(transform.position));
    }
    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful) { 
        if (pathSuccessful) { 
            path = new Path(waypoints, transform.position, turnDst, stoppingDst);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }
    public void SetTarget(Vector3 target) { this.target = target; }
    public void CreatePathRequest() { PathRequestManager.RequestPath(new PathRequest(transform.position, target, OnPathFound)); }
    IEnumerator FollowPath() {
        followingPath = true;
        pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);
        float speedPercent = 1;

        float nodeCheckTimer = Grid.nodeDiameter / speed;

        while (followingPath) {

            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            if (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D)) { if (pathIndex == path.finishLineIndex) followingPath = false; else pathIndex++; }

            if (!followingPath) break;

            if (pathIndex >= path.slowDownIndex && stoppingDst > 0) {
                speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
                if (speedPercent < 0.001f) followingPath = false;
            }

            Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
            transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);

            if (nodeCheckTimer <= 0) {
                Node newNode = Grid.GetNodeFromWorldPosition(transform.position);
                if (node != newNode) AddPenaltyToCurrentNode(newNode);
                nodeCheckTimer = Grid.nodeDiameter / speed;
            } else nodeCheckTimer -= Time.deltaTime;
            
            if (wait) yield return new WaitForSeconds(Grid.nodeDiameter / speed);

            yield return null;
        }
        path = null;
    }
    public int GetRemainLookPoint() { 
        if (path != null) return path.finishLineIndex - pathIndex;
        return -1;
    }
    void AddPenaltyToCurrentNode(Node newNode) {
        if (node != null) {
            node.movementPenalty -= unitNodePenalty * 5;
        }
        node = newNode;
        node.movementPenalty += unitNodePenalty * 5;
    }
    void OnDrawGizmos() { if (path != null && followingPath)  path.DrawWithGizmos(target); }

    private void OnDestroy() { UnitSelections.Instance.unitList.Remove(this); UnitSelections.Instance.unitsSelected.Remove(this); }

    private void OnTriggerEnter(Collider other)
    {
        if (!followingPath) return;
        Unit cUnit = other.GetComponent<Unit>();
        if (cUnit.followingPath) { if (cUnit.GetRemainLookPoint() < GetRemainLookPoint()) wait = true; }
        else if (cUnit.GetRemainLookPoint() == 0) followingPath = false;
    }

    private void OnTriggerExit(Collider other) { wait = false; }
}
