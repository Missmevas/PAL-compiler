/*
	File: Interfaces.cs
	Purpose: defines the interfaces that make up the compiler toolkit.
	Version: 2.1
	Date: 22nd May 2010.
	Author: Allan C. Milne.

	Namespace: AllanMilne.Ardkit
	Exposes: IToken, ICompilerError, IScanner, IParser, ISymbol, ISymbolTable, ISemantics.

	Description:
	This file defines all the interfaces for the components that make up the Ardkit compiler toolkit.
	These components are those appropriate to the construction of a recursive-descent compiler.
*/

using System;
using System.IO;
using System.Collections.Generic;


namespace AllanMilne.PALCompiler
{

//=== dummy class just to provide version and copyright information.
public class Interfaces {

	private static ComponentInfo info = new ComponentInfo (
		"Interfaces", "2.1", "May 2010", "Allan C. Milne", "Interface designs for toolkit components");
	public static ComponentInfo Info
	{ get { return info; } }

//=== The interfaces.

} // end Interfaces class.

public interface IToken {
	String TokenType { get; }
	String TokenValue { get; }
	int Line { get; }
	int Column { get; }
	bool Is (String s);
	String ToString ();
} // end IToken interface.


public interface ICompilerError {
	IToken ErrorToken { get; }
	String ToString();
} // end ICompilerError interface.


public interface IScanner {
	IToken CurrentToken { get; }
	bool EndOfFile { get; }
	void Init (TextReader src, List<ICompilerError> errs);
	IToken NextToken () ;
} // end IScanner interface.


public interface IParser {
	List<ICompilerError> Errors { get; }
	bool Parse (TextReader src);
	bool IsRecovering { get; }
} // end IParser interface.


public interface ISymbol {
	String Name { get; }
	int Type { get; }
	IToken Source { get; }
} // end ISymbol interface.


public interface ISymbolTable : IEnumerable<ISymbol> {
	bool IsDefined (String name);
	bool Add (ISymbol symbol);
	ISymbol Get (String name);
} // end ISymbolTable interface.


public interface ISemantics {
	int CurrentType { get; set; }
	void ResetCurrentType ();
} // end ISemantics interface.

} // end namespace.
