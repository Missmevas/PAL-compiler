/*
	File:    Symbols.cs
	Purpose: A design pattern for representing user-defined symbols.
	Author:  Allan C. Milne.
	Version: 2.1
	Date:    14th February 2010.

	Namespace: AllanMilne.Ardkit
	Uses: ISymbol, IToken, LanguageType.
	Exposes:  Symbol, VarSymbol, ConstSymbol, ArraySymbol, FunctionSymbol.

	Description:
	This class hierarchy represents the meaning of a user defined name (identifier) as declared in the source program.
	This meaning is reflected by the attributes of the symbol; 
	in the base class, its source token and language type is maintained.
	Maintaining the source token provides access both to the actual symbol name and also to where it was declared;
	this latter will be useful for error reporting and/or debugging if required by the compiler environment.
	The language type of the symbol is implemented as an integer as exposed by the LanguageType class.

	The abstract Symbol class forms the basis of all other symbol representations
	with the subclasses representing specific meanings of an identifier.
	To support common uses of identifiers as variables, constants, arrays and function names,
	The VarSymbol, ConstSymbol, ArraySymbol and FunctionSymbol subclasses are defined.

	The ArraySymbol class represents Arrays that have multiple dimensions, are regular and each dimension has a lower and upper bound.
	The FunctionSymbol class  represents functions/procedures/methods that may take an arbitrary number of formal parameters,
	these parameters may only be of the standard language types; i.e. not arrays.
	The collection of Symbol objects is maintained by  a symbol table as used in the semantic analysis phase of compilation.

	Use for a specific language/compiler:
	1. For languages that only use identifiers to denote variables, constants, arrays and functions, the classes provided here are sufficient.
	2. For languages that have additional uses of identifiers for other entities:
		a) Define a subclass of Symbol for each different use.
		b) Add attributes, properties and methods as required to model the use.

*/

using System;
using System.Collections.Generic;

namespace AllanMilne.PALCompiler
{


//=== The base abstract symbol class.
public abstract class Symbol : ISymbol {


	private static ComponentInfo info = new ComponentInfo (
		"Symbol", "2.1", "February 2010", "Allan C. Milne", "Representing user-defined symbols of a source program");
	public static ComponentInfo Info
	{ get { return info; } }

   //-- attributes and accessor methods.

   protected IToken source;
   public IToken Source
   { get { return source; } }

   public String Name
   { get { return source.TokenValue; } }

   protected int type;
   public int Type 
   { get { return type; } }

   //--- constructor method
   public Symbol (IToken token, int typ) {
      this.source = token;
      this.type = typ;
   } // end Symbol constructor method.

} // end Symbol class.

//=== representing an identifier used as a variable.
public class VarSymbol : Symbol {

   public VarSymbol (IToken token, int typ) 
   : base (token, typ)
   { }

} // end VarSymbol class.


//=== representation of an identifier used to define a constant .
public class ConstSymbol : Symbol {

   public ConstSymbol (IToken token, int typ)
   : base (token, typ)
   { }

} // end ConstSymbol class.


//=== Representation of an identifier used as an array name.
//=== Arrays have multiple dimensions, are regular and each dimension has a lower and upper bound.
//=== Dimensions can be created with explicit upper and lower bounds or with a sice, if the latter then a lower bound of 0 is used.
public class ArraySymbol : Symbol {

   //--- represents the bounds of one dimension.
   private struct ArrayBounds {
      public int LowerBound;
      public int UpperBound;
      public ArrayBounds (int lower, int upper) {
         LowerBound = lower;
         UpperBound = upper;
      }
   } // end ArrayBounds struct.

   //--- the bounds of each dimension.
   private List<ArrayBounds> dimensions;

   //--- return the number of dimensions in the array.
   public int Dimensions
   { get { return dimensions.Count; } }

   //--- Return the size and bounds of the specified dimension; dimensions start counting from  1.
   public int Size (int d) {
      if (d<=0 || d>dimensions.Count) return 0;
      ArrayBounds bounds = dimensions[d-1];
      return bounds.UpperBound - bounds.LowerBound + 1;
   }
   public int LowerBound (int d) {
      if (d<=0 || d>dimensions.Count) return 0;
      return dimensions[d-1].LowerBound;
   }
   public int UpperBound (int d) {
      if (d<=0 || d>dimensions.Count) return 0;
      return dimensions[d-1].UpperBound;
   }

   //--- Determine if an index is within the bounds of a dimension.
   public bool InBounds (int d, int index) {
      if (d<=0 || d>dimensions.Count) return false;
      return (index >= dimensions[d-1].LowerBound) && (index <= dimensions[d-1].UpperBound);
   }

   //--- overloaded to add a new dimension.
   public void AddDimension (int size) {   // assumes lower bound of 0.
      if (size <= 0)
           dimensions.Add (new ArrayBounds(0,0));
      else dimensions.Add (new ArrayBounds (0, size-1));
   }
   public void AddDimension (int lower, int upper) {   // specific lower and upper bounds.
      if (lower > upper) lower = upper = 0;
      dimensions.Add (new ArrayBounds (lower, upper));
   }

   //--- overloaded constructor methods.
   public ArraySymbol (IToken token, int type) 
   : base (token, type) {   // no dimensions specified.
      dimensions = new List<ArrayBounds>();
   }
   public ArraySymbol (IToken token, int type, params int[] sizes)
   : base (token, type) {   // params are sizes for dimensions with lower bound of 0.
      dimensions = new List<ArrayBounds>();
      for (int i=0; i<sizes.Length; i++)
         AddDimension (sizes[i]);
   } // end constructor methods.

} // end ArraySymbol Class.


//=== Representation of an identifier used as a function name.
//=== Allows any number of formal parameters, although these must be of elementary language types.
public class FunctionSymbol : Symbol {

   //--- represents a formal parameter.
   private struct FormalParameter {
      public int Type;
      public String Name;
      public FormalParameter (int t, String n) {
         Type = t;
         Name = n;
      }
   } // end FormalParameter struct.

   //--- the types and names of the formal parameters.
   private List<FormalParameter> formalParams;

   //--- Returns the number of formal parameters.
   public int Parameters 
   { get { return formalParams.Count; } }

   //--- add a new formal parameter.
   public void AddParam (int type, String name) { 
      formalParams.Add (new FormalParameter (type, name)); 
   }

   //--- Return the type and name of the n'th formal parameter - start at parameter 1 index .
   public int ParamType (int n) {
      if (n <1 || n>formalParams.Count)
         return LanguageType.Undefined;
    return formalParams[n-1].Type;
   }
   public String ParamName (int n) {
      if (n <1 || n>formalParams.Count)
         return "";
    return formalParams[n-1].Name;
   }

   //--- constructor method.
   public FunctionSymbol (IToken token, int type) 
   : base (token, type) {
    formalParams = new List<FormalParameter>(); 
   }

} // end FunctionSymbol class.

} // end namespace.
