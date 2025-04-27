using Godot;
using System;

public partial class Mat : Draggable
{
	Sprite2D Sprite;
	public override void _Ready()
	{
		base._Ready();
		Sprite = GetNode<Sprite2D>("Sprite2D");
		GrabMenus();
	}
	public void SetTexture(Texture2D texture){
		Sprite.Texture = texture;
		float width = CellWidth * Dimensions.X;
		float height = CellWidth * Dimensions.Y;
		float scaleX = width / texture.GetWidth();
		float scaleY = height / texture.GetHeight();
		Sprite.Scale = new(scaleX, scaleY);
	}
	public void SetStats(int cellWidth, int width, int height, Texture2D texture){
		SetStats(cellWidth, width, height);
		SetTexture(texture);
	}
	protected virtual void GrabMenus(){}
}
