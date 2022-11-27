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
        foreach(Wall w in World.Walls)
         {
             DrawWalls(canvas, w, dirtyRect);
         }

        // Draw the Powerups
        foreach (Powerup p in World.Powerups)
        {
            canvas.DrawImage(powerup, (float)(World.WorldSize / 2 + p.Loc.X - 5), (float)(World.WorldSize / 2 + p.Loc.Y - 5), 10, 10);
        }
            // Draw the Players
        }
    
    private void DrawWalls(ICanvas canvas, Wall w, RectF dirtyRect)
    {
        canvas.DrawImage(wall, (float)(World.WorldSize/2 + w.P1.X -25), (float)(World.WorldSize/2 + w.P1.Y - 25), 50, 50);
        canvas.DrawImage(wall, (float)(World.WorldSize/2 + w.P2.X - 25), (float)(World.WorldSize/2 + w.P2.Y - 25), 50, 50);
    }
}
