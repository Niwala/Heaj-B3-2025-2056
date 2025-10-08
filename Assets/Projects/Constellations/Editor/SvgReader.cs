using System.Collections.Generic;
using System.Globalization;
using System.Xml;

using UnityEngine;

public static class SvgReader
{
    public const int curvePrecision = 5;

    public static void Read(string filepath)
    {
        using (XmlReader reader = XmlReader.Create(filepath))
        {
            reader.MoveToContent();

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        Debug.Log("Start Element " + reader.Name);
                        break;
                    case XmlNodeType.Text:
                        Debug.Log("Text Node: " + reader.Value);
                        break;
                    case XmlNodeType.EndElement:
                        Debug.Log("End Element " + reader.Name);
                        break;
                    default:
                        Debug.Log($"Other node {reader.NodeType} with value {reader.Value}");
                        break;
                }

                bool isPath = reader.Name == "path";

                if (reader.HasAttributes)
                {
                    reader.MoveToFirstAttribute();
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        if (isPath && reader.ValueType == typeof(string) && reader.Name == "d")
                        {
                            Vector2[] path = ParsePath(reader.Value);
                            for (int j = 0; j < path.Length - 1; j++)
                            {
                                Debug.DrawLine(path[j], path[j + 1], Color.red, 5.0f);
                            }
                        }

                        Debug.Log("\tAttribute : " + reader.Name + "  " + reader.ValueType + "   " + reader.Value);
                        reader.MoveToNextAttribute();
                    }
                }
            }
        }
    }

    private static Vector2[] ParsePath(string path)
    {
        string[] values = path.Split(' ');
        List<Vector2> points = new List<Vector2>();

        Vector2 position = Vector2.zero;
        char command = 'M';

        for (int i = 0; i < values.Length; i++)
        {
            //Read command
            if (values[i].Length == 1 && char.IsLetter(values[i][0]))
            {
                command = values[i][0];
            }

            //Apply command
            else
            {
                switch (command)
                {
                    case 'M': //Move to absolute position
                        position = ReadVector(values[i]);
                        break;

                    case 'm': //Move to relative position
                        position += ReadVector(values[i]);
                        break;

                    case 'L': //Line to absolute position
                        points.Add(position);
                        position = ReadVector(values[i]);
                        points.Add(position);
                        break;

                    case 'l': //Line to relative position
                        points.Add(position);
                        position += ReadVector(values[i]);
                        points.Add(position);
                        break;

                    case 'H': //Horizontal line to absolute position
                        points.Add(position);
                        position.x = ReadFloat(values[i]);
                        points.Add(position);
                        break;

                    case 'h': //Horizontal line to relative position
                        points.Add(position);
                        position.x += ReadFloat(values[i]);
                        points.Add(position);
                        break;

                    case 'V': //Vertical line to absolute position
                        points.Add(position);
                        position.y = ReadFloat(values[i]);
                        points.Add(position);
                        break;

                    case 'v': //Vertical line to relative position
                        points.Add(position);
                        position.y += ReadFloat(values[i]);
                        points.Add(position);
                        break;

                    case 'C': //Bezier curve - absolute
                        {
                            Vector2 current = position;
                            Vector2 tangentOut = ReadVector(values[i++]);
                            Vector2 tangentIn = ReadVector(values[i++]);
                            Vector2 target = ReadVector(values[i]);

                            for (int k = 1; k < curvePrecision; k++)
                            {
                                float t = k / (curvePrecision - 1.0f);

                                Vector2 p = CubicBezier(current, tangentOut, tangentIn, target, t);
                                points.Add(p);
                            }
                            position = target;
                        }
                        break;

                    case 'c': //Bezier curve - relative
                        {
                            Vector2 current = Vector2.zero;
                            Vector2 tangentOut = ReadVector(values[i++]);
                            Vector2 tangentIn = ReadVector(values[i++]);
                            Vector2 target = ReadVector(values[i]);

                            for (int k = 1; k < curvePrecision; k++)
                            {
                                float t = k / (curvePrecision - 1.0f);

                                Vector2 p = CubicBezier(current, tangentOut, tangentIn, target, t);
                                points.Add(position + p);
                            }
                            position += target;
                        }

                        break;

                    default:
                        break;
                }
            }
        }

        Vector2 ReadVector(string value)
        {
            string[] floatValues = value.Split(',');
            float x = float.Parse(floatValues[0], CultureInfo.InvariantCulture);
            float y = float.Parse(floatValues[1], CultureInfo.InvariantCulture);
            return new Vector2(x, y);
        }

        float ReadFloat(string value)
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            return u * u * u * p0
                 + 3f * u * u * t * p1
                 + 3f * u * t * t * p2
                 + t * t * t * p3;
        }


        return points.ToArray();
    }
}
