
using Godot;

public struct GridNode{
    public Vector2I Position;
    public Vector2I Parent;
    public float Cost;
    public GridNode(Vector2I position, Vector2I parent, float cost) {
        this.Position = position;
        this.Parent = parent;
        this.Cost = cost;
    }
}