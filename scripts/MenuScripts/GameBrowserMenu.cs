
using Godot;

public partial class GameBrowserMenu: Menu{
    public override void _Ready()
    {
        base._Ready();
        GetNode<Button>("VBox/HBox2/Back").ButtonDown += ()=>{
			menuSystem.PopMenu();
		};
    }
}