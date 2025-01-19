
using Godot;

public partial class MainMenu: Menu{
    public override void _Ready()
    {
        base._Ready();
        GetNode<Button>("HBox/VBox/Join").ButtonDown += ()=>{
			menuSystem.PushMenu("GameBrowser");
		};
        GetNode<Button>("HBox/VBox/Quit").ButtonDown += ()=>{
			GetTree().Quit();
		};
    }
}