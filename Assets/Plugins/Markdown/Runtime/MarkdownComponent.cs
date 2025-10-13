using UnityEngine;

public class MarkdownComponent : MonoBehaviour
{
#if UNITY_EDITOR
    [Multiline]
    public string note;

    public virtual bool requireSceneRepaint => false;
#endif
}