
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Pool<T>{
    List<T> Dead = new();
    HashSet<T> Alive = new();
    public Func<T> NewFn;
    public Action<T> InitFn;
    public Action<T> FreeFn;
    public T GetNew(){
        T res;
        if (Dead.Count > 0){
            res = Dead.Last();
            Dead.RemoveAt(Dead.Count-1);
        }
        else{
            res = NewFn();
        }
        Alive.Add(res);
        InitFn(res);
        return res;
    }
    public void Free(T t){
        if (!Alive.Contains(t)){
            GD.PrintErr("Attempted to free an unmanaged object");
            return;
        }
        FreeFn(t);
        Alive.Remove(t);
        Dead.Add(t);
    }
    public void FreeAll(){
        var alive = Alive.ToArray();
        foreach (var item in alive)
        {
            Free(item);
        }
    }
    public Pool(Func<T> NewFn){
        this.NewFn = NewFn;
        InitFn = (T t) => {};
        FreeFn = (T t) => {};
    }
}