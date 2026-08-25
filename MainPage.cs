namespace PacmanMobile;

public sealed class MainPage : ContentPage
{
    private readonly GameEngine game = new();
    private readonly GraphicsView board;
    private readonly Label scoreLabel;
    private readonly Label livesLabel;
    private readonly Label stateLabel;
    private DateTime lastFrame = DateTime.UtcNow;
    private double accumulatedMilliseconds;

    public MainPage()
    {
        Title = "Pacman Mobile";
        BackgroundColor = Color.FromArgb("#090B18");
        Padding = new Thickness(14, 18, 14, 14);

        scoreLabel = MakeStatusLabel("PUNTOS  0", LayoutOptions.Start);
        stateLabel = MakeStatusLabel("LISTO", LayoutOptions.Center);
        livesLabel = MakeStatusLabel("● ● ●", LayoutOptions.End);

        Grid status = new()
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            Margin = new Thickness(2, 0, 2, 8)
        };
        status.Add(scoreLabel, 0);
        status.Add(stateLabel, 1);
        status.Add(livesLabel, 2);

        board = new GraphicsView
        {
            Drawable = new GameDrawable(game),
            BackgroundColor = Color.FromArgb("#090B18"),
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            MinimumHeightRequest = 280
        };

        Button up = DirectionButton("▲", Direction.Up);
        Button left = DirectionButton("◀", Direction.Left);
        Button down = DirectionButton("▼", Direction.Down);
        Button right = DirectionButton("▶", Direction.Right);

        Grid directionPad = new()
        {
            RowDefinitions =
            {
                new RowDefinition(58),
                new RowDefinition(58)
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(72),
                new ColumnDefinition(72),
                new ColumnDefinition(72)
            },
            ColumnSpacing = 7,
            RowSpacing = 7,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10, 0, 8)
        };
        directionPad.Add(up, 1, 0);
        directionPad.Add(left, 0, 1);
        directionPad.Add(down, 1, 1);
        directionPad.Add(right, 2, 1);

        Button playButton = ActionButton("JUGAR", Color.FromArgb("#2952CC"));
        Button pauseButton = ActionButton("PAUSA", Color.FromArgb("#303548"));
        Button resetButton = ActionButton("REINICIAR", Color.FromArgb("#303548"));
        playButton.Clicked += (_, _) => { game.Start(); UpdateStatus(); };
        pauseButton.Clicked += (_, _) => { game.TogglePause(); UpdateStatus(); };
        resetButton.Clicked += (_, _) => { game.Reset(); UpdateStatus(); board.Invalidate(); };

        Grid actions = new()
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 8
        };
        actions.Add(playButton, 0);
        actions.Add(pauseButton, 1);
        actions.Add(resetButton, 2);

        Grid layout = new()
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            }
        };
        layout.Add(status, 0, 0);
        layout.Add(board, 0, 1);
        layout.Add(directionPad, 0, 2);
        layout.Add(actions, 0, 3);
        Content = layout;

        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(40), GameLoop);
    }

    private bool GameLoop()
    {
        DateTime now = DateTime.UtcNow;
        accumulatedMilliseconds += (now - lastFrame).TotalMilliseconds;
        lastFrame = now;

        if (game.Running)
        {
            while (accumulatedMilliseconds >= 185)
            {
                game.Step();
                accumulatedMilliseconds -= 185;
            }
            UpdateStatus();
        }
        else
        {
            accumulatedMilliseconds = 0;
        }

        game.AnimationPhase += 0.24F;
        board.Invalidate();
        return true;
    }

    private Button DirectionButton(string text, Direction direction)
    {
        Button button = new()
        {
            Text = text,
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#252A3A"),
            CornerRadius = 14
        };
        button.Clicked += (_, _) =>
        {
            game.SetDirection(direction);
            if (!game.Running && !game.Paused && !game.Finished) game.Start();
            UpdateStatus();
        };
        return button;
    }

    private static Button ActionButton(string text, Color background) => new()
    {
        Text = text,
        FontSize = 12,
        FontAttributes = FontAttributes.Bold,
        TextColor = Colors.White,
        BackgroundColor = background,
        CornerRadius = 12,
        HeightRequest = 48
    };

    private static Label MakeStatusLabel(string text, LayoutOptions alignment) => new()
    {
        Text = text,
        TextColor = Colors.White,
        FontSize = 13,
        FontAttributes = FontAttributes.Bold,
        HorizontalOptions = alignment,
        VerticalOptions = LayoutOptions.Center
    };

    private void UpdateStatus()
    {
        scoreLabel.Text = $"PUNTOS  {game.Score}";
        livesLabel.Text = game.Lives > 0 ? string.Join(" ", Enumerable.Repeat("●", game.Lives)) : "—";
        stateLabel.Text = game.Won ? "¡GANASTE!" : game.GameOver ? "FIN" : game.Paused ? "PAUSA" : game.Running ? "JUGANDO" : "LISTO";
    }
}
