using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public float speed = 1f;
    public float turnSpeed = 3f;
    Path path;

    public int unitNodePenalty;

    public bool followingPath;
    public LayerMask unitMask;
    public float raycastDistance = .2f;

    Node node;
    Vector3 target;

    void Start() { UnitSelections.Instance.unitList.Add(this); }
    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful) { 
        if (pathSuccessful) { 
            path = new Path(waypoints, transform.position);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }
    public void SetTarget(Vector3 target) { this.target = target; }
    public void CreatePathRequest() { PathRequestManager.RequestPath(new PathRequest(transform.position, target, OnPathFound)); }
    IEnumerator FollowPath() {
        followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[0]);

        float speedPercent = 1;

        while (followingPath) {

            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D)) { if (pathIndex == path.finishLineIndex) { followingPath = false; break; } else pathIndex++; }

            if (!followingPath) break;

            Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
            transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);

            Node newNode = Grid.GetNodeFromWorldPosition(transform.position);
            if (node != newNode) AddPenaltyToCurrentNode(newNode);

            #region Raycast

            Ray rayForward = new Ray(transform.position, transform.forward);
            RaycastHit hitForward;
            Debug.DrawRay(transform.position, transform.forward, Color.blue);

            if (Physics.Raycast(rayForward, out hitForward, raycastDistance, unitMask))
            {
                Debug.Log("forward hit");
                Unit cUnit = hitForward.transform.GetComponent<Unit>();
                if (cUnit.followingPath && !cUnit.CheckFaceToFace(GetInstanceID())) yield return new WaitForSeconds(Grid.nodeDiameter / speed);
                else
                {
                    cUnit.MarkTheCurrentNode();
                    followingPath = false;
                    CreatePathRequest();
                }
            }
            #endregion

            yield return null;
        }
    }
    bool CheckFaceToFace(int instanceID) => GetInstanceID() == instanceID;
    public void MarkTheCurrentNode() { StartCoroutine("MarkNode"); }
    IEnumerator MarkNode() {
        Node node = Grid.GetNodeFromWorldPosition(transform.position);
        node.movementPenalty += unitNodePenalty;
        yield return new WaitForSeconds(speed / 2);
        node.movementPenalty -= unitNodePenalty;
    }
    void AddPenaltyToCurrentNode(Node newNode) {
        if (node != null) node.movementPenalty -= unitNodePenalty * 5;
        node = newNode;
        node.movementPenalty += unitNodePenalty * 5;
    }
    void OnDrawGizmos() { if (path != null && followingPath)  path.DrawWithGizmos(target); }

    private void OnDestroy() { UnitSelections.Instance.unitList.Remove(this); }
}
