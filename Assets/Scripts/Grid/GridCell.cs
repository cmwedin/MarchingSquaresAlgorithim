using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell<TGridObject>
{
    private GridCell<TGridObject>[] neighbors;
    private TGridObject value;

    public TGridObject GetValue()
    {
        return value;
    }

    public void SetValue(TGridObject value)
    {
        this.value = value;
    }

    public GridCell(Vector2Int index)
    {
        this.Index = index;
        neighbors = new GridCell<TGridObject>[8];
    }
    public Vector2Int Index { get; set; }

    public void SetNeighbor(GridDirection direction, GridCell<TGridObject> cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
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
}
