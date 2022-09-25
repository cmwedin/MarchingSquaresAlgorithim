using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MarchingSquares : MonoBehaviour
{
    //* sibling components
    private MarchingMesh Mesh;
    
    //* editor values
    [SerializeField] PotentialSO potentialSO;
    [SerializeField] ComputeShader caseEvaluationShader;

    [SerializeField] private float threshold;
    [SerializeField] private float resolution;
    [SerializeField] private float xLowerBound;
    [SerializeField] private float xUpperBound;
    [SerializeField] private float yLowerBound;
    [SerializeField] private float yUpperBound;
    //? probably a more elegant way to do the bounds

    //* private values
    private Func<Vector2,float> Potential;
    private bool running = false;
    private int gridWidth, gridHeight;


    private float TotalHeight {get => XUpperBound - XLowerBound;}
    private float TotalWidth {get => YUpperBound - YLowerBound;}
    public float XLowerBound { get => xLowerBound;}
    public float XUpperBound { get => xUpperBound;}
    public float YLowerBound { get => yLowerBound;}
    public float YUpperBound { get => yUpperBound;}


    //*public methods
    public void SetPotential(Func<Vector2,float> _potential) {
        Potential = _potential;
    }
    public float GetPotentialAt(Vector2 pos) {
        if(Potential == null) {throw new Exception("trying to sample an unset potential");}
        return Potential(pos);
    }
    public float GetPotentialAt(GridCell<bool> cell) {
        if(Potential == null) {throw new Exception("trying to sample an unset potential");}
        return Potential(cell.GetWorldPos());
    }
    public bool TestCellCenter(GridCell<bool> cell) {
        float centerValue = GetPotentialAt(cell.GetWorldPos() + new Vector2(resolution/2,resolution/2));
        return centerValue >= threshold;
    }
    public Vector3 LerpCells(GridCell<bool> cell1, GridCell<bool> cell2) {
        if(cell1.GetValue() == cell2.GetValue()) {
            Debug.LogWarning("Trying to interpolate between two cells with the same value");
            return cell1.GetWorldPos();
        }
        float value1 = GetPotentialAt(cell1);
        float value2 = GetPotentialAt(cell2);
        float t = (threshold - value1) / (value2 - value1);
        return Vector3.Lerp(cell1.GetWorldPos(),cell2.GetWorldPos(),t);
    }
    //? public wrapper to ensure the algorithm isn't called multiple times per instance 
    //? probably a more elegant way to do this
    public void Run() {
        if(running) {
            Debug.LogWarning("Marching squares algorthim called while already running");
            return;
        }
        InnerRun();
        return;
    }

    //*private methods
    //? Main
    private void InnerRun() {
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
            new Vector2(XLowerBound, YLowerBound) 
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
        Debug.Log($"Started grid partition  and evaluation at {watch.ElapsedMilliseconds}ms");
        SquareStruct[] data = GenerateComputeShaderData(testingGrid);
        int squareMemorySize = 5 *sizeof(int);
        ComputeBuffer computeBuffer = new ComputeBuffer(data.Length, squareMemorySize);
        computeBuffer.SetData(data);
        Debug.Log($"Started triangulation at {watch.ElapsedMilliseconds}ms");
        computeBuffer.Dispose();
        Mesh.TriangulateFromPotential(testingGrid);
        running = false;
        watch.Stop();
        Debug.Log($"marching squares synchronous algorithm run in {watch.ElapsedMilliseconds}ms");
        return;
    }
    private SquareStruct[] GenerateComputeShaderData(CustomGrid<bool> testingGrid) {
        int interiorCellCount = (gridHeight - 1) * (gridWidth - 1);
        SquareStruct[] data = new SquareStruct[interiorCellCount];
        int k = 0;
        for (int i = 0; i < gridWidth - 1; i++) {
            for (int j = 0; j < gridHeight - 1; j++) {
                SquareStruct square = new SquareStruct();
                square.bottomLeft = testingGrid.GetGridValue(new Vector2Int(i, j)) ? 1 : 0;
                square.topLeft = testingGrid.GetGridValue(new Vector2Int(i+1, j)) ? 1 : 0;
                square.topRight = testingGrid.GetGridValue(new Vector2Int(i+1, j+1)) ? 1 : 0;
                square.bottomRight = testingGrid.GetGridValue(new Vector2Int(i, j+1)) ? 1 : 0;
                data[k] = square;
                k++;
            }
        }
        return data;
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
        Mesh.marchingSquares = this;
        if(potentialSO != null) {
            Potential = potentialSO.Evaluate;
        }
    }
    //? Start is called before the first frame update
    void Start()
    {
        gridWidth = Mathf.FloorToInt(TotalWidth/resolution);
        gridHeight = Mathf.FloorToInt(TotalHeight/resolution);
        Run(); //! testing purposes only
    }

    //? Update is called once per frame
    void Update() {
        
    }


}
