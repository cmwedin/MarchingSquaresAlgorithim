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
    private MarchingMesh marchingMesh;
    
    //* editor values
    [SerializeField] PotentialSO potentialSO;
    [SerializeField] ComputeShader computeShader;
    int CaseEvaluationKernel { get => computeShader.FindKernel("ClassifySquares"); }

    [SerializeField] private float threshold;
    [SerializeField] private float resolution;
    [SerializeField] private float xLowerBound;
    [SerializeField] private float xUpperBound;
    [SerializeField] private float yLowerBound;
    [SerializeField] private float yUpperBound;
    //? probably a more elegant way to do the bounds

    //* private values
    private Func<Vector2,float> Potential;
    private int gridWidth, gridHeight;
    float[,] potentialValues;

    private Vector3 TestGridIndexToWorldPos(int i, int j) {
        float xPos = XLowerBound + i * resolution , yPos = YLowerBound + j * resolution;
        return new Vector3(xPos, yPos, 0);
    }

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

    //*private methods
    //? Main
    public void Run() {
        var watch = Stopwatch.StartNew(); //? for performance testing
        if(Potential == null) {
            Debug.LogWarning("Potential never set, using default magnitude potential");
            SetPotential(x => x.magnitude);
        }
        //? the testing grid is a grid of bools representing wether each point is >= or < the threshold value (1 and 0 respectively)
        // CustomGrid<bool> testingGrid = new CustomGrid<bool>(
        //     gridWidth, 
        //     gridHeight, 
        //     resolution,
        //     new Vector2(XLowerBound, YLowerBound) 
        // );
        List<GridCell<bool>> boundaryPoints = new List<GridCell<bool>>();
        //TODO this could probably be replace by a function in CustomGrid that iterates an action over each index
        for (int i = 0; i < gridWidth; i++) {
            for (int j = 0; j < gridHeight; j++) {
                potentialValues[i,j] = Potential(TestGridIndexToWorldPos(i,j));
            }
        }
        Debug.Log($"Started grid partition  and evaluation at {watch.ElapsedMilliseconds}ms");
        marchingMesh.SetUVPotentialChanel(potentialValues,threshold);
        SquareStruct[] data = GenerateComputeShaderData();
        int squareMemorySize = 17 * sizeof(float) + sizeof(int);
        ComputeBuffer computeBuffer = new ComputeBuffer(data.Length, squareMemorySize);
        computeBuffer.SetData(data);
        computeShader.SetBuffer(0, "squareBuffer", computeBuffer);
        computeShader.SetFloat("threshold", threshold);
        computeShader.Dispatch(CaseEvaluationKernel, data.Length / 10, 1, 1);
        computeBuffer.GetData(data);
        computeBuffer.Dispose();
            //? used to verify the compute shader did something
            int sumofallcases = (from d in data select d.squareType).Sum();
            Debug.Log($"all identified cases sum to {sumofallcases}");
        // Debug.Log($"Started triangulation at {watch.ElapsedMilliseconds}ms");
        // Mesh.TriangulateFromPotential(testingGrid);
        watch.Stop();
        Debug.Log($"marching squares synchronous algorithm run in {watch.ElapsedMilliseconds}ms");
        return;
    }
    private SquareStruct[] GenerateComputeShaderData() {
        int interiorCellCount = (gridHeight - 1) * (gridWidth - 1);
        SquareStruct[] data = new SquareStruct[interiorCellCount];
        int k = 0;
        for (int i = 0; i < gridWidth - 1; i++) {
            for (int j = 0; j < gridHeight - 1; j++) {
                SquareStruct square = new SquareStruct();
                square.cornerValues[0] = potentialValues[i,j];
                square.bottomLeft = TestGridIndexToWorldPos(i, j);
                
                square.cornerValues[1] = potentialValues[i,j+1];
                square.topLeft = TestGridIndexToWorldPos(i, j+1);
                
                square.cornerValues[2] = potentialValues[i+1,j+1];
                square.topRight = TestGridIndexToWorldPos(i+1, j+1);
                
                square.cornerValues[3] = potentialValues[i+1,j];
                square.bottomRight = TestGridIndexToWorldPos(i+1, j);
                
                Vector3 centerPos = TestGridIndexToWorldPos(i, j);
                centerPos.x += resolution / 2;
                centerPos.y += resolution / 2;
                square.centerValue = Potential(centerPos);

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
        marchingMesh = GetComponent<MarchingMesh>();
        marchingMesh.marchingSquares = this;
        if(potentialSO != null) {
            Potential = potentialSO.Evaluate;
        }
    }
    //? Start is called before the first frame update
    void Start()
    {
        gridWidth = Mathf.FloorToInt(TotalWidth/resolution);
        gridHeight = Mathf.FloorToInt(TotalHeight/resolution);
        if (gridWidth == 1 || gridHeight == 1) { throw new Exception($"Must have at least 2 testing points in each dimension (current testing grid is {gridWidth} x {gridHeight})"); }
        potentialValues = new float[gridWidth, gridHeight];
        marchingMesh.GenerateBaseMesh(gridWidth, gridHeight);
        Run(); //! testing purposes only
    }

    //? Update is called once per frame
    void Update() {
        
    }


}
