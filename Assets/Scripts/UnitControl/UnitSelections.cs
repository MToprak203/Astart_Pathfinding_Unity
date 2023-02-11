using System.Collections.Generic;
using UnityEngine;

public class UnitSelections
{
    public List<Unit> unitList = new List<Unit>();
    public List<Unit> unitsSelected = new List<Unit>();

    static UnitSelections instance = new UnitSelections();
    public static UnitSelections Instance { get { return instance; } }

    public void ClickSelect(Unit unit) {
        DeselectAll();
        unitsSelected.Add(unit);
        unit.GetComponent<Renderer>().material.color = Color.red;
    }
    
    public void ShiftClickSelect(Unit unit) {
        if (!unitsSelected.Contains(unit)) { unitsSelected.Add(unit); unit.GetComponent<Renderer>().material.color = Color.red; }
        else { unitsSelected.Remove(unit); unit.GetComponent<Renderer>().material.color = Color.white; }
    }

    public void DragSelect(Unit unit) { 
        if (!unitsSelected.Contains(unit)) {
            unitsSelected.Add(unit);
            unit.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    public void DeselectAll() {
        foreach (Unit unit in unitsSelected) unit.GetComponent<Renderer>().material.color = Color.white;
        unitsSelected.Clear();
    }
}
