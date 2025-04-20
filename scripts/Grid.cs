using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Grid : Area2D
{
	[Export] public int Width = 25;
	[Export] public int Height = 25;
	[Export] public int CellWidth = 70;
	[Export] public Color GridColor = Colors.White;
	[Export] public Texture2D BaseTexture;
	[Export] public bool BaseTextureTiles = false;
	[Export] public bool ShowGrid = true;
	[Export] public bool ShowWalls = true;
	List<(Vector2I, Vector2I)> Walls = new();
	// Dictionary<Vector2I, List<int>> WallCache = new();
	Pool<Line2D> LinePool;
	Pool<LightOccluder2D> OccluderPool;
	Pool<Sprite2D> MatPool;
	Dictionary<Vector2I, GridNode> PathLookup = new();
	Camera camera;
	bool IsDragging = false;
	// Called when the node enters the scene tree for the first time.
	public Line2D GetLine(){
		return LinePool.GetNew();
	}
	public void FreeLine(Line2D line){
		LinePool.Free(line);
	}
	public override void _Ready()
	{
		var collider = GetChild<CollisionShape2D>(2);
		var shape = new RectangleShape2D
		{
			Size = new Vector2(CellWidth * Width, CellWidth * Height)
		};
		collider.Position = new Vector2(CellWidth * Width/2,CellWidth * Height/2);
		collider.Shape = shape;
		camera = GetChild<Camera>(3);
		InputEvent += InputMethod;
		LinePool = new(() =>
		{
			var line = new Line2D();
			GetNode("Lines").AddChild(line);
			return line;
		})
		{
			InitFn = (Line2D l) =>{
				l.DefaultColor = Colors.White;
			},
			FreeFn = (Line2D l) =>{
				l.Points = Array.Empty<Vector2>();
			}
		};
		OccluderPool = new(() => {
			var oc = new LightOccluder2D();
			GetNode("Occluders").AddChild(oc);
			return oc;
		}){
			InitFn = (LightOccluder2D oc)=>{
				oc.Visible = true;
			},
			FreeFn = (LightOccluder2D oc)=>{
				oc.Visible = false;
			}
		};
		MatPool = new(()=>{
			var spr = new Sprite2D();
			GetNode("Mats").AddChild(spr);
			return spr;
		}){
			InitFn = (Sprite2D spr)=>{
				spr.Visible = true;
			},
			FreeFn = (Sprite2D spr)=>{
				spr.Visible = false;
			},
		};
	}
	void PlaceOccluder(int x0, int y0, int x1, int y1){
		Vector2[] vectors = {new(x0,y0), new(x1,y1)};
		var oc = OccluderPool.GetNew();
		oc.Occluder = new()
		{
			Polygon = vectors
		};
	}
	void PlaceMat(string texPath, int x, int y, int w, int h){
		var img = Image.LoadFromFile(texPath);
		var tex = ImageTexture.CreateFromImage(img);
		var spr = MatPool.GetNew();
		spr.Texture = tex;
	}
	void InputMethod(Node viewport, InputEvent @event, long shapeIdx){
		if(@event is InputEventMouseMotion mouseMotion){
			if(IsDragging){
				camera.Pan(-mouseMotion.Relative);
			}
		}
		if(@event is InputEventMouseButton mousButt){
			// GD.Print("yo");
			if(mousButt.ButtonIndex != MouseButton.Right) return;
			if (mousButt.Pressed && !IsDragging){
				IsDragging = true;
			// 	line = grid.GetLine();
			// 	var x = (int)Position.X / CellWidth;
			// 	var y = (int)Position.Y / CellWidth;
			// 	DragStart = new(x,y);
			// 	grid.GenPaths(DragStart, 5);
			}
			if (!mousButt.Pressed && IsDragging){
				IsDragging = false;
			// 	grid.FreeLine(line);
			// 	line = null;
			// 	// Snap to grid
			// 	var x = (int)Position.X / CellWidth;
			// 	var y = (int)Position.Y / CellWidth;
			// 	x = x * CellWidth + CellWidth/2;
			// 	y = y * CellWidth + CellWidth/2;
			// 	Position = new Vector2(x,y);
			}
		}
	}
	void AddWall(int x0, int y0, int x1, int y1){
		Walls.Add((new Vector2I(x0,y0), new Vector2I(x1,y1)));
	}
	static bool IsIntersecting(Vector2I wallStart, Vector2I wallEnd, Vector2I pathStart, Vector2I pathEnd)
	{
		Vector2 a = new(wallStart.X, wallStart.Y);
		Vector2 b = new(wallEnd.X, wallEnd.Y);
		Vector2 c = new(pathStart.X + 0.5f, pathStart.Y + 0.5f);
		Vector2 d = new(pathEnd.X + 0.5f, pathEnd.Y + 0.5f);

		// Shamelessly stolen from here: https://gamedev.stackexchange.com/questions/26004/how-to-detect-2d-line-on-line-collision
		float denominator = ((b.X - a.X) * (d.Y - c.Y)) - ((b.Y - a.Y) * (d.X - c.X));
		float numerator1 = ((a.Y - c.Y) * (d.X - c.X)) - ((a.X - c.X) * (d.Y - c.Y));
		float numerator2 = ((a.Y - c.Y) * (b.X - a.X)) - ((a.X - c.X) * (b.Y - a.Y));

		// Detect coincident lines (has a problem, read below)
		if (denominator == 0) return numerator1 == 0 && numerator2 == 0;
		
		float r = numerator1 / denominator;
		float s = numerator2 / denominator;

		return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Draw()
	{
		var width = Width * CellWidth;
		var height = Height * CellWidth;
		DrawTextureRect(BaseTexture, new Rect2(0, 0, width, height), BaseTextureTiles);
		if(ShowGrid){
			for (int y = 0; y < Height + 1; y++){
				DrawLine(new Vector2(0, y * CellWidth), new Vector2(width, y * CellWidth), GridColor);
			}
			for (int x = 0; x < Width + 1; x++){
				DrawLine(new Vector2(x * CellWidth,0), new Vector2(x * CellWidth,height), GridColor);
			}
		}
		if(ShowWalls){
			foreach ((var v0, var v1) in Walls)
			{
				var x0 = v0.X * CellWidth;
				var y0 = v0.Y * CellWidth;
				var x1 = v1.X * CellWidth;
				var y1 = v1.Y * CellWidth;
				DrawLine(new Vector2(x0,y0), new Vector2(x1,y1), Colors.Red);
			}
		}
	}
	static GridNode PopLeast(ref Dictionary<Vector2I, GridNode> dict){
		var least = new GridNode(new Vector2I(0,0), new Vector2I(0,0), Mathf.Inf);
		if (dict.Count == 0){
			GD.PrintErr("Tried to pop from empty dict!");
			return least;
		}
		foreach (var val in dict.Values)
		{
			if (val.Cost < least.Cost){
				least = val;
			}
		}
		dict.Remove(least.Position);
		return least;
	}
	static void PushLeast(ref Dictionary<Vector2I,GridNode> dict, GridNode node){
		if(dict.ContainsKey(node.Position)){
			var comp = dict[node.Position];
			if (comp.Cost < node.Cost){return;}
		}
		dict[node.Position] = node;
	}
	public void GenPaths(Vector2I start, float range, float rangeInc = 0){
		if(rangeInc == 0)rangeInc = range;
		PathLookup.Clear();
		Vector2I[] directions = {
			new(1,0),
			new(0,-1),
			new(-1,0),
			new(0,1),
			new(1,-1),
			new(-1,-1),
			new(-1,1),
			new(1,1),
		};
		Dictionary<Vector2I, GridNode> open = new()
		{
			{ start, new GridNode(start, start, 0) }
		};
		while (open.Count > 0){
			var node = PopLeast(ref open);
			if(node.Cost > range){break;}
			PushLeast(ref PathLookup, node);
			foreach (var dir in directions)
			{
				var pos = new Vector2I(dir.X + node.Position.X, dir.Y + node.Position.Y);
				var deltaCost = dir.X == 0 || dir.Y == 0 ? 1.0f : 1.5f;
				var cost = node.Cost + deltaCost; // Todo: implement difficult terrain
				var next = new GridNode(pos, node.Position, cost);
				if(Walls.Any((w)=>{
					var (a,b) = w;
					return IsIntersecting(a, b, pos, node.Position);
				})){continue;}
				PushLeast(ref open, next);
			}
		}
	}
	public Vector2I[] GetPath(Vector2I dest){
		if(!PathLookup.ContainsKey(dest)){
			return Array.Empty<Vector2I>();
		}
		var temp = new List<Vector2I>();
		var node = PathLookup[dest];
		while(true){
			temp.Add(node.Position);
			if (node.Cost == 0) break;
			node = PathLookup[node.Parent];
		}
		return temp.ToArray();
	}
}
