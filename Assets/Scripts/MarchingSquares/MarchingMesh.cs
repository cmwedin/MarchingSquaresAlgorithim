using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SadSapphicGames.MeshUtilities;
public class MarchingMesh : MonoBehaviour
{
    //* Sibling components
    public MarchingSquares marchingSquares { get; set; }
    //* Editor Values
    [SerializeField] bool debug;
    [SerializeField] bool useVertColors;
    [SerializeField] Color canvasColor, curveColor;
    [SerializeField] float curveWidth=0.01f;
    //* Private Values
    private Mesh mesh;
    private MeshRenderer meshRenderer;
    private MeshUtilityWrapper meshWrapper;

    //* Public Methods
    public void TriangulateFromPotential(CustomGrid<bool> potentialTests) {
        meshWrapper.ResetMesh();
        // ? draw the canvas
        meshWrapper.AddQuad(
            new Vector3(marchingSquares.XLowerBound,marchingSquares.YLowerBound,1),
            new Vector3(marchingSquares.XLowerBound, marchingSquares.YUpperBound, 1),
            new Vector3(marchingSquares.XUpperBound,marchingSquares.YUpperBound,1),
            new Vector3(marchingSquares.XUpperBound,marchingSquares.YLowerBound,1)
        );
        meshWrapper.AddQuadColor(canvasColor);
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
                //TriangulateSimpleCase(cell, neighbors);
                break;
            case 15: //? 1111
                //TriangulateSimpleCase(cell, neighbors);
                break;
            
            //? one point inside the surface
            case 1: //? 0001
                //? most simple of these cases, the off cells are simply the forward neighbors
                TriangulateOneDifferent(cell, neighbors.ToArray());
                break;
            //? the order of offCells in the following cases is relevent (it proceeds clockwise from the onCell)
            case 2: { //? 0010
                GridCell<bool> onCell = neighbors[0];
                GridCell<bool>[] offCells = new GridCell<bool>[3];
                offCells[0] = neighbors[1];
                offCells[1] = neighbors[2];
                offCells[2] = cell;
                TriangulateOneDifferent(onCell, offCells);
            }
                break;
            case 4: {//? 0100
                GridCell<bool> onCell = neighbors[1];
                GridCell<bool>[] offCells = new GridCell<bool>[3];
                offCells[0] = neighbors[2];
                offCells[1] = cell;
                offCells[2] = neighbors[0];
                TriangulateOneDifferent(onCell, offCells);
            }
                break;
            case 8: {//? 1000
                GridCell<bool> onCell = neighbors[2];
                GridCell<bool>[] offCells = new GridCell<bool>[3];
                offCells[0] = cell;
                offCells[1] = neighbors[0];
                offCells[2] = neighbors[1];
                TriangulateOneDifferent(onCell, offCells);
            }
                break;
            
            //? one point outside the surface
            case 14: {//? 1110
                //? this configurations simple case
                TriangulateOneDifferent(cell, neighbors.ToArray());
            }    
                break;
            case 13: {//? 1101
                GridCell<bool> offCell = neighbors[0];
                //? again the order of onCells is relevant in the following cases
                GridCell<bool>[] onCells = new GridCell<bool>[3];
                onCells[0] = neighbors[1];
                onCells[1] = neighbors[2];
                onCells[2] = cell;
                TriangulateOneDifferent(offCell, onCells);            
            }
                break;
            case 11: {//? 1011
                GridCell<bool> offCell = neighbors[1];
                GridCell<bool>[] onCells = new GridCell<bool>[3];
                onCells[0] = neighbors[2];
                onCells[1] = cell;
                onCells[2] = neighbors[0];
                TriangulateOneDifferent(offCell, onCells);                   
            }
                break;
            case 7: {//? 0111
                GridCell<bool> offCell = neighbors[2];
                GridCell<bool>[] onCells = new GridCell<bool>[3];
                onCells[0] = cell;
                onCells[1] = neighbors[0];
                onCells[2] = neighbors[1];
                TriangulateOneDifferent(offCell, onCells);                   
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
                TriangulateSaddle(insidePoints, outsidePoints,marchingSquares.TestCellCenter(cell));
            }
                break;
            case 10: {//? 1010
                GridCell<bool>[] insidePoints = new GridCell<bool>[2];
                GridCell<bool>[] outsidePoints = new GridCell<bool>[2];
                insidePoints[0] = neighbors[0];
                outsidePoints[0] = cell;
                insidePoints[1] = neighbors[2];
                outsidePoints[1] = neighbors[1];
                TriangulateSaddle(insidePoints, outsidePoints,marchingSquares.TestCellCenter(cell));             
            }
                break;
            default: throw new Exception($"unable to categorize cell at {cell.Index}, case {caseID}");
        }
    }
    private void TriangulateOneDifferent(GridCell<bool> onCell, GridCell<bool>[] offCells) {
        Vector3[] interpolation = new Vector3[3];
        for (int i = 0; i < 3; i++) {
            interpolation[i] = marchingSquares.LerpCells(onCell,offCells[i]);
        }
        Vector3[] normals = new Vector3[2];
        normals[0] = curveWidth * Vector3.Cross(interpolation[1] - interpolation[0], Vector3.back).normalized;
        normals[1] = curveWidth * Vector3.Cross(interpolation[2] - interpolation[1], Vector3.back).normalized;

        meshWrapper.AddLineSegment(interpolation[0],interpolation[1],normals[0]);
        meshWrapper.AddQuadColor(curveColor);
        meshWrapper.AddLineSegment(interpolation[1],interpolation[2],normals[1]);
        meshWrapper.AddQuadColor(curveColor);
    }

    private void TriangulateEdge(GridCell<bool>[] insideEdge, GridCell<bool>[] outsideEdge) {
        Vector3[] interpolation = new Vector3[2];
        for (int i = 0; i < 2; i++) {
            interpolation[i] = marchingSquares.LerpCells(outsideEdge[i],insideEdge[i]);    
        }
        Vector3 normal = curveWidth * Vector3.Cross(interpolation[1] - interpolation[0], Vector3.back).normalized;
        meshWrapper.AddLineSegment(interpolation[0],interpolation[1],normal);
        meshWrapper.AddQuadColor(curveColor);
    }
    private void TriangulateSaddle(GridCell<bool>[] onPoints, GridCell<bool>[] offPoints, bool centerIn) {
        Vector3[,] interpolation = new Vector3[2,2];
        for (int i = 0; i < 2; i++) {
            interpolation[0,i] = marchingSquares.LerpCells(onPoints[0], offPoints[i]);
            interpolation[1,i] = marchingSquares.LerpCells(onPoints[1], offPoints[i]);
        }
        Vector3[] normals = new Vector3[2];
        if(centerIn) {
            normals[0] = Vector3.Cross(interpolation[0,0] - interpolation[1,0],Vector3.back);
            normals[1] = Vector3.Cross(interpolation[0,1] - interpolation[1,1], Vector3.back);
        }

        throw new NotImplementedException();
    }

    private int IdentifyCase(bool[] cellValues) { //? each case it represented by a binary number in which the starting cell (bottom left) is the first digit continuing clockwise
        if(cellValues.Length != 4) throw new System.Exception("IdentifyCase requires an argument of 4 values ordered from bottom left proceding clockwise");
        BitArray binaryArg = new BitArray(cellValues);
        byte[] argBytes = new byte[1];
        binaryArg.CopyTo(argBytes,0);
        return argBytes[0];
    }

    //* Private Methods
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
