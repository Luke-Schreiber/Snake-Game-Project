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
    private IImage fire1;
    private IImage fire2;
    private IImage fire3;
    private IImage fire4;
    private IImage fire5;
    private IImage fire6;

    private bool initializedForDrawing = false;

    private int WallWidth = 50;
    private int SnakeWidth = 10;
    private int PowerWidth = 16;
    private int ViewSize = 900;
    private int ExplosionFrame = 0;

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
        fire1 = loadImage("fire1.png");
        fire2 = loadImage("fire2.png");
        fire3 = loadImage("fire3.png");
        fire4 = loadImage("fire4.png");
        fire5 = loadImage("fire5.png");
        fire6 = loadImage("fire6.png");

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

        float playerX;
        float playerY;

        lock (World.WorldState)
        {
            
            if (World.Snakes.ContainsKey(Controller.getID()))
            {
                Snake s = World.Snakes[Controller.getID()];
                playerX = World.WorldSize / 2 + (float)s.Body[s.Body.Count - 1].X;
                playerY = World.WorldSize / 2 + (float)s.Body[s.Body.Count - 1].Y;
                canvas.Translate(-playerX + (ViewSize / 2), -playerY + (ViewSize / 2));
            }
            

            // Draw the background
            canvas.DrawImage(background, 0, 0, World.WorldSize, World.WorldSize);

            // Draw the Walls
            foreach (Wall w in World.Walls)
            {
                DrawWalls(canvas, w, dirtyRect);
            }

            // Draw the Powerups
            foreach (Powerup p in World.Powerups)
            {
                if (!p.Died)
                    canvas.DrawImage(powerup, (float)(World.WorldSize / 2 + p.Loc.X - (PowerWidth/2)), (float)(World.WorldSize / 2 + p.Loc.Y - (PowerWidth / 2)), PowerWidth, PowerWidth);
            }

            // Draw the Players
            foreach (var s in World.Snakes)
            {
                if (s.Value.Alive)
                {
                    DrawSnakes(canvas, s.Value, dirtyRect);
                }
                else if (!s.Value.Alive)
                {
                    DeathExplosion(canvas, s.Value, dirtyRect);
                }
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

    private void DeathExplosion(ICanvas canvas, Snake s, RectF dirtyRect)
    {
        for (int i = 1; i < s.Body.Count; i++)
        {

            if (World.FramesSinceDeath[s.ID] < 5)
            {
                //canvas.DrawImage(fire1, (float)(World.WorldSize / 2 + s.Body[i].X - 20), (float)(World.WorldSize / 2 + s.Body[i].Y - 20), 40, 40);
                ExplosionsInMiddle(canvas, s.Body[i], s.Body[i - 1], fire1, dirtyRect);
            }
            else if (World.FramesSinceDeath[s.ID] < 10)
            {
                ExplosionsInMiddle(canvas, s.Body[i], s.Body[i - 1], fire2, dirtyRect);
            }
            //canvas.DrawImage(fire2, (float)(World.WorldSize / 2 + s.Body[i].X - 20), (float)(World.WorldSize / 2 + s.Body[i].Y - 20), 40, 40);
            else if (World.FramesSinceDeath[s.ID] < 15)
            {
                ExplosionsInMiddle(canvas, s.Body[i], s.Body[i - 1], fire3, dirtyRect);
            }
            //canvas.DrawImage(fire3, (float)(World.WorldSize / 2 + s.Body[i].X - 20), (float)(World.WorldSize / 2 + s.Body[i].Y - 20), 40, 40);
            else if (World.FramesSinceDeath[s.ID] < 20)
            {
                ExplosionsInMiddle(canvas, s.Body[i], s.Body[i - 1], fire4, dirtyRect);
            }
            //canvas.DrawImage(fire4, (float)(World.WorldSize / 2 + s.Body[i].X - 20), (float)(World.WorldSize / 2 + s.Body[i].Y - 20), 40, 40);
            else if (World.FramesSinceDeath[s.ID] < 25)
            {
                ExplosionsInMiddle(canvas, s.Body[i], s.Body[i - 1], fire5, dirtyRect);
            }
            //canvas.DrawImage(fire5, (float)(World.WorldSize / 2 + s.Body[i].X - 20), (float)(World.WorldSize / 2 + s.Body[i].Y - 20), 40, 40);
            else if (World.FramesSinceDeath[s.ID] < 30)
            {
                ExplosionsInMiddle(canvas, s.Body[i], s.Body[i - 1], fire6, dirtyRect);
            }
            //canvas.DrawImage(fire6, (float)(World.WorldSize / 2 + s.Body[i].X - 20), (float)(World.WorldSize / 2 + s.Body[i].Y - 20), 40, 40);
            
        }
    }

    private void ExplosionsInMiddle(ICanvas canvas, Vector2D p1, Vector2D p2, IImage image, RectF dirtyRect)
    {
        //vertical up case
        if (p1.X - p2.X == 0 && p1.Y < p2.Y)
        {
            for (double i = p1.Y; i < p2.Y; i+= 20)
            {
                canvas.DrawImage(image, (float)(World.WorldSize / 2 + p1.X - 20), (float)(World.WorldSize / 2 + i - 20), 40, 40);
            }
        }
        //vertical down case
        if (p1.X - p2.X == 0 && p1.Y > p2.Y)
        {
            for (double i = p1.Y; i > p2.Y; i -= 20)
            {
                canvas.DrawImage(image, (float)(World.WorldSize / 2 + p1.X - 20), (float)(World.WorldSize / 2 + i - 20), 40, 40);
            }
        }
        //horizontal right case
        if (p1.Y - p2.Y == 0 && p1.X > p2.X)
        {
            for (double i = p1.X; i > p2.X; i -= 20)
            {
                canvas.DrawImage(image, (float)(World.WorldSize / 2 + i - 20), (float)(World.WorldSize / 2 + p1.Y - 20), 40, 40);
            }
        }
        //horizontal left case
        if (p1.Y - p2.Y == 0 && p1.X < p2.X)
        {
            for (double i = p1.X; i < p2.X; i += 20)
            {
                canvas.DrawImage(image, (float)(World.WorldSize / 2 + i - 20), (float)(World.WorldSize / 2 + p1.Y - 20), 40, 40);
            }
        }
    }
}
