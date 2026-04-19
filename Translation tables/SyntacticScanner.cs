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
        public int ParsingTable(Nonterminal nt, Token token)
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
    }
}
