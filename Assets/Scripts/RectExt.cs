using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class RectExt
{
    public static bool EqualSides(this RectInt in_main, RectInt in_other)
    {
        return in_main.xMin == in_other.xMin && in_main.xMax == in_other.xMax && in_main.yMin == in_other.yMin && in_main.yMax == in_other.yMax;
    }
}

