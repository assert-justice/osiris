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
			if(edit.Text[edit.Text.Length-1] == '\n'){
				Submit(edit.Text);
				edit.Text = "";
			}
		};
	}
	void Submit(string text){
        var ast = DiceParser.Parse(text);
        text = ast.ToString();
		Label label = new()
		{
			Text = text,
            AutowrapMode = TextServer.AutowrapMode.Word,
		};
		container.AddChild(label);
	}
}
