using Godot;
using System;

public class NotebookList : Node2D {
	[Export]
	public string DBFilePath;
	
	//Local XDocument containing a parsed version of the dialogue
	private XDocument characterAttributes;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Parse the XML file and store result in characterAttributes
		DialogueController._ParseXML(ref characterAttributes, DBFilePath);
	}
	
	/**
	 * @brief Queries the local XDocument for a given dialogue
	 * @param dialogueID, the id of the dialogue being queried
	 * @return a string array containing all of the lines of the dialogue
	 */
	public string[] _QueryDialogue(string dialogueID) {
		// Query the data and write out resulting texts as a string array
		var query = from dialogue in dialogueTree.Root.Descendants("dialogue")
					where dialogue.Attribute("id").Value == dialogueID
					select dialogue.Elements("text");
					
		/*var optionQuery = from text in query.Descendants("option")
						  select text.Value;*/
					
		List<string> res = new List<string>();
		
		//Check for options
		//if(query.Elements("option").isEmpty()) {
			//Map the queried XElements to their respective values
			var newList = query.Select(x => x.Select(y => y.Value));
			
			//Painfully convert the IEnumeration to a usable list
			//This step is necessary, since one can't obtain the size of an 
			//IEnumeration without iterating through it #lazyevaluation
			foreach(var elem in newList) {
				foreach(var text in elem) {
					res.Add(text);
				}
			}
		//}
		
		return res.ToArray();
	}
}
