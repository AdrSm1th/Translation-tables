using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Translation_tables
{
    public enum Nonterminal
    {
        Program,
        Function,
        StatementList,
        Statement,
        Declaration,
        Assignment,
        ForStatement,
        Block,
        OptExpr,
        Expr,
        OrRest,
        AndExpr,
        AndRest,
        AddExpr,
        AddRest,
        MulExpr,
        MulRest,
        Primary,
        DeclRest
    }

    struct SymbolInfo
    {
        public bool typeDefined;
        public bool valueDefined;
    }

    class SyntacticScanner
    {
        private Stack<object> stack = new Stack<object>();
        private List<Token> inputTokens;
        private int currentTokenIndex = 0;
        private List<string> errors = new List<string>();
        private PermanentTable permanentTable;
        private VariablesTable variablesTable;

        private Stack<Token> operatorStack = new Stack<Token>();
        private List<string> postfixOutput = new List<string>();
        private Dictionary<string, SymbolInfo> symbolTable = new Dictionary<string, SymbolInfo>();

        public SyntacticScanner(List<Token> tokens, PermanentTable permTable, VariablesTable variablesTable)
        {
            inputTokens = tokens;
            stack.Push(new Token(-1, 0));
            stack.Push(Nonterminal.Program);
            permanentTable = permTable;
            this.variablesTable = variablesTable;
        }

        private Token GetCurrentToken()
        {
            if (currentTokenIndex < inputTokens.Count) return inputTokens[currentTokenIndex];
            else return new Token(-1, 0);
        }

        private void Coincidence()
        {
            stack.Pop();
            currentTokenIndex++;
        }

        private bool Error(string errorText)
        {
            errors.Add(errorText);

            while (currentTokenIndex < inputTokens.Count)
            {
                Token t = inputTokens[currentTokenIndex];
                if (t.GetTokenType() == 1 && (t.GetId() == 1 || t.GetId() == 8))
                {
                    currentTokenIndex++;
                    break;
                }
                currentTokenIndex++;
            }


            while (stack.Count > 0)
            {
                if (stack.Peek() is Nonterminal nt &&
                    (nt == Nonterminal.Statement || nt == Nonterminal.StatementList || nt == Nonterminal.Function))
                {
                    return true;
                }
                stack.Pop();
            }
            return false;
        }

        private string GetTokenTypeName(int tokenType, int tokenId)
        {
            switch (tokenType)
            {
                case 0:
                    return permanentTable.Words[tokenId].name;
                case 1:
                    return permanentTable.Separators[tokenId].name;
                case 2:
                    return permanentTable.Operators[tokenId].name;
                case 3:
                    return "constant";
                case 4:
                    return "constant";
                case 5:
                    return "identifier";
                default:
                    return "";
            }
        }

        public bool Scan()
        {
            while (stack.Count > 0)
            {
                Token currentToken = GetCurrentToken();
                object element = stack.Peek();
                if (element is Token)
                {
                    Token token = (Token)element;
                    int currentTokenType = currentToken.GetTokenType();

                    if (currentToken.GetTokenType() == -1)
                    {
                        if (token.GetTokenType() == -1)
                        {
                            Console.WriteLine("Разбор успешно завершён!");
                            break;
                        }
                        else
                        {
                            errors.Add("Ошибка: лишние токены после конца программы");
                            break;
                        }
                    }

                    if (currentTokenType == 0 || currentTokenType == 1 || currentTokenType == 2)
                    {
                        if (currentTokenType == token.GetTokenType() && currentToken.GetId() == token.GetId())
                        {
                            Coincidence();
                            ProcessTerminal(token);
                        }
                        else
                        {
                            string exp = GetTokenTypeName(token.GetTokenType(), token.GetId());
                            string rec = GetTokenTypeName(currentTokenType, currentToken.GetId());
                            if (!Error($"Expected {exp}, received {rec}")) return false;
                        }
                    }

                    else if (currentTokenType == 3 || currentTokenType == 4 || currentTokenType == 5)
                    {
                        bool match = false;
                        if (token.GetTokenType() == 5 && currentTokenType == 5) match = true;
                        else if ((token.GetTokenType() == 3 || token.GetTokenType() == 4) &&
                                 (currentTokenType == 3 || currentTokenType == 4)) match = true;

                        if (match)
                        {
                            Token actualToken = currentToken;  // сохранить, потому что Coincidence сдвинет индекс
                            Coincidence();
                            ProcessTerminal(actualToken);      // передаём реальный токен
                        }
                        else
                        {
                            string exp = GetTokenTypeName(token.GetTokenType(), token.GetId());
                            string rec = GetTokenTypeName(currentTokenType, currentToken.GetId());
                            if (!Error($"Expected {exp}, received {rec}")) return false;
                        }
                    }
                }

                else if (element is Nonterminal)
                {
                    Nonterminal nonterminal = (Nonterminal)element;
                    int ruleId = ParsingTable(nonterminal, currentToken);
                    if (ruleId == -1)
                    {
                        string rec = GetTokenTypeName(currentToken.GetTokenType(), currentToken.GetId());
                        if (!Error($"There is no rule for {nonterminal.ToString()} with token {rec}")) return false;
                        continue;
                    }
                    else
                    {
                        stack.Pop();
                        Rules(ruleId);
                    }
                }

                else if (element is string s && s == "eps") { stack.Pop(); continue; }
            }

            string output = "";

            output += "\nПостфиксная запись:\n" + string.Join(" ", postfixOutput);
            File.WriteAllText("output_syntax.txt", output);

            if (errors.Count == 0)
                File.WriteAllText("output_error.txt", "Разбор завершён успешно, ошибок нет.");

            else
            {
                string result = "";
                foreach (string err in errors)
                    result += err + Environment.NewLine;
                File.WriteAllText("output_error.txt", result);
            }
            return true;

        }

        public int ParsingTable(Nonterminal nt, Token token)
        {
            int tokenType = token.GetTokenType();
            int tokenId = token.GetId();
            switch (nt)
            {
                case Nonterminal.Program:
                    {
                        if (tokenType == 0 && tokenId == 3) return 0;
                        else return -1;
                    }

                case Nonterminal.Function:
                    {
                        if (tokenType == 0 && tokenId == 3) return 1;
                        else return -1;
                    }

                case Nonterminal.StatementList:
                    {
                        if (tokenType == 0 && (tokenId == 3 || tokenId == 2) || tokenType == 5 || (tokenType == 1 && tokenId == 7)) return 2;
                        else if (tokenType == -1 || tokenType == 1 && tokenId == 8) return 3;
                        else return -1;
                    }

                case Nonterminal.Statement:
                    {
                        if (tokenType == 0 && tokenId == 3) return 4;
                        else if (tokenType == 5) return 5;
                        else if (tokenType == 0 && tokenId == 2) return 6;
                        else if (tokenType == 1 && tokenId == 7) return 7;
                        else return -1;
                    }

                case Nonterminal.Declaration:
                    {
                        if (tokenType == 0 && tokenId == 3) return 8;
                        else return -1;
                    }

                case Nonterminal.Assignment:
                    {
                        if (tokenType == 5) return 9;
                        else return -1;
                    }

                case Nonterminal.ForStatement:
                    {
                        if (tokenType == 0 && tokenId == 2) return 10;
                        return -1;
                    }

                case Nonterminal.Block:
                    {
                        if (tokenType == 1 && tokenId == 7) return 11;
                        else return -1;
                    }

                case Nonterminal.OptExpr:
                    {
                        if (tokenType == 5 || tokenType == 3 || tokenType == 4 || (tokenType == 1 && tokenId == 3)) return 12;
                        else if ((tokenType == 1 && tokenId == 1) || tokenType == 1 && tokenId == 4) return 13;
                        else return -1;
                    }

                case Nonterminal.Expr:
                    {
                        if (tokenType == 5 || tokenType == 3 || tokenType == 4 || (tokenType == 1 && tokenId == 3)) return 14;
                        else return -1;
                    }

                case Nonterminal.OrRest:
                    {
                        if (tokenType == 2 && tokenId == 5) return 15;
                        else if (tokenType == 1 && (tokenId == 1 || tokenId == 4)) return 16;
                        else return -1;
                    }

                case Nonterminal.AndExpr:
                    {
                        if (tokenType == 5 || tokenType == 3 || tokenType == 4 || (tokenType == 1 && tokenId == 3)) return 17;
                        else return -1;
                    }

                case Nonterminal.AndRest:
                    {
                        if (tokenType == 2 && tokenId == 2) return 18;
                        if ((tokenType == 2 && tokenId == 5) || (tokenType == 1 && (tokenId == 1 || tokenId == 4))) return 19;
                        else return -1;
                    }

                case Nonterminal.AddExpr:
                    {
                        if (tokenType == 5 || tokenType == 3 || tokenType == 4 || (tokenType == 1 && tokenId == 3)) return 20;
                        else return -1;
                    }

                case Nonterminal.AddRest:
                    {
                        if (tokenType == 2 && tokenId == 3) return 21;
                        else if (tokenType == 2 && tokenId == 0) return 22;
                        else if ((tokenType == 1 && (tokenId == 1 || tokenId == 4)) || (tokenType == 2 && (tokenId == 2 || tokenId == 5))) return 23;
                        else return -1;
                    }

                case Nonterminal.MulExpr:
                    {
                        if (tokenType == 5 || tokenType == 3 || tokenType == 4 || (tokenType == 1 && tokenId == 3)) return 24;
                        else return -1;
                    }

                case Nonterminal.MulRest:
                    {
                        if (tokenType == 2 && tokenId == 1) return 25;
                        else if ((tokenType == 2 && (tokenId == 3 || tokenId == 0 || tokenId == 2 || tokenId == 5)) || (tokenType == 1 && (tokenId == 1 || tokenId == 4))) return 26;
                        else return -1;
                    }

                case Nonterminal.Primary:
                    {
                        if (tokenType == 5) return 27;
                        else if (tokenType == 3 || tokenType == 4) return 28;
                        else if (tokenType == 1 && tokenId == 3) return 29;
                        else return -1;
                    }

                case Nonterminal.DeclRest:
                    {
                        if (tokenType == 1 && tokenId == 1) return 30;
                        else if (tokenType == 2 && tokenId == 4) return 31;
                        else return -1;
                    }

                default:
                    {
                        return -1;
                    }
            }
        }

        public void Rules(int ruleId)
        {
            switch (ruleId)
            {
                case 0:
                    {
                        stack.Push(Nonterminal.Function);
                        break;
                    }

                case 1:
                    {
                        stack.Push(Nonterminal.Block);
                        stack.Push(new Token(1, 4));
                        stack.Push(new Token(1, 3));
                        stack.Push(new Token(0, 4));
                        stack.Push(new Token(0, 3));
                        break;
                    }

                case 2:
                    {
                        stack.Push(Nonterminal.StatementList);
                        stack.Push(Nonterminal.Statement);
                        break;
                    }

                case 3:
                    {
                        stack.Push("eps");
                        break;
                    }

                case 4:
                    {
                        stack.Push(Nonterminal.Declaration);
                        break;
                    }

                case 5:
                    {
                        stack.Push(Nonterminal.Assignment);
                        break;
                    }

                case 6:
                    {
                        stack.Push(Nonterminal.ForStatement);
                        break;
                    }

                case 7:
                    {
                        stack.Push(Nonterminal.Block);
                        break;
                    }

                case 8:
                    {
                        stack.Push(Nonterminal.DeclRest);
                        stack.Push(new Token(5, 0));
                        stack.Push(new Token(0, 3));
                        break;
                    }

                case 9:
                    {
                        stack.Push(new Token(1, 1));
                        stack.Push(Nonterminal.Expr);
                        stack.Push(new Token(2, 4));
                        stack.Push(new Token(5, 0));
                        break;
                    }

                case 10:
                    {
                        stack.Push(Nonterminal.Statement);
                        stack.Push(new Token(1, 4));
                        stack.Push(Nonterminal.OptExpr);
                        stack.Push(new Token(1, 1));
                        stack.Push(Nonterminal.OptExpr);
                        stack.Push(new Token(1, 1));
                        stack.Push(Nonterminal.OptExpr);
                        stack.Push(new Token(1, 3));
                        stack.Push(new Token(0, 2));
                        break;
                    }

                case 11:
                    {
                        stack.Push(new Token(1, 8));
                        stack.Push(Nonterminal.StatementList);
                        stack.Push(new Token(1, 7));
                        break;
                    }

                case 12:
                    {
                        stack.Push(Nonterminal.Expr);
                        break;
                    }

                case 13:
                    {
                        stack.Push("eps");
                        break;
                    }

                case 14:
                    {
                        stack.Push(Nonterminal.OrRest);
                        stack.Push(Nonterminal.AndExpr);
                        break;
                    }

                case 15:
                    {
                        stack.Push(Nonterminal.OrRest);
                        stack.Push(Nonterminal.AndExpr);
                        stack.Push(new Token(2, 5));
                        break;
                    }

                case 16:
                    {
                        stack.Push("eps");
                        break;
                    }

                case 17:
                    {
                        stack.Push(Nonterminal.AndRest);
                        stack.Push(Nonterminal.AddExpr);
                        break;
                    }

                case 18:
                    {
                        stack.Push(Nonterminal.AndRest);
                        stack.Push(Nonterminal.AddExpr);
                        stack.Push(new Token(2, 2));
                        break;
                    }

                case 19:
                    {
                        stack.Push("eps");
                        break;
                    }

                case 20:
                    {
                        stack.Push(Nonterminal.AddRest);
                        stack.Push(Nonterminal.MulExpr);
                        break;
                    }

                case 21:
                    {
                        stack.Push(Nonterminal.AddRest);
                        stack.Push(Nonterminal.MulExpr);
                        stack.Push(new Token(2, 3));
                        break;
                    }

                case 22:
                    {
                        stack.Push(Nonterminal.AddRest);
                        stack.Push(Nonterminal.MulExpr);
                        stack.Push(new Token(2, 0));
                        break;
                    }

                case 23:
                    {
                        stack.Push("eps");
                        break;
                    }

                case 24:
                    {
                        stack.Push(Nonterminal.MulRest);
                        stack.Push(Nonterminal.Primary);
                        break;
                    }

                case 25:
                    {
                        stack.Push(Nonterminal.MulRest);
                        stack.Push(Nonterminal.Primary);
                        stack.Push(new Token(2, 1));
                        break;
                    }

                case 26:
                    {
                        stack.Push("eps");
                        break;
                    }

                case 27:
                    {
                        stack.Push(new Token(5, 0));
                        break;
                    }

                case 28:
                    {
                        stack.Push(new Token(4, 0));
                        break;
                    }

                case 29:
                    {
                        stack.Push(new Token(1, 4));
                        stack.Push(Nonterminal.Expr);
                        stack.Push(new Token(1, 3));
                        break;
                    }

                case 30:
                    {
                        stack.Push(new Token(1, 1));
                        break;
                    }

                case 31:
                    {
                        stack.Push(new Token(1, 1));
                        stack.Push(Nonterminal.Expr);
                        stack.Push(new Token(2, 4));
                        break;
                    }
            }
        }
        private int GetPriority(Token t)
        {
            if (t.GetTokenType() == 1)
            {
                if (t.GetId() == 3 || t.GetId() == 4) return 0;
                if (t.GetId() == 1) return -1;
                if (t.GetId() == 8) return -2;
            }
            if (t.GetTokenType() == 2)
            {
                switch (t.GetId())
                {
                    case 4: return 1;   // =
                    case 5: return 3;   // ||
                    case 2: return 4;   // &&
                    case 3: return 7;   // +
                    case 0: return 7;   // -
                    case 1: return 8;   // *
                }
            }
            return -1;
        }

        private void ProcessOperator(Token op)
        {
            int prio = GetPriority(op);
            while (operatorStack.Count > 0)
            {
                Token top = operatorStack.Peek();
                int topPrio = GetPriority(top);
                if (topPrio >= prio && topPrio != 0)
                {
                    postfixOutput.Add(GetTokenTypeName(top.GetTokenType(), top.GetId()));
                    operatorStack.Pop();
                }
                else break;
            }
            operatorStack.Push(op);
        }

        private void ProcessLeftParen(Token paren)
        {
            operatorStack.Push(paren);
        }

        private void ProcessRightParen()
        {
            while (operatorStack.Count > 0)
            {
                Token top = operatorStack.Peek();
                if (top.GetTokenType() == 1 && top.GetId() == 3)
                {
                    operatorStack.Pop();
                    break;
                }
                postfixOutput.Add(GetTokenTypeName(top.GetTokenType(), top.GetId()));
                operatorStack.Pop();
            }
        }

        private void ProcessTerminal(Token t)
        {
            int type = t.GetTokenType();
            int id = t.GetId();

            if (type == 3 || type == 5)
            {
                postfixOutput.Add(variablesTable.dynamicElements[id].Name);
            }
            else if (type == 4)
            {
                postfixOutput.Add(variablesTable.dynamicElements[id].Value.ToString());
            }
            else if (type == 2)
            {
                ProcessOperator(t);
            }
            else if (type == 1)
            {
                if (id == 3)
                    ProcessLeftParen(t);
                else if (id == 4)
                    ProcessRightParen();
                else if (id == 1 || id == 8)
                {
                    while (operatorStack.Count > 0)
                    {
                        Token top = operatorStack.Pop();
                        if (top.GetTokenType() == 1 && top.GetId() == 3) continue;
                        postfixOutput.Add(GetTokenTypeName(top.GetTokenType(), top.GetId()));
                    }
                    if (id == 1) postfixOutput.Add(";");
                }
            }
        }
    }
}