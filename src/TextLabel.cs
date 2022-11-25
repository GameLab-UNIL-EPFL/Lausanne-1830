using Godot;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

/**
 * @brief Translatable Label
 */
public class TextLabel : Label {
	private Context context;
	private XDocument labelsDB;
	
	[Export]
	public string id = "";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Retrieve context for language
		context = GetNode<Context>("/root/Context");
		labelsDB = context._GetLabelsDB();
		
		// Set the value of the label
		Text = string.Format("{0} :", QueryLabel());
	}
	
	private string QueryLabel() {
		// Sanity Check
		if(id == "") {
			throw new Exception("Label ID must not be empty!!");
		}
		// Query the data and write out resulting texts as a string array
		var query = from label in labelsDB.Root.Descendants("label")
					where label.Attribute("id").Value == id
					select label;
					
		// Simply Extract the string value from the query
		foreach(XElement l in query) {
			return l.Value;
		}
		
		// If we get to this point then the label is empty
		throw new Exception("No Label found in query for id: " + id);
	}
}
