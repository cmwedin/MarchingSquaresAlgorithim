using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingMesh : MonoBehaviour
{
    //* Private Values
    private Mesh mesh;
    static List<Vector3> vertices = new List<Vector3>();
    static List<int> triangles = new List<int>();
    static List<Color> colors = new List<Color>();

    //* Public Methods
    public void TriangulateFromPotential(CustomGrid<bool> potentialTests) {
        Clear();
        var cellList = potentialTests.GetCellList();
        
    }
    private void TriangulateCell(GridCell<bool> cell) {
        
    }

    //* Private Methods
    private void Clear() {
        mesh.Clear();
        vertices.Clear();
        colors.Clear();
        triangles.Clear();
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
