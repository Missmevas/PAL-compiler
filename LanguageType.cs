/*
	File:    LanguageType.cs
	Purpose: Encapsulation of typical language type representations.
	Author:  Allan C. Milne.
	Version: 2.0
	Date:    10th february 2010.

	Namespace: AllanMilne.Ardkit
	Exposes:  LanguageType.

	Description:
	This class encapsulates some typical language types that may be encoutnered in a small language.
	No object of this class may be instantiated as the constructor is defined as protected;
	this is protected to allow subclasses to be defined..
	Static integer constants are exposed to act as identifiers for the supported types.
	A static ToString method will return the string name of a supplied type identifier.
 Static conversion methods are exposed to convert a string into a value of each of the supported types;
	the representation of these values in this implementation is the corresponding .Net type and the conversion is implemented by the relevant Convert.Toxxx method.

	Use for a specific language/compiler:
	The actual language types required to be represented depend on the language being processed.
	The underlying data representation of values of these required types depend on the artifacts to be generated and the target execution platform.
	For many small languages this class will be adequate both in terms of coverage and data representation.

	If you require a different representation of the value of a type supported by this class then
	1. Define a subclass of LanguageType.
	2. Create a 'new' static conversion method for the type, returning the new data representation of the value.
	3. Define a private constructor method to inhibit object instantiation.

	If you require to have additional language types then
	1. Define a subclass of LanguageType.
	2. expose a relevant static int constant for the new type; ensure it is different from those already in use.
	3. Create a 'new' static ToString method so that it returns the string name for the new type and calls LanguageType.ToString to return the names of all other types.
	4. Define a static conversion method for the new type.
	5. Define a private constructor method to inhibit object instantiation.

*/


using System;

namespace AllanMilne.PALCompiler
{

public class LanguageType {

	private static ComponentInfo info = new ComponentInfo (
		"LanguageType", "2.0", "February 2010", "Allan C. Milne", "Encapsulation of typical language type representations");
	public static ComponentInfo Info
	{ get { return info; } }

	//--- Identifiers for the language types supported.
	public const int Undefined = 0;
	public const int Integer   = 1;
	public const int Real      = 2;
	public const int Boolean   = 3;
	public const int String    = 4;

	//--- Return the string name of a type.
	public static String ToString (int type) {
		switch (type) {
	case Undefined:	return "undefined"; 
	case Integer:	return "integer"; 
	case Real:	return "real";
	case Boolean:	return "boolean";
	case String:	return "string";
	}
	return "UNKNOWN TYPE - CONTACT DEVELOPER.";
	} // end ToString method.

	//=== Conversion methods for each supported type.

	public static int ToInteger (String str)
	{ return Convert.ToInt32 (str); }

	public static double ToReal (String str)
	{ return Convert.ToDouble (str); }

	public static bool ToBoolean (String str)
	{ return Convert.ToBoolean (str); }


	//--- protected constructor to inhibit object instantiation.
	protected LanguageType () {}

} // end LanguageType class.

} // end namespace.