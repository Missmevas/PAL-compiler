/*
	File:    RdParser.cs
	Purpose: Recursive descent parser base class.
	Author:  Allan C. Milne.
	Version: 2.2
	Date:    9th March 2011.

	namespace: AllanMilne.Ardkit
	Uses: IScanner, IParser, IToken, ICompilerError, SyntaxError.
	Exposes:  RdParser, RecoveringRdParser.

	Description:
	A practical recursive-descent (Rd) parser encapsulation.
	The RdParser abstract class provides the base functionality with a one-shot parser that terminates on a syntax error.
	The RecoveringRdParser abstract class extends this to use Turner's error recovery method to allow parsing of the entire source stream.
	A collection of CompilerError objects is used to contain all errors detected;
	this is exposed as an Errors property with the error objects sorted in line/column order..
	If the Errors property is called before the Parse() method then it will return null.

	Use for a specific language/compiler:
	A subclass must be defined that encapsulates the RD recogniser methods for the language rules.
	1. Define a class inheriting from either RdParser or RecoveringRdParser.
	2. The constructor must 		call the base constructor, passing the language-specific scanner object.
	3. Define the recStarter() recogniser method for the starter symbol of the language.
	4. Define the private RD recogniser methods for each rule in the language specification.
	5. If semantic analysis is to be included then see the semantic analyser class for information.
*/

using System;
using System.IO;
using System.Collections.Generic;


namespace AllanMilne.PALCompiler
{

//=== the base parser abstract class.
public abstract class RdParser : IParser {

	private static ComponentInfo info = new ComponentInfo (
		"RdParser", "2.2", "March 2011", "Allan C. Milne", "Recursive-descent parsing abstract classes");
	public static ComponentInfo Info
	{ get { return info; } }

   //--- attributes defining the state of the parse.
   protected IScanner       scanner;        // the scannner.
   protected List<ICompilerError> errs;     // collection of error instances.

   //--- The constructor method.
   public RdParser (IScanner scan)  {
      this.scanner = scan;
      this.errs = null;
   } // end Parser constructor method


   //=== The public API.

   //--- always returns false for this one-shot compiler.
   public bool IsRecovering
   { get { return false; } }

   //--- returns the collection of error objects.
   public List<ICompilerError> Errors
   { get {  return errs;  } }

   //--- Initiate the parse and return whether or not the parse was successful.
   public virtual bool Parse (TextReader source) {
      errs       = new List<ICompilerError> ();
      scanner.Init (source, errs);
      scanner.NextToken();              // seed the scanner with the first token.
      try {                             // mechanism to allow parse to be aborted.
         recStarter ();                 // initiate parse by calling the recogniser for the starter symbol.
         mustBe (Token.EndOfFile);   // check there is no other text.
      } catch (CompilerErrorException) {}
      errs.Sort ();      // put the errors in line/column order.
      return errs.Count == 0;
   } // end Parse method.

   //=== end of the public API.


   //=== Recursive-descent primitives.

   //--- Check if a supplied token is present.
   protected bool have (String mightBe)
   {  return scanner.CurrentToken.Is (mightBe);  }

   //--- ensure that the next token is the token required; 
   //--- if present then consume it, if not then abort parse.
   protected virtual void mustBe (String shouldBe) {
      if (have (shouldBe)) {
         scanner.NextToken();
      } else {
         syntaxError (shouldBe);
      }
   } // end mustBe method.

   //--- handle syntax error;
   //--- add error to list and abort parse.
   protected virtual void syntaxError (String shouldBe) {
      errs.Add (new SyntaxError (scanner.CurrentToken, shouldBe));
      throw new CompilerErrorException ();         // abort the parse.
   } // end syntaxError method.

        protected virtual void syntaxError2(String shouldBe)
        {
            errs.Add(new SyntaxError2(scanner.CurrentToken, shouldBe));
            throw new CompilerErrorException();         // abort the parse.
        } // end syntaxError method.


        //=== recogniser method for the starter symbol.
        protected abstract void recStarter ();

} // end RdParser class


//=== Abstract Rd parser class including Turner's error recovery method.
public abstract class RecoveringRdParser : RdParser {

   //--- state of the parse.
   private bool recovering;      // indicates whether or not in recovering mode.

   //--- constructor method.
   public RecoveringRdParser (IScanner scan)
   : base (scan)
   {
      recovering = false;
   } // end constructor method.


   //=== the public API.

   //--- is the parser recovering from a previous error?
   public new bool IsRecovering 
   { get { return recovering; } }


   //=== Overriden RD primitive methods.

   //--- Ensure that a specified token is present; an error if not present.
   //--- If ok then consumes the token and reads in the next one; if not ok then controls error handling.
   //--- Uses Turner's error recovery approach.
   protected override void mustBe (String shouldBe) {
      if (recovering) {
         // in recovering mode so Synchronize input with expected token.
         while (!have (shouldBe) && !scanner.EndOfFile) {
            scanner.NextToken();
         }
         if (scanner.EndOfFile) return;      // unable to synchronise before EOF.
         scanner.NextToken();
         recovering = false;
      }
      else {
         // Not recovering so check if we have what we expect.
         if (have (shouldBe)) {
            scanner.NextToken();      // everything ok so get the next token.
         } else {
            syntaxError (shouldBe); 
         }
      }
   } // end mustBe method.

   //--- Process syntax errors.
   protected override void syntaxError (String shouldBe) {
      if (recovering) return;   // don't report errors when recovering.
      errs.Add (new SyntaxError (scanner.CurrentToken, shouldBe));
      recovering = true;   // go into recovering mode.
   } // end syntaxError method.

        protected override void syntaxError2(String shouldBe)
        {
            if (recovering) return;   // don't report errors when recovering.
            errs.Add(new SyntaxError2(scanner.CurrentToken, shouldBe));
            recovering = true;   // go into recovering mode.
        } // end syntaxError method.


        //=== recogniser method for the starter symbol.
        protected override abstract void recStarter ();

} // end RecoveringRdParser class.


//=== Exception used to indicate a compiler error was found to allow premature termination of parse.
class CompilerErrorException : ApplicationException {

} // end CompilerErrorException class.

    // at this time no implementation of this class is required, only its existance.


    public class customParser : RecoveringRdParser
    {
        private customSemantics semantics;
        private List<IToken> tokens;
        private List<IToken> wrongTokens;

        public customParser() :
            base(new PALScanner())
        {
            semantics = new customSemantics(this); 
            tokens = new List<IToken>();
            wrongTokens = new List<IToken>();

        }

        //<Program> ::= PROGRAM Identifier WITH <VarDecls> IN(<Statement>)+ END

        protected override void recStarter()
        {
            if (have("PROGRAM"))
            {
                Scope.OpenScope();
                
                
                mustBe("PROGRAM");
                if (have(Token.IdentifierToken))
                {
                    mustBe(Token.IdentifierToken);
                }
                else
                {
                    syntaxError("Program name");
                }

                mustBe("WITH");
                recVarDeclaration();
                mustBe("IN");

                do
                {
                    recStatement();

                } while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || have("OUTPUT"));

                mustBe("END");
               
            }

            else
            {
                syntaxError("PROGRAM");
            }

            Scope.CloseScope();

        }

        // <VarDecls> ::= (<IdentList> AS <Type>)* ;

        private void recVarDeclaration()
        {
            var type = -1;

            while (have(Token.IdentifierToken))
            {
                tokens.Add(scanner.CurrentToken);

                mustBe(Token.IdentifierToken);
                recIdentList();
                if (have("AS"))
                {
                    mustBe("AS");
                }
                else if (have(Token.IdentifierToken))
                {
                    syntaxError(",");
                }
                else
                {
                    syntaxError("AS");
                }

                type = recType();

                if (type != -1)
                {
                    foreach (IToken tkn in tokens)
                    {
                        semantics.DeclareID(tkn, type); // declare ID
                    }
                    tokens.Clear();
                }

            }

           

        }

        //<IdentList> ::= Identifier ( , Identifier)* ;
        private void recIdentList()
        {
            if (have(","))
            {
                mustBe(",");
                if (have(Token.IdentifierToken))
                {
                    tokens.Add(scanner.CurrentToken);
                    mustBe(Token.IdentifierToken);
 
                    recIdentList();
                }
                else if (have("AS"))
                {
                    syntaxError2("Misplaced comma");
                }
                else
                {
                    syntaxError("Identifier");
                }
            }
        }

        // <Type> ::= REAL | INTEGER ;
        private int recType()
        {
            int varType;

            if (have("REAL"))
            {
                mustBe("REAL");
                varType = LanguageType.Real;
            }
            else if (have("INTEGER"))
            {
                mustBe("INTEGER");
                varType = LanguageType.Integer;
            }
            else
            {
                varType = LanguageType.Undefined;
                syntaxError("Type"); //?
            }

            return varType;
            // return semantic information
        }

        //<Statement> ::= <Assignment> | <Loop> | <Conditional> | <I-o> ;
        private void recStatement()
        {
            if (have(Token.IdentifierToken))
            {
               recAssignment();
            }
            else if (have("UNTIL"))
            {
                recLoop();
            }
            else if (have("IF"))
            {
                recConditional();
            }
            else if (have("INPUT") || have("OUTPUT"))
            {
                recIo();
            }
            else
            {
                syntaxError("Statement");
            }

        }

        //<Assignment> ::= Identifier = <Expression> ;
        private void recAssignment()
        {
            if (have(Token.IdentifierToken))
            {

                int varType = semantics.CheckID(scanner.CurrentToken);

                mustBe(Token.IdentifierToken);
                mustBe("=");
                
                int expType = recExpression(varType);

                if (varType != LanguageType.Undefined)
                {
                    foreach (IToken wrongtkn in wrongTokens)
                    {

                        semantics.CheckTypesSame(wrongtkn, varType, expType);

                    }

                    wrongTokens.Clear();
                }
                
            }
            else
            {
                syntaxError("Identifier");
            }
        }

        //<Expression> ::= <Term> ( (+|-) <Term>)* ;
        private int recExpression(int leftValue)
        {
            
            int lExpr = recTerm(leftValue);
            while (have("+") || have("-"))
            {
                if (have("+"))
                {
                    mustBe("+");
                    
                }
                else if (have("-"))
                {
                    mustBe("-");
                    
                }
                
                int rExpr = recTerm(leftValue);

            }
            return lExpr;
        }

        //<Term> ::= <Factor> ( (*|/) <Factor>)* ;
        private int recTerm(int leftValue)
        {
            int lTerm = recFactor(leftValue);
            while (have("*") || have("/"))
            {
                if (have("*"))
                {
                    mustBe("*");
                }

                else if (have("/"))
                {
                    mustBe("/");
                }
                var exp = scanner.CurrentToken;
                int rTerm = LanguageType.Undefined;

                if (leftValue != LanguageType.Undefined)
                {
                     rTerm = recFactor(leftValue);
                }
                else
                {
                     rTerm = recFactor(lTerm);
                }
                
                //semantics.CheckTypesSame(exp, lTerm, rTerm); // do poprawy

                if (lTerm!= LanguageType.Undefined && leftValue == LanguageType.Undefined)
                {
                    foreach (IToken wrongtkn in wrongTokens)
                    {

                        //Console.WriteLine("Term: token " + wrongtkn + " type: " + lTerm);
                        semantics.CheckTypesSame(wrongtkn, lTerm, rTerm);

                    }

                    wrongTokens.Clear();
                }


            }

            return lTerm;

        }

        //<Factor> ::= (+|-)? ( <Value> | "(" <Expression> ")" ) ;
        private int recFactor(int leftValue)
        {
            if (have("+"))
            {
                mustBe("+");
            }

            else if (have("-"))
            {
                mustBe("-");
            }

            int factorType = LanguageType.Undefined;

            if (have(Token.IdentifierToken) || have(Token.IntegerToken) || have(Token.RealToken))
            {
               factorType = recValue(leftValue);

            }
            else if (have("("))
            {
                mustBe("(");
                factorType = recExpression(leftValue);
                mustBe(")");
            }

            return factorType;

        }

        //<Value> ::= Identifier | IntegerValue | RealValue ;
        private int recValue(int leftValue)
        {
            int recValue = LanguageType.Undefined;

            if (have(Token.IdentifierToken))
            {
               recValue = semantics.CheckID(scanner.CurrentToken);
                if (recValue != leftValue && leftValue!=LanguageType.Undefined)
                {
                    wrongTokens.Add(scanner.CurrentToken);
                }
               mustBe(Token.IdentifierToken);

            }
            else if (have(Token.IntegerToken))
            {
                recValue = LanguageType.Integer;
                if (recValue != leftValue  && leftValue != LanguageType.Undefined)
                {
                    wrongTokens.Add(scanner.CurrentToken);
                }
                mustBe(Token.IntegerToken);
                
            }
            else if (have(Token.RealToken))
            {
                recValue = LanguageType.Real;
                if (recValue != leftValue && leftValue != LanguageType.Undefined)
                {
                    wrongTokens.Add(scanner.CurrentToken);
                }
                mustBe(Token.RealToken);               
            }
            else
            {
                syntaxError("Integer/Real/Identifier");
            }

           
            return recValue;
        }


        //<Loop> ::= UNTIL <BooleanExpr> REPEAT (<Statement>)* ENDLOOP ;
        private void recLoop()
        {
            if (have("UNTIL"))
            {
                mustBe("UNTIL");
                recBoolean();
                
                mustBe("REPEAT");
                while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || have("OUTPUT"))
                {
                    recStatement();
                  
                }

              mustBe("ENDLOOP");

            }
            else
            {
                syntaxError("UNTIL");
            }

        }

        //<Conditional> ::= IF <BooleanExpr> THEN (<Statement>)* (ELSE (<Statement>)* )? ENDIF ;
        private void recConditional()
        {
            if (have("IF"))
            {
                mustBe("IF");
                recBoolean();
                mustBe("THEN");

                while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || have("OUTPUT"))

                {
                    recStatement();
                }

                if (have("ELSE"))
                {
                    mustBe("ELSE");

                    while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || have("OUTPUT"))
                    {
                        recStatement();
                    }
                }

                mustBe("ENDIF");
            }

            else
            {
                syntaxError("IF");
            }
        }

        //<BooleanExpr> ::= <Expression> ("<" | "=" | ">") <Expression> ;
        private void recBoolean()
        {
            int lExpr = recExpression(LanguageType.Undefined);

            if (have("<") || have(">") || have("="))
            {
                if (have("<"))
                {
                    mustBe("<");
                }
                else if (have("="))
                {
                    mustBe("=");
                   
                }
                else if (have(">"))
                {
                    mustBe(">");
                }

               
                int rExpr = recExpression(lExpr);

                foreach (IToken wrongtkn in wrongTokens)
                {
                    //Console.WriteLine("Boolean: token " + wrongtkn + " type: " + lExpr);
                    semantics.CheckTypesSame(wrongtkn, lExpr, rExpr);

                }

                wrongTokens.Clear();
                

            }

            else
            {
                syntaxError("<, > or =");
            }
        }


        //<I-o> ::= INPUT <IdentList> | OUTPUT<Expression>( , <Expression>)* ;
        private void recIo()
        {
            if (have("INPUT"))
            {
                mustBe("INPUT");
                if (have(Token.IdentifierToken))
                {
                    semantics.CheckID(scanner.CurrentToken);
                    mustBe(Token.IdentifierToken);
                    recIdentList();
                }
            }
            else if (have("OUTPUT"))
            {
                mustBe("OUTPUT");
                
                int type = recExpression(LanguageType.Undefined);
                while (have(","))
                {
                    mustBe(",");
                    recExpression(LanguageType.Undefined);
                }
            }
            else
            {
                syntaxError("IO error");
            }
        }


    }

} // end namespace.