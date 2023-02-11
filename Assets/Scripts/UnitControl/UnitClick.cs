using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitClick : MonoBehaviour
{
    private Camera cam;
    public LayerMask unitMask;
    public LayerMask ground;

    bool abortCoroutine;
    #region SquareFormation

    [SerializeField, Range(1f, 5f)] float spread = 1;
    [SerializeField, Range(0f, 10f)] float noise = 0;

    IEnumerator SquareFormation(Vector3 target, List<Unit> selectedUnits) {
        yield return new WaitForEndOfFrame();
        abortCoroutine = false;
        int size = selectedUnits.Count;
        int unitIndex = 0;
        int length = Mathf.FloorToInt(Mathf.Sqrt(size)) + 1;
        Debug.Log(length);
        var middleOffset = new Vector3(length * .5f, 0, length * .5f);

        for (int x = 0; x < length; x++) {
            for (int z = 0; z < length; z++) {
                if (abortCoroutine) yield break;
                Vector3 pos = new Vector3(x, 0, z);
                pos = (pos - middleOffset) * spread * Grid.nodeDiameter + target;
                pos += GetNoise(pos);
                Unit unit = selectedUnits[unitIndex];
                unit.SetTarget(pos);
                unit.CreatePathRequest();
                unitIndex++;
                if (unitIndex >= size) yield break;
                yield return new WaitForSeconds(.01f);
            }
        }
        
        
    }
    #endregion

    void Start() { cam = Camera.main; }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, unitMask)) {
                if (Input.GetKey(KeyCode.LeftShift)) UnitSelections.Instance.ShiftClickSelect(hit.collider.GetComponent<Unit>());
                else UnitSelections.Instance.ClickSelect(hit.collider.GetComponent<Unit>());
            }
            else { if (Input.GetKey(KeyCode.LeftShift)) UnitSelections.Instance.DeselectAll(); }
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground)) {
                Vector3 target = hit.point;
                abortCoroutine = true;
                StartCoroutine(SquareFormation(target, UnitSelections.Instance.unitsSelected));
            }
        }
    }

    Vector3 GetNoise(Vector3 pos) { 
        float _noise = Mathf.PerlinNoise(pos.x * noise, pos.y * noise);
        return new Vector3(_noise, 0, _noise);
    }

}
