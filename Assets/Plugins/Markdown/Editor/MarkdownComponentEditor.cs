using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[CustomEditor(typeof(MarkdownComponent), true)]
public class MarkdownComponentEditor : Editor
{
    private MarkdownComponent component;
    private MarkdownDocument document;

    private bool editMode
    {
        get => _editMode;
        set
        {
            _editMode = value;
            OnEditModeChanged();
        }
    }
    private bool _editMode;

    private VisualElement editModeRoot;
    private VisualElement documentRoot;
    private VisualElement documentContainer;

    private static Dictionary<MarkdownComponent, MarkdownComponentEditor> editors = new Dictionary<MarkdownComponent, MarkdownComponentEditor>();

    protected virtual void OnEnable()
    {
        component = target as MarkdownComponent;
        if (component != null && !editors.ContainsKey(component))
            editors.Add(component, this);
    }

    protected virtual void OnDisable()
    {
        component = target as MarkdownComponent;
        if (component != null && editors.ContainsKey(component))
            editors.Remove(component);
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        //Roots
        editModeRoot = new VisualElement();
        root.Add(editModeRoot);
        documentRoot = new VisualElement();
        root.Add(documentRoot);


        //Default document
        document = ScriptableObject.CreateInstance<MarkdownDocument>();
        document.file = (target as MarkdownComponent).note;
        document.binding = serializedObject;
        documentContainer = document.GenerateUI();
        documentContainer.style.paddingLeft = 10;
        documentContainer.style.paddingTop = 12;
        documentContainer.style.paddingRight = 10;
        documentContainer.style.paddingBottom = 10;
        documentRoot.Add(documentContainer);

        //Note field
        TextField noteField = new TextField();
        noteField.BindProperty(serializedObject.FindProperty(nameof(component.note)));
        noteField.style.flexDirection = FlexDirection.Column;
        noteField.style.minHeight = 200;
        noteField.multiline = true;
        noteField[0].style.alignSelf = Align.Stretch;
        editModeRoot.Add(noteField);

        //Other fields
        FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i].Name == nameof(component.note))
                continue;

            SerializedProperty prop = serializedObject.FindProperty(fields[i].Name);
            if (prop == null)
                continue;

            PropertyField field = new PropertyField(prop);
            editModeRoot.Add(field);
        }

        OnEditModeChanged();

        return root;
    }

    private void OnEditModeChanged()
    {
        documentRoot.style.display = editMode ? DisplayStyle.None : DisplayStyle.Flex;
        editModeRoot.style.display = editMode ? DisplayStyle.Flex : DisplayStyle.None;
        document.file = component.note;
        document.UpdateUI();
    }

    [MenuItem("CONTEXT/MarkdownComponent/Edit")]
    public static void OpenEditMode(MenuCommand command)
    {
        MarkdownComponent mdc = command.context as MarkdownComponent;

        if (mdc == null)
            return;

        if (editors.ContainsKey(mdc))
            editors[mdc].editMode = !editors[mdc].editMode;
    }

    private class ToolbarButton : Button
    {
        public ToolbarButton(string name)
        {
            this.text = name;
            style.marginBottom = style.marginLeft = style.marginRight = style.marginTop = 0;
            style.paddingBottom = style.paddingLeft = style.paddingRight = style.paddingTop = 5;
            style.borderBottomWidth = style.borderTopWidth = style.borderLeftWidth = style.borderRightWidth = 0;
            style.borderBottomLeftRadius = style.borderBottomRightRadius = style.borderTopLeftRadius = style.borderTopRightRadius = 0;
            style.height = 24;
            style.minWidth = 100;
        }
    }
}