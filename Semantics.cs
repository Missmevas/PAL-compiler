/*
	File:    Semantics.cs
	Purpose: A class design pattern encapsulating a base for semantic analysis.
	Author:  Allan C. Milne.
	Version: 2.1
	Date:    23rd March 2010.

	Namespace: AllanMilne.Ardkit
	Uses: ISemantics, IParser, ICompilerError, LanguageType.
	Exposes:  Semantics.

	Description:
	A design pattern for a class that encapsulates semantic analysis methods
	that can be called from the RD parsing recogniser methods.
	A reference is required back to the parent parser so that semantic analysis
	can add semantic errors to the parser's error collection and, if a RecoveringRdParser, detect if the parser is recovering.

	Since semantics are language specific this class is very outline in nature, providing only basic functionality.

	Use for a specific language/compiler:
	Semantic analysis is very much language dependant and will require specific semantic analysis methods to be defined.
	1. Define a class inheriting from Semantics.
	2. define a constructor taking an IParser parameter and calling the base constructor with this parameter.
	3. Define additional members as required for the processing of the language.
		a) Public semantic analysis methods reflecting the semantics of the syntactic elements.
		b) Private attributes, if required, to maintain semantic state across syntactic elements.
	4. If required, define additional subclasses inheriting from CompilerError to represent the different semantic errors that may occur - see CompilerError.cs for details.
	5. in the language'-specific parser 
		a) define an attribute composing with the above language-specific semantic analyser class.
		b) In the parser's constructor method, initialise the attribute to an instance of the language semantic analyser class, passing (this) as parameter.
		c) Add calls to the semantic analysis methods at appropriate points inn the RD recogniser methods.

	Note the semantic analyser attribute of the parser that must be added in the last point above;
	this cannot be included in the RdParser base class as the language-specific RD recognisers require to 
	access specific semantic analysis methods that are defined in the language-specific semantic analyser and therefore the attribute must have this type.

*/

using System;

namespace AllanMilne.PALCompiler
{

//===The semantic analysis class.
public abstract class Semantics : ISemantics {

	private static ComponentInfo info = new ComponentInfo (
		"Semantics", "2.1", "March 2010", "Allan C. Milne", "Abstract semantic analyser base class");
	public static ComponentInfo Info
	{ get { return info; } }

   //--- attributes and properties used with semantic analysis methods of subclasses.

   protected IParser parser;                 // reference back to calling parser object.
    protected int currentType;              // current type expected in the parse.

   //--- add error object if the parser is not recovering.
   protected void semanticError (ICompilerError err) {
      if (parser.IsRecovering) return;      // don't record semantic errors if already recovering from syntax error.
      parser.Errors.Add (err);
      if (!(parser is RecoveringRdParser))
         throw new CompilerErrorException();     // terminate compilation on 1st error.
   } // end semanticError method.

   //--- public API.

   public int CurrentType {
   get { return currentType; }
   set { currentType = value; }
   }

   public void ResetCurrentType ()
   { currentType = LanguageType.Undefined; }

   // Constructor method; requires reference to calling parser.
   public Semantics (IParser p) {
      parser = p;
      currentType = LanguageType.Undefined;
   } // end constructor method.

} // end Semantics class.

	public class customSemantics : Semantics
	{
		public customSemantics(IParser p)
		: base(p)
		{

		}

		public void DeclareID(IToken id, int varType)
		{
			if (!id.Is(Token.IdentifierToken)) return;
			Scope symbols = Scope.CurrentScope;
			if (symbols.IsDefined(id.TokenValue))
			{
				semanticError(new AlreadyDeclaredError(
				id, symbols.Get(id.TokenValue)));
			}
			else
			{
				symbols.Add(new VarSymbol(id, varType));
			}
		} // end DeclareId method.

		public int CheckID(IToken id)
		{
			if (!Scope.CurrentScope.IsDefined(id.TokenValue))
			{
				semanticError(new NotDeclaredError(id));
				return LanguageType.Undefined;
			}

			else
			{
				return Scope.CurrentScope.Get(id.TokenValue).Type;
			}
		}

		public void CheckTypesSame(IToken token, int oldType, int newType)
		{
			if (oldType != newType)
			{
				semanticError(new TypeConflictError(token, newType, oldType));
			}
		}

	}

} // end namespace.