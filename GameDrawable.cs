namespace PacmanMobile;

public sealed class GameDrawable(GameEngine game) : IDrawable
{
    private readonly GameEngine game = game;
    private static readonly Color Background = Color.FromArgb("#090B18");
    private static readonly Color Wall = Color.FromArgb("#2952CC");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Background;
        canvas.FillRectangle(dirtyRect);

        float scale = MathF.Min(dirtyRect.Width / game.Columns, dirtyRect.Height / game.Rows);
        float boardWidth = scale * game.Columns;
        float boardHeight = scale * game.Rows;
        float offsetX = (dirtyRect.Width - boardWidth) / 2F;
        float offsetY = (dirtyRect.Height - boardHeight) / 2F;

        for (int y = 0; y < game.Rows; y++)
        {
            for (int x = 0; x < game.Columns; x++)
            {
                float px = offsetX + x * scale;
                float py = offsetY + y * scale;
                char tile = game.TileAt(x, y);

                if (tile == '#')
                {
                    canvas.FillColor = Wall;
                    canvas.FillRoundedRectangle(px + 2, py + 2, scale - 4, scale - 4, scale * 0.2F);
                }
                else if (tile is '.' or 'o')
                {
                    canvas.FillColor = Colors.White;
                    float radius = tile == 'o' ? scale * 0.18F : MathF.Max(2F, scale * 0.07F);
                    canvas.FillCircle(px + scale / 2F, py + scale / 2F, radius);
                }
            }
        }

        DrawPlayer(canvas, offsetX, offsetY, scale);
        for (int index = 0; index < game.Ghosts.Count; index++)
            DrawGhost(canvas, game.Ghosts[index], index, offsetX, offsetY, scale);

        if (!game.Running)
        {
            string message = game.Won ? "¡GANASTE!" : game.GameOver ? "FIN DEL JUEGO" : game.Paused ? "PAUSA" : "TOCA UNA FLECHA";
            canvas.FillColor = Color.FromRgba(0, 0, 0, 175);
            canvas.FillRectangle(offsetX, offsetY, boardWidth, boardHeight);
            canvas.FontColor = Colors.White;
            canvas.FontSize = MathF.Max(18F, scale * 0.8F);
            canvas.Font = new Microsoft.Maui.Graphics.Font("Arial", 700);
            canvas.DrawString(message, offsetX, offsetY, boardWidth, boardHeight,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }

    private void DrawPlayer(ICanvas canvas, float offsetX, float offsetY, float scale)
    {
        float cx = offsetX + game.Player.X * scale + scale / 2F;
        float cy = offsetY + game.Player.Y * scale + scale / 2F;
        float radius = scale * 0.4F;
        canvas.FillColor = Colors.Gold;
        canvas.FillCircle(cx, cy, radius);

        float angle = game.Player.Facing switch
        {
            Direction.Right => 0F,
            Direction.Down => 90F,
            Direction.Left => 180F,
            Direction.Up => 270F,
            _ => 0F
        };
        float opening = 17F + MathF.Abs(MathF.Sin(game.AnimationPhase)) * 26F;
        PointF p1 = Polar(cx, cy, radius + 1, angle - opening);
        PointF p2 = Polar(cx, cy, radius + 1, angle + opening);
        PathF mouth = new();
        mouth.MoveTo(cx, cy);
        mouth.LineTo(p1.X, p1.Y);
        mouth.LineTo(p2.X, p2.Y);
        mouth.Close();
        canvas.FillColor = Background;
        canvas.FillPath(mouth);
    }

    private void DrawGhost(ICanvas canvas, Ghost ghost, int index, float offsetX, float offsetY, float scale)
    {
        float cx = offsetX + ghost.X * scale + scale / 2F;
        float cy = offsetY + ghost.Y * scale + scale / 2F;
        float radius = scale * 0.36F;
        canvas.FillColor = game.Frightened ? Colors.RoyalBlue : index % 2 == 0 ? Colors.OrangeRed : Colors.HotPink;
        canvas.FillCircle(cx, cy - scale * 0.08F, radius);
        canvas.FillRectangle(cx - radius, cy - scale * 0.08F, radius * 2, radius * 0.9F);

        PathF feet = new();
        feet.MoveTo(cx - radius, cy + radius * 0.75F);
        feet.LineTo(cx - radius * 0.5F, cy + radius * 0.35F);
        feet.LineTo(cx, cy + radius * 0.75F);
        feet.LineTo(cx + radius * 0.5F, cy + radius * 0.35F);
        feet.LineTo(cx + radius, cy + radius * 0.75F);
        feet.Close();
        canvas.FillPath(feet);

        canvas.FillColor = Colors.White;
        canvas.FillCircle(cx - radius * 0.35F, cy - radius * 0.25F, radius * 0.13F);
        canvas.FillCircle(cx + radius * 0.35F, cy - radius * 0.25F, radius * 0.13F);
    }

    private static PointF Polar(float cx, float cy, float radius, float degrees)
    {
        float radians = degrees * MathF.PI / 180F;
        return new PointF(cx + MathF.Cos(radians) * radius, cy + MathF.Sin(radians) * radius);
    }
}
