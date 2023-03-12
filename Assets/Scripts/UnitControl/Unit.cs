using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public float speed = 1f;
    public float turnSpeed = 3f;
    public float turnDst = 0f;
    public float stoppingDst = 0f;
    Path path;

    int pathIndex;
    public bool followingPath = false;
    public LayerMask unitMask;

    Node node;
    Vector3 target;

    public int nodeUnitPanelty = 50;

    Vector3 posOnCollision;
    bool backToPos = false;

    void Start() { 
        UnitSelections.Instance.unitList.Add(this);
        AddPenaltyToCurrentNode(Grid.GetNodeFromWorldPosition(transform.position));
    }
    void Update() {
        if (backToPos) {
            transform.position = Vector3.Lerp(transform.position, posOnCollision, Time.deltaTime);
            if (Vector3.Distance(transform.position, posOnCollision) < 0.001f) backToPos = false;
        }
    }
    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful, string ID) { 
        if (pathSuccessful) { 
            path = new Path(waypoints, transform.position, turnDst, stoppingDst, ID);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }
    public void SetTarget(Vector3 target) { this.target = target; }
    public void CreatePathRequest(string ID) {
        if (Grid.GetNodeFromWorldPosition(transform.position) == Grid.GetNodeFromWorldPosition(target)) return;
        PathRequestManager.RequestPath(new PathRequest(transform.position, target, OnPathFound, ID)); 
    }
    IEnumerator FollowPath() {
        followingPath = true;
        backToPos = false;
        pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);
        float speedPercent = 1;

        float nodeCheckTimer = Grid.nodeDiameter / speed;

        while (followingPath) {
            
            #region Cross Line Check

                Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
                if (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D)) { if (pathIndex == path.finishLineIndex) followingPath = false; else pathIndex++; }

                #endregion

            if (!followingPath) break;

            #region Slowdown Check

            if (pathIndex >= path.slowDownIndex && stoppingDst > 0) {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
                    if (speedPercent < 0.001f) followingPath = false;
                }

            #endregion

            #region Movement And Rotation

                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
                #endregion

            #region Node Update

            if (nodeCheckTimer <= 0) {
                Node newNode = Grid.GetNodeFromWorldPosition(transform.position);
                if (node != newNode) AddPenaltyToCurrentNode(newNode);
            }
            else nodeCheckTimer -= Time.deltaTime;

            #endregion
            
            yield return null;
        }
        posOnCollision = transform.position;
    }
    void AddPenaltyToCurrentNode(Node newNode) {
        if (node != null) {
            node.hasUnit = false;
            node.movementPenalty -= nodeUnitPanelty;
        }
        node = newNode;
        node.hasUnit = true;
        node.movementPenalty += nodeUnitPanelty;
    }
    void OnDrawGizmos() { if (path != null && followingPath)  path.DrawWithGizmos(target); }

    private void OnDestroy() { UnitSelections.Instance.unitList.Remove(this); UnitSelections.Instance.unitsSelected.Remove(this); }

    void OnCollisionExit(Collision collision) {
        if (!followingPath) backToPos = true;
    }
}
