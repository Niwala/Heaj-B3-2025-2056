using UnityEngine;

[System.Serializable]
public struct MarkdownNote
{
#if UNITY_EDITOR
    [Multiline]
    public string note;
    public NoteStyle style;

    public enum NoteStyle
    {
        Default,
        NoFold,
        InspectorWide,
        InspectorHeader
    }

#endif

    public MarkdownNote(string note)
    {
#if UNITY_EDITOR
        this.note = note;
        style = NoteStyle.Default;
#endif
    }
}