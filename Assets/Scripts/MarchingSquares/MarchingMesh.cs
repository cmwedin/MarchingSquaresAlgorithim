using System;
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
    //static List<Color> colors = new List<Color>();

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
        //? get the cells neighbors and make sure it isnt on the edge of the grid
        List<GridCell<bool>> neighbors = cell.GetForwardNeighbors();
        if(neighbors.Count < 3) {
            debugLog($"cell at {cell.Index} Identified as edge");
            return;} //? because we triangulate each section from its bottom left corner we can skip the edges of the grid
        //? get the values of the cell and its neighbors
        bool[] cellValues = new bool[4];
        cellValues[0] = cell.GetValue();
        for (int i = 1; i < 4; i++) {
            cellValues[i] = neighbors.ToArray()[i-1].GetValue();
        }
        //? identify the case of the cell by converting the bool array (interpreted as a 4 dig binary number) to an int
        int caseID = IdentifyCase(cellValues);
        debugLog($"cell at index {cell.Index} identified as case {caseID}");
        //? Triangulate the cell according to its coresponding case
        switch (caseID) {
            // ? all points either inside or outside the surface
            case 0:
                TriangulateSimpleCase(cell, neighbors);
                break;
            case 15:
                TriangulateSimpleCase(cell, neighbors);
                break;
            default: return;
        }

        //? draw the new mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
    }
    private void TriangulateSimpleCase(GridCell<bool> cell, List<GridCell<bool>> neighbors) {
        bool inside = cell.GetValue();
        Vector3 v1 = cell.GetWorldPos();
        Vector3 v2 = neighbors[0].GetWorldPos();
        Vector3 v3 = neighbors[1].GetWorldPos();
        Vector3 v4 = neighbors[2].GetWorldPos();
        AddQuad(v1,v2,v3,v4);
    }

    private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
            int vertIndex = vertices.Count;

            vertices.Add(v1); //@ vertIndex
            vertices.Add(v2); //@ vertIndex + 1
            vertices.Add(v3); //@ vertIndex + 2
            vertices.Add(v4); //@ vertIndex + 3

            triangles.Add(vertIndex);
            triangles.Add(vertIndex + 1);
            triangles.Add(vertIndex + 2);

            triangles.Add(vertIndex );
            triangles.Add(vertIndex + 2);
            triangles.Add(vertIndex + 3);
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
