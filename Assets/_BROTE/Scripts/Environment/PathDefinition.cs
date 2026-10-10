using System;
using UnityEngine;
[Serializable] public sealed class PathDefinition
{
    [Serializable] public sealed class Route { public string name; public float width; public Vector2[] points; }
    public Route[] routes;
}
