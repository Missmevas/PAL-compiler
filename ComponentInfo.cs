/*
 	File:    ComponentInfo.cs
	Purpose: Provides toolkit-wide funtionality for version & copyright info.
	Author:  Allan C. Milne.
	Version: 2.3
	Date:    21st March 2011.

	Namespace: AllanMilne.Ardkit
	Exposes: ComponentInfo, Ardkit.

	Description:
	ComponentInfo represents the version and copyright information for a component.
	Use within a component class:
		private static ComponentInfo info = new ComponentInfo (
			"title, "version", "date", "author", "description");
		public static ComponentInfo Info { get { return info; } }

	The Ardkit class exposes this property to provide info on the toolkit as a whole.

*/

using System;

namespace AllanMilne.PALCompiler
{

public sealed class ComponentInfo {

	private String componentName;
	private String version;
	private String date; 
	private String author;
	private String description;

	public String ComponentName 
	{ get { return componentName; } }

	public String Version
	{ get { return version; } }

	public String Date
	{ get { return date; } }

	public String Author
	{ get { return author; } }

	public String Description
	{ get { return description; } }

	public String Copyright
	{ get {  return String.Format ("{0} v{1}: (c) {2}, {3}",
		componentName, version, author, date); }  }

	public override String ToString () {
		return String.Format ("{0} - {1}.", Copyright, Description);
	} // end ToString method.

	//--- constructor method
	public ComponentInfo (String componentName, String version, String date,
			String author, String description) {
		this.componentName = componentName;
		this.version       = version;
		this.date          = date;
		this.author        = author;
		this.description   = description;
	} // end ComponentInfo constructor method.

} // end ComponentInfo class.


//=== Encapsulates information for the toolkit as a whole.
public sealed class Ardkit {

	private static ComponentInfo info = new ComponentInfo (
		"Ardkit", "2.3", "March 2011", "Allan C. Milne", "Ardkit recursive-descent compiler toolkit");

	public static ComponentInfo Info
	{ get { return info; } }

	//--- information for all components.
	public static ComponentInfo[] Components {
	get { return new ComponentInfo[] {
		Ardkit.Info,
		Interfaces.Info,
		RdParser.Info,
		Scanner.Info,
		Semantics.Info,
		Token.Info,
		LanguageType.Info,
		SymbolTable.Info,
		Symbol.Info,
		CompilerError.Info
		};
	}
	}

} // end Ardkit class.

} // end namespace.
