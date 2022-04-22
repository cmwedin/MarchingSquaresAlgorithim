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

    public Vector2Int Index { get; set; }

    public void SetNeighbor(GridDirection direction, GridCell<TGridObject> cell) {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public GridCell<TGridObject> GetNeighbor(GridDirection direction) {
        return neighbors[(int)direction];
    }
}
