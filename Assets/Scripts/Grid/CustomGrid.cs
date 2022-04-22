using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGrid<TGridObject>
{
    //? width and height are in terms of cell number
    private int width;
    private int height;
    private float cellSize;
    private GridCell<TGridObject>[,] gridArray;

    //? grid constructor
    public CustomGrid(int width, int height, float cellSize) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
    
        gridArray = new GridCell<TGridObject>[width,height];
        Vector2Int index;          
        for (int y = 0; y < width; y++) {
            bool finalRow = y+1 < height;
            for (int x = 0; x < width; x++) {
                bool finalColumn = x+1 < width;
                index = new Vector2Int(x,y);
                gridArray[x,y].Index = index;
                gridArray[x,y].SetValue(default(TGridObject));
                //? could probably optimize this
                if(!finalRow) gridArray[x,y].SetNeighbor(GridDirection.U, gridArray[x,y+1]);
                if(!finalRow && !finalColumn) gridArray[x,y].SetNeighbor(GridDirection.UL, gridArray[x+1,y+1]);
                if(!finalColumn) gridArray[x,y].SetNeighbor(GridDirection.L, gridArray[x+1,y]);
                //? SetNeighbor automatically sets the inverse neighbor as well so you only need to set the forward ones in the loop
            }
        }
    }

    public void SetGridValue(Vector2Int index, TGridObject value) {
        if (!CheckIndex(index)) return;
        gridArray[index.x,index.y].SetValue(value);
        
    }
    public TGridObject GetGridValue(Vector2Int index) {
        if (!CheckIndex(index)) return default(TGridObject);
        return gridArray[index.x,index.y].GetValue();
    }
    public Vector3 GetWorldPos (Vector2Int index) {
        return new Vector3(index.x,index.y) * cellSize;
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

