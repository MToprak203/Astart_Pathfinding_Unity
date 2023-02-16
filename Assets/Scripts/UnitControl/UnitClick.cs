using System.Collections;
using UnityEngine;

public class UnitClick : MonoBehaviour
{
    private Camera cam;
    public LayerMask unitMask;
    public LayerMask ground;

    Vector3 currentTarget;

    const float moveCD = 0.1f;
    float moveCDTimer = moveCD;

    #region SquareFormation

    [SerializeField, Range(1f, 5f)] float spread = 1;
    [SerializeField, Range(0f, 10f)] float noise = 0;

    IEnumerator SquareFormation() {
        yield return new WaitForEndOfFrame();
        int size = UnitSelections.Instance.unitsSelected.Count;
        int unitIndex = 0;
        int length = Mathf.FloorToInt(Mathf.Sqrt(size)) + 1;

        for (int x = -length / 2; x <= length / 2; x++) {
            for (int z = -length / 2; z <= length / 2; z++) {
                Vector3 pos = new Vector3(x, 0, z);
                pos = pos * Grid.nodeDiameter * spread + currentTarget;
                pos += GetNoise(pos);
                Unit unit = UnitSelections.Instance.unitsSelected[unitIndex];
                unit.SetTarget(pos);
                unit.CreatePathRequest();
                unitIndex++;
                if (unitIndex == size) yield break;
                yield return null;
            }
        }
    }
    #endregion

    void Start() { cam = Camera.main; }

    void Update() {

        moveCDTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, unitMask)) {
                if (Input.GetKey(KeyCode.LeftShift)) UnitSelections.Instance.ShiftClickSelect(hit.collider.GetComponent<Unit>());
                else UnitSelections.Instance.ClickSelect(hit.collider.GetComponent<Unit>());
            }
            else { if (Input.GetKey(KeyCode.LeftShift)) UnitSelections.Instance.DeselectAll(); }
        }

        if (moveCDTimer < 0f && UnitSelections.Instance.unitsSelected.Count > 0 && Input.GetMouseButtonDown(1)) {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground)) {
                currentTarget = hit.point;
                StopCoroutine("SquareFormation");
                StartCoroutine("SquareFormation");
                moveCDTimer = moveCD;
            }
        }
    }

    Vector3 GetNoise(Vector3 pos) { 
        float _noise = Mathf.PerlinNoise(pos.x * noise, pos.y * noise);
        return new Vector3(_noise, 0, _noise);
    }

}
