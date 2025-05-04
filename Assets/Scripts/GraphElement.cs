using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class GraphElement : VisualElement
{
    [SerializeField, DontCreateProperty]
    Vector2[] _points = new Vector2[0];


    [UxmlAttribute, CreateProperty]
    public Vector2[] Points
    {
        get => _points;
        set
        {
            _points = value;
            MarkDirtyRepaint();
        }
    }

    public GraphElement()
    {
        generateVisualContent += GenerateVisualContent;
    }

    private Vector2 NormalizePoint(Vector2 point, Vector2 normalization_vector)
    {
        return new Vector2(point.x * normalization_vector.x, point.y * normalization_vector.y);
    }

    private void GenerateVisualContent(MeshGenerationContext context)
    {
        float width = contentRect.width;
        float height = contentRect.height;

        float max_y = 0;
        float max_x = 0;

        foreach (Vector2 point in _points)
        {
            max_y = math.max(point.y, max_y);
            max_x = math.max(point.x, max_x);
        }

        if(max_x == 0 || max_y == 0 || _points.Length == 0)
            return;

        Painter2D painter = context.painter2D;
        painter.strokeColor = Color.blue;
        painter.lineWidth = 1f;
        painter.BeginPath();

        Vector2 normalization_vector = new(1/max_x * width, 1/max_y * height);

        Vector2 past = NormalizePoint(_points[0], normalization_vector);
        painter.MoveTo(new Vector2(past.x, height - past.y));

        for (int i = 1; i < _points.Length; i++)
        {
            Vector2 next = NormalizePoint(_points[i], normalization_vector);
            painter.LineTo(new Vector2(next.x, height - next.y));            
        }

        painter.Stroke();
    }
}
