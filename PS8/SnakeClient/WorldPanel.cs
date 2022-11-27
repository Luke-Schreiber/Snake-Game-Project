using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using IImage = Microsoft.Maui.Graphics.IImage;
#if MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#else
using Microsoft.Maui.Graphics.Win2D;
#endif
using Color = Microsoft.Maui.Graphics.Color;
using System.Reflection;
using Microsoft.Maui;
using System.Net;
using Font = Microsoft.Maui.Graphics.Font;
using SizeF = Microsoft.Maui.Graphics.SizeF;

namespace SnakeGame;
public class WorldPanel : IDrawable
{
    private IImage wall;
    private IImage powerup;
    private IImage background;

    private bool initializedForDrawing = false;

    private int WallWidth = 50;
    private int SnakeWidth = 10;
    private int PowerWidth = 16;

#if MACCATALYST
    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeGame.Resources.Images";
        return PlatformImage.FromStream(assembly.GetManifestResourceStream($"{path}.{name}"));
    }
#else
  private IImage loadImage( string name )
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeGame.Resources.Images";
        var service = new W2DImageLoadingService();
        return service.FromStream( assembly.GetManifestResourceStream( $"{path}.{name}" ) );
    }
#endif

    public WorldPanel()
    {
    }

    private void InitializeDrawing()
    {
        wall = loadImage( "WallSprite.png" );
        powerup = loadImage("powerup.png");
        background = loadImage( "Background.png" );
        initializedForDrawing = true;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (!initializedForDrawing)
        {
            InitializeDrawing();
        }
        // undo previous transformations from last frame
        canvas.ResetState();

        // Draw the background
        canvas.DrawImage(background, 0, 0, World.WorldSize, World.WorldSize);

        // Draw the Walls
        foreach (Wall w in World.Walls)
        {
            DrawWalls(canvas, w, dirtyRect);
        }
        lock (World.WorldState)
        {

            // Draw the Powerups
            foreach (Powerup p in World.Powerups)
            {
                if (!p.Died)
                    canvas.DrawImage(powerup, (float)(World.WorldSize / 2 + p.Loc.X - (PowerWidth/2)), (float)(World.WorldSize / 2 + p.Loc.Y - (PowerWidth / 2)), PowerWidth, PowerWidth);
            }

            // Draw the Players
            foreach (Snake s in World.Snakes)
            {
                if (!s.Died)
                    DrawSnakes(canvas, s, dirtyRect);
            }
        }
    }

    private void DrawWalls(ICanvas canvas, Wall w, RectF dirtyRect)
    {
        canvas.DrawImage(wall, (float)(World.WorldSize / 2 + w.P1.X - (WallWidth/2)), (float)(World.WorldSize / 2 + w.P1.Y - (WallWidth/2)), WallWidth, WallWidth);
        canvas.DrawImage(wall, (float)(World.WorldSize / 2 + w.P2.X - (WallWidth/2)), (float)(World.WorldSize / 2 + w.P2.Y - (WallWidth/2)), WallWidth, WallWidth);

        for (double i = w.P1.X+WallWidth; i <= w.P2.X-WallWidth; i+=WallWidth)
        {
            canvas.DrawImage(wall, (float)(World.WorldSize / 2 + i - (WallWidth/2)), (float)(World.WorldSize / 2 + w.P1.Y - (WallWidth/2)), WallWidth, WallWidth);
        }
        for (double i = w.P1.Y+WallWidth; i <= w.P2.Y-WallWidth; i += WallWidth)
        {
            canvas.DrawImage(wall, (float)(World.WorldSize / 2 + w.P2.X - (WallWidth/2)), (float)(World.WorldSize / 2 + i - (WallWidth/2)), WallWidth, WallWidth);
        }
        for (double i = w.P1.X - WallWidth; i >= w.P2.X + WallWidth; i -= WallWidth)
        {
            canvas.DrawImage(wall, (float)(World.WorldSize / 2 + i - (WallWidth/2)), (float)(World.WorldSize / 2 + w.P1.Y - (WallWidth/2)), WallWidth, WallWidth);
        }
        for (double i = w.P1.Y - WallWidth; i >= w.P2.Y + WallWidth; i -= WallWidth)
        {
            canvas.DrawImage(wall, (float)(World.WorldSize / 2 + w.P2.X - (WallWidth/2)), (float)(World.WorldSize / 2 + i - (WallWidth/2)), WallWidth, WallWidth);
        }
    }

    private void DrawSnakes(ICanvas canvas, Snake s, RectF dirtyRect)
    {
        canvas.FillColor = Colors.Red;
        for (int i = 0; i < s.Body.Count; i++)
        {
            canvas.FillRoundedRectangle((float)(World.WorldSize / 2 + s.Body[i].X - (SnakeWidth / 2)), (float)(World.WorldSize / 2 + s.Body[i].Y - (SnakeWidth / 2)), SnakeWidth, SnakeWidth, SnakeWidth/2);
            if (i != 0)
            {
                //vertical case
                if (s.Body[i].X - s.Body[i - 1].X == 0)
                    canvas.FillRectangle((float)(World.WorldSize / 2 + s.Body[i].X - (SnakeWidth / 2)), (float)(World.WorldSize / 2 + s.Body[i].Y), SnakeWidth, -(float)(s.Body[i].Y - s.Body[i - 1].Y));
                //horizontal case
                else
                    canvas.FillRectangle((float)(World.WorldSize / 2 + s.Body[i].X), (float)(World.WorldSize / 2 + s.Body[i].Y - (SnakeWidth / 2)), -(float)(s.Body[i].X - s.Body[i - 1].X), SnakeWidth);

                if (i == s.Body.Count - 1)
                    canvas.DrawString(s.Name + ": " + s.Score, (float)(World.WorldSize / 2 + s.Body[i].X - (SnakeWidth / 2)), (float)(World.WorldSize / 2 + s.Body[i].Y - (SnakeWidth / 2)-10), HorizontalAlignment.Center);
            }
        }
    }
    
}
