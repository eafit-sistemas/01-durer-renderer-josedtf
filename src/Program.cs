using System;
using SkiaSharp;

public static class Program
{
    public static void Main()
    {
        InputData data = InputData.LoadFromJson("input.json");
        Shape2D projected = ProjectShape(data.Model);
        projected.Print(); // The tests check for the correct projected data to be printed
        Render(projected, data.Parameters, "output.jpg");
    }

    private static void Render(Shape2D shape, RenderParameters parameters, string outputPath)
    {
        int size = parameters.Resolution;

        using var bitmap = new SKBitmap(size, size);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var paint = new SKPaint
        {
            Color = SKColors.Black,
            StrokeWidth = 2,
            IsAntialias = true
        };

        int n = shape.Points.Length;
        SKPoint[] screenPoints = new SKPoint[n];

        for (int i = 0; i < n; i++)
        {
            float x = shape.Points[i][0];
            float y = shape.Points[i][1];

            // Transformación a coordenadas de pantalla
            float sx = (x - parameters.XMin) / (parameters.XMax - parameters.XMin) * size;
            float sy = (parameters.YMax - y) / (parameters.YMax - parameters.YMin) * size;

            screenPoints[i] = new SKPoint(sx, sy);
        }

        // Dibujar líneas
        foreach (var line in shape.Lines)
        {
            int i1 = line[0];
            int i2 = line[1];

            canvas.DrawLine(screenPoints[i1], screenPoints[i2], paint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        using var stream = File.OpenWrite(outputPath);
        data.SaveTo(stream);
    }

    private static Shape2D ProjectShape(Model3D model)
    {
        int n = model.VertexTable.Length;
        float[][] points2D = new float[n][];

        for (int i = 0; i < n; i++)
        {
            float x = model.VertexTable[i][0];
            float y = model.VertexTable[i][1];
            float z = model.VertexTable[i][2];

            // Proyección perspectiva simple
            float xp = x / z;
            float yp = y / z;

            points2D[i] = new float[] { xp, yp };
        }

        return new Shape2D
        {
            Points = points2D,
            Lines = model.EdgeTable
        };
    }
}

