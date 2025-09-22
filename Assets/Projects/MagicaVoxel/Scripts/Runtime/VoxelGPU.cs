using UnityEngine;
using UnityEngine.VFX;

[VFXType(VFXTypeAttribute.Usage.GraphicsBuffer)]
struct VoxelGPU
{
    public Vector3 myColor;
    public Vector3 myPosition;
}