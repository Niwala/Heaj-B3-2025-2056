using UnityEngine;

public class PlyObject : ScriptableObject
{
    public int count;
    public Hash128 hash;

    [HideInInspector]
    public Vector3[] positions;

    [HideInInspector]
    public Vector3[] colors;

    public bool IsValid()
    {
        return positions != null && positions.Length > 0;
    }


}
