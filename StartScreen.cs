using Godot;
using System;

public class StartScreen : Spatial
{
    [Export] Vector2 circleSelectPosition1 = Vector2.Zero;
    [Export] Vector2 circleSelectPosition2 = Vector2.Zero;
    [Export] Vector2 circleSelectPosition3 = Vector2.Zero;
    [Export] Vector2 circleSelectPosition4 = Vector2.Zero;

    bool hackerModeUnlocked = false;
    bool transitioningToHackerMode = false;
    bool transitioning = false;
    bool showingControls = false;

    enum Selection
    {
        NormalMode,
        HackerMode,
        Controls,
        Quit
    }
    Selection currentSelection = Selection.NormalMode;

    protected AnimatedSprite circleSelect;
    protected Label hackerMode;
    protected Label hackerModePopup;
    protected Godot.Node save;
    protected GlobalColors globalColors;
    protected Control labels;
    protected ColorRect background;
    protected Player player;
    protected WorldEnvironment worldEnvironment;
    protected AnimationPlayer transitionAP;
    protected Audio audio;
    protected Label highScore;
    protected Control controlsPopup;
    protected Control controlsPopupBackground;
    protected Control controlsPopupLabels;

    protected PackedScene tunnelRingScene;


    public override async void _Ready()
    {
        circleSelect = GetNode<AnimatedSprite>("Labels/CircleSelect");
        hackerMode = GetNode<Label>("Labels/HackerMode");
        hackerModePopup = GetNode<Label>("Labels/HackerModePopup");
        save = GetNode<Godot.Node>("/root/Save");
        globalColors = GetNode<GlobalColors>("/root/GlobalColors");
        labels = GetNode<Control>("Labels");
        background = GetNode<ColorRect>("Background");
        player = GetNode<Player>("Player");
        worldEnvironment = GetNode<WorldEnvironment>("WorldEnvironment");
        transitionAP = GetNode<AnimationPlayer>("Transition/AnimationPlayer");
        audio = GetNode<Audio>("/root/Audio");
        highScore = GetNode<Label>("Labels/HighScore");
        controlsPopup = GetNode<Control>("ControlsPopup");
        controlsPopupBackground = GetNode<Control>("ControlsPopup/Background");
        controlsPopupLabels = GetNode<Control>("ControlsPopup/Labels");

        controlsPopup.Hide();

        tunnelRingScene = GD.Load<PackedScene>("res://TunnelRing.tscn");

        if (globalColors.HackerMode)
        {
            globalColors.HackerMode = false;
            globalColors.ShiftPalette();
        }

        float highScoreNumber = (float)save.Call("load_game");
        highScore.Text = "High Score: " + highScoreNumber.ToString("000000");

        hackerModeUnlocked = highScoreNumber >= 50000;
        hackerModePopup.Hide();
        if (!hackerModeUnlocked)
        {
            hackerMode.Modulate = new Color(1,1,1,0.392f);
            hackerModePopup.Show();
        }

        circleSelect.Position = circleSelectPosition1;
        player.Frozen = true;

        // foreach (Node tunnelRing in GetNode<Spatial>("Tunnels").GetChildren())
        // {
        //     if (!(tunnelRing is TunnelRing))
        //     {
        //         continue;
        //     }
        //     ((TunnelRing)tunnelRing).player = player;
        //     await ToSignal(GetTree().CreateTimer(0), "timeout");
        // }

        await ToSignal(GetTree().CreateTimer(0), "timeout");

        Spatial tunnels = GetNode<Spatial>("Tunnels");
        for (int i = 1; i < 100; i++)
        {
            TunnelRing tunnelRing = tunnelRingScene.Instance<TunnelRing>();
            tunnelRing.Translation = Vector3.Forward * (50 + i*i/8);
            tunnelRing.player = player;
            AddChild(tunnelRing);
        }

        audio.StartTitle();
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("pause"))
        {
            if (!GetTree().Paused)
            {
                GetTree().Paused = true;
                Show();
                circleSelect.Position = circleSelectPosition1;
                currentSelection = Selection.NormalMode;
            }
            else
            {
                GetTree().Paused = false;
                Hide();
            }
        }

        if (Input.IsActionJustPressed("up") && !showingControls && !transitioning)
        {
            if (currentSelection == Selection.HackerMode)
            {
                circleSelect.Position = circleSelectPosition1;
                currentSelection = Selection.NormalMode;
            }
            else if (currentSelection == Selection.Controls)
            {
                if (hackerModeUnlocked)
                {
                    circleSelect.Position = circleSelectPosition2;
                    currentSelection = Selection.HackerMode;
                }
                else
                {
                    circleSelect.Position = circleSelectPosition1;
                    currentSelection = Selection.NormalMode;
                }
            }
            else if (currentSelection == Selection.Quit)
            {
                circleSelect.Position = circleSelectPosition3;
                currentSelection = Selection.Controls;
            }
        }
        else if (Input.IsActionJustPressed("down") && !showingControls && !transitioning)
        {
            if (currentSelection == Selection.NormalMode)
            {
                if (hackerModeUnlocked)
                {
                    circleSelect.Position = circleSelectPosition2;
                    currentSelection = Selection.HackerMode;
                }
                else
                {
                    circleSelect.Position = circleSelectPosition3;
                    currentSelection = Selection.Controls;
                }
            }
            else if (currentSelection == Selection.HackerMode)
            {
                circleSelect.Position = circleSelectPosition3;
                currentSelection = Selection.Controls;
            }
            else if (currentSelection == Selection.Controls)
            {
                circleSelect.Position = circleSelectPosition4;
                currentSelection = Selection.Quit;
            }
        }

        if (Input.IsActionJustPressed("ui_accept") && !transitioning)
        {
            GetTree().Paused = false;
            if (showingControls)
            {
                showingControls = false;
                controlsPopup.Hide();
            }
            else
            {
                if (currentSelection == Selection.NormalMode)
                {
                    transitionAP.Play("TransitionOut");
                    transitioningToHackerMode = false;
                    transitioning = true;
                }
                else if (currentSelection == Selection.HackerMode)
                {
                    transitionAP.Play("TransitionOut");
                    transitioningToHackerMode = true;
                    transitioning = true;
                }
                else if (currentSelection == Selection.Controls)
                {
                    controlsPopup.Show();
                    showingControls = true;
                }
                else if (currentSelection == Selection.Quit)
                {
                    GetTree().Quit();
                }
            }
        }

        // globalColors.SetPalette(1);

        labels.Modulate = globalColors.text;
        background.Modulate = globalColors.bg2;
        circleSelect.Modulate = globalColors.text;
        worldEnvironment.Environment.BackgroundSky.Set("sun_color", globalColors.bg1);
        worldEnvironment.Environment.BackgroundSky.Set("sky_top_color", globalColors.bg2);
        controlsPopupBackground.Modulate = globalColors.bg2;
        controlsPopupLabels.Modulate = globalColors.text;
    }

    private void sig_TransitionAPFinished(string animName)
    {
        if (animName == "TransitionOut")
        {
            globalColors.HackerMode = transitioningToHackerMode;
            GetTree().ChangeScene("res://World.tscn");
        }
    }
}
