using System;
using System.Collections.Generic;
using System.Linq;
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
        Primary
    }

    class SyntacticScanner
    {
        private Stack<object> stack = new Stack<object>();
        private List<Token> inputTokens;
        private int currentTokenIndex = 0;

        public SyntacticScanner(List<Token> tokens)
        {
            inputTokens = tokens;
            stack.Push('$');
            stack.Push(Nonterminal.Program);
        }

        private Token CurrentToken()
        {
            if (currentTokenIndex < inputTokens.Count)
                return inputTokens[currentTokenIndex];
            else
                return new Token(-1, -1);
        }

        private int ParsingTable(Nonterminal nt, Token token)
        {
            int tokenType = token.GetTokenType();
            int tokenId = token.GetId();
            switch(nt)
            {
                case Nonterminal.Program:
                    {
                        if (tokenType == 0 && tokenId == 3) return 1;
                        else return -1;
                    }

                case Nonterminal.Function:
                    {
                        if (tokenType == 0 && tokenId == 3) return 2;
                        else return -1;
                    }

                case Nonterminal.StatementList:
                    {
                        if(tokenType == 0 && (tokenId == 3 || tokenId == 2) || tokenType == 5 || tokenType == 1 && tokenId == 7) return 3;
                        else if (tokenType == 1 && tokenId == 11 || tokenType == 1 && tokenId == 8) return 4;
                        else return -1;
                    }

                case Nonterminal.Statement:
                    {
                        if (tokenType == 0 && tokenId == 3) return 5;
                        else if (tokenType == 5) return 6;
                        else if (tokenType == 0 && tokenId == 2) return 7;
                        else if (tokenType == 1 && tokenId == 7) return 8;
                        else return -1;
                    }

                case Nonterminal.Declaration:
                    {
                        if (tokenType == 0 && tokenId == 3) return 9;
                        else return -1;
                    }

                case Nonterminal.Assignment:
                    {
                        if (tokenType == 5) return 10;
                        else return -1;
                    }

                case Nonterminal.ForStatement:
                    {
                        if (tokenType == 0 && tokenId == 2) return 11;
                        return -1;
                    }

                case Nonterminal.Block:
                    {
                        if (tokenType == 1 && tokenId == 7) return 12;
                        else return -1;
                    }

                case Nonterminal.OptExpr:
                    {
                        if (tokenType == 5 || tokenType == 3 || tokenType == 4 || tokenType == 1 && tokenId == 3) return 13;
                        else if (tokenType == 1 && tokenId == 1 || tokenType == 1 && tokenId == 4) return 14;
                        else return -1;
                    }

                case Nonterminal.Expr:
                    {
                        if (tokenType == 5 || tokenType == 3 || tokenId == 4 || tokenType == 1 && tokenId == 3) return 15;
                        else return -1;
                    }

                case Nonterminal.OrRest:
                    {
                        if (tokenType == 2 && tokenId == 5) return 16;
                        else if (tokenType == 1 && (tokenId == 1 || tokenId == 4)) return 17;
                        else return -1;
                    }

                case Nonterminal.AndExpr:
                    {
                        if (tokenType == 5 || tokenType == 3 || tokenId == 4 || tokenType == 1 && tokenType == 3) return 18;
                        else return -1;
                    }

                case Nonterminal.AddRest:
                    {
                        if (tokenType == 1 && tokenId == 2) return 19;
                        else if (tokenType == 1 &&  tokenId == 5 || tokenType == 2 && (tokenId == 1 || tokenId == 4)) return 20;
                        else return -1;
                    }

                case Nonterminal.MulExpr:
                    {
                        if (tokenType == 5 || tokenType == 3 || tokenId == 4 || tokenType == 1 && tokenType == 3) return 25;
                        else return -1;
                    }

                case Nonterminal.MulRest:
                    {
                        if (tokenType == 2 && tokenId == 1) return 26;
                        else if(tokenType == 1 && (tokenId == 1 || tokenId == 4) || tokenType == 1 && (tokenId == 2 || tokenId == 5 || tokenId == 0 || tokenId == 3)) return 27;
                        else return -1;
                    }

                case Nonterminal.Primary:
                    {
                        if (tokenType == 5) return 28;
                        else if (tokenType == 3 || tokenType == 4) return 29;
                        else if (tokenType == 1 && tokenId == 3) return 30;
                        else return -1;
                    }

                default:
                    {
                        return -1;
                    }
            }
        }

        private void Rules(int ruleId)
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
                        stack.Push(new Token(0, 3));
                        stack.Push(new Token(0, 4));
                        stack.Push(new Token(1, 3));
                        stack.Push(new Token(1, 4));
                        stack.Push(Nonterminal.Block);
                        break;
                    }

                case 2:
                    {
                        stack.Push(Nonterminal.Statement);
                        stack.Push(Nonterminal.StatementList);
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
                        stack.Push(new Token(0, 3));
                        stack.Push(new Token(5, 0));
                        break;
                    }

                case 9:
                    {
                        stack.Push(new Token(5, 0));
                        stack.Push(new Token(2, 4));
                        stack.Push(Nonterminal.Expr);
                        break;
                    }

                case 10:
                    {
                        stack.Push(new Token(0, 2));
                        stack.Push(new Token(1, 3));
                        stack.Push(Nonterminal.OptExpr);
                        stack.Push(new Token(1, 1));
                        stack.Push(Nonterminal.OptExpr);
                        stack.Push(new Token(1, 1));
                        stack.Push(Nonterminal.OptExpr);
                        stack.Push(new Token(1, 4));
                        stack.Push(Nonterminal.Statement);
                        break;
                    }

                case 11:
                    {
                        stack.Push(new Token(1, 7));
                        stack.Push(Nonterminal.Block);
                        stack.Push(new Token(1, 8));
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
                        stack.Push(Nonterminal.AndExpr);
                        stack.Push(Nonterminal.OrRest);
                        break;
                    }

                case 15:
                    {
                        stack.Push(new Token(2, 5));
                        stack.Push(Nonterminal.AndExpr);
                        stack.Push(Nonterminal.OrRest);
                        break;
                    }

                case 16:
                    {
                        stack.Push("eps");
                        break;
                    }

                case 17:
                    {
                        stack.Push(Nonterminal.AddExpr);
                        stack.Push(Nonterminal.AndRest);
                        break;
                    }

                case 18:
                    {
                        stack.Push(new Token(2, 2));
                        stack.Push(Nonterminal.AddExpr);
                        stack.Push(Nonterminal.AndRest);
                        break;
                    }

                case 19:
                    {
                        stack.Push("eps");
                        break;
                    }

                case 20:
                    {
                        stack.Push(Nonterminal.MulExpr);
                        stack.Push(Nonterminal.AddRest);
                        break;
                    }

                case 21:
                    {
                        stack.Push(new Token(2, 3));
                        stack.Push(Nonterminal.MulExpr);
                        stack.Push(Nonterminal.AddRest);
                        break;
                    }

                case 22:
                    {
                        stack.Push(new Token(2, 0));
                        stack.Push(Nonterminal.MulExpr);
                        stack.Push(Nonterminal.AddRest);
                        break;
                    }

                case 23:
                    {
                        stack.Push("eps");
                        break;
                    }

                case 24:
                    {
                        stack.Push(Nonterminal.Primary);
                        stack.Push(Nonterminal.MulRest);
                        break;
                    }

                case 25:
                    {
                        stack.Push(new Token(2, 1));
                        stack.Push(Nonterminal.Primary);
                        stack.Push(Nonterminal.MulRest);
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
                        stack.Push(new Token(1, 3));
                        stack.Push(Nonterminal.Expr);
                        stack.Push(new Token(1, 4));
                        break;
                    }
            }
        }
    }
}
