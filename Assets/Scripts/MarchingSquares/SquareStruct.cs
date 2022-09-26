using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SquareStruct
{
    //? 4dig binary number representing with 0 for corners outside and 1 for corners inside (starting from bottom left proceeding clockwise)
    public int squareType;
    public Vector4 cornerValues;
}
