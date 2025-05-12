using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;


/// <summary>
/// Custom UI element for displaying graphs
/// </summary>
[UxmlElement]
public partial class GraphElement : VisualElement
{
    /// <summary>
    /// Place graph zero at the middle line
    /// </summary>
    [UxmlAttribute, CreateProperty]
    public bool middle_zero = false;

    /// <summary>
    /// Connect graph end to zero
    /// </summary>
    [UxmlAttribute, CreateProperty]
    public bool pretty_finish = false;

    /// <summary>
    /// Draw line where graph crosses zero
    /// </summary>    
    [UxmlAttribute, CreateProperty]
    public bool draw_zero_crossing = false;

    /// <summary>
    /// Graph x axis start offset
    /// </summary>
    [UxmlAttribute, CreateProperty]
    public float start_offset = 10;

    /// <summary>
    /// Main graph line color
    /// </summary>    
    [UxmlAttribute, CreateProperty]
    public Color line_color = Color.black;

    /// <summary>
    /// Graph click color
    /// </summary>
    [UxmlAttribute, CreateProperty]
    public Color click_color = Color.red;
  
    /// <summary>
    /// Stored graph points
    /// </summary>  
    [SerializeField, DontCreateProperty]
    Vector2[] _points = new Vector2[0];

    /// <summary>
    /// Graph points interface
    /// </summary>
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

    /// <summary>
    /// Stored mouse clicks
    /// </summary>
    [SerializeField, DontCreateProperty]
    double[] _clicks = new double[0];

    /// <summary>
    /// Interface for setting mouse clicks
    /// </summary>
    [UxmlAttribute, CreateProperty]
    public double[] Clicks
    {
        get => _clicks;
        set
        {
            _clicks = value;
            MarkDirtyRepaint();
        }
    }

    /// <summary>
    /// Stored graph points
    /// </summary>
    [SerializeField, DontCreateProperty]
    double[] _data = new double[0];

    /// <summary>
    /// Graph points interface
    /// </summary>
    [UxmlAttribute, CreateProperty]
    public double[] Data
    {
        get => _data;
        set
        {
            _data = value;
            MarkDirtyRepaint();
        }
    }

    /// <summary>
    /// UI element constructor
    /// </summary>
    public GraphElement()
    {
        generateVisualContent += GenerateVisualContent;
    }

    /// <summary>
    /// Apply normalization vector to passed points
    /// </summary>
    /// <param name="point"> Point to be normalized </param>
    /// <param name="normalization_vector"> Normalization vector </param>
    /// <returns> Normalized point </returns>
    private Vector2 NormalizePoint(Vector2 point, Vector2 normalization_vector)
    {
        return new Vector2(point.x * normalization_vector.x, point.y * normalization_vector.y);
    }

    /// <summary>
    /// Draw graph
    /// </summary>
    /// <param name="context"> graphics context </param>
    private void GenerateVisualContent(MeshGenerationContext context)
    {
        Painter2D painter = context.painter2D;

        // Get graph element size
        float width = contentRect.width - start_offset;
        float height = contentRect.height;

        // Basic data validation
        if(_clicks.Length == 0 || _points.Length == 0)
            return;

        // Compute bounding box
        double duration = _clicks[^1] - _clicks[0];
        float max_y = float.NegativeInfinity;
        float min_y = float.PositiveInfinity;

        foreach (Vector2 point in _points)
        {
            max_y = math.max(point.y, max_y);
            min_y = math.min(point.y, min_y);
        }
        
        // Compute vector for normalization, offset and first point
        Vector2 normalization_vector = new(
            1 / (float)duration * width, 
            1 / (max_y-min_y) * height / (middle_zero ? 2 : 1));

        Vector2 offset_vector = NormalizePoint(new Vector2(0, min_y), normalization_vector);
        Vector2 past = NormalizePoint(_points[0], normalization_vector);

        // Draw clicks
        painter.strokeColor = click_color;
        foreach (double click_time in Clicks)
        {
            double scaled_x = click_time / duration * width + start_offset;
            Vector2 bottom = new((float)scaled_x , 0);
            Vector2 top = new((float)scaled_x, height);

            painter.BeginPath();
            painter.MoveTo(bottom);
            painter.LineTo(top);
            painter.Stroke();
        }

        // Draw peaks
        painter.strokeColor = Color.red;
        foreach (double data in Data)
        {
            double scaled_x = data / duration * width + start_offset;
            Vector2 bottom = new((float)scaled_x , 0);
            Vector2 top = new((float)scaled_x, height);

            painter.BeginPath();
            painter.MoveTo(bottom);
            painter.LineTo(top);
            painter.Stroke();
        }

        if(draw_zero_crossing)
        {
            Vector2 next = NormalizePoint(Vector2.zero, normalization_vector);
            Vector2 middle_line = new Vector2(next.x + start_offset, (middle_zero ? height / 2 : height) - next.y) + offset_vector;            


            painter.BeginPath();
            painter.MoveTo(new Vector2(0, middle_line.y));
            painter.LineTo(new Vector2(width + start_offset, middle_line.y));
            painter.Stroke();
        }

        // Draw graph
        painter.strokeColor = line_color;
        painter.lineWidth = 1f;
        painter.BeginPath();

        if(pretty_finish)
        {
            painter.MoveTo(new Vector2(0, middle_zero ? height / 2 : height));
            painter.LineTo(new Vector2(past.x + start_offset, (middle_zero ? height / 2 : height) - past.y) + offset_vector);
        }
        else
            painter.MoveTo(new Vector2(past.x + start_offset, (middle_zero ? height / 2 : height) - past.y) + offset_vector);


        for (int i = 1; i < _points.Length; i++)
        {
            Vector2 next = NormalizePoint(_points[i], normalization_vector);
            painter.LineTo(new Vector2(next.x + start_offset, (middle_zero ? height / 2 : height) - next.y) + offset_vector);            
        }
        if(pretty_finish)
            painter.LineTo(new Vector2(width + start_offset, middle_zero ? height / 2 : height));
        painter.Stroke();


       
    }
}
