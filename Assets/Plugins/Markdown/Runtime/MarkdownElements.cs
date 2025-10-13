using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MarkdownElement : VisualElement
{

}

public class Title : MarkdownElement
{
    public Title(int id, string text)
    {
        TextElement textElement = new TextElement() { name = "title-text" };
        textElement.text = text;
        textElement.style.fontSize = 24 - id * 2;
        style.marginBottom = style.marginTop = Mathf.Max(0, 12 - id);

        name = "title";
        Add(textElement);
    }
}

public class Quote : MarkdownElement
{
    public Quote(int indent, string text)
    {
        VisualElement quoteContainer = new VisualElement() { name = "quote-container" };
        quoteContainer.style.flexDirection = FlexDirection.Row;
        for (int i = 0; i < indent; i++)
        {
            VisualElement indentElement = new VisualElement() { name = "quote-indent"};
            indentElement.style.flexGrow = 1;
            indentElement.style.flexShrink = 0;
            indentElement.style.width = 4;
            indentElement.style.marginRight = 10;
            indentElement.style.backgroundColor = Color.gray;
            quoteContainer.Add(indentElement);
        }
        Add(quoteContainer);

        TextElement textElement = new TextElement() { name = "quote-text"};
        textElement.style.fontSize = 16;
        textElement.style.marginBottom = textElement.style.marginTop = 8;
        textElement.text = text;
        Add(textElement);

        name = "quote";
        style.flexDirection = FlexDirection.Row;

    }
}

public class Line : MarkdownElement
{
    public Line()
    {
        name = "Line";
        style.flexGrow = 1;
        style.height = 1;
        style.backgroundColor = Color.gray;
        style.marginTop = style.marginBottom = 10;
    }
}

public class CodeLine : MarkdownElement
{
    public CodeLine(string text)
    {
        TextElement textElement = new TextElement() { name = "code-line" };
        textElement.text = text;
        Add(textElement);
    }
}

public class CodeContainer : MarkdownElement
{
    private TextField textField;
    public string code { get => textField.value; set => textField.value = value; }

    public CodeContainer()
    {
        textField = new TextField() { name = "code-text-field" };
        Add(textField);
        SetStyle();
    }

    public CodeContainer(string code)
    {
        textField = new TextField() { name = "code-text-field" };
        textField.isReadOnly = true;
        this.code = code;
        Add(textField);
        SetStyle();
    }

    public void SetStyle()
    {
        style.marginLeft = 0;
        textField.style.marginLeft = 0;
        textField.style.marginTop = 3;
        textField.style.marginBottom = 10;
        textField.style.paddingBottom = textField.style.paddingLeft = textField.style.paddingRight = textField.style.paddingTop = 5;
        textField.style.borderBottomLeftRadius = textField.style.borderBottomRightRadius = textField.style.borderTopLeftRadius = textField.style.borderTopRightRadius = 4;
    }

}