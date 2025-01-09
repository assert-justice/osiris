
using System;
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
            // GD.Print(Peek());
            if(Whitespace()){}
            else if(Plus()){}
            else if(Minus()){}
            else if(Times()){}
            else if(LeftParen()){}
            else if(RightParen()){}
            else if(Dice()){}
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
        if(!" \t\r\n".Contains(c)) return false;
        while(!AtEof() && " \t\r\n".Contains(c)){
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
        while(!AtEof() && IsDigit(Peek())){
            Advance();
        }
        var lit = src[start..current];
        tokens.Add(new(){
            Type = DiceTokenType.Number,
            Literal = lit,
            Value = int.Parse(lit),
        });
        return true;
    }
    bool Dice(){
        var c = Peek();
        if(!IsDigit(c) && c != 'd') return false;
        while(!AtEof() && IsDigit(Peek())){
            Advance();
        }
        if(AtEof() || Peek() != 'd'){
            current = start;
            return false;
        }
        var countLit = src[start..current];
        if(countLit.Length == 0) countLit = "1";
        Advance();
        if(AtEof() || !IsDigit(Peek())){
            current = start;
            return false;
        }
        int tempStart = current;
        while(!AtEof() && IsDigit(Peek())){
            Advance();
        }
        var sizeLit = src[tempStart..current];
        var lit = src[start..current];
        tokens.Add(new(){
            Type = DiceTokenType.Dice,
            Literal = lit,
            // Value = int.Parse(lit),
            DiceCount = int.Parse(countLit),
            DiceSize = int.Parse(sizeLit),
        });
        return true;
    }
    bool Plus(){
        if(Peek() != '+') return false;
        Advance();
        tokens.Add(new(){
            Type = DiceTokenType.Plus,
            Literal = "+",
        });
        return true;
    }
    bool Minus(){
        if(Peek() != '-') return false;
        Advance();
        tokens.Add(new(){
            Type = DiceTokenType.Minus,
            Literal = "-",
        });
        return true;
    }
    bool Times(){
        if(Peek() != '*') return false;
        Advance();
        tokens.Add(new(){
            Type = DiceTokenType.Times,
            Literal = "*",
        });
        return true;
    }
    bool LeftParen(){
        if(Peek() != '(') return false;
        Advance();
        tokens.Add(new(){
            Type = DiceTokenType.LeftParen,
            Literal = "(",
        });
        return true;
    }
    bool RightParen(){
        if(Peek() != ')') return false;
        Advance();
        tokens.Add(new(){
            Type = DiceTokenType.RightParen,
            Literal = ")",
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
        var e = Mul();
        while(!AtEof() && (Peek().Type == DiceTokenType.Plus || Peek().Type == DiceTokenType.Minus)){
            var op = Advance();
            var right = Mul();
            if(op.Type == DiceTokenType.Plus){e = new ExprAdd(e, right);}
            else{e = new ExprSub(e, right);}
        }
        return e;
    }
    Expr Mul(){
        var e = Unary();
        while(!AtEof() && Peek().Type == DiceTokenType.Times){
            Advance();
            var right = Unary();
            e = new ExprMul(e, right);
        }
        return e;
    }
    Expr Unary(){
        if(Peek().Type == DiceTokenType.Minus){
            Advance();
            return new ExprUnary(Unary());
        }
        return Primary();
    }
    Expr Primary(){
        if(Peek().Type == DiceTokenType.LeftParen){
            Advance();
            var val = Expression();
            GD.Print(val);
            if(Peek().Type != DiceTokenType.RightParen){
                hasErr = true;
            }
            else Advance();
            return new ExprPrimary(val);
        }
        else if(Peek().Type == DiceTokenType.Dice){
            return new ExprDice(Advance());
        }
        else return new ExprLiteral(Advance().Value);
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
    public int DiceSize;
    public int DiceCount;
}

abstract class Expr{
    public abstract int Eval();
    public abstract string Summary();
}

class ExprPrimary: Expr{
    Expr Val;
    public ExprPrimary(Expr val){
        Val = val;
    }

    public override int Eval()
    {
        return Val.Eval();
    }

    public override string Summary()
    {
        return $"({Val.Summary()})";
    }
}
class ExprUnary: Expr{
    Expr Val;
    public ExprUnary(Expr val){
        Val = val;
    }

    public override int Eval()
    {
        return -Val.Eval();
    }

    public override string Summary()
    {
        return $"-{Val.Summary()}";
    }
}
class ExprLiteral: Expr{
    int Val;
    public ExprLiteral(int val){
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
class ExprDice: Expr{
    int Val;
    string Sum;
    int Size;
    int Count;
    public ExprDice(DiceToken token){
        Size = token.DiceSize;
        Count = token.DiceCount;
        Val = 0;
        Sum = $"{Count}d{Size}";
        for (int i = 0; i < token.DiceCount; i++)
        {
            int num = Mathf.CeilToInt(GD.Randf() * token.DiceSize);
            Val += num;
            Sum += $"[{num}]";
        }
    }

    public override int Eval()
    {
        return Val;
    }

    public override string Summary()
    {
        return Sum;
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

class ExprSub: Expr{
    Expr Left;
    Expr Right;
    public ExprSub(Expr left, Expr right){
        Left = left; Right = right;
    }

    public override int Eval()
    {
        return Left.Eval()-Right.Eval();
    }

    public override string Summary()
    {
        return $"{Left.Summary()} - {Right.Summary()}";
    }
}

class ExprMul: Expr{
    Expr Left;
    Expr Right;
    public ExprMul(Expr left, Expr right){
        Left = left; Right = right;
    }

    public override int Eval()
    {
        return Left.Eval()*Right.Eval();
    }

    public override string Summary()
    {
        return $"{Left.Summary()} * {Right.Summary()}";
    }
}