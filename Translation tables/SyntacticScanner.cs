//SynctaticsScannes.cs

using System.IO;
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
        DeclRest,
        ConstDeclaration,
        AssignmentExpr
    }

    class SyntacticScanner
    {
        private Stack<object> stack = new Stack<object>();
        private List<Token> inputTokens;
        private int currentTokenIndex = 0;
        private List<string> errors = new List<string>();
        private PermanentTable permanentTable;
        private Stack<Token> operatorStack = new Stack<Token>();
        private List<string> postfixOutput = new List<string>();
        private Dictionary<string, bool> declaredVars = new Dictionary<string, bool>();
        private bool inDeclaration = false;
        private bool inConstDeclaration = false;
        private string lastDeclaredVarName = null;
        private bool lastDeclHasInit = false;
        private string lastConstName = null;
        private string assignmentTargetName = null;
        private bool constExprCapture = false;
        private int constExprStart = 0;

        public List<string> Errors => errors;

        public string PostfixString => string.Join(" ", postfixOutput);

        public SyntacticScanner(List<Token> tokens, PermanentTable permTable)
        {
            inputTokens = tokens;
            stack.Push(new Token(-1, 0));
            stack.Push(Nonterminal.Program);
            permanentTable = permTable;
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

        private VariablesTable variablesTable;

        public SyntacticScanner(List<Token> tokens, PermanentTable permTable, VariablesTable varTable)
        {
            inputTokens = tokens;
            stack.Push(new Token(-1, 0));
            stack.Push(Nonterminal.Program);
            permanentTable = permTable;
            variablesTable = varTable;
        }

        private string GetTokenString(Token token)
        {
            int type = token.GetTokenType();
            int id = token.GetId();

            switch (type)
            {
                case 0:
                    return permanentTable.Words[id].name;
                case 1:
                    return permanentTable.Separators[id].name;
                case 2:
                    return permanentTable.Operators[id].name;
                case 3:
                    if (id >= 0 && id < variablesTable.dynamicElements.Length && variablesTable.dynamicElements[id].Name != null)
                        return variablesTable.dynamicElements[id].Name.ToString();
                    else
                        return "Name constant";
                case 4:
                    if (id >= 0 && id < variablesTable.dynamicElements.Length && variablesTable.dynamicElements[id].Name != null)
                        return variablesTable.dynamicElements[id].Value.ToString();
                    else
                        return "constant";
                case 5:
                    if (id >= 0 && id < variablesTable.dynamicElements.Length && variablesTable.dynamicElements[id].Name != null)
                        return variablesTable.dynamicElements[id].Name;
                    else
                        return "identifier";
                default:
                    return "";
            }
        }
        private bool Error(string errorText, Token errorToken)
        {
            inDeclaration = false;
            inConstDeclaration = false;
            assignmentTargetName = null;

            int line = errorToken.GetLine();
            int pos = errorToken.GetPos();
            errors.Add($"[Syntax ERROR] Line {line}, Pos {pos}: {errorText}");

            // пропуск токенов до ';' или '}' (без удаления)
            while (currentTokenIndex < inputTokens.Count)
            {
                Token t = inputTokens[currentTokenIndex];
                if (t.GetTokenType() == 1 && (t.GetId() == 1 || t.GetId() == 8))
                    break;
                currentTokenIndex++;
            }

            // очистка стека до безопасного нетерминала
            while (stack.Count > 0)
            {
                if (stack.Peek() is Nonterminal nt &&
                    (nt == Nonterminal.Statement || nt == Nonterminal.StatementList || nt == Nonterminal.Function))
                {
                    if (currentTokenIndex < inputTokens.Count)
                    {
                        Token t = inputTokens[currentTokenIndex];
                        if (t.GetTokenType() == 1 && (t.GetId() == 1 || t.GetId() == 8))
                            currentTokenIndex++; // пропускаем разделитель, чтобы начать следующую инструкцию
                    }
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

        private Token PeekNextToken()
        {
            if (currentTokenIndex + 1 < inputTokens.Count)
                return inputTokens[currentTokenIndex + 1];
            else
                return new Token(-1, 0);
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
                            Console.WriteLine("Debuild successful!");
                            break;
                        }
                        else
                        {
                            errors.Add("Error: extra tokens after the end of the program");
                            break;
                        }
                    }

                    if (currentTokenType == 0 || currentTokenType == 1 || currentTokenType == 2)
                    {
                        if (currentTokenType == token.GetTokenType() && currentToken.GetId() == token.GetId())
                        {
                            Token actualToken = currentToken;
                            Coincidence();
                            ProcessTerminal(actualToken);
                        }
                        else
                        {
                            string exp = GetTokenTypeName(token.GetTokenType(), token.GetId());
                            string rec = GetTokenTypeName(currentTokenType, currentToken.GetId());
                            Error($"Expected {exp}, received {rec}", currentToken);
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
                            Token actualToken = currentToken;
                            Coincidence();
                            ProcessTerminal(actualToken);
                        }
                        else
                        {
                            string exp = GetTokenTypeName(token.GetTokenType(), token.GetId());
                            string rec = GetTokenTypeName(currentTokenType, currentToken.GetId());
                            Error($"Expected {exp}, received {rec}", currentToken);
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
                        Error($"Expected {nonterminal.ToString()}, received {rec}", currentToken);
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

            while (operatorStack.Count > 0)
            {
                Token top = operatorStack.Pop();
                if (top.GetTokenType() == 1 && top.GetId() == 3) continue;
                postfixOutput.Add(GetTokenString(top));
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
                        if (tokenType == 0 && (tokenId == 3 || tokenId == 2 || tokenId == 1)
                            || tokenType == 5
                            || (tokenType == 1 && tokenId == 7)) return 2;
                        else if (tokenType == -1 || tokenType == 1 && tokenId == 8) return 3;
                        else return -1;
                    }

                case Nonterminal.Statement:
                    {
                        if (tokenType == 0 && tokenId == 3) return 4;
                        else if (tokenType == 5) return 5;
                        else if (tokenType == 0 && tokenId == 2) return 6;
                        else if (tokenType == 1 && tokenId == 7) return 7;
                        else if (tokenType == 0 && tokenId == 1) return 32;
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
                        else return -1;
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
                        if (tokenType == 5)
                        {
                            Token next = PeekNextToken();
                            if (next.GetTokenType() == 2 && next.GetId() == 4)
                                return 34;
                            else
                                return 14;
                        }
                        else if (tokenType == 3 || tokenType == 4 || (tokenType == 1 && tokenId == 3))
                            return 14;
                        else
                            return -1;
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

                case Nonterminal.ConstDeclaration:
                    {
                        if (tokenType == 0 && tokenId == 1) return 33;
                        else return -1;
                    }

                case Nonterminal.AssignmentExpr:
                    if (tokenType == 5) return 35;
                    else return -1;

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
                        inDeclaration = true;
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

                case 32:
                    {
                        stack.Push(Nonterminal.ConstDeclaration);
                        break;
                    }

                case 33:
                    {
                        inConstDeclaration = true;
                        stack.Push(new Token(1, 1));
                        stack.Push(Nonterminal.Expr);
                        stack.Push(new Token(2, 4));
                        stack.Push(new Token(3, 0));
                        stack.Push(new Token(0, 3));
                        stack.Push(new Token(0, 1));
                        break;
                    }

                case 34:
                    stack.Push(Nonterminal.AssignmentExpr);
                    break;

                case 35:
                    stack.Push(Nonterminal.Expr);
                    stack.Push(new Token(2, 4));
                    stack.Push(new Token(5, 0));
                    break;
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
                string op = GetTokenString(t);
                return op switch
                {
                    "=" => 1,
                    "||" => 3,
                    "&&" => 4,
                    "+" or "-" => 7,
                    "*" => 8,
                    _ => -1
                };
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
                    postfixOutput.Add(GetTokenString(top));
                    operatorStack.Pop();
                }
                else break;
            }
            operatorStack.Push(op);
        }

        private void ProcessLeftParen(Token paren) => operatorStack.Push(paren);

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
                postfixOutput.Add(GetTokenString(top));
                operatorStack.Pop();
            }
        }

        private void ProcessTerminal(Token t)
        {
            int type = t.GetTokenType();
            int id = t.GetId();

            if (type == 5)
            {
                if (inDeclaration)
                {
                    string varName = GetTokenString(t);
                    if (declaredVars.ContainsKey(varName))
                    {
                        errors.Add($"[Semantic ERROR] Line {t.GetLine()}, Pos {t.GetPos()}: Variable '{varName}' is already declared");
                    }
                    else
                    {
                        declaredVars[varName] = true;
                        int idx = variablesTable.Search(varName);
                        if (idx != -1)
                            variablesTable.UpdateLexemeAttributes(varName, VarType.Int, false, false);
                        else
                            variablesTable.InsertLexeme(varName, 0, false, VarType.Int, false);

                        lastDeclaredVarName = varName;
                        lastDeclHasInit = false;
                    }
                    inDeclaration = false;
                }

                else
                {
                    string varName = GetTokenString(t);
                    if (!declaredVars.ContainsKey(varName))
                    {
                        errors.Add($"[Semantic ERROR] Line {t.GetLine()}, Pos {t.GetPos()}: Undeclared variable '{varName}'");
                    }

                    bool isAssignmentTarget = false;
                    if (currentTokenIndex < inputTokens.Count)
                    {
                        Token nextToken = inputTokens[currentTokenIndex];
                        if (nextToken.GetTokenType() == 2 && nextToken.GetId() == 4)
                        {
                            isAssignmentTarget = true;
                        }
                    }

                    int idx = variablesTable.Search(varName);
                    if (idx != -1)
                    {
                        var lex = variablesTable.dynamicElements[idx];

                        if (isAssignmentTarget && lex.Const)
                        {
                            errors.Add($"[Semantic ERROR] Line {t.GetLine()}, Pos {t.GetPos()}: Cannot assign to constant '{varName}'");
                        }

                        if (!isAssignmentTarget && !lex.IsInitialized)
                        {
                            errors.Add($"[Semantic ERROR] Line {t.GetLine()}, Pos {t.GetPos()}: Variable '{varName}' is used before initialization");
                        }
                    }

                    if (isAssignmentTarget)
                    {
                        assignmentTargetName = varName;
                    }
                }

                postfixOutput.Add(GetTokenString(t));
            }
            else if (type == 3 || type == 4)
            {
                if (type == 3 && inConstDeclaration)
                {
                    string constName = GetTokenString(t);
                    if (declaredVars.ContainsKey(constName))
                    {
                        errors.Add($"[Semantic ERROR] Line {t.GetLine()}, Pos {t.GetPos()}: Named constant '{constName}' already declared");
                    }
                    else
                    {
                        declaredVars[constName] = true;
                        lastConstName = constName;

                        int idx = variablesTable.Search(constName);
                        if (idx != -1)
                            variablesTable.UpdateLexemeAttributes(constName, VarType.Int, false, true);
                        else
                            variablesTable.InsertLexeme(constName, 0, true, VarType.Int, true);
                    }
                }
                postfixOutput.Add(GetTokenString(t));
            }
            else if (type == 2)
            {
                string op = GetTokenString(t);
                if (op == "=" && inDeclaration)
                {
                    lastDeclHasInit = true;
                }
                if (op == "=" && inConstDeclaration)
                {
                    constExprCapture = true;
                    constExprStart = postfixOutput.Count;
                }
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
                        postfixOutput.Add(GetTokenString(top));
                    }
                    if (id == 1)
                    {
                        postfixOutput.Add(";");

                        if (constExprCapture && inConstDeclaration && lastConstName != null)
                        {
                            var exprSegment = postfixOutput.GetRange(constExprStart, postfixOutput.Count - constExprStart - 2);
                            int? value = EvaluatePostfix(exprSegment);
                            if (value.HasValue)
                            {
                                int idx = variablesTable.Search(lastConstName);
                                if (idx != -1)
                                {
                                    var lex = variablesTable.dynamicElements[idx];
                                    lex.Value = value.Value;
                                    variablesTable.dynamicElements[idx] = lex;
                                }
                            }
                            constExprCapture = false;
                        }

                        if (inDeclaration && lastDeclaredVarName != null)
                        {
                            if (lastDeclHasInit)
                            {
                                variablesTable.UpdateLexemeAttributes(
                                    lastDeclaredVarName, VarType.Int, true, false);
                            }
                        }

                        if (!inDeclaration && !inConstDeclaration && assignmentTargetName != null)
                        {
                            int idx = variablesTable.Search(assignmentTargetName);
                            if (idx != -1)
                            {
                                var lex = variablesTable.dynamicElements[idx];
                                lex.IsInitialized = true;
                                variablesTable.dynamicElements[idx] = lex;
                            }
                            assignmentTargetName = null;
                        }
                    }
                    if (inConstDeclaration && lastConstName != null)
                    {
                        variablesTable.UpdateLexemeAttributes(
                            lastConstName, VarType.Int, true, true);
                    }

                    inDeclaration = false;
                    inConstDeclaration = false;
                    lastDeclaredVarName = null;
                    lastConstName = null;
                    lastDeclHasInit = false;
                }
            }
        }

        private int? EvaluatePostfix(List<string> postfix)
        {
            var stack = new Stack<int>();
            foreach (string token in postfix)
            {
                if (int.TryParse(token, out int num))
                    stack.Push(num);
                else if (IsOperator(token))
                {
                    if (stack.Count < 2) return null;
                    int right = stack.Pop();
                    int left = stack.Pop();
                    int result = Compute(token, left, right);
                    stack.Push(result);
                }
                else // идентификатор – не константа, вычислить невозможно
                    return null;
            }
            return stack.Count == 1 ? stack.Pop() : null;
        }

        public void FoldConstants()
        {
            var operators = new HashSet<string> { "+", "-", "*", "&&", "||", "=" };

            var segment = new List<string>();
            var result = new List<string>();

            foreach (string token in postfixOutput)
            {
                bool isSeparator = token == ";" || token == "{" || token == "}";

                if (isSeparator)
                {
                    FoldSegment(segment);
                    result.AddRange(segment);
                    segment.Clear();
                    result.Add(token);
                }
                else if (operators.Contains(token) || int.TryParse(token, out _) || variablesTable.Search(token) != -1)
                {
                    segment.Add(token);
                }
                else
                {
                    result.Add(token);
                }
            }

            if (segment.Count > 0)
            {
                FoldSegment(segment);
                result.AddRange(segment);
            }

            postfixOutput = result;
        }

        private void FoldSegment(List<string> expr)
        {
            for (int i = 0; i < expr.Count; i++)
            {
                string token = expr[i];
                if (int.TryParse(token, out _)) continue;
                int idx = variablesTable.Search(token);
                if (idx != -1)
                {
                    var lex = variablesTable.dynamicElements[idx];
                    if (lex.Const)
                        expr[i] = lex.Value.ToString();
                }
            }

            var stack = new Stack<(string val, bool isConst)>();
            var folded = new List<string>();

            foreach (string token in expr)
            {
                if (int.TryParse(token, out int num))
                {
                    stack.Push((token, true));
                }
                else if (IsOperator(token))
                {
                    if (stack.Count >= 2)
                    {
                        var right = stack.Pop();
                        var left = stack.Pop();
                        if (left.isConst && right.isConst)
                        {
                            int lv = int.Parse(left.val);
                            int rv = int.Parse(right.val);
                            int res = Compute(token, lv, rv);
                            stack.Push((res.ToString(), true));
                            continue;
                        }
                        else
                        {
                            stack.Push(left);
                            stack.Push(right);
                        }
                    }
                    folded.Add(token);
                }
                else
                {
                    stack.Push((token, false));
                }
            }

            var temp = new Stack<string>();
            while (stack.Count > 0)
                temp.Push(stack.Pop().val);
            while (temp.Count > 0)
                folded.Add(temp.Pop());

            expr.Clear();
            expr.AddRange(folded);
        }

        private bool IsOperator(string s) =>
            s == "+" || s == "-" || s == "*" || s == "&&" || s == "||";

        private int Compute(string op, int a, int b)
        {
            return op switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "&&" => (a != 0 && b != 0) ? 1 : 0,
                "||" => (a != 0 || b != 0) ? 1 : 0,
                _ => 0
            };
        }
    }
}

