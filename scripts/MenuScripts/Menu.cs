
using Godot;

public abstract partial class Menu: Control{
    protected MenuSystem menuSystem;
    public override void _Ready()
    {
        menuSystem = GetParent<MenuSystem>();
        VisibilityChanged += () => {
            if(Visible) OnWake();
            else OnSleep();
        };
    }
    public virtual void OnWake(){}
    public virtual void OnSleep(){}
}