using Godot;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

public class DialogueController : Node {
	
	//File at which the scene's dialogue is stored
	[Export]
	public string SceneDialogueFile = "res://assets/04_dialogues/01_Palud/testDialogue.xml";
	
	//Local XDocument containing a parsed version of the dialogue
	private XDocument dialogueTree;
	
	private string SanitizePath(string path) {
		string path_tmp = "";
		if(OS.HasFeature("editor")) {
			//Running from an editor binary.
			//`path` will contain the absolute path located in the project root.
			path_tmp = ProjectSettings.GlobalizePath(path);
		} else {
			//Running from an exported project.
			//`path` will contain the absolute path to `hello.txt` next to the executable.
			//This is *not* identical to using `ProjectSettings.globalize_path()` with a `res://` path,
			//but is close enough in spirit.
			string san_path = path.Split(':')[1];
			san_path = san_path.Substring(2, san_path.Length);
			path_tmp = OS.GetExecutablePath().GetBaseDir().PlusFile(san_path);
		}
		return path_tmp;
	}
	
	/**
	 * @brief Parses the XML file and loads it into a local XDocument
	 */
	private void ParseXML() {
		if(SceneDialogueFile == null) {
			throw new Exception("No dialogue was input for the scene!");
		}
		
		//Load XML file into a XDocument for querying
		string newPath = SanitizePath(SceneDialogueFile);
		var xml = XDocument.Load(newPath);
		
		//Sanity check
		if(xml != null) {
			dialogueTree = xml;
		} else {
			throw new Exception("Unable to load xml file!");
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		ParseXML();
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
		
		List<string> res = new List<string>();
		var newList = query.Select(x => x.Select(y => y.Value));
					
		foreach(var elem in newList) {
			foreach(var text in elem) {
				res.Add(text);
			}
		}
		
		return res.ToArray();
	}
}
