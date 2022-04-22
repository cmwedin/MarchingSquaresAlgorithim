using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GridDirection
{
    U,UR,R,DR,D,DL,L,UL
}
public static class GridDirectionExtensions {
    public static GridDirection Opposite(this GridDirection direction) {
        return (int)direction < 4 ? (direction + 4) : (direction - 4);
    }
    public static GridDirection Previous(this GridDirection direction) {
        return direction == GridDirection.U ? GridDirection.UL : (direction - 1);
    }
public static GridDirection Next(this GridDirection direction) {
        return direction == GridDirection.UL ? GridDirection.U : (direction + 1);
    }
}

public class GridMetrics
{

}
