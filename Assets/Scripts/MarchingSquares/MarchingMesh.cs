using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingMesh : MonoBehaviour
{
    //* Sibling components
    public MarchingSquares MarchingSquares { get; set; }
    //* Editor Values
    [SerializeField] bool debug;
    [SerializeField] Color interiorColor, exteriorColor, curveColor;
    //* Private Values
    private Mesh mesh;
    static List<Vector3> vertices = new List<Vector3>();
    static List<int> triangles = new List<int>();
    static List<Color> colors = new List<Color>();

    //* Public Methods
    public void TriangulateFromPotential(CustomGrid<bool> potentialTests) {
        ClearMesh();
        debugLog("Triangulating marching squares mesh");
        var cellList = potentialTests.GetCellList();
        foreach (var cell in cellList) {
            TriangulateCell(cell);
        }
        //? draw the new mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
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
        //TODO this can probably be simplified further through symmetry 
        switch (caseID) {
            // ? all points either inside or outside the surface
            case 0: //? 0000
                TriangulateSimpleCase(cell, neighbors);
                break;
            case 15: //? 1111
                TriangulateSimpleCase(cell, neighbors);
                break;
            
            //? one point inside the surface
            case 8: //? 1000
                //? most simple of these cases, the off cells are simply the forward neighbors
                TriangulateOneOn(cell, neighbors.ToArray());
                break;
            //? the order of offCells in the following cases is relevent (it proceeds clockwise from the onCell)
            case 4: { //? 0100
                GridCell<bool> onCell = neighbors[0];
                GridCell<bool>[] offCells = new GridCell<bool>[3];
                offCells[0] = neighbors[1];
                offCells[1] = neighbors[2];
                offCells[2] = cell;
                TriangulateOneOn(onCell, offCells);
            }
                break;
            case 2: {//? 0010
                GridCell<bool> onCell = neighbors[1];
                GridCell<bool>[] offCells = new GridCell<bool>[3];
                offCells[0] = neighbors[2];
                offCells[1] = cell;
                offCells[2] = neighbors[0];
                TriangulateOneOn(onCell, offCells);
            }
                break;
            case 1: {//? 0001
                GridCell<bool> onCell = neighbors[2];
                GridCell<bool>[] offCells = new GridCell<bool>[3];
                offCells[0] = cell;
                offCells[1] = neighbors[0];
                offCells[2] = neighbors[1];
                TriangulateOneOn(onCell, offCells);
            }
                break;
            
            //? one point outside the surface
            case 7: {//? 0111
            }    
                break;
            case 11: {//? 1011
            }
                break;
            case 13: {//? 1101
            }
                break;
            case 14: {//? 1110
            }
                break;

            //? one edge inside the surface
            case 12: {//? 1100
            }
                break;
            case 6: {//? 0110
            }
                break;
            case 3: //? 0011
                break;
            case 9: {//? 1001
            }
                break;

            //? saddle point
            case 10: {//? 1010
            }
                break;
            case 5: {//? 0101 
            }
                break;
        }

        

    }
    private void TriangulateSimpleCase(GridCell<bool> cell, List<GridCell<bool>> neighbors) {
        bool inside = cell.GetValue();
        Vector3 v1 = cell.GetWorldPos();
        Vector3 v2 = neighbors[0].GetWorldPos();
        Vector3 v3 = neighbors[1].GetWorldPos();
        Vector3 v4 = neighbors[2].GetWorldPos();
        AddQuad(v1,v2,v3,v4);
        if(inside) AddQuadColor(interiorColor);
        else AddQuadColor(exteriorColor);
    }
    private void TriangulateOneOn(GridCell<bool> onCell, GridCell<bool>[] offCells) {
        //? first draws the simple off triangle
        Vector3 on = onCell.GetWorldPos();
        Vector3 off0 = offCells[0].GetWorldPos();
        Vector3 off1 = offCells[1].GetWorldPos();
        Vector3 off2 = offCells[2].GetWorldPos();
        Vector3[] interpolation = new Vector3[3];
        for (int i = 0; i < 3; i++) {
            interpolation[i] = MarchingSquares.LerpCells(onCell,offCells[i]);
        }
        AddTriangle(on,interpolation[0],interpolation[1]);
        AddTriangleColor(interiorColor, curveColor, curveColor);
        AddTriangle(on,interpolation[1],interpolation[2]);
        AddTriangleColor(interiorColor, curveColor, curveColor);
        AddTriangle(interpolation[0],off0,interpolation[1]);
        AddTriangleColor(curveColor,exteriorColor,curveColor);
        AddTriangle(interpolation[1],off2,interpolation[2]);
        AddTriangleColor(curveColor,exteriorColor,curveColor);
        AddTriangle(interpolation[1],off0,off1);
        AddTriangleColor(curveColor,exteriorColor,exteriorColor);
        AddTriangle(interpolation[1],off1,off2);
        AddTriangleColor(curveColor,exteriorColor,exteriorColor); 
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

        triangles.Add(vertIndex);
        triangles.Add(vertIndex + 2);
        triangles.Add(vertIndex + 3);
    }
    void AddQuadColor(Color c1) {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c1);

    }
    void AddQuadColor(Color c1, Color c2) {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);

    }
    void AddQuadColor(Color c1, Color c2, Color c3, Color c4) {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }
    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
        int vertIndex = vertices.Count;

        vertices.Add(v1); //@ vertIndex
        vertices.Add(v2); //@ vertIndex + 1
        vertices.Add(v3); //@ vertIndex + 2

        triangles.Add(vertIndex);
        triangles.Add(vertIndex + 1);
        triangles.Add(vertIndex + 2);
    }
    void AddTriangleColor(Color color) {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }
    void AddTriangleColor(Color c1, Color c2, Color c3) {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }

    private int IdentifyCase(bool[] cellValues) { //? each case it represented by a binary number in which the starting cell (bottom left) is the first digit continuing clockwise
        if(cellValues.Length != 4) throw new System.Exception("IdentifyCase requires an argument of 4 values ordered from bottom left proceding clockwise");
        BitArray binaryArg = new BitArray(cellValues);
        byte[] argBytes = new byte[1];
        binaryArg.CopyTo(argBytes,0);
        return argBytes[0];
    }

    //* Private Methods
    private void ClearMesh() {
        mesh.Clear();
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
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
