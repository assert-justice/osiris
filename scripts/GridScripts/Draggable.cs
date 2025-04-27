using Godot;
using System;

public partial class Draggable : Area2D
{
    protected bool IsDragging = false;
    protected Vector2I IPosition = new();
	protected Vector2I DragStart = new();
    protected Vector2I Dimensions = new();
    protected Vector2 Offset = new();
    protected int CellWidth;
    protected CollisionShape2D CollisionShape;
    public override void _Ready()
	{
		InputEvent += InputMethod;
        CollisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
	}
    public override void _Process(double delta)
	{
		if(IsDragging){
			var mPos = GetGlobalMousePosition();
			Position = mPos + Offset;
			var x = (int)Position.X / CellWidth;
			var y = (int)Position.Y / CellWidth;
			IPosition = new(x,y);
		}
	}
    protected void SetStats(int cellWidth, int width, int height){
        CellWidth = cellWidth;
        Dimensions = new(width, height);
        var shape = new RectangleShape2D
        {
            Size = new Vector2(CellWidth * width, CellWidth * height)
        };
        CollisionShape.Shape = shape;
        CollisionShape.Position = new(cellWidth * width / 2, cellWidth * height / 2);
    }
    public void SetIPosition(Vector2I pos){
        IPosition = pos;
    }
    public bool IsEnabled(){
        return !CollisionShape.Disabled;
    }
    public void SetEnabled(bool enabled){
        CollisionShape.Disabled = !enabled;
    }
    void InputMethod(Node viewport, InputEvent @event, long shapeIdx){
		if(@event is InputEventMouseButton mouseButt){
			if(mouseButt.ButtonIndex != MouseButton.Left) return;
			if (mouseButt.Pressed && !IsDragging){
				IsDragging = true;
                Offset = Position - mouseButt.Position;
				var x = (int)Position.X / CellWidth;
				var y = (int)Position.Y / CellWidth;
				DragStart = new(x,y);
			}
			if (!mouseButt.Pressed && IsDragging){
				IsDragging = false;
				// Snap to grid
				var x = (int)Position.X / CellWidth;
				var y = (int)Position.Y / CellWidth;
				x = x * CellWidth + CellWidth/2;
				y = y * CellWidth + CellWidth/2;
				Position = new Vector2(x,y);
			}
		}
	}
}
