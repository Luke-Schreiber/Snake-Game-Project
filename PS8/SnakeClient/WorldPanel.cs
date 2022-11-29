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
using WinRT;

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

        // Lock tied to the adding of new objects from the controller, preventing things from being added at the same time
        // that they have to be drawn
        lock (World.WorldState)
        {
            
            // if statement used to make the view window follow the players snake
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
                // draw a player if they are alive
                if (s.Value.Alive)
                {
                    DrawSnakes(canvas, s.Value, dirtyRect);
                }
                // if a player is no longer alive, play the death animation
                else if (!s.Value.Alive)
                {
                    DeathExplosion(canvas, s.Value, dirtyRect);
                }
            }
        }
    }

    /// <summary>
    /// Private helper method used to draw walls. Uses a given wall object to draw the endpoints of the wall along with all the sections inbetween
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="w"></param>
    /// <param name="dirtyRect"></param>
    private void DrawWalls(ICanvas canvas, Wall w, RectF dirtyRect)
    {
        // draw the 2 end sections of the wall
        canvas.DrawImage(wall, (float)(World.WorldSize / 2 + w.P1.X - (WallWidth/2)), (float)(World.WorldSize / 2 + w.P1.Y - (WallWidth/2)), WallWidth, WallWidth);
        canvas.DrawImage(wall, (float)(World.WorldSize / 2 + w.P2.X - (WallWidth/2)), (float)(World.WorldSize / 2 + w.P2.Y - (WallWidth/2)), WallWidth, WallWidth);

        // only one of these for loops will run at a time, depending on the relative locations of the 2 wall end sections;
        // when p2 is above,below,left, or right of p1
        for (double i = w.P1.X+WallWidth/2; i <= w.P2.X-WallWidth/2; i+=WallWidth)
        {
            canvas.DrawImage(wall, (float)(World.WorldSize / 2 + i - (WallWidth/2)), (float)(World.WorldSize / 2 + w.P1.Y - (WallWidth/2)), WallWidth, WallWidth);
        }
        for (double i = w.P1.Y+WallWidth/2; i <= w.P2.Y-WallWidth / 2; i += WallWidth)
        {
            canvas.DrawImage(wall, (float)(World.WorldSize / 2 + w.P2.X - (WallWidth/2)), (float)(World.WorldSize / 2 + i - (WallWidth/2)), WallWidth, WallWidth);
        }
        for (double i = w.P1.X - WallWidth/2; i >= w.P2.X + WallWidth/2; i -= WallWidth)
        {
            canvas.DrawImage(wall, (float)(World.WorldSize / 2 + i - (WallWidth/2)), (float)(World.WorldSize / 2 + w.P1.Y - (WallWidth/2)), WallWidth, WallWidth);
        }
        for (double i = w.P1.Y - WallWidth/2; i >= w.P2.Y + WallWidth/2; i -= WallWidth)
        {
            canvas.DrawImage(wall, (float)(World.WorldSize / 2 + w.P2.X - (WallWidth/2)), (float)(World.WorldSize / 2 + i - (WallWidth/2)), WallWidth, WallWidth);
        }
    }

    /// <summary>
    /// Private helper method for drawing snakes, draws the vertices of a snakes body and then draws rectangles to fill the spaces between them
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="s"></param>
    /// <param name="dirtyRect"></param>
    private void DrawSnakes(ICanvas canvas, Snake s, RectF dirtyRect)
    {
        // Retrieve the current snake order from World.
        int order = World.SnakeOrder[s.ID]%8;

        // Every 8 Snakes is painted with a different color.
        if (order == 0)
            canvas.FillColor = Colors.Red;
        else if (order == 1)
            canvas.FillColor = Colors.Coral;
        else if (order == 2)
            canvas.FillColor = Colors.Teal;
        else if (order == 3)
            canvas.FillColor = Colors.Yellow;
        else if (order == 4)
            canvas.FillColor = Colors.Pink;
        else if (order == 5)
            canvas.FillColor = Colors.Purple;
        else if (order == 6)
            canvas.FillColor = Colors.DimGray;
        else if (order == 7)
            canvas.FillColor = Colors.Azure;
        else if (order == 8)
            canvas.FillColor = Colors.PeachPuff;

        // for each vertex in a snakes body, draw it and then draw a rectangle connection back to the previous vertex
        for (int i = 0; i < s.Body.Count; i++)
        {
            // draw the vertices of the snake
            canvas.FillRoundedRectangle((float)(World.WorldSize / 2 + s.Body[i].X - (SnakeWidth / 2)), (float)(World.WorldSize / 2 + s.Body[i].Y - (SnakeWidth / 2)), SnakeWidth, SnakeWidth, SnakeWidth/2);

            // draw connections back to previous vertices, with in if statement to not attempt this at the tail
            if (i != 0)
            {
                // if statement to make sure snakes wrap around the world correctly and a section is not drawn over the whole map
                if (WorldWrapHelper(s.Body[i], s.Body[i-1]))
                {
                    // vertical case for when the 2 vertices are above or below eachother
                    if (s.Body[i].X - s.Body[i - 1].X == 0)
                        canvas.FillRectangle((float)(World.WorldSize / 2 + s.Body[i].X - (SnakeWidth / 2)), (float)(World.WorldSize / 2 + s.Body[i].Y), SnakeWidth, -(float)(s.Body[i].Y - s.Body[i - 1].Y));
                    // horizontal case for when the 2 vertices are left or right of eachother
                    else
                        canvas.FillRectangle((float)(World.WorldSize / 2 + s.Body[i].X), (float)(World.WorldSize / 2 + s.Body[i].Y - (SnakeWidth / 2)), -(float)(s.Body[i].X - s.Body[i - 1].X), SnakeWidth);
                }

                // draws the name and score of the snake under its head
                if (i == s.Body.Count - 1)
                    canvas.DrawString(s.Name + ": " + s.Score, (float)(World.WorldSize / 2 + s.Body[i].X - (SnakeWidth / 2)), (float)(World.WorldSize / 2 + s.Body[i].Y - (SnakeWidth / 2)-10), HorizontalAlignment.Center);
            }
        }
    }

    /// <summary>
    /// First private helper for drawing the explosion animation for when a snake dies. The final effect is for
    /// an explosion animation to play at each vertex of a snake, and at many points in between each vertex
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="s"></param>
    /// <param name="dirtyRect"></param>
    private void DeathExplosion(ICanvas canvas, Snake s, RectF dirtyRect)
    {
        // for each vertex of a snake, draw the explosion animations at and between them
        for (int i = 1; i < s.Body.Count; i++)
        {
            // this series of if statements uses the framesSinceDeath counter for snakes to determine which stage of the explosion
            // animation they are in so it knows which one to draw, and then calls ExplosionsInMiddle to actually draw them
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
        //vertical up case for drawing explosions at and between two points when this snake section was moving up
        // at the time it died
        if (p1.X - p2.X == 0 && p1.Y < p2.Y)
        {
            // if statement to make sure explosions are not drawn over the whole map if a snake has wrapped from side to side
            if (WorldWrapHelper(p1, p2))
            {
                for (double i = p1.Y; i < p2.Y; i += 20)
                {
                    canvas.DrawImage(image, (float)(World.WorldSize / 2 + p1.X - 20), (float)(World.WorldSize / 2 + i - 20), 40, 40);
                }
            }
        }
        // vertical down case for drawing explosions at and between two points when this snake section was moving down
        // at the time it died
        if (p1.X - p2.X == 0 && p1.Y > p2.Y)
        {
            // if statement to make sure explosions are not drawn over the whole map if a snake has wrapped from side to side
            if (WorldWrapHelper(p1, p2))
            {
                for (double i = p1.Y; i > p2.Y; i -= 20)
                {
                    canvas.DrawImage(image, (float)(World.WorldSize / 2 + p1.X - 20), (float)(World.WorldSize / 2 + i - 20), 40, 40);
                }
            }
        }
        // horizontal right case for drawing explosions at and between two points when this snake section was moving right
        // at the time it died
        if (p1.Y - p2.Y == 0 && p1.X > p2.X)
        {
            // if statement to make sure explosions are not drawn over the whole map if a snake has wrapped from side to side
            if (WorldWrapHelper(p1, p2))
            {
                for (double i = p1.X; i > p2.X; i -= 20)
                {
                    canvas.DrawImage(image, (float)(World.WorldSize / 2 + i - 20), (float)(World.WorldSize / 2 + p1.Y - 20), 40, 40);
                }
            }
        }
        //horizontal left case for drawing explosions at and between two points when this snake section was moving right
        // at the time it died
        if (p1.Y - p2.Y == 0 && p1.X < p2.X)
        {
            // if statement to make sure explosions are not drawn over the whole map if a snake has wrapped from side to side
            if (WorldWrapHelper(p1, p2))
            {
                for (double i = p1.X; i < p2.X; i += 20)
                {
                    canvas.DrawImage(image, (float)(World.WorldSize / 2 + i - 20), (float)(World.WorldSize / 2 + p1.Y - 20), 40, 40);
                }
            }
        }
    }

    /// <summary>
    /// Helper method for draw methods, used to determine if a snake has wrapped to the other side of the world and they
    /// should not be drawn over the whole thing
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    private bool WorldWrapHelper(Vector2D p1, Vector2D p2)
    {
        return Math.Abs(p1.X - p2.X) < World.WorldSize && Math.Abs(p1.Y - p2.Y) < World.WorldSize;
    }
}
