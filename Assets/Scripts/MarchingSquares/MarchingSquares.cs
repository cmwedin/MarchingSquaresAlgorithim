using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MarchingSquares : MonoBehaviour
{
    //* sibling components
    private MarchingMesh Mesh;
    
    //* editor values
    [SerializeField] private float threshold;
    [SerializeField] private float resolution;
    [SerializeField] private float xLowerBound, xUpperBound;
    [SerializeField] private float yLowerBound, yUpperBound;
    //? probably a more elegant way to do the bounds

    //* private values
    private Func<Vector2,float> Potential;
    private bool running = false;
    private int gridWidth, gridHeight;
    private float TotalHeight {get => xUpperBound - xLowerBound;}
    private float TotalWidth {get => yUpperBound - yLowerBound;}

    
    //*public methods
    public void SetPotential(Func<Vector2,float> _potential) {
        Potential = _potential;
    }
    //? public wrapper to ensure the algorithm isn't called multiple times per instance 
    //? probably a more elegant way to do this
    public void Run() {
        if(running) {
            Debug.LogWarning("Marching squares algorthim called while already running");
            return;
        }
        run();
        return;
    }

    //*private methods
    //? Main
    private void run() {
        var watch = Stopwatch.StartNew(); //? for performance testing
        running = true;
        if(Potential == null) {
            Debug.LogWarning("Potential never set, using default magnitude potential");
            SetPotential(x => x.magnitude);
        }
        //? the testing grid is a grid of bools representing wether each point is >= or < the threshold value (1 and 0 respectively)
        CustomGrid<bool> testingGrid = new CustomGrid<bool>(
            gridWidth, 
            gridHeight, 
            resolution,
            new Vector2(xLowerBound, yLowerBound) 
        );
        List<GridCell<bool>> boundaryPoints = new List<GridCell<bool>>();
        //TODO this could probably be replace by a function in CustomGrid that iterates an action over each index
        Vector2Int index;
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                index = new Vector2Int(x,y);
                //TODO this section could would be extracted passed into CustomGrid.IterateOverIndex(SetTestingGridValue(Vector2Int index))
                Vector2 testingPos = testingGrid.GetWorldPos(index);
                float testingValue = Potential(testingPos);
                testingGrid.SetGridValue(index, testingValue >= threshold);
            }
        }
        Mesh.TriangulateFromPotential(testingGrid);
        running = false;
        watch.Stop();
        Debug.Log($"marching squares algorithm run in {watch.ElapsedMilliseconds}ms");
        return;
    }

    private void DrawMesh(CustomGrid<bool> testingGrid) {
        //TODO cell = testingGrid.GetCell(0,0)
        //TODO if !cell.CheckNeighbors(cell.value != neighbor.value).empty
            //TODO cellsToDraw.Add(cell)

        throw new NotImplementedException();
    }

    //* Monobehavior methods
    private void Awake() {
        Mesh = GetComponent<MarchingMesh>();
    }
    //? Start is called before the first frame update
    void Start()
    {
        gridWidth = Mathf.FloorToInt(TotalWidth/resolution);
        gridHeight = Mathf.FloorToInt(TotalHeight/resolution);
        run(); //! testing purposes only
    }

    //? Update is called once per frame
    void Update()
    {
        
    }
}
