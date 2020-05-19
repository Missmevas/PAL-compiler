/*
 	File:    Token.cs
	Purpose: Representing terminal symbols of a language.
	Author:  Allan C. Milne.
	Version: 2.4
	Date:    11th April 2011.

	Namespace: AllanMilne.Ardkit
	Uses: IToken.
	Exposes: Token.

	Description:
	 An implementation of a terminal symbol representation for a language.
	The Token class represents both 
	* simple terminal types such as punctuation and keywords
	* microsyntax tokens such as identifiers and literal constants.
	This version reflects the notation convention of microsyntax names of EBNF version 3.

	The TokenType property 
	- for simple terminals is the terminal string 
	- for microsyntax tokens is a representation of the microsyntax terminal type;
	  constants are defined for the typical microsyntax terminal types.

	The TokenValue property is the actual input terminal string, both for simple and microsyntax tokens.
	The start position (line, column) of the token in the source stream is provided.

	Use for a specific language/compiler:
	* if the language contains only integers, reals, booleans and/or strings then no additional code is required.
	* For other microsyntax terminal types then string constants identifying the type should be defined in some class,
	  this might be a class inheriting from Token and used only to encapsulate these new constants.

*/

using System;

namespace AllanMilne.PALCompiler
{

//=== The representation of a terminal symbol.
public class Token : IToken { 

	private static ComponentInfo info = new ComponentInfo (
		"Token", "2.4", "April 2011", "Allan C. Milne", "Representing terminal symbols of a language");
	public static ComponentInfo Info
	{ get { return info; } }

   //--- String constants to use with the TokenType property.
   public const String EndOfFile       = "EndOfFile";
   public const String InvalidChar     = "InvalidChar";
   public const String InvalidToken    = "InvalidToken";
   public const String IdentifierToken = "Identifier";
   public const String IntegerToken    = "Integer";
   public const String RealToken       = "Real";
   public const String StringToken    = "String";
   public const String BooleanToken   = "Boolean";

   //--- private fields with public properties.
   private String tokType;     // token value or type. 
   public  String TokenType
   { get { return tokType; } }

   private String tokValue;   // actual terminal string in source.
   public String TokenValue
   { get { return tokValue; } }

   private int    linePos, colPos;  // position in source stream. 
   public  int Line 
   { get { return linePos; } }
   public  int Column 
   { get { return colPos; } }

   //--- determine if this token is of a specified token type.
   public bool Is (String s) 
   { return tokType.Equals (s); }

   //--- String representation of token with type and value if appropriate.
   public override String ToString () {
      if (tokType.Equals (tokValue))
           return tokType;
      else return String.Format ("{0:s} (\"{1:s}\")", tokType, tokValue);
   }

   //--- constructor methods.
   public Token (String type, int lne, int col) {
      this.tokType  = type;
      this.tokValue = type;
      this.linePos  = lne;   
      this.colPos = col;
   } 
   public Token (String type, String token, int lne, int col) {
      this.tokType  = type;
      this.tokValue = token;
      this.linePos  = lne;   
      this.colPos = col;
   } // end Token constructor methods.

} // end Token class.

} // end namespace.
