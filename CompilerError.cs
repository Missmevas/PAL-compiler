/*
	File:    CompilerError.cs
	Purpose: Representation of an error instance within a compiler.
	Author:  Allan C. Milne.
	Version: 2.2
	Date:    9th March 2011.

	Namespace: AllanMilne.Ardkit
	Uses: ICompilerError, IToken, ISymbol, LanguageType.
	Exposes:  CompilerError, IoError, LexicalError, SyntaxError, NotDeclaredError, AlreadyDeclaredError, TypeConflictError.

	Description:
	A class hierarchy that represents the errors that may occur during compilation.
	The CompilerError class is the abstract base class for all error types with
	the different actual error type instances represented by the specific subclasses.
	The overridden ToString() method provides an appropriate error message; 
	the base class version provides the message prefix including the error position.
	The base class also implements the IComparable interface allowing a collection of error objects to be sorted by line/column.

	Also defined are subclasses that represent typical errors that may occur in the scanning, parsing and semantic analysis phases.

	Use with a specific compiler/language:
	Semantic errors are specific to the language being processed;
	while the classes defined in the toolkit may reflect those required for a language it may be that additional semantic error classes will be required.
	1. For each kind of semantic error not supported by the toolkit, define a subclass from CompilerError.
	2. Include any attributes required to specify the error.
	3. define a constructor that takes at least the error token and calls the base constructor with this token.
	4. override the ToString method to provide an appropriate error message;
	this should call base.ToString() to provide the message prefix.

*/

using System;
using System.Text;
using System.IO;


namespace AllanMilne.PALCompiler
{

//=== Representation of a compiler error.
public abstract class CompilerError : ICompilerError, IComparable {

	private static ComponentInfo info = new ComponentInfo (
		"CompilerError", "2.2", "March 2011", "Allan C. Milne", "Class hierarchy representing compiler errors");
	public static ComponentInfo Info
	{ get { return info; } }

   protected IToken  token;                 // the token causing the error.
   public IToken ErrorToken
   { get { return token; } }

   //--- constructor method.
   public CompilerError (IToken where) {
      token = where;
   }

   //--- Implements the Icomparable interface.
   public int CompareTo (Object obj) {
      IToken thisToken = this.ErrorToken;
      IToken thatToken = ((CompilerError)obj).ErrorToken;
      if (thisToken.Line < thatToken.Line)          return -1;   // this < obj
      else if (thisToken.Line > thatToken.Line)     return 1;    // this > obj
      else if (thisToken.Column < thatToken.Column) return -1;   // this < obj
      else if (thisToken.Column > thatToken.Column) return 1;    // this > obj
      else                               return 0;    // this = obj
   } // end CompareTo method.

   //--- Provides a message prefix with the ine/column number of the error.
   public override String ToString () {
      return String.Format ("({0:d},{1:d}) : ", token.Line, token.Column); 
   }

} // end CompilerError abstract class.


//=== Represent an input/output error detected while reading the soruce program stream.
public class IoError : CompilerError {

   private IOException cause;
   public IOException Cause
   { get { return cause; } }

   public IoError (IToken where, IOException e)
      : base (where)
   {
      cause = e;
   }

   public override String ToString () {
      return String.Format ("{0:s}I/O error reading from source.\n{1:s}", base.ToString(), cause.ToString());
   }

} // end IoError class.


//=== Represent an error in constructing a token as detected in the scanning process.
public class LexicalError : CompilerError {

   public LexicalError (IToken where)
   : base (where)
   {}

   public override String ToString () {
         return String.Format ("{0:s} Lexical error {1:s} found.", base.ToString(), token.ToString());
   }

} // end LexicalError class.


//=== Represent a syntax error as detected during the RD parsing phase.
public class SyntaxError : CompilerError {

   private String expected;
   public String Expected
   { get { return expected; } }

   public SyntaxError (IToken where, String shouldBe)
      : base (where)
   {
      expected = shouldBe;
   }

   public override String ToString () {
      return String.Format ("{0:s}'{1:s}' found where '{2:s}' expected.", base.ToString(), token.ToString(), expected);
   }

} // end SyntaxError class.


	public class SyntaxError2 : CompilerError
	{

		private String expected;
		public String Expected
		{ get { return expected; } }

		public SyntaxError2(IToken where, String shouldBe)
		   : base(where)
		{
			expected = shouldBe;
		}

		public override String ToString()
		{
			return String.Format("{0:s}{1:s} - '{2:s}' found.", base.ToString(), expected, token.ToString());
		}

	} // end SyntaxError class.


	//=== Represent an undecalred identifier as detected in semantic analysis.
	public class NotDeclaredError : CompilerError {

	public NotDeclaredError (IToken id)
	: base (id)
	{}

	public override String ToString () {
		return String.Format ("{0:s} Identifier '{1:s}' is not declared.",
			base.ToString(), token.TokenValue);
	}// end ToString method.

} // end NotDeclaredError class.


//=== Represent an identifier that has already been declared as detected in semantic analysis.
public class AlreadyDeclaredError : CompilerError {

	private ISymbol original;
	public ISymbol OriginalDeclaration
	{ get { return original; } }

	public AlreadyDeclaredError (IToken id, ISymbol orig)
	: base (id)
	{
		original = orig;
	}

	public override String ToString () {
		int line = original.Source.Line;
		return String.Format ("{0:s} Identifier '{1:s}' is already declared at line {2:d}.",
			base.ToString(), token.TokenValue, line);
	}// end ToString method.

} // end AlreadyDeclaredError class.


//=== Represent a type conflict error as detected in semantic analysis.
public class TypeConflictError : CompilerError {

	private int typeFound;
	public int TypeFound
	{ get { return typeFound; } }

	private int typeExpected;
	public int TypeExpected
	{ get { return typeExpected; } }

	public TypeConflictError (IToken id, int found, int expected)
	: base (id)
	{
		typeFound = found;
		typeExpected = expected;
	}

	public override String ToString () {
		return String.Format ("{0:s} Type conflict: '{1:s}' is of type {2:s} where {3:s} is expected.",
			base.ToString(), token.ToString(), LanguageType.ToString(typeFound), LanguageType.ToString(typeExpected));
	}// end ToString method.

} // end TypeConflictError class.

} // end namespace.
