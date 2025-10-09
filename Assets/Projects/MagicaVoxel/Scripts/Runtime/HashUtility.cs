using UnityEngine;

public static class HashUtility
{

    public static void Append(this ref Hash128 hash, Vector3 vector)
    {
        hash.Append(vector.x);
        hash.Append(vector.y);
        hash.Append(vector.z);
    }

}
