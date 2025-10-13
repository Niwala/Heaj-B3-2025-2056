using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

public class MarkdownDocument : ScriptableObject
{
    public string file;
    public VisualElement root;

#if UNITY_EDITOR
    public SerializedObject binding;
#endif

    private int orderedListValue = -1;
    private bool inCodeBlock = false;
    private bool inMultiLineCodeBlock = false;
    public string multilineCode = "";
    private VisualElement container;

    public VisualElement GenerateUI()
    {
        root = new VisualElement();
        ParseFile();
        return root;
    }

    public void UpdateUI()
    {
        root.Clear();
        ParseFile();
    }

    private void ParseFile()
    {
        if (string.IsNullOrEmpty(file))
            return;

        string[] lines = file.Split(
            new string[] { "\r\n", "\r", "\n" },
            StringSplitOptions.None
        );

        float spacing = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            if (!string.IsNullOrEmpty(lines[i]))
            {
                VisualElement element = ParseLine(lines[i]);

                if (element == null)
                    continue;

                //Add spacing
                element.style.marginTop = spacing;
                spacing = 0;

                //Add element
                if (container != null && container != element)
                    container.Add(element);
                else
                    root.Add(element);
            }
            else
            {
                spacing += 20;
            }
        }
    }

    private VisualElement ParseLine(string line)
    {
        //Enable flags - Multiline code block
        if (inMultiLineCodeBlock)
        {
            if (line.EndsWith("```"))
            {
                CodeContainer codeContainer = new CodeContainer(multilineCode);
                ResetDefaultState();
                return codeContainer;
            }
            else
            {
                if (string.IsNullOrEmpty(multilineCode))
                    multilineCode = line;
                else
                    multilineCode += "\n" + line;
                return null;
            }
        }

        if (inCodeBlock)
        {
            if (line.StartsWith("    "))
            {
                CodeContainer codeContainer = container as CodeContainer;
                codeContainer.code += "\n" + line.Remove(0, "    ".Length);
                return null;
            }
            else
            {
                ResetDefaultState();
            }
        }


        //Titles
        {
            var titleRegex = new Regex(@"^(#{1,6}) ");
            var match = titleRegex.Match(line);
            if (match.Success)
            {
                string titlePrefix = match.Groups[1].Value;
                ResetDefaultState();
                return new Title(titlePrefix.Length, MarkdownToRichText(line.Remove(0, titlePrefix.Length + 1)));
            }
        }

        //Quotes
        {
            var titleRegex = new Regex(@"^((\s*>\s*){1,10}) ");
            var match = titleRegex.Match(line);
            if (match.Success)
            {
                string quotePrefix = match.Groups[1].Value;
                int indent = (quotePrefix.StartsWith("> ") ? Mathf.CeilToInt(quotePrefix.Length / 2.0f) : quotePrefix.Length);
                ResetDefaultState();
                return new Quote(indent, MarkdownToRichText(line.Remove(0, quotePrefix.Length + 1)));
            }
        }

        //Line
        if (line.StartsWith("___") || line.StartsWith("---") || line.StartsWith("***"))
        {
            ResetDefaultState();
            return new Line();
        }

        //Lists
        {
            //Unordered
            {
                var listRegex = new Regex(@"^((\s*(\+|-|\*)){1,10}) ");
                var match = listRegex.Match(line);
                if (match.Success)
                {
                    string listPrefix = match.Groups[1].Value;
                    int indent = listPrefix.Length;
                    TextElement text = new TextElement();
                    text.style.marginLeft = indent * 10;
                    text.text = "• " + MarkdownToRichText(line).Remove(0, listPrefix.Length + 1);
                    ResetDefaultState();
                    return text;
                }
            }

            //Ordered
            {
                var listRegex = new Regex(@"^(\d+)");
                var match = listRegex.Match(line);
                if (match.Success)
                {
                    string listPrefix = match.Groups[0].Value;
                    int value = int.Parse(listPrefix);
                    if (orderedListValue >= 0)
                        value = orderedListValue + 1;
                    orderedListValue = value;

                    TextElement text = new TextElement();
                    text.style.marginLeft = 10;
                    text.text = value + ". " + MarkdownToRichText(line).Remove(0, listPrefix.Length + 1);
                    ResetDefaultState(IgnoreFlags.OrderedList);
                    return text;
                }
            }
        }

        //Code
        if (line.StartsWith("    "))
        {
            inCodeBlock = true;
            ResetDefaultState(IgnoreFlags.codeBlock);
            container = new CodeContainer(line.Remove(0, 4));
            return container;
        }

        if (line.StartsWith("```"))
        {
            inMultiLineCodeBlock = true;
            multilineCode = "";
            ResetDefaultState(IgnoreFlags.multiLineCodeBlock);
            return null;
        }

#if UNITY_EDITOR
        if (line.StartsWith("$ "))
        {

            SerializedProperty prop = binding.FindProperty(line.Remove(0, 2));
            if (prop != null)
            {
               PropertyField field = new PropertyField(prop);
               BindingExtensions.Bind(field, binding);
                return field;
            }
        }

        if (line.StartsWith("!["))
        {
            GetTooltipAndContent(line, out string tooltip, out string contentPath);
            SerializedProperty prop = binding.FindProperty(contentPath);
            if (prop != null)
            {
                if (prop.boxedValue is Texture2D tex2D)
                {
                    if (tooltip.Contains("Editor"))
                    {
                        IMGUIContainer imGuiContainer = new IMGUIContainer();
                        SetBorder(imGuiContainer, 1, 0, Color.black);
                        imGuiContainer.style.width = tex2D.width + 2;
                        imGuiContainer.style.height = tex2D.height + 22;
                        Editor elementEditor = Editor.CreateEditor(tex2D);
                        imGuiContainer.onGUIHandler += DrawGUI;
                        imGuiContainer.RegisterCallback((DetachFromPanelEvent e) => GameObject.DestroyImmediate(elementEditor));

                        void DrawGUI()
                        {
                            GUILayout.BeginArea(new Rect(1, 1, tex2D.width + 2, tex2D.height + 22));
                            GUILayout.BeginHorizontal();
                            elementEditor.OnPreviewSettings();
                            GUILayout.EndHorizontal();
                            elementEditor.OnPreviewGUI(new Rect(0, 20, tex2D.width, tex2D.height), GUIStyle.none);
                            GUILayout.EndArea();
                        }

                        return imGuiContainer;
                    }

                    VisualElement texture = new VisualElement() { name = "image" };
                    texture.tooltip = tooltip;
                    texture.style.backgroundImage = Background.FromTexture2D(tex2D);
                    texture.style.width = tex2D.width;
                    texture.style.height = tex2D.height;
                    return texture;
                }
            }
        }

#endif

        {
            TextElement text = new TextElement();
            text.text = MarkdownToRichText(line);
            ResetDefaultState();
            return text;
        }

    }

    public static string ConvertLinks(string input)
    {
        string result = Regex.Replace(input, @"\[(?<text>[^\]]+)\]\((?<url>[^\)]+)\)", @"<link=""${url}"">${text}</link>");
        return result;
    }

    private static void SetBorder(VisualElement ve, float width, float radius, Color color)
    {
        ve.style.borderLeftWidth = ve.style.borderRightWidth = ve.style.borderTopWidth = ve.style.borderBottomWidth = width;
        ve.style.borderBottomLeftRadius = ve.style.borderBottomRightRadius = ve.style.borderTopLeftRadius = ve.style.borderTopRightRadius = radius;
        ve.style.borderBottomColor = ve.style.borderTopColor = ve.style.borderLeftColor = ve.style.borderRightColor = color;
    }


    private static void SetPadding(VisualElement ve, float padding)
    {
        ve.style.paddingLeft = ve.style.paddingRight = ve.style.paddingTop = ve.style.paddingBottom = padding;
    }


    private static void GetTooltipAndContent(string markdown, out string tooltip, out string contentPath)
    {
        //Regex pattern: ![tooltip](path)
        var match = Regex.Match(markdown, @"!\[(.*?)\]\((.*?)\)");

        if (match.Success)
        {
            tooltip = match.Groups[1].Value; //Tooltip
            contentPath = match.Groups[2].Value; //Path
        }
        else
        {
            tooltip = string.Empty; //Empty if no match
            contentPath = string.Empty; //Empty if no match
        }
    }

    private void ResetDefaultState(IgnoreFlags flags = IgnoreFlags.None)
    {
        if ((flags & IgnoreFlags.OrderedList) == IgnoreFlags.None)
            orderedListValue = -1;

        if ((flags & IgnoreFlags.codeBlock) == IgnoreFlags.None)
        {
            inCodeBlock = false;
            container = null;
        }

        if ((flags & IgnoreFlags.multiLineCodeBlock) == IgnoreFlags.None)
        {
            inMultiLineCodeBlock = false;
            container = null;
        }
    }

    public enum IgnoreFlags
    {
        None,
        OrderedList,
        codeBlock,
        multiLineCodeBlock,
    }

    public string MarkdownToRichText(string markdown)
    {
        //Links
        markdown = Regex.Replace(markdown, @"\[(?<text>[^\]]+)\]\((?<url>[^)]+)\)", "<a href=\"${url}\">${text}</a>");

        //Bold & Italic
        markdown = Regex.Replace(markdown, @"(___|\*\*\*)(.*?)(___|\*\*\*)", "<b>$2</b>");

        //Bold
        markdown = Regex.Replace(markdown, @"(__|\*\*)(.*?)(__|\*\*)", "<b>$2</b>");

        //Italic
        markdown = Regex.Replace(markdown, @"(_|\*)(.*?)(_|\*)", "<i>$2</i>");

        //Strikethrough
        markdown = Regex.Replace(markdown, @"~~(.*?)~~", "<s>$1</s>");

        //Underline
        markdown = Regex.Replace(markdown, @"<ins>(.*?)</ins>", "<u>$1</u>");


        //Typographic replacements
        markdown = Regex.Replace(markdown, @"\((c|C)\)", "©");
        markdown = Regex.Replace(markdown, @"\((r|R)\)", "®");
        markdown = Regex.Replace(markdown, @"\((tm|TM)\)", "™");
        markdown = Regex.Replace(markdown, @"(\+-)", "±");

        return markdown;


    }

}
