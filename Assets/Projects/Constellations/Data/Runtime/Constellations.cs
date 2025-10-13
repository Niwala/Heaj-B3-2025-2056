using System;
using System.Collections.Generic;

using UnityEngine;

public class Constellations : ScriptableObject
{
    public float width;
    public float height;
    public List<Path> paths = new List<Path>();

    [Serializable]
    public struct Path
    {
        public Vector2[] positions;

        public Path(Vector2[] positions)
        {
            this.positions = positions;
        }
    }
}
