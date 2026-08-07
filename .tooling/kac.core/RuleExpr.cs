// ---------------------------------------------------------------------------
// The expression layer for `rules:` — see SPEC.md for the design and its limits
// ---------------------------------------------------------------------------

namespace kac.core;

// A compiled `expr:`. Held by the rule that declared it and handed back to RuleExpr.Eval; nothing
// else has reason to look inside one, and nothing outside this assembly can make one.
public abstract record Expr
{
    private protected Expr() { }
}

internal sealed record Lit(object? Value) : Expr;

internal sealed record Not(Expr Operand) : Expr;

internal sealed record Bin(string Op, Expr Left, Expr Right) : Expr;

internal sealed record Call(string Name, IReadOnlyList<Expr> Args) : Expr;

// The three types an expression deals in. Deliberately not a type system: enough to catch an
// expression that could never mean anything, and no more.
internal enum ValueType
{
    Str,
    Int,
    Bool
}

// A malformed `expr:` is a defect in the schema rather than in any document, and it is the same defect
// for every corpus that has taken that schema. It stops the load loudly instead of degrading into a
// check that silently never fires.
public sealed class RuleExprException(string message) : Exception(message);

public static class RuleExpr
{
    // Every function an expression may call, with the types it takes and returns. The grammar knows
    // nothing about these — adding a fact is adding a row here and a method on Facts.
    private static readonly Dictionary<string, (ValueType[] Args, ValueType Returns)> Functions =
        new(StringComparer.Ordinal)
        {
            ["field"] = ([ValueType.Str], ValueType.Str),
            ["present"] = ([ValueType.Str], ValueType.Bool),
            ["section"] = ([ValueType.Str], ValueType.Bool),
            ["first_section"] = ([], ValueType.Str),
            ["links"] = ([], ValueType.Int),
            ["words"] = ([], ValueType.Int),
            ["matches"] = ([ValueType.Str], ValueType.Bool),
            ["section_matches"] = ([ValueType.Str, ValueType.Str], ValueType.Bool)
        };

    // Parse and type-check. Throws RuleExprException on anything wrong, because a rule that cannot be
    // compiled is a rule nobody can rely on, and the moment to say so is the moment the schema loads.
    public static Expr Compile(string source)
    {
        var expr = new Parser(source).ParseAll();
        var type = TypeOf(expr, source);
        if (type != ValueType.Bool)
            throw new RuleExprException(
                $"expression must ask a yes/no question, but '{source}' evaluates to {Name(type)}.");
        return expr;
    }

    // True where the document satisfies the rule. A finding is emitted when this is false, so the
    // expression reads as the condition that ought to hold rather than as the fault.
    public static bool Eval(Expr expr, Facts facts) => Value(expr, facts) is true;

    // -- evaluation --

    private static object? Value(Expr expr, Facts f) =>
        expr switch
        {
            Lit lit => lit.Value,
            Not not => Value(not.Operand, f) is not true,
            Call call => Invoke(call, f),
            Bin bin => Binary(bin, f),
            _ => null
        };

    private static object? Invoke(Call call, Facts f)
    {
        var args = call.Args.Select(a => Value(a, f)).ToArray();
        return call.Name switch
        {
            "field" => f.Field((string)args[0]!),
            "present" => f.Present((string)args[0]!),
            "section" => f.Section((string)args[0]!),
            "first_section" => f.FirstSection(),
            "links" => f.Links(),
            "words" => f.Words(),
            "matches" => f.Matches((string)args[0]!),
            "section_matches" => f.SectionMatches((string)args[0]!, (string)args[1]!),
            _ => null
        };
    }

    private static object? Binary(Bin bin, Facts f)
    {
        // Short-circuiting, so `present('x') and field('x') == 'y'` never asks the second half about a
        // field the first half has just said is not there.
        switch (bin.Op)
        {
            case "and": return Value(bin.Left, f) is true && Value(bin.Right, f) is true;
            case "or": return Value(bin.Left, f) is true || Value(bin.Right, f) is true;
            case "implies": return Value(bin.Left, f) is not true || Value(bin.Right, f) is true;
        }

        var (l, r) = (Value(bin.Left, f), Value(bin.Right, f));

        // A comparison where either side is absent is false, and `!=` is the negation of `==`, so it is
        // true. One rule, applied to every operator, so a rule that wants to say something about a field
        // that may be missing guards it — `present('x') implies …` — rather than relying on the
        // comparison to guess which way silence should fall.
        switch (bin.Op)
        {
            case "==": return l is not null && r is not null && Equals(l, r);
            case "!=": return !(l is not null && r is not null && Equals(l, r));
        }

        if (l is null || r is null) return false;

        var order = l is int li && r is int ri
            ? li.CompareTo(ri)
            : string.CompareOrdinal((string)l, (string)r);

        return bin.Op switch
        {
            "<" => order < 0,
            "<=" => order <= 0,
            ">" => order > 0,
            ">=" => order >= 0,
            "+" => (int)l + (int)r,
            "-" => (int)l - (int)r,
            "*" => (int)l * (int)r,
            // Integer division, and by zero yields zero. Nothing in the taxonomy divides; the operator
            // is here because the grammar is frozen, and a rule that trips over it should read as a
            // threshold nobody meets rather than as a crash mid-corpus.
            "/" => (int)r == 0 ? 0 : (int)l / (int)r,
            _ => null
        };
    }

    // -- type checking --

    private static ValueType TypeOf(Expr expr, string source)
    {
        switch (expr)
        {
            case Lit { Value: string }: return ValueType.Str;
            case Lit { Value: int }: return ValueType.Int;
            case Lit: return ValueType.Bool;

            case Not not:
                Expect(TypeOf(not.Operand, source), ValueType.Bool, "not", source);
                return ValueType.Bool;

            case Call call:
            {
                if (!Functions.TryGetValue(call.Name, out var sig))
                    throw new RuleExprException(
                        $"'{call.Name}' is not a fact an expression can ask for, in '{source}'. "
                        + $"Known: {string.Join(", ", Functions.Keys.Order(StringComparer.Ordinal))}.");
                if (call.Args.Count != sig.Args.Length)
                    throw new RuleExprException(
                        $"'{call.Name}' takes {sig.Args.Length} argument(s), got {call.Args.Count}, in '{source}'.");
                for (var i = 0; i < call.Args.Count; i++)
                    Expect(TypeOf(call.Args[i], source), sig.Args[i], call.Name, source);
                return sig.Returns;
            }

            case Bin bin:
            {
                var (l, r) = (TypeOf(bin.Left, source), TypeOf(bin.Right, source));
                switch (bin.Op)
                {
                    case "and" or "or" or "implies":
                        Expect(l, ValueType.Bool, bin.Op, source);
                        Expect(r, ValueType.Bool, bin.Op, source);
                        return ValueType.Bool;

                    // Equality is defined for any two values of the same type; ordering only where the
                    // values have one — strings by ISO-friendly ordinal order, integers by magnitude.
                    case "==" or "!=":
                        if (l != r) throw Mismatch(bin.Op, l, r, source);
                        return ValueType.Bool;

                    case "<" or "<=" or ">" or ">=":
                        if (l != r || l == ValueType.Bool) throw Mismatch(bin.Op, l, r, source);
                        return ValueType.Bool;

                    default:
                        Expect(l, ValueType.Int, bin.Op, source);
                        Expect(r, ValueType.Int, bin.Op, source);
                        return ValueType.Int;
                }
            }

            default:
                throw new RuleExprException($"unsupported expression in '{source}'.");
        }
    }

    private static void Expect(ValueType actual, ValueType wanted, string where, string source)
    {
        if (actual != wanted)
            throw new RuleExprException(
                $"'{where}' wants {Name(wanted)} but was given {Name(actual)}, in '{source}'.");
    }

    private static RuleExprException Mismatch(string op, ValueType l, ValueType r, string source) =>
        new($"'{op}' cannot compare {Name(l)} with {Name(r)}, in '{source}'.");

    private static string Name(ValueType t) => t switch
    {
        ValueType.Str => "text",
        ValueType.Int => "a number",
        _ => "a yes/no answer"
    };

    // -- parsing --
    //
    // Recursive descent over the grammar SPEC.md freezes, loosest binding first:
    //   implies → or → and → comparison → + - → * / → not → primary

    private sealed class Parser(string source)
    {
        private readonly List<(string Kind, string Text, int Pos)> tokens = Lex(source);
        private int at;

        public Expr ParseAll()
        {
            var expr = Implies();
            if (Peek().Kind != "end")
                throw new RuleExprException($"unexpected '{Peek().Text}' at {Peek().Pos} in '{source}'.");
            return expr;
        }

        private Expr Implies()
        {
            var left = Or();
            while (Take("implies")) left = new Bin("implies", left, Or());
            return left;
        }

        private Expr Or()
        {
            var left = And();
            while (Take("or")) left = new Bin("or", left, And());
            return left;
        }

        private Expr And()
        {
            var left = Comparison();
            while (Take("and")) left = new Bin("and", left, Comparison());
            return left;
        }

        // Not chained: `a < b < c` is a sentence rather than a condition, and reads as one to whoever
        // wrote it. The grammar allows at most one comparison per level, so the second is a parse error.
        private Expr Comparison()
        {
            var left = Additive();
            if (Peek().Kind == "op" && Peek().Text is "==" or "!=" or "<" or "<=" or ">" or ">=")
            {
                var op = Next().Text;
                return new Bin(op, left, Additive());
            }

            return left;
        }

        private Expr Additive()
        {
            var left = Multiplicative();
            while (Peek().Kind == "op" && Peek().Text is "+" or "-")
                left = new Bin(Next().Text, left, Multiplicative());
            return left;
        }

        private Expr Multiplicative()
        {
            var left = Unary();
            while (Peek().Kind == "op" && Peek().Text is "*" or "/")
                left = new Bin(Next().Text, left, Unary());
            return left;
        }

        private Expr Unary() => Take("not") ? new Not(Unary()) : Primary();

        private Expr Primary()
        {
            var token = Next();
            switch (token.Kind)
            {
                case "str": return new Lit(token.Text);
                case "int": return new Lit(int.Parse(token.Text));
                case "(":
                {
                    var inner = Implies();
                    if (Next().Kind != ")")
                        throw new RuleExprException($"expected ')' at {token.Pos} in '{source}'.");
                    return inner;
                }
                case "ident":
                {
                    if (Next().Kind != "(")
                        throw new RuleExprException(
                            $"'{token.Text}' must be called with parentheses, in '{source}'.");
                    var args = new List<Expr>();
                    if (Peek().Kind != ")")
                        do args.Add(Implies());
                        while (Take(","));
                    if (Next().Kind != ")")
                        throw new RuleExprException($"expected ')' closing '{token.Text}(' in '{source}'.");
                    return new Call(token.Text, args);
                }
                default:
                    throw new RuleExprException(
                        $"expected a value at {token.Pos} but found '{token.Text}' in '{source}'.");
            }
        }

        private (string Kind, string Text, int Pos) Peek() => tokens[at];
        private (string Kind, string Text, int Pos) Next() => tokens[at < tokens.Count - 1 ? at++ : at];

        private bool Take(string kindOrWord)
        {
            var token = Peek();
            var matches = token.Kind == kindOrWord || (token.Kind == "word" && token.Text == kindOrWord);
            if (matches) at++;
            return matches;
        }

        private static List<(string, string, int)> Lex(string s)
        {
            var tokens = new List<(string, string, int)>();
            var i = 0;
            while (i < s.Length)
            {
                if (char.IsWhiteSpace(s[i])) { i++; continue; }
                var start = i;

                if (s[i] == '\'')
                {
                    // A doubled quote is one quote, as in YAML and SQL. There are no backslash escapes,
                    // because the strings that most need a quote in them are regular expressions and a
                    // second escaping layer over those is how they become unreadable.
                    var text = new System.Text.StringBuilder();
                    var j = i + 1;
                    while (true)
                    {
                        if (j >= s.Length) throw new RuleExprException($"unterminated string at {i} in '{s}'.");
                        if (s[j] == '\'')
                        {
                            if (j + 1 >= s.Length || s[j + 1] != '\'') break;
                            text.Append('\'');
                            j += 2;
                            continue;
                        }

                        text.Append(s[j++]);
                    }

                    tokens.Add(("str", text.ToString(), start));
                    i = j + 1;
                }
                else if (char.IsAsciiDigit(s[i]))
                {
                    while (i < s.Length && char.IsAsciiDigit(s[i])) i++;
                    tokens.Add(("int", s[start..i], start));
                }
                else if (char.IsAsciiLetter(s[i]) || s[i] == '_')
                {
                    while (i < s.Length && (char.IsAsciiLetterOrDigit(s[i]) || s[i] == '_')) i++;
                    var text = s[start..i];
                    tokens.Add((text is "and" or "or" or "not" or "implies" ? "word" : "ident", text, start));
                }
                else if (s[i] is '(' or ')' or ',')
                {
                    tokens.Add((s[i].ToString(), s[i].ToString(), start));
                    i++;
                }
                else
                {
                    var two = i + 1 < s.Length ? s[i..(i + 2)] : "";
                    if (two is "==" or "!=" or "<=" or ">=") { tokens.Add(("op", two, start)); i += 2; }
                    else if (s[i] is '<' or '>' or '+' or '-' or '*' or '/')
                    {
                        tokens.Add(("op", s[i].ToString(), start));
                        i++;
                    }
                    else throw new RuleExprException($"unexpected character '{s[i]}' at {i} in '{s}'.");
                }
            }

            tokens.Add(("end", "end of expression", s.Length));
            return tokens;
        }
    }
}
