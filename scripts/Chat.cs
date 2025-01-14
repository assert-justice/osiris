using Godot;
using System;

public partial class Chat : Node
{
	TextEdit edit;
	VBoxContainer container;
	public override void _Ready()
	{
		edit = GetChild<TextEdit>(0);
		container = GetChild(1).GetChild<VBoxContainer>(0);
		edit.TextChanged += ()=>{
			if(edit.Text[^1] == '\n'){
				Submit(edit.Text);
				edit.Text = "";
			}
		};
	}
	void Submit(string text){
		if(text.StartsWith("/r ")){
			var ast = DiceParser.Parse(text[3..]);
			text = ast.ToString();
		}
		Label label = new()
		{
			Text = text,
			AutowrapMode = TextServer.AutowrapMode.Word,
		};
		container.AddChild(label);
	}
}
