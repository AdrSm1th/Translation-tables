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
        START, IDENTIFIER, CONSTANT, OPERATOR, SEPARATOR, COMMENT, BLOCKCOMMENT, NAMED_CONSTANT, ERROR
    }

    struct Token
    {
        private int Type;
        private int Id;
        private int Line;
        private int Pos;

        public Token(int type, int id, int line = 0, int pos = 0)
        {
            Type = type;
            Id = id;
            Line = line;
            Pos = pos;
        }

        public int GetId() => Id;
        public int GetTokenType() => Type;
        public string GetToken() => $"({Type}, {Id})";
        public int GetLine() => Line;
        public int GetPos() => Pos;
    }

    class Scanner
    {
        private State CurrentState { get; set; } = State.START;
        private PermanentTable PermanentTable { get; set; }
        private VariablesTable VariablesTable { get; set; }
        private string Buffer { get; set; } = "";
        private List<Token> Tokens = new List<Token>();
        private int position = 0;
        private string output = string.Empty;
        private int Line { get; set; } = 1;
        private int Pos { get; set; }

        private int startLine = 1, startPos = 1;

        public Scanner(PermanentTable permanentTable, VariablesTable variablesTable)
        {
            PermanentTable = permanentTable;
            VariablesTable = variablesTable;
        }
        public List<Token> GetTokens() { return Tokens; }
        private void ProcessNewLine()
        {
            Line++;
            Pos = 0;
            output += "\n";
        }
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

                switch (CurrentState)
                {
                    case State.START:
                        {
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
                            int value = 0;
                            if (elem.type != null)
                            {
                                switch (elem.type)
                                {
                                    case "letter":
                                        startLine = Line; startPos = Pos;
                                        CurrentState = State.IDENTIFIER;
                                        Buffer += ch;
                                        break;
                                    case "operator":
                                        CurrentState = State.OPERATOR;
                                        Buffer += ch;
                                        break;
                                    case "separator":

                                        if (elem.name == "/" && position + 1 < input.Length)
                                        {
                                            if (input[position + 1] == '/')
                                            {
                                                position++;
                                                CurrentState = State.COMMENT;
                                                Buffer = "";
                                                break;
                                            }
                                            else if (input[position + 1] == '*')
                                            {
                                                position++;
                                                CurrentState = State.BLOCKCOMMENT;
                                                Buffer = "/*";
                                                break;
                                            }
                                        }

                                        int sepId = BinarySearch.Search(ch.ToString(), PermanentTable.Separators);
                                        if (sepId != -1)
                                        {
                                            startLine = Line; startPos = Pos;
                                            Tokens.Add(new Token(1, sepId, startLine, startPos));
                                            output += Tokens[curTokenId++].GetToken();
                                        }
                                        Buffer = "";
                                        break;
                                }
                            }
                            else if (ch == ' ' || ch == '\r' || ch == '\t')
                            {
                                continue;
                            }
                            else if (ch == '\n')
                            {
                                ProcessNewLine();
                            }
                            else if (int.TryParse(ch.ToString(), out value))
                            {
                                startLine = Line; startPos = Pos;
                                CurrentState = State.CONSTANT;
                                Buffer += value.ToString();
                            }
                            else if (ch == '\'')
                            {
                                startLine = Line; startPos = Pos;
                                Buffer = "'";
                                CurrentState = State.CONSTANT;
                            }
                            else if (ch == '\r')
                            {
                                continue;
                            }
                            else
                            {
                                Error($"Unknown symbol '{ch}'");
                                Buffer = "";
                            }
                        }
                        break;


                    case State.IDENTIFIER:
                        {

                            Word elem = new Word();
                            int id = BinarySearch.Search(ch.ToString(), PermanentTable.Alphabet);
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

                            if (ch == ' ' || elem.type == "operator" || elem.type == "separator")
                            {
                                int wordId = BinarySearch.Search(Buffer, PermanentTable.Words);
                                if (wordId != -1)
                                {
                                    Tokens.Add(new Token(0, wordId, startLine, startPos));
                                    output += Tokens[curTokenId++].GetToken();
                                    if (PermanentTable.Words[wordId].name == "const")
                                    {
                                        CurrentState = State.NAMED_CONSTANT;
                                        Buffer = "";
                                        break;
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

                                    if (hash == -1)
                                        hash = VariablesTable.InsertLexeme(Buffer, 0, false);
                                    Tokens.Add(new Token(5, hash, startLine, startPos));
                                    output += Tokens[curTokenId++].GetToken();
                                }
                                Buffer = "";
                                CurrentState = State.START;
                                position--;
                                Pos--;
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
                                Tokens.Add(new Token(2, opId, startLine, startPos));
                                output += Tokens[curTokenId++].GetToken();
                                Buffer = "";
                                CurrentState = State.START;
                                break;
                            }
                            opId = BinarySearch.Search(Buffer, PermanentTable.Operators);
                            if (opId != -1)
                            {
                                Tokens.Add(new Token(2, opId, startLine, startPos));
                                output += Tokens[curTokenId++].GetToken();
                            }
                            Buffer = "";
                            CurrentState = State.START;
                            position--;
                            Pos--;
                        }
                        break;

                    case State.CONSTANT:
                        {
                            int value = 0;
                            if (int.TryParse(ch.ToString(), out value))
                            {
                                Buffer += value.ToString();
                            }
                            else
                            {

                                Word elem = new Word();
                                int id = BinarySearch.Search(ch.ToString(), PermanentTable.Alphabet);
                                if (id != -1) elem = PermanentTable.Alphabet[id];
                                if (elem.type == "letter")
                                {
                                    Error("Invalid number");
                                    break;
                                }

                                int hash = VariablesTable.Search(Buffer);
                                if (hash == -1)
                                {
                                    hash = VariablesTable.InsertLexeme(Buffer, int.Parse(Buffer), true);
                                }
                                Tokens.Add(new Token(4, hash, startLine, startPos));
                                output += Tokens[curTokenId++].GetToken();
                                CurrentState = State.START;
                                Buffer = "";
                                position--;
                                Pos--;
                            }
                        }
                        break;

                    case State.COMMENT:
                        if (ch == '\r') continue;
                        if (ch == '\n' || ch == '$')
                        {
                            
                            if (ch == '\n')
                            {
                                ProcessNewLine();
                            }
                            CurrentState = State.START;
                            Buffer = "";
                        }
                        break;

                    case State.BLOCKCOMMENT:
                        if (ch == '\r') continue;
                        if (ch == '\n')
                        {
                            ProcessNewLine();
                        }
                        Buffer += ch;
                        if (Buffer.Length >= 2 && Buffer.Substring(Buffer.Length - 2) == "*/")
                        {
                            CurrentState = State.START;
                            Buffer = "";
                        }
                        break;

                    case State.NAMED_CONSTANT:
                        {
                            if (ch != ';')
                                Buffer += ch.ToString();
                            else
                            {
                                CurrentState = State.START;
                                string[] parts = Buffer.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                Buffer = "";

                                if (parts.Length < 4)
                                {
                                    Error("Invalid constant declaration");
                                    break;
                                }

                                int intId = BinarySearch.Search(parts[0], PermanentTable.Words);
                                Tokens.Add(new Token(0, intId, startLine, startPos));
                                output += Tokens[curTokenId++].GetToken();

                                bool validName = true;
                                foreach (char c in parts[1])
                                {
                                    if (!char.IsUpper(c) && c != '_')
                                    {
                                        validName = false;
                                        break;
                                    }
                                }
                                if (!validName)
                                {
                                    Buffer = parts[1];
                                    Error("Constant name must be uppercase letters only");
                                    break;
                                }

                                int hash = VariablesTable.Search(parts[1]);
                                if (hash == -1)
                                    hash = VariablesTable.InsertLexeme(parts[1], 0, true);
                                Tokens.Add(new Token(3, hash, startLine, startPos));
                                output += Tokens[curTokenId++].GetToken();

                                int eqId = BinarySearch.Search("=", PermanentTable.Operators);
                                if (eqId == -1) { Error("Operator = not found"); break; }
                                Tokens.Add(new Token(2, eqId, startLine, startPos));
                                output += Tokens[curTokenId++].GetToken();

                                if (!int.TryParse(parts[3], out int val))
                                {
                                    Buffer = parts[3];
                                    Error("Invalid constant value");
                                    break;
                                }
                                int valHash = VariablesTable.Search(parts[3]);
                                if (valHash == -1)
                                    valHash = VariablesTable.InsertLexeme(parts[3], val, true);
                                Tokens.Add(new Token(4, valHash, startLine, startPos));
                                output += Tokens[curTokenId++].GetToken();

                                int semiId = BinarySearch.Search(";", PermanentTable.Separators);
                                Tokens.Add(new Token(1, semiId, startLine, startPos));
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
            CurrentState = State.ERROR;
        }
    }
}