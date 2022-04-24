using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingMesh : MonoBehaviour
{
    //* Editor Values
    [SerializeField] bool debug;
    //* Private Values
    private Mesh mesh;
    static List<Vector3> vertices = new List<Vector3>();
    static List<int> triangles = new List<int>();
    static List<Color> colors = new List<Color>();

    //* Public Methods
    public void TriangulateFromPotential(CustomGrid<bool> potentialTests) {
        Clear();
        debugLog("Triangulating marching squares mesh");
        var cellList = potentialTests.GetCellList();
        foreach (var cell in cellList) {
            TriangulateCell(cell);
        }
    }
    private void TriangulateCell(GridCell<bool> cell) {
        debugLog($"Triangulating cell at {cell.Index}");
        List<GridCell<bool>> neighbors = cell.GetForwardNeighbors();
        if(neighbors.Count < 3) {
            debugLog($"cell at {cell.Index} Identified as edge");
            return;} //? because we triangulate each section from its bottom left corner we can skip the edges of the grid
        bool[] cellValues = new bool[4];
        cellValues[0] = cell.GetValue();
        for (int i = 1; i < 4; i++) {
            cellValues[i] = neighbors.ToArray()[i-1].GetValue();
        }
        int caseID = IdentifyCase(cellValues);
        debugLog($"cell at index {cell.Index} identified as case {caseID}");
    }
    private int IdentifyCase(bool[] cellValues) { //? each case it represented by a binary number in which the starting cell (bottom left) is the first digit continuing clockwise
        if(cellValues.Length != 4) throw new System.Exception("IdentifyCase requires an argument of 4 values ordered from bottom left proceding clockwise");
        BitArray binaryArg = new BitArray(cellValues);
        byte[] argBytes = new byte[1];
        binaryArg.CopyTo(argBytes,0);
        return argBytes[0];
    }

    //* Private Methods
    private void Clear() {
        mesh.Clear();
        vertices.Clear();
        colors.Clear();
        triangles.Clear();
    }
    private void debugLog(string message) {
        if(debug) Debug.Log(message);
    }

    //* MonoBehaviour Methods
    void Awake()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Marching Squares Mesh";
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
