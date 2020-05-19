/*
	File:    SCanner.cs
	Purpose: Abstract design specification for a scanner.
	Author:  Allan C. Milne.
	Version: 2.3
	Date:    3rd April 2011.

	namespace: AllanMilne.Ardkit
	Uses: IScanner, IToken, Token, ICompilerError, LexicalError,  IoError.
	Exposes:  Scanner.

	Description:
	This defines the abstract class required for a scanner (lexical analyser)
	that finds terminal symbols (tokens) in a source program.
	The Init() method should be called prior to any other properties or methods; if not then
		CurrentToken will return null
		EndOfFile will return false
		NextToken() will do nothing

	Use for a specific language/compiler:
	This requires the getNextToken() method to be implemented in a concrete subclass to find and return the next terminal symbol.
	1. Define a class inheriting from Scanner.
	2. implement the protected abstract getNextToken() method, this must
		a) create an appropriate Token object for the terminal symbol found,
		b) return this token, and
		c) if an error is detected create the Token object either of type InvalidChar or InvalidToken.

	The following protected members are exposed to use within the getNextToken() implementation method:
	List<ICompilerError> errors;
		- errors collection passed from the parser.
	int line { get; }
	int column { get; }
		- position of the current character in the input stream.
	char currentChar;
		- current character to be processed.
	String lineRemaining { get; }
		- the rest of the current input line from the current character onwards.
	void consumeInput (int numChars)
		- move on numChars characters in the input stream from the current character; don't go past end-of-line.

*/

using System;
using System.Collections.Generic;
using System.IO;


namespace AllanMilne.PALCompiler
{

    //=== The scanner abstract class.
    public abstract class Scanner : IScanner
    {

        private static ComponentInfo info = new ComponentInfo(
            "Scanner", "2.3", "March 2011", "Allan C. Milne", "Abstract base class for a scanner");
        public static ComponentInfo Info
        { get { return info; } }


        //--- The state of the scanning process.

        private TextReader source;                 // input source code.
        private IToken currentTok;                 // current (last) token found.
        private int inpLine;                       // How many lines have been read in.
        private int charIndex;                     // index of current char in input line.
        private String inputLine;                  // current input line, include \n character.

        //--- protected members are available to the derived class which implements the getNextToken() method.
        //--- Hide input line from the derived class; access if by properties below to ensure correct handling but retain compatibility with previous Ardkit version.

        protected List<ICompilerError> errors;     // error collection from compiler.

        // Position of current char in input stream; read-only.
        protected int line { get { return inpLine; } }
        protected int column { get { return charIndex + 1; } }

        // current char to be processed from the current input line.
        protected char currentChar;

        // Allow access to the rest of the input line from the current character onwards.
        protected String lineRemaining
        {
            get
            {
                if (inputLine == null || charIndex >= inputLine.Length)
                    return "";
                else return inputLine.Substring(charIndex);
            }
        } // end lineRemaining property.
          // ... and to consume a number of characters from the input line; don't go beyond end of current line.
        protected void consumeInput(int numChars)
        {
            charIndex += numChars;
            if (charIndex < inputLine.Length)
            {
                currentChar = inputLine[charIndex];
            }
            else
            {
                getNextChar();   // to get the next input line.
            }
        } // end consumeInput method.

        protected const char eofChar = (char)25;   // EOF on input.

        //--- The constructor method.
        public Scanner()
        {
            this.source = null;
            this.errors = null;
            this.currentTok = null;
            this.inpLine = 0;
            this.charIndex = -1;
            this.inputLine = "";
            this.currentChar = (char)0;
        } // end Scanner constructor method.

        //--- The character input.
        protected void getNextChar()
        {
            if (source == null) return;                    // Init() not been called.
            if (currentChar == eofChar) return;            // EOF already found.

            charIndex++;
            // if we are still in the input line then set the current character and return.
            if (charIndex < inputLine.Length)
            {
                currentChar = inputLine[charIndex];
                return;
            }

            // Consumed all characters so need to read in another line.
            try
            {
                inputLine = source.ReadLine();
                charIndex = 0;
                inpLine++;
            }
            catch (IOException e)
            {
                errors.Add(new IoError(new Token(Token.EndOfFile, "", line, column), e));
                inputLine = null;      // force end-of-file.
            }
            if (inputLine == null)
            {                    // EOF found.
                currentChar = eofChar;
            }
            else
            {
                inputLine = inputLine + "\n";
                currentChar = inputLine[0];
            }
        } // end getNextChar method.

        //=== the public API.

        //--- initialise the scanner prior to processing.
        public void Init(TextReader src, List<ICompilerError> errs)
        {
            this.source = src;
            this.errors = errs;
            this.currentTok = new Token("", 0, 0);
            this.inpLine = 0;
            this.charIndex = 1;
            this.inputLine = "";

            getNextChar();    // get the first char ready for processing.

        } // end Init  method.

        //--- return current (last) token found.
        public IToken CurrentToken
        { get { return currentTok; } }

        //--- return whether or not EOF has been reached.
        public bool EndOfFile
        { get { return currentTok.Is(Token.EndOfFile); } }

        //--- The method to find the next token from the input stream.
        public IToken NextToken()
        {
            if (currentTok == null) return null;      // Init() has not been called.
            if (EndOfFile) return currentTok;         // will be EOF token.
            currentTok = GetNextToken();              // This is the business end.
            if (currentTok.Is(Token.InvalidChar) || currentTok.Is(Token.InvalidToken))
                errors.Add(new LexicalError(currentTok));
            return currentTok;
        } // end NextToken method.


        //--- the abstract method to actually process and get the next token for a specific language.

        protected abstract IToken GetNextToken();  // {} removed - CS0500 ERROR



        private void display(IToken token)
        {
            Console.WriteLine("{0} at ({1},{2}) ",
            token.TokenType, token.Line, token.Column);
            if (!token.TokenType.Equals(token.TokenValue))
                Console.WriteLine(" actual token : {0}",
                token.TokenValue);
        }

    } // end Scanner class.


} // end namespace.
