
using System.Collections.Generic;
using Godot;

public class DiceParser{
    List<DiceToken> tokens = new();
    string src;
    int start = 0;
    int current = 0;
    bool hasErr = false;
    public static DiceAST Parse(string src){
        DiceParser parser = new(src);
        // foreach (var item in parser.tokens)
        // {
        //     GD.Print(item.Literal);
        // }
        // GD.Print(parser.tokens.Count);
        return new(parser.tokens.ToArray());
    }
    DiceParser(string src){
        this.src = src;
        while(!hasErr && !AtEof()){
            start = current;
            GD.Print(Peek());
            if(Whitespace()){}
            else if(Plus()){}
            else if(Number()){}
            else{
                GD.Print("oops");
                break;
            }
        }
        if(hasErr) tokens.Clear();
    }
    char Peek(){
        return src[current];
    }
    char Advance(){
        var c = Peek();
        current++;
        return c;
    }
    bool AtEof(){
        return current >= src.Length;
    }
    bool Whitespace(){
        var c = Peek();
        if(!" \t\r".Contains(c)) return false;
        while(!AtEof() && " \t\r".Contains(c)){
            c = Advance();
        }
        return true;
    }
    static bool IsDigit(char c){
        return "1234567890".Contains(c);
    }
    bool Number(){
        var c = Peek();
        if(!IsDigit(c)) return false;
        while(!AtEof() && IsDigit(c)){
            c = Advance();
        }
        if(!AtEof())current--;
        var lit = src.Substring(start, current-start);
        tokens.Add(new(){
            Type = DiceTokenType.Number,
            Literal = lit,
            Value = int.Parse(lit),
        });
        return true;
    }
    bool Plus(){
        if(Peek() != '+') return false;
        Advance();
        // GD.Print("yo");
        tokens.Add(new(){
            Type = DiceTokenType.Plus,
            Literal = "+",
        });
        return true;
    }
}

public class DiceAST{
    DiceToken[] Tokens;
    Expr root;
    int current = 0;
    bool hasErr = false;
    public DiceAST(DiceToken[] tokens){
        Tokens = tokens;
        root = Expression();
    }
    public int Eval(){
        return root.Eval();
    }
    public string Summary(){
        return root.Summary();
    }
    public override string ToString()
    {
        return $"{Eval()}: {Summary()}";
    }
    public bool HasErr(){
        return hasErr;
    }
    bool AtEof(){
        return current >= Tokens.Length;
    }
    DiceToken Peek(){
        return Tokens[current];
    }
    DiceToken Advance(){
        var x = Peek();
        current++;
        return x;
    }
    Expr Expression(){
        return Add();
    }
    Expr Add(){
        var e = Primary();
        while(!AtEof() && Peek().Type == DiceTokenType.Plus){
            Advance();
            var right = Expression();
            e = new ExprAdd(e, right);
        }
        return e;
    }
    Expr Primary(){
        return new ExprPrimary(Advance().Value);
    }
}

public enum DiceTokenType{
    Number,
    Plus,
    Minus,
    Times,
    LeftParen,
    RightParen,
    Dice,
}

public struct DiceToken{
    public DiceTokenType Type;
    public string Literal;
    public int Value;
}

abstract class Expr{
    public abstract int Eval();
    public abstract string Summary();
}

class ExprPrimary: Expr{
    int Val;
    public ExprPrimary(int val){
        Val = val;
    }

    public override int Eval()
    {
        return Val;
    }

    public override string Summary()
    {
        return Val.ToString();
    }
}
class ExprAdd: Expr{
    Expr Left;
    Expr Right;
    public ExprAdd(Expr left, Expr right){
        Left = left; Right = right;
    }

    public override int Eval()
    {
        return Left.Eval()+Right.Eval();
    }

    public override string Summary()
    {
        return $"{Left.Summary()} + {Right.Summary()}";
    }
}