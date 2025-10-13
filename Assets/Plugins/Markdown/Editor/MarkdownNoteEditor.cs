using System;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(MarkdownNote), true)]
public class MarkdownNoteEditor : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        //Container
        NoteVisualElement container = new NoteVisualElement(property.displayName, property);
        container.style.minHeight = 20;

        return container;
    }

    public class NoteVisualElement : VisualElement
    {
        private SerializedProperty property;
        private SerializedProperty noteProperty;
        private SerializedProperty lockedProperty;
        private MarkdownDocument document;
        private Foldout foldout;
        public VisualElement defaultUI;
        public PropertyField editModeUI;

        public delegate VisualElement GenerateUIFunction(string markdown);

        private bool editMode;
        private int noteStyle
        {
            get
            {
                return (lockedProperty != null) ? lockedProperty.enumValueIndex : 0;
            }
            set
            {
                if ((value != 0) && !foldout.value)
                    foldout.value = true;
                lockedProperty.enumValueIndex = value;
                lockedProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        private const string foldoutPrefName = "MarkdownNote.Foldout";


        public NoteVisualElement(string name, SerializedProperty property)
        {
            //Default ui
            MarkdownNote template = default;
            this.property = property;
            noteProperty = property.FindPropertyRelative(nameof(template.note));
            lockedProperty = property.FindPropertyRelative(nameof(template.style));
            this.RegisterCallback<MouseDownEvent>(OnMouseDown);
            document = new MarkdownDocument();

            //Style
            base.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1.0f);

            //Foldout
            foldout = new Foldout();
            foldout.text = name;
            foldout.style.flexGrow = 1;
            foldout.style.paddingLeft = 10;
            foldout.value = EditorPrefs.GetBool(foldoutPrefName, true);
            foldout.RegisterValueChangedCallback(OnFoldoutChanged);
            Add(foldout);

            //Edit mode field
            this.editModeUI = new PropertyField(noteProperty);

            Add(editModeUI);
            OnEditModeChanged();
        }

        private void OnEditModeChanged()
        {
            if (defaultUI != null)
            {
                defaultUI.RemoveFromHierarchy();
            }

            if (editMode)
            {
                editModeUI.style.display = DisplayStyle.Flex;
                foldout.style.display = DisplayStyle.None;
            }
            else
            {
                document.file = noteProperty.stringValue;
                defaultUI = document.GenerateUI();
                Add(defaultUI);
                editModeUI.style.display = DisplayStyle.None;
                foldout.style.display = DisplayStyle.Flex;
                defaultUI.style.display = foldout.value ? DisplayStyle.Flex : DisplayStyle.None;
            }

            ApplyStyle();
        }

        private void OnFoldoutChanged(ChangeEvent<bool> e)
        {
            defaultUI.style.display = e.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            EditorPrefs.SetBool(foldoutPrefName, e.newValue);
        }

        private void ApplyStyle()
        {
            Vector4 margin = default;
            Vector4 padding = default;
            float radius = default;
            bool borders = default;

            switch ((MarkdownNote.NoteStyle)noteStyle)
            {
                case MarkdownNote.NoteStyle.Default:
                    foldout.style.display = DisplayStyle.Flex;
                    margin = new Vector4(3, 0, -2, 3);
                    padding = new Vector4(3, 6, 3, 3);
                    radius = 3;
                    borders = false;
                    break;

                case MarkdownNote.NoteStyle.NoFold:
                    foldout.style.display = DisplayStyle.None;
                    margin = new Vector4(3, 0, -2, 3);
                    padding = new Vector4(3, 6, 3, 3);
                    radius = 3;
                    borders = false;
                    break;

                case MarkdownNote.NoteStyle.InspectorWide:
                    foldout.style.display = DisplayStyle.None;

                    margin = new Vector4(3, -15, -6, 3);
                    padding = new Vector4(4, 18, 6, 6);
                    radius = 0;
                    borders = false;
                    break;

                case MarkdownNote.NoteStyle.InspectorHeader:
                    foldout.style.display = DisplayStyle.None;
                    margin = new Vector4(3, -15, -6, -22);
                    padding = new Vector4(4, 18, 6, 6);
                    radius = 0;
                    borders = true;
                    break;
            }


            style.borderTopLeftRadius = style.borderTopRightRadius =
            style.borderBottomLeftRadius = style.borderBottomRightRadius = radius;

            style.marginBottom = margin.x;
            style.marginLeft = margin.y;
            style.marginRight = margin.z;
            style.marginTop = margin.w;

            style.paddingBottom = padding.x;
            style.paddingLeft = padding.y;
            style.paddingRight = padding.z;
            style.paddingTop = padding.w;

            style.borderBottomColor = Color.black * 0.2f;
            style.borderBottomWidth = borders ? 1 : 0;

            style.borderTopColor = Color.black * 0.2f;
            style.borderTopWidth = borders ? 1 : 0;
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            if (e.button == 1)
            {
                e.StopPropagation();
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Edit note"), editMode, () => { editMode = !editMode; OnEditModeChanged(); });

                string[] names = Enum.GetNames(typeof(MarkdownNote.NoteStyle));
                int j = noteStyle;
                for (int i = 0; i < names.Length; i++)
                {
                    int k = i;
                    menu.AddItem(new GUIContent($"Style/{names[i]}"), i == j, () => { noteStyle = k; OnEditModeChanged(); });
                }

                menu.ShowAsContext();
            }
        }
    }
}