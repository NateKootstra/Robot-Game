using Godot;

public partial class Exit : Node2D
{
    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.Escape))
            GetTree().ChangeSceneToFile("res://Menu.tscn");
    }
}
