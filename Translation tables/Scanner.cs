using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translation_tables
{
    public enum State
    {
        START, IDENTIFIER, CONSTANT, OPERATOR, EQUATION, SEPARATOR, COMMENT, BLOCKCOMMENT, NAMED_CONSTANT, ERROR
    }

    struct Token
    {
        private int Type; //0 - Keyword //1 - Separator //2 - Operator //3 - Named constant //4 - Constant //5 - Identifier
        private int Id;

        public Token(int type, int id)
        {
            Type = type;
            Id = id;
        }

        public int GetId() { return Id; }
        public int GetTokenType() { return Type; }
        public string GetToken() { return $"({Type.ToString()}, {Id}) "; }
    }

    class Scanner
    {
        private State CurrentState { get; set; } = State.START;
        private PermanentTable PermanentTable { get; set; }
        private VariablesTable VariablesTable { get; set; }
        private string Buffer { get; set; } = "";
        private List<Token> Tokens = new List<Token>();
        private bool initialize = false;
        private int position = 0;
        private string output = string.Empty;
        private int Line { get; set; } = 1;
        private int Pos { get; set; }

        public Scanner(PermanentTable permanentTable, VariablesTable variablesTable)
        {
            PermanentTable = permanentTable;
            VariablesTable = variablesTable;
        }

        public List<Token> GetTokens() { return Tokens; }

        public void Scan(string filename)
        {
            string programInput = File.ReadAllText(filename);
            programInput += "$";
            ReadOnlySpan<char> input = programInput.AsSpan();
            int curTokenId = 0;

            for (; position < input.Length; position++)
            {
                Pos++;
                char ch = input[position];
                int id = 0;
                Word elem = new Word();

                id = BinarySearch.Search(ch.ToString(), PermanentTable.Alphabet);
                if (id != -1) elem = PermanentTable.Alphabet[id];

                if (id == -1)
                {
                    id = BinarySearch.Search(ch.ToString(), PermanentTable.Operators);
                    if (id != -1) elem = PermanentTable.Operators[id];
                }

                if (id == -1)
                {
                    id = BinarySearch.Search(ch.ToString(), PermanentTable.Separators);
                    if (id != -1) elem = PermanentTable.Separators[id];
                }

                switch (CurrentState)
                {
                    case State.START:
                        {
                            int value = 0;
                            if (elem.type != null)
                            {
                                switch (elem.type)
                                {
                                    case "letter":
                                        CurrentState = State.IDENTIFIER;
                                        break;
                                    case "operator":
                                        CurrentState = State.OPERATOR;
                                        break;
                                    case "separator":
                                        if (elem.name == "/")
                                        {
                                            if (position + 1 < input.Length && input[position + 1] == '/')
                                            {
                                                position++;
                                                CurrentState = State.COMMENT;
                                                Buffer = "";
                                                break;
                                            }
                                            else if (position + 1 < input.Length && input[position + 1] == '*')
                                            {
                                                position++;
                                                CurrentState = State.BLOCKCOMMENT;
                                                Buffer = "/*";
                                                break;
                                            }
                                            else
                                            {
                                                CurrentState = State.OPERATOR;
                                            }
                                        }
                                        break;
                                }
                                Buffer += ch;
                            }
                            else if (ch == ' ' || ch == '\r' || ch == '\t')
                            {
                                continue;
                            }
                            else if (ch == '\n')
                            {
                                output += "\n";
                                Line++;
                                Pos = 0;
                            }
                            else if (int.TryParse(ch.ToString(), out value))
                            {
                                CurrentState = State.CONSTANT;
                                Buffer += value.ToString();
                            }
                            else if (ch == '\'')
                            {
                                Buffer = "'";
                                CurrentState = State.CONSTANT;
                            }
                            else
                                Console.WriteLine($"Unknown symbol '{ch}' at position {position}");
                        }
                        break;

                    case State.IDENTIFIER:
                        {
                            if (ch == ' ' || elem.type == "operator" || elem.type == "separator")
                            {
                                int wordId = BinarySearch.Search(Buffer, PermanentTable.Words);
                                if (wordId != -1)
                                {
                                    if (initialize && PermanentTable.Words[wordId].name != "main")
                                    {
                                        Error("reserved word can't be identifier");
                                        break;
                                    }
                                    Tokens.Add(new Token(0, wordId));
                                    output += Tokens[curTokenId++].GetToken();
                                    if (PermanentTable.Words[wordId].name == "const")
                                    {
                                        CurrentState = State.NAMED_CONSTANT;
                                        Buffer = "";
                                        break;
                                    }
                                    if (PermanentTable.Words[wordId].name == "int")
                                    {
                                        initialize = true;
                                    }
                                }
                                else
                                {
                                    if (!char.IsLetter(Buffer[0]))
                                    {
                                        Error("Identifier must start with letter");
                                        break;
                                    }
                                    bool valid = true;
                                    if (!char.IsLetter(Buffer[0])) valid = false;
                                    foreach (char c in Buffer)
                                    {
                                        if (!char.IsLetterOrDigit(c) && c != '_')
                                        {
                                            valid = false;
                                            break;
                                        }
                                    }
                                    if (!valid)
                                    {
                                        Error("Invalid identifier");
                                        break;
                                    }
                                    int hash = VariablesTable.Search(Buffer);
                                    if (initialize && hash != -1)
                                    {
                                        Error("Variable already declared");
                                        break;
                                    }
                                    if (hash == -1)
                                        hash = VariablesTable.InsertLexeme(Buffer, 0, false);
                                    Tokens.Add(new Token(5, hash));
                                    output += Tokens[curTokenId++].GetToken();
                                }
                                Buffer = "";
                                CurrentState = State.START;
                                position--;
                                Pos--;
                                initialize = false;
                            }
                            else
                            {
                                Buffer += ch;
                            }
                        }
                        break;

                    case State.OPERATOR:
                        {
                            string two = Buffer + ch;
                            int opId = BinarySearch.Search(two, PermanentTable.Operators);
                            if (opId != -1)
                            {
                                Tokens.Add(new Token(2, opId));
                                output += Tokens[curTokenId++].GetToken();
                                Buffer = "";
                                CurrentState = State.START;
                                break;
                            }
                            opId = BinarySearch.Search(Buffer, PermanentTable.Operators);
                            if (opId != -1)
                            {
                                Tokens.Add(new Token(2, opId));
                                output += Tokens[curTokenId++].GetToken();
                            }
                            Buffer = "";
                            CurrentState = State.START;
                            position--;
                            Pos--;
                        }
                        break;

                    case State.EQUATION:
                        {
                            if (ch != ';' && ch != ',' && ch != ')')
                            {
                                Buffer += ch;
                            }
                            else
                            {
                                int value = 0;
                                string[] parts = Buffer.Split(' ');
                                foreach (string part in parts)
                                {
                                    int hash = VariablesTable.Search(part);
                                    if (int.TryParse(part, out value))
                                    {
                                        if (hash == -1)
                                        {
                                            hash = VariablesTable.InsertLexeme(part, value, true);
                                        }
                                        Tokens.Add(new Token(4, hash));
                                        output += Tokens[curTokenId++].GetToken();
                                    }
                                    else if (hash != -1)
                                    {
                                        if (VariablesTable.dynamicElements[hash].Const)
                                        {
                                            Tokens.Add(new Token(3, hash));
                                        }
                                        else
                                        {
                                            Tokens.Add(new Token(5, hash));
                                        }
                                        output += Tokens[curTokenId++].GetToken();
                                    }
                                    else if (BinarySearch.Search(part, PermanentTable.Operators) != -1)
                                    {
                                        Tokens.Add(new Token(2, BinarySearch.Search(part, PermanentTable.Operators)));
                                        output += Tokens[curTokenId++].GetToken();
                                    }
                                    else
                                    {
                                        Error("Type missmatch");
                                        CurrentState = State.ERROR;
                                        Buffer = "";
                                        break;
                                    }
                                    CurrentState = State.START;
                                    Tokens.Add(new Token(1, BinarySearch.Search(ch.ToString(), PermanentTable.Separators)));
                                    output += Tokens[curTokenId++].GetToken();
                                    Buffer = "";
                                }
                            }
                        }
                        break;

                    case State.SEPARATOR:
                        {
                            CurrentState = State.START;
                            Tokens.Add(new Token(1, BinarySearch.Search(Buffer, PermanentTable.Separators)));
                            output += Tokens[curTokenId++].GetToken();
                            Buffer = "";
                            position--;
                            Pos--;
                        }
                        break;

                    case State.CONSTANT:
                        {
                            int value = 0;
                            if (initialize)
                            {
                                while (position < input.Length && input[position] != ' ' && input[position] != ';' && input[position] != '\n')
                                {
                                    Buffer += input[position].ToString();
                                    position++;
                                }
                                Error("Identifier name can't start with number");
                                break;
                            }
                            if (int.TryParse(ch.ToString(), out value))
                            {
                                Buffer += value.ToString();
                            }
                            else if (elem.type == "letter")
                            {
                                Error("Invalid number");
                                break;
                            }
                            else
                            {
                                int hash = VariablesTable.Search(Buffer);
                                if (hash == -1)
                                {
                                    hash = VariablesTable.InsertLexeme(Buffer, int.Parse(Buffer), true);
                                }
                                Tokens.Add(new Token(4, hash));
                                output += Tokens[curTokenId++].GetToken();
                                CurrentState = State.START;
                                Buffer = "";
                                position--;
                                Pos--;
                            }
                        }
                        break;

                    case State.COMMENT:
                        {
                            if (Buffer.Length > 2 && Buffer[0].ToString() + Buffer[1].ToString() == "/*")
                            {
                                CurrentState = State.BLOCKCOMMENT;
                            }
                            else if (ch == '\n' || ch == '$')
                            {
                                CurrentState = State.START;
                                Buffer = "";
                                position--;
                                Pos--;
                            }
                            else
                            {
                                Buffer += ch.ToString();
                            }
                        }
                        break;

                    case State.BLOCKCOMMENT:
                        {
                            int bufferSize = Buffer.Length;
                            if (bufferSize >= 2 && Buffer[bufferSize - 2].ToString() + Buffer[bufferSize - 1].ToString() == "*/")
                            {
                                CurrentState = State.START;
                                Buffer = "";
                                position--;
                                Pos--;
                            }
                            else
                            {
                                Buffer += ch.ToString();
                            }
                        }
                        break;

                    case State.NAMED_CONSTANT:
                        {
                            if (ch != ';')
                                Buffer += ch.ToString();
                            else
                            {
                                CurrentState = State.START;
                                string[] parts = Buffer.Split(' ');
                                Buffer = "";
                                Tokens.Add(new Token(0, BinarySearch.Search(parts[0], PermanentTable.Words)));
                                output += Tokens[curTokenId++].GetToken();
                                bool flag = true;
                                foreach (char letter in parts[1])
                                {
                                    int lId = BinarySearch.Search(letter.ToString(), PermanentTable.Alphabet);
                                    if (char.IsLower(letter) || lId == -1)
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    Buffer = parts[1].ToString();
                                    Error("Invalid name for constant");
                                    break;
                                }
                                int hash = VariablesTable.InsertLexeme(parts[1], int.Parse(parts[3]), true);
                                Tokens.Add(new Token(3, hash));
                                output += Tokens[curTokenId++].GetToken();
                                Tokens.Add(new Token(1, BinarySearch.Search(";", PermanentTable.Separators)));
                                output += Tokens[curTokenId++].GetToken();
                            }
                        }
                        break;

                    case State.ERROR:
                        {
                            if (ch == ';' || ch == '\n')
                            {
                                CurrentState = State.START;
                                Buffer = "";
                            }
                        }
                        break;
                }
            }

            if (CurrentState == State.BLOCKCOMMENT)
            {
                output += $"\n[ERROR] Line: {Line}, Pos: {Pos} -> Unclosed block comment\n";
            }

            File.WriteAllText("output.txt", output);
        }

        private void Error(string message)
        {
            output += $"\n[ERROR] Line: {Line}, Pos: {Pos}, Token: '{Buffer}' -> {message}\n";
            Buffer = "";
            initialize = false;
            CurrentState = State.ERROR;
        }
    }
}