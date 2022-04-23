using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGrid<TGridObject>
{
    //? position of index 0,0
    private Vector2 anchor = Vector2.zero;
    private int width;
    private int height;
    private float cellSize;
    private GridCell<TGridObject>[,] gridArray;

    //? grid constructor
    public CustomGrid(int width, int height, float cellSize, Vector2 worldSpaceAnchor) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
    
        gridArray = new GridCell<TGridObject>[width,height];          
        InitializeGridCells();
        InitializeNeighbors(); 
        //? potential target for optimization: merge these two functions into one loop 
    }

    private void InitializeGridCells(){
        Vector2Int index;          
        for (int y = 0; y < width; y++) {
            for (int x = 0; x < width; x++) {
                index = new Vector2Int(x,y);
                gridArray[x,y] = new GridCell<TGridObject>(index);
                gridArray[x,y].SetValue(default(TGridObject));
            }
        }        
    }
    private void InitializeNeighbors() {
        Vector2Int index;          
        for (int y = 0; y < width; y++) {
            bool finalRow = y+1 >= height;
            for (int x = 0; x < width; x++) {
                bool finalColumn = x+1 >= width;
                index = new Vector2Int(x,y);
                //? could probably optimize this
                if(!finalRow) gridArray[x,y].SetNeighbor(GridDirection.U, gridArray[x,y+1]);
                if(!finalRow && !finalColumn) gridArray[x,y].SetNeighbor(GridDirection.UL, gridArray[x+1,y+1]);
                if(!finalColumn) gridArray[x,y].SetNeighbor(GridDirection.L, gridArray[x+1,y]);
                //? SetNeighbor automatically sets the inverse neighbor as well so you only need to set the forward ones in the loop
            }
        }
    }

    //TODO Void InterateOverIndex(Action<vector2Int>(Vector2Int index)) {for each index do Action}

    /* prototype
    public void InterateOverIndices(Action<Vector2Int> Action) {
        Vector2Int index;
        for (int y = 0; y < width; y++) {
            bool finalRow = y+1 >= height;
            for (int x = 0; x < width; x++) {
                bool finalColumn = x+1 >= width;
                index = new Vector2Int(x,y);
                Action(index);
            }
        }
    }*/

    public void SetGridValue(Vector2Int index, TGridObject value) {
        if (!CheckIndex(index)) return;
        gridArray[index.x,index.y].SetValue(value);
    }
    public List<GridCell<TGridObject>> GetCellList() {
        var output = new List<GridCell<TGridObject>>();
        foreach (var cell in gridArray) {
            output.Add(cell);
        }
        return output;
    }
    public TGridObject GetGridValue(Vector2Int index) {
        if (!CheckIndex(index)) return default(TGridObject);
        return gridArray[index.x,index.y].GetValue();
    }
    public Vector2 GetWorldPos (Vector2Int index) {
        Vector2 localPos = new Vector2(index.x,index.y) * cellSize;
        return localPos + anchor;
    }
    public Vector2 GetIndex(Vector3 WorldPos) {  
        Vector2Int index = new Vector2Int(
            Mathf.FloorToInt(WorldPos.x / cellSize),
            Mathf.FloorToInt(WorldPos.y / cellSize)
        );
        return index;
    }

    public bool CheckIndex(Vector2Int index) {
        if (index.x < width && index.x >= 0 && index.y < height && index.y >=0) {
            return true;
        } else {
            Debug.LogWarning($"({index.x},{index.y}) is not a valid index");
            return false;
        }
    }
}

