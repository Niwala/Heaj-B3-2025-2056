using System.Xml;
using System.IO;

using UnityEditor.AssetImporters;
using UnityEngine;
using System.Globalization;
using System.Collections.Generic;

[ScriptedImporter(0, extension)]
public class ConstellationsImporter : ScriptedImporter
{
    public const string extension = "svg";

    //Properties
    public int curveResolution = 6;


    //Hidden
    [HideInInspector]
    public Constellations constellations;

    public override void OnImportAsset(AssetImportContext ctx)
    {
        //Create main object
        constellations = ScriptableObject.CreateInstance<Constellations>();
        ctx.AddObjectToAsset("main", constellations);
        ctx.SetMainObject(constellations);

        List<Vector2> points = new List<Vector2>();

        using (XmlReader reader = XmlReader.Create(assetPath))
        {
            while (reader.Read())
            {
                //Read header
                if (reader.Name == "svg")
                {
                    reader.MoveToFirstAttribute();
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        if (reader.Name == "width")
                            constellations.width = ParseFloat(reader.Value);
                        else if (reader.Name == "height")
                            constellations.height = ParseFloat(reader.Value);

                        reader.MoveToNextAttribute();
                    }
                }


                //Read paths
                if (reader.Name == "path")
                {
                    reader.MoveToFirstAttribute();
                    for (int i = 0; i < reader.AttributeCount; i++)
                    {
                        if (reader.Name == "d")
                        {
                            points.Clear();
                            ParsePath(reader.Value, points);
                            if (points.Count > 1)
                                constellations.paths.Add(new Constellations.Path(points.ToArray()));
                        }

                        reader.MoveToNextAttribute();
                    }
                }
            }
        }
    }

    private void ParsePath(string path, List<Vector2> points)
    {
        char tool = 'M';
        Vector2 cursor = Vector2.zero;

        string[] values = path.Split(' ');

        for (int i = 0; i < values.Length; i++)
        {
            string value = values[i];

            //Tool change
            if (value.Length == 1 && char.IsLetter(value[0]))
            {
                tool = value[0];

                if (tool != 'M' && tool != 'm')
                    points.Add(cursor);
            }

            //Execute tool
            else
            {
                switch (tool)
                {
                    case 'M':
                        cursor = ParseVector2(value);
                        break;

                    case 'm':
                        cursor += ParseVector2(value);
                        break;

                    case 'L':
                        {
                            Vector2 newPos = ParseVector2(value);
                            Debug.DrawLine(cursor, newPos, Color.red, 15.0f);
                            cursor = newPos;
                            points.Add(cursor);
                        }
                        break;

                    case 'l':
                        {
                            Vector2 newPos = cursor + ParseVector2(value);
                            Debug.DrawLine(cursor, newPos, Color.red, 15.0f);
                            cursor = newPos;
                            points.Add(cursor);
                        }
                        break;

                    case 'H':
                        {
                            Vector2 newPos = cursor;
                            newPos.x = ParseFloat(value);
                            Debug.DrawLine(cursor, newPos, Color.red, 15.0f);
                            cursor = newPos;
                            points.Add(cursor);
                        }
                        break;

                    case 'h':
                        {
                            Vector2 newPos = cursor;
                            newPos.x += ParseFloat(value);
                            Debug.DrawLine(cursor, newPos, Color.red, 15.0f);
                            cursor = newPos;
                            points.Add(cursor);
                        }
                        break;

                    case 'V':
                        {
                            Vector2 newPos = cursor;
                            newPos.y = ParseFloat(value);
                            Debug.DrawLine(cursor, newPos, Color.red, 15.0f);
                            cursor = newPos;
                            points.Add(cursor);
                        }
                        break;

                    case 'v':
                        {
                            Vector2 newPos = cursor;
                            newPos.y += ParseFloat(value);
                            Debug.DrawLine(cursor, newPos, Color.red, 15.0f);
                            cursor = newPos;
                            points.Add(cursor);
                        }
                        break;

                    case 'C':
                        {
                            Vector2 p0 = cursor;
                            Vector2 p1 = ParseVector2(values[i++]);
                            Vector2 p2 = ParseVector2(values[i++]);
                            Vector2 p3 = ParseVector2(values[i]);
                            DrawBezierCurve(p0, p1, p2, p3);
                            DrawBezierCurve(p0, p1, p2, p3, points);
                            cursor = p3;
                        }
                        break;

                    case 'c':
                        {
                            Vector2 p0 = cursor;
                            Vector2 p1 = cursor + ParseVector2(values[i++]);
                            Vector2 p2 = cursor + ParseVector2(values[i++]);
                            Vector2 p3 = cursor + ParseVector2(values[i]);
                            DrawBezierCurve(p0, p1, p2, p3);
                            DrawBezierCurve(p0, p1, p2, p3, points);
                            cursor = p3;
                        }
                        break;
                }
            }

        }

        //M : Move to
        //L : Line to
        //H : Horizontal line to
        //V : Vertical line to
        //C : Cubic bezier
    }

    private void DrawBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector2 a = p0;
        for (int i = 1; i < curveResolution; i++)
        {
            float t = i / (curveResolution - 1.0f);
            Vector2 b = CubicBezier(p0, p1, p2, p3, t);
            Debug.DrawLine(a, b, Color.red, 15.0f);
            a = b;
        }
    }

    private void DrawBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, List<Vector2> points)
    {
        for (int i = 1; i < curveResolution; i++)
        {
            float t = i / (curveResolution - 1.0f);
            Vector2 b = CubicBezier(p0, p1, p2, p3, t);
            points.Add(b);
        }
    }

    private static Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        return u * u * u * p0
             + u * u * t * p1 * 3.0f
             + u * t * t * p2 * 3.0f
             + t * t * t * p3;
    }

    private static float ParseFloat(string value)
    {
        return float.Parse(value, CultureInfo.InvariantCulture);
    }

    private static Vector2 ParseVector2(string value)
    {
        string[] vec = value.Split(',');
        float x = float.Parse(vec[0], CultureInfo.InvariantCulture);
        float y = float.Parse(vec[1], CultureInfo.InvariantCulture);
        return new Vector2(x, y);
    }
}