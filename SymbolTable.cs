/*
	File:    SymbolTable.cs
	Purpose: Encapsulating a symbol table and scoping mechanism for use in a compiler.
	Author:  Allan C. Milne.
	Version: 2.1
	Date:    22nd May 2010.

	Namespace: AllanMilne.Ardkit
	Uses: ISymbolTable, ISymbol.
	Exposes:  SymbolTable, Scope.

	Description:
	This design pattern is for a class encapsulating the functionality of a basic symbol table
	and a scoping mechanism encapsulating that symbol table for scoped language constructions.

	The symbol table provides methods for adding and retrieving symbols;
	a generic Dictionary class is used to store the identifier name/Symbol pairs.
SymbolTable implements the IEnumerable interface to allow extraction of all symbls using a foreach statement;
the returned enumerator iterates the Symbol values only (i.e. not the keys).

	The scope  mechanism models scoped declarations through encapsulating symbol tables via stacked scope objects,
	this stack modelling the nested pattern of scope constructions in a program.
	Each Scope object denotes a single scope construction in the source program.
	This implementation exposes both static and instance methods to provide the public functionality;
	- static methods provide the mechanism for opening/closing scope constructions with the current scope maintained;
	- the ISymbolTable instance interface is implemented for accessing symbols in the current and nested symbol tables;
- the Ienumerable eterates only on the current scope construction symbols;
	- the Depth() instance method is exposed to return the level of nesting of a name.
	Symbol tables are searched in reverse order of the nested scopes (i.e. a stack).
	Since static methods are used this is a variation of the Singleton pattern and 
	therefore only one scope mechanism is available per compilation process.

	Use for a specific language/compiler:
	no amendments are required for most languages.
	This implementation is not suitable where there is a requirement for multiple concurrent scopes.
*/

using System;
using System.Collections;
using System.Collections.Generic;


namespace AllanMilne.PALCompiler
{

//=== The symbol table.
public class SymbolTable : ISymbolTable {

	private static ComponentInfo info = new ComponentInfo (
		"SymbolTable/Scope", "2.1", "May 2010", "Allan C. Milne", "Representing the collection of all user-defined symbols in a source program");
	public static ComponentInfo Info
	{ get { return info; } }

   private Dictionary<String,ISymbol> symbols;   // collection of all symbols.

   //--- constructor method.
   public SymbolTable () {
      this.symbols     = new Dictionary<String,ISymbol> ();
   } // end constructor method.

   //--- implementing the Ienumerable interface on the Symbol values.
   //--- Note this must be implemented both for the Collections and Collections.Generic versions.
   IEnumerator<ISymbol> IEnumerable<ISymbol>.GetEnumerator ()
   {  return symbols.Values.GetEnumerator();  }
   IEnumerator IEnumerable.GetEnumerator ()
   {  return symbols.Values.GetEnumerator();  }

   //--- Check if named symbol already exists in the table.
   public bool IsDefined (String name) 
  {    return symbols.ContainsKey (name); }

   //--- Add a symbol to the symbol table;
   //--- return true if successful false otherwise.
   public bool Add (ISymbol symbol) {
      if (IsDefined (symbol.Name)) return false;      // no duplicate names allowed.
      symbols.Add (symbol.Name, symbol);
      return true;
   } // end Add method.

   //--- Get a specific symbol; returns null if not in the table.
   public ISymbol Get (String name) {
      ISymbol symb;
      if (symbols.TryGetValue (name, out symb))
           return symb;
      else return null;
   } // end Get method.

} // end SymbolTable class.


//=== Representation of scope.
public class Scope : ISymbolTable {

   //--- the static class variables and methods.

   private static Scope current = null;      // the current Scope object.

   //--- returns the current scope object - can be used to save the current scope stack.
   public static Scope CurrentScope
   { get {  return current;  } }

   public static  Scope OpenScope () {
      Scope s = new Scope (current);
      current = s;
      return s;
   }

   public static void CloseScope () {
      if (current == null) return;
      current = current.outer;
   }

   //--- Restore a scope stack.
   public void RestoreScope (Scope s)
   { current = s; }


   //--- The instance variables and methods.

   private Scope outer;           // links to the containing Scope object.
   private ISymbolTable symbols;   // the symbol table for this scope.

   //--- note the constructor method is private - called from OpenScope().
   private Scope (Scope parent) {
      outer = parent;
      symbols = new SymbolTable ();
   } // end Scope constructor method.

   //--- implement the IEnumerable interface; 
   //--- note this only iterates over symbols in the symbol table of this scope, it does not apply to any parent scopes.
   //--- Note this must be implemented both for the Collections and Collections.Generic versions.
    IEnumerator<ISymbol> IEnumerable<ISymbol>.GetEnumerator ()
   {  return symbols.GetEnumerator();  }
    IEnumerator IEnumerable.GetEnumerator ()
   {  return symbols.GetEnumerator();  }

   //--- determine if name is already in scope.
   public bool IsDefined (String name) {
      bool defined = symbols.IsDefined (name);     // look in current symbol table.
      if (defined || outer==null)
           // either name is found in current symbol table or there is no outer table - so return what we've got.
           return defined;
      else return outer.IsDefined (name);     // not found so search recursively through outer scope.
   } // end IsDefined method.

   //--- add a symbol to the current scope.
   //--- returns false if name already in current table.
   public bool Add (ISymbol symbol) 
   {  return symbols.Add (symbol);  }

   //--- return a required symbol; looks through nested scopes if necessary.
   public ISymbol Get (String name) { 
      ISymbol s = symbols.Get (name);
      if (s!=null || outer==null)
           return s;                  // found or there is no enclosing scope.
      else return outer.Get (name);   // search in outer scope.
   } // end Get method.

   //--- Find the level of scoping of a name.
   //--- Current scope is defined as level 0.
   //--- If the name is not in scope then it will return the maximum scoping level.
   public int Depth (String name) {
      if (symbols.IsDefined(name) || outer==null)
           return 0;
      else return (1 + outer.Depth(name));
   } // end Depth method.

} // end Scope class.

} // end namespace.
