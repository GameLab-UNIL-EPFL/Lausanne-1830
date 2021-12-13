using Godot;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

public struct Text_t {
	bool hasOptions;
	string[] options;
};

public struct Dialogue_t {
	Text_t[] texts;
	string type; //onDemand or onApproach
	string target1; //Name of person talking
};

public class DialogueController : Node {
	
	//File at which the scene's dialogue is stored
	[Export]
	public string SceneDialogueFile = "res://assets/04_dialogues/01_Palud/testDialogue.xml";
	
	//Local XDocument containing a parsed version of the dialogue
	private XDocument dialogueTree;
	
	/**
	 * @brief Converts a local res:// to a global usable path,
	 * depending on the how the executable is run.
	 * @param path, the local godot resource path needed to be sanitized.
	 * @return a global path usable by Linq (so accessible outside of Godot).
	 */
	public static string _SanitizePath(string path) {
		string path_tmp = "";
		/*if(OS.HasFeature("editor")) {
			//Running from an editor binary.
			//`path` will contain the absolute path located in the project root.
			path_tmp = ProjectSettings.GlobalizePath(path);
		} else {
			//Running from an exported project.
			//`path` will contain the absolute path, next to the executable.
			//This is *not* identical to using `ProjectSettings.globalize_path()` with a `res://` path,
			//but is close enough in spirit.
			string san_path = path.Split(':')[1];
			san_path = san_path.Substring(2, san_path.Length);
			path_tmp = OS.GetExecutablePath().GetBaseDir().PlusFile(san_path);
		}*/
		return ProjectSettings.GlobalizePath(path);
	}
	
	/**
	 * @brief Parses the XML file and loads it into a local XDocument
	 */
	public static void _ParseXML(ref XDocument targetXML, string filePath) {
		if(filePath == null) {
			throw new Exception("No xml file was input for the scene!");
		}
		
		//Load XML file into a XDocument for querying
		string newPath = _SanitizePath(filePath);
		var xml = XDocument.Load(newPath);
		
		//Sanity check
		if(xml != null) {
			targetXML = xml;
		} else {
			throw new Exception("Unable to load xml file!");
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		_ParseXML(ref dialogueTree, SceneDialogueFile);
	}
	
	/**
	 * @brief Queries the local XDocument for a given dialogue
	 * @param dialogueID, the id of the dialogue being queried
	 * @return a string array containing all of the lines of the dialogue
	 */
	public string[]/*Dialogue_t*/ _QueryDialogue(string dialogueID) {
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
