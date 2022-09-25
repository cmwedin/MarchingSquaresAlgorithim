using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell<TGridObject>
{
    private CustomGrid<TGridObject> parentGrid;
    private GridCell<TGridObject>[] neighbors;
    private TGridObject value;
    public Vector2Int Index { get; set; }

    // * Constructors
    public GridCell(Vector2Int index, CustomGrid<TGridObject> parent) {
        this.Index = index;
        this.parentGrid = parent;
        neighbors = new GridCell<TGridObject>[8];
    }
    
    // * Public Methods
    public TGridObject GetValue() {
        return value;
    }

    public void SetValue(TGridObject value) {
        this.value = value;
    }

    public Vector2 GetWorldPos() {
        return parentGrid.GetWorldPos(Index);
    }

    public void SetNeighbor(GridDirection direction, GridCell<TGridObject> cell)
    {
        //Debug.Log($"Setting {direction} neighbor of cell at {Index}");
        neighbors[(int)direction] = cell;
        if(cell.GetNeighbor(direction.Opposite()) != this) cell.SetNeighbor(direction.Opposite(), this);
    }
    //? returns a list of directions with which a condition is satisfied 
    public List<GridDirection> CheckNeighbors(Func<GridCell<TGridObject>,GridCell<TGridObject>,bool> Condition){
        List<GridDirection> output = new List<GridDirection>();
        foreach (int direction in Enum.GetValues(typeof(GridDirection))) { //? potentially more complicated then necessary
        if(Condition(this, neighbors[direction])) {
            output.Add((GridDirection)direction);
        }}
        return output;
    }

    public GridCell<TGridObject> GetNeighbor(GridDirection direction)
    {
        return neighbors[(int)direction];
    }
    
    //? returns a list of all non-null neighbors of the cell
    public List<GridCell<TGridObject>> GetAllNeighbors(){
        var output = new List<GridCell<TGridObject>>();
        foreach(int direction in Enum.GetValues(typeof(GridDirection))) {
            if(neighbors[direction] != null) output.Add(neighbors[direction]);
        }
        return output;
    }
    
    //? returns a list of the non-null neighbors of the cell in the U UR and R directions
    public List<GridCell<TGridObject>> GetForwardNeighbors(){ //! this function may be inefficiently implemented
        //Debug.Log($"getting forward neighbors of cell at {Index}");
        GridDirection direction = GridDirection.U;
        var output = new List<GridCell<TGridObject>>();
        while (direction < GridDirection.DR) {
            if (neighbors[(int)direction] != null) { 
                output.Add(neighbors[(int)direction]);
                //Debug.Log($"{direction} neighbor found");
            }
            direction = direction.Next();
        }
        return(output);
    }

    //?opposite of GetForwardNeighbors
    public List<GridCell<TGridObject>> GetBackwardNeighbors() {
        GridDirection direction = GridDirection.UL;
        var output = new List<GridCell<TGridObject>>();
        while (direction > GridDirection.R) {
            if (neighbors[(int)direction] != null) output.Add(neighbors[(int)direction]);
            direction = direction.Previous();
        }
        return(output);        
    }
    
    //? gets all non-null neighbors between two specified directions (inclusive or exclusive)
    public List<GridCell<TGridObject>> GetNeighborsBetween(
        GridDirection direction1, 
        GridDirection direction2, 
        bool inclusive = true
    ) {
        var output = new List<GridCell<TGridObject>>();
        bool clockwise = direction2 >= direction1;
        GridDirection direction = direction1;
        if(inclusive && neighbors[(int)direction1] != null) output.Add(neighbors[(int)direction1]);
        if(clockwise) {
            while(direction < direction2) {
                if (neighbors[(int)direction] != null) output.Add(neighbors[(int)direction]);
                direction = direction.Next();
            }
        } else {
            while(direction > direction2) {
                if (neighbors[(int)direction] != null) output.Add(neighbors[(int)direction]);
                direction = direction.Previous();
            }
        }
        if(direction != direction2) throw new System.Exception("GetNeighborsBetween did not end loop on direction2");
        if(inclusive && neighbors[(int)direction2] != null) output.Add(neighbors[(int)direction2]);
        return output;
    }
}
