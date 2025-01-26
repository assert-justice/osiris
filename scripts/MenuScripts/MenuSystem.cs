using Godot;
using System;
using System.Collections.Generic;

public partial class MenuSystem : Control
{
	Stack<string> menuStack = new();
	public override void _Ready()
	{
		menuStack.Push(GetChild(0).Name);
		SetMenu();
	}

	public override void _Process(double delta)
	{
	}
	void SetMenu(){
		foreach (var child in GetChildren())
		{
			if (child is Control c){
				c.Visible = false;
			}
		}
		GetNode<Control>(menuStack.Peek()).Visible = true;
	}
	public void PushMenu(string menuName){
		menuStack.Push(menuName);
		SetMenu();
	}
	public string PopMenu(){
		var name = menuStack.Peek();
		if(menuStack.Count > 1) menuStack.Pop();
		SetMenu();
		return name;
	}
}
