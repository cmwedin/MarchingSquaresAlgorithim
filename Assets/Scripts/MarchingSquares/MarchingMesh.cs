using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SadSapphicGames.MeshUtilities;
public class MarchingMesh : MonoBehaviour
{
    //* Sibling components
    public MarchingSquares MarchingSquares { get; set; }
    //* Editor Values
    [SerializeField] bool debug;
    [SerializeField] bool useVertColors;
    [SerializeField] Color interiorColor, exteriorColor, curveColor;
    //* Private Values
    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private MeshUtilityWrapper meshWrapper;
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
        meshWrapper.UpdateMesh();
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
            case 1: //? 0001
                //? most simple of these cases, the off cells are simply the forward neighbors
                TriangulateOneOn(cell, neighbors.ToArray());
                break;
            //? the order of offCells in the following cases is relevent (it proceeds clockwise from the onCell)
            case 2: { //? 0010
                GridCell<bool> onCell = neighbors[0];
                GridCell<bool>[] offCells = new GridCell<bool>[3];
                offCells[0] = neighbors[1];
                offCells[1] = neighbors[2];
                offCells[2] = cell;
                TriangulateOneOn(onCell, offCells);
            }
                break;
            case 4: {//? 0100
                GridCell<bool> onCell = neighbors[1];
                GridCell<bool>[] offCells = new GridCell<bool>[3];
                offCells[0] = neighbors[2];
                offCells[1] = cell;
                offCells[2] = neighbors[0];
                TriangulateOneOn(onCell, offCells);
            }
                break;
            case 8: {//? 1000
                GridCell<bool> onCell = neighbors[2];
                GridCell<bool>[] offCells = new GridCell<bool>[3];
                offCells[0] = cell;
                offCells[1] = neighbors[0];
                offCells[2] = neighbors[1];
                TriangulateOneOn(onCell, offCells);
            }
                break;
            
            //? one point outside the surface
            case 14: {//? 1110
                //? this configurations simple case
                TriangulateOneOff(cell, neighbors.ToArray());
            }    
                break;
            case 13: {//? 1101
                GridCell<bool> offCell = neighbors[0];
                //? again the order of onCells is relevant in the following cases
                GridCell<bool>[] onCells = new GridCell<bool>[3];
                onCells[0] = neighbors[1];
                onCells[1] = neighbors[2];
                onCells[2] = cell;
                TriangulateOneOff(offCell, onCells);            
            }
                break;
            case 11: {//? 1011
                GridCell<bool> offCell = neighbors[1];
                GridCell<bool>[] onCells = new GridCell<bool>[3];
                onCells[0] = neighbors[2];
                onCells[1] = cell;
                onCells[2] = neighbors[0];
                TriangulateOneOff(offCell, onCells);                   
            }
                break;
            case 7: {//? 0111
                GridCell<bool> offCell = neighbors[2];
                GridCell<bool>[] onCells = new GridCell<bool>[3];
                onCells[0] = cell;
                onCells[1] = neighbors[0];
                onCells[2] = neighbors[1];
                TriangulateOneOff(offCell, onCells);                   
            }
                break;

            //? one edge inside the surface
            case 3: {//? 0011
                GridCell<bool>[] insideEdge = new GridCell<bool>[2];
                GridCell<bool>[] outsideEdge = new GridCell<bool>[2];
                insideEdge[0] = neighbors[0];
                outsideEdge[0] = neighbors[1];
                insideEdge[1] = cell;
                outsideEdge[1] = neighbors[2];
                TriangulateEdge(insideEdge, outsideEdge);
            }
                break;
            case 6: {//? 0110 
                GridCell<bool>[] insideEdge = new GridCell<bool>[2];
                GridCell<bool>[] outsideEdge = new GridCell<bool>[2];
                insideEdge[0] = neighbors[1];
                outsideEdge[0] = neighbors[2];
                insideEdge[1] = neighbors[0];
                outsideEdge[1] = cell;
                TriangulateEdge(insideEdge, outsideEdge);                
            }
                break;
            case 12: {//? 1100
                GridCell<bool>[] insideEdge = new GridCell<bool>[2];
                GridCell<bool>[] outsideEdge = new GridCell<bool>[2];
                insideEdge[0] = neighbors[2];
                outsideEdge[0] = cell;
                insideEdge[1] = neighbors[1];
                outsideEdge[1] = neighbors[0];
                TriangulateEdge(insideEdge, outsideEdge);
            }
                break;
            case 9: {//? 1001
                GridCell<bool>[] insideEdge = new GridCell<bool>[2];
                GridCell<bool>[] outsideEdge = new GridCell<bool>[2];
                insideEdge[0] = cell;
                outsideEdge[0] = neighbors[0];
                insideEdge[1] = neighbors[2];
                outsideEdge[1] = neighbors[1];
                TriangulateEdge(insideEdge, outsideEdge);            
            }
                break;

            //? saddle point
            case 5: {//? 0101
                GridCell<bool>[] insidePoints = new GridCell<bool>[2];
                GridCell<bool>[] outsidePoints = new GridCell<bool>[2];
                insidePoints[0] = cell;
                outsidePoints[0] = neighbors[0];
                insidePoints[1] = neighbors[1];
                outsidePoints[1] = neighbors[2];
                TriangulateSaddle(insidePoints, outsidePoints,MarchingSquares.TestCellCenter(cell));
            }
                break;
            case 10: {//? 1010
                GridCell<bool>[] insidePoints = new GridCell<bool>[2];
                GridCell<bool>[] outsidePoints = new GridCell<bool>[2];
                insidePoints[0] = neighbors[0];
                outsidePoints[0] = cell;
                insidePoints[1] = neighbors[2];
                outsidePoints[1] = neighbors[1];
                TriangulateSaddle(insidePoints, outsidePoints,MarchingSquares.TestCellCenter(cell));             
            }
                break;
            default: throw new Exception($"unable to categorize cell at {cell.Index}, case {caseID}");
        }
    }

    private void TriangulateSimpleCase(GridCell<bool> cell, List<GridCell<bool>> neighbors) {
        bool inside = cell.GetValue();
        Vector3 v1 = cell.GetWorldPos();
        Vector3 v2 = neighbors[0].GetWorldPos();
        Vector3 v3 = neighbors[1].GetWorldPos();
        Vector3 v4 = neighbors[2].GetWorldPos();
        meshWrapper.AddQuad(v1,v2,v3,v4);
        if(inside) meshWrapper.AddQuadColor(interiorColor);
        else meshWrapper.AddQuadColor(exteriorColor);
    }
    private void TriangulateOneOn(GridCell<bool> onCell, GridCell<bool>[] offCells) {
        Vector3 on = onCell.GetWorldPos();
        Vector3 off0 = offCells[0].GetWorldPos();
        Vector3 off1 = offCells[1].GetWorldPos();
        Vector3 off2 = offCells[2].GetWorldPos();
        Vector3[] interpolation = new Vector3[3];
        for (int i = 0; i < 3; i++) {
            interpolation[i] = MarchingSquares.LerpCells(onCell,offCells[i]);
        }
        meshWrapper.AddTriangle(on,interpolation[0],interpolation[1]);
        meshWrapper.AddTriangleColor(interiorColor, curveColor, curveColor);
        meshWrapper.AddTriangle(on,interpolation[1],interpolation[2]);
        meshWrapper.AddTriangleColor(interiorColor, curveColor, curveColor);
        meshWrapper.AddTriangle(interpolation[0],off0,interpolation[1]);
        meshWrapper.AddTriangleColor(curveColor,exteriorColor,curveColor);
        meshWrapper.AddTriangle(interpolation[1],off2,interpolation[2]);
        meshWrapper.AddTriangleColor(curveColor,exteriorColor,curveColor);
        meshWrapper.AddTriangle(interpolation[1],off0,off1);
        meshWrapper.AddTriangleColor(curveColor,exteriorColor,exteriorColor);
        meshWrapper.AddTriangle(interpolation[1],off1,off2);
        meshWrapper.AddTriangleColor(curveColor,exteriorColor,exteriorColor); 
    }
    private void TriangulateOneOff(GridCell<bool> offCell, GridCell<bool>[] onCells) {
        Vector3 off = offCell.GetWorldPos();
        Vector3 on0 = onCells[0].GetWorldPos();
        Vector3 on1 = onCells[1].GetWorldPos();
        Vector3 on2 = onCells[2].GetWorldPos();
        Vector3[] interpolation = new Vector3[3];
        for (int i = 0; i < 3; i++) {
            interpolation[i] = MarchingSquares.LerpCells(offCell,onCells[i]);
        }
        meshWrapper.AddTriangle(off,interpolation[0],interpolation[1]);
        meshWrapper.AddTriangleColor(exteriorColor, curveColor, curveColor);
        meshWrapper.AddTriangle(off,interpolation[1],interpolation[2]);
        meshWrapper.AddTriangleColor(exteriorColor, curveColor, curveColor);
        meshWrapper.AddTriangle(interpolation[0],on0,interpolation[1]);
        meshWrapper.AddTriangleColor(curveColor,interiorColor,curveColor);
        meshWrapper.AddTriangle(interpolation[1],on2,interpolation[2]);
        meshWrapper.AddTriangleColor(curveColor,interiorColor,curveColor);
        meshWrapper.AddTriangle(interpolation[1],on0,on1);
        meshWrapper.AddTriangleColor(curveColor,interiorColor,interiorColor);
        meshWrapper.AddTriangle(interpolation[1],on1,on2);
        meshWrapper.AddTriangleColor(curveColor,interiorColor,interiorColor);               
        }
    private void TriangulateEdge(GridCell<bool>[] insideEdge, GridCell<bool>[] outsideEdge) {
        Vector3 on0 = insideEdge[0].GetWorldPos();
        Vector3 on1 = insideEdge[1].GetWorldPos();
        Vector3 off0 = outsideEdge[0].GetWorldPos();
        Vector3 off1 = outsideEdge[1].GetWorldPos();
        Vector3[] interpolation = new Vector3[2];
        for (int i = 0; i < 2; i++) {
            interpolation[i] = MarchingSquares.LerpCells(outsideEdge[i],insideEdge[i]);    
        }
        meshWrapper.AddTriangle(on0, interpolation[0],on1);
        meshWrapper.AddTriangleColor(interiorColor,curveColor, interiorColor);
        meshWrapper.AddTriangle(interpolation[0],interpolation[1],on1);
        meshWrapper.AddTriangleColor(curveColor,curveColor,interiorColor);
        meshWrapper.AddTriangle(interpolation[0],off0,interpolation[1]);
        meshWrapper.AddTriangleColor(curveColor,exteriorColor,curveColor);
        meshWrapper.AddTriangle(off0,off1,interpolation[1]);
        meshWrapper.AddTriangleColor(exteriorColor,exteriorColor,curveColor);
    }
    private void TriangulateSaddle(GridCell<bool>[] onPoints, GridCell<bool>[] offPoints, bool centerIn) {
        Vector3 on0 = onPoints[0].GetWorldPos();
        Vector3 off0 = offPoints[0].GetWorldPos();
        Vector3 on1 = onPoints[1].GetWorldPos();
        Vector3 off1 = offPoints[1].GetWorldPos();
        Vector3[,] interpolation = new Vector3[2,2];
        for (int i = 0; i < 2; i++) {
            interpolation[0,i] = MarchingSquares.LerpCells(onPoints[0], offPoints[i]);
            interpolation[1,i] = MarchingSquares.LerpCells(onPoints[1], offPoints[i]);
        }
        meshWrapper.AddTriangle(on0, interpolation[0,0], interpolation[0,1]);
        meshWrapper.AddTriangleColor(interiorColor);
        meshWrapper.AddTriangle(interpolation[0,0], off0, interpolation[1,0]);
        meshWrapper.AddTriangleColor(exteriorColor);
        meshWrapper.AddTriangle(interpolation[1,0],on1, interpolation[1,1]);
        meshWrapper.AddTriangleColor(interiorColor);
        meshWrapper.AddTriangle(interpolation[1,1], off1,interpolation[0,1]);
        meshWrapper.AddTriangleColor(exteriorColor);
        meshWrapper.AddQuad(interpolation[0,0],interpolation[1,0],interpolation[1,1],interpolation[0,1]);
        meshWrapper.AddQuadColor(centerIn ? interiorColor : exteriorColor);
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
    void Awake() {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        meshRenderer = GetComponent<MeshRenderer>();
        mesh.name = "Marching Squares Mesh";
        meshWrapper = new MeshUtilityWrapper(mesh, meshRenderer, useVertColors);
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
