using UnityEngine;

public class PlyObject : ScriptableObject
{
    public int count;
    public Vector3[] positions;
    public Vector3[] colors;

    public bool IsValid()
    {
        return positions != null && positions.Length > 0;
    }
}