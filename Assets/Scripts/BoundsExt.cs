using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class CBoundsExt
{
    public static float GetLeft(this Bounds inBounds)
    {
        return inBounds.center.x - inBounds.extents.x;
    }

    public static float GetRight(this Bounds inBounds)
    {
        return inBounds.center.x + inBounds.extents.x;
    }

    public static float GetTop(this Bounds inBounds)
    {
        return inBounds.center.z + inBounds.extents.z;
    }

    public static float GetBottom(this Bounds inBounds)
    {
        return inBounds.center.z - inBounds.extents.z;
    }
}

