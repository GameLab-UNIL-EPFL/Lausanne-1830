using Godot;
using System;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

public class DialogueController : Node {
	
	//File at which the scene's dialogue is stored
	[Export]
	public string SceneDialogueFile = "res://db/dialogues/xml/Dialogues.xml";
	
	//Local XDocument containing a parsed version of the dialogue
	private XDocument dialogueTree;
	
	//Used to lock the Dialogue controller during an interaction
	private bool IsOccupied
	{get; set;}
	
	//Used to store the texts of each target
	private Queue<String> target0Text;
	private Queue<String> target1Text;
	
	public const string ON_DEMAND = "onDemand";
	public const string ON_APPROACH = "onApproach";
	
	/**
	 * @brief Parses the XML file and loads it into a local XDocument
	 */
	public static void _ParseXML(ref XDocument targetXML, string filePath) {
		if(filePath == null) {
			throw new Exception("No xml file was input for the scene!");
		}
		
		//Load XML file into a XDocument for querying
		var file = new File();
		if (file.Open(filePath, File.ModeFlags.Read) != 0) {
			throw new Exception("Unable to open the xml file: " + filePath);
		}
		var loadedXML = file.GetAsText();
		var xml = XDocument.Parse(loadedXML);
		file.Close();
		
		//Sanity check
		if(xml != null) {
			targetXML = xml;
		} else {
			throw new Exception("Unable to load xml file!");
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		//Initialize dialogue queues
		target0Text = new Queue<String>();
		target1Text = new Queue<String>();
		
		//Load XML and init state
		IsOccupied = false;
		_ParseXML(ref dialogueTree, SceneDialogueFile);
	}
	
	/**
	 * @brief Queries the local XDocument for a given dialogue
	 * @param dialogueID, the id of the dialogue being queried
	 * @param type, either onApproach or onDemand, i.e. the type of the requested dialogue
	 * @param targetNum, the number of the target speaking.
	 * @return a string array containing all of the lines of the dialogue
	 */
	private string[] QueryDialogue(string dialogueID, string type, int targetNum = 0) {
		// Query the data and write out resulting texts as a string array
		var query = from dialogue in dialogueTree.Root.Descendants("dialogue")
					where dialogue.Attribute("id").Value == dialogueID &&
						dialogue.Attribute("type").Value == type &&
						int.Parse(dialogue.Attribute("ntargets").Value) > targetNum
					select dialogue.Elements("text");
					
		List<string> res = new List<string>();
		
		//Iterate through query results and pick a text for each selection  
		Random rnd = new Random();
		foreach(var txt in query) {
			//Check for options
			var nOptions = txt.Descendants("option").Count();
			if(nOptions == 0) {
				//No options -> text can directly be added to res
				var txtValues = txt.Select(x => x.Value);
				
				//Sanity check
				if(txtValues == null) {
					throw new Exception("Query must select something!");
				}
				
				//Query only returns an iterator, so we iterate
				foreach(var t in txtValues) {
					res.Add(t);
				}
			} else {
				//Pick an option at random
				int nextText = rnd.Next(0, nOptions);
				var optionQuery = from opt in txt.Descendants("option")
								  where int.Parse(opt.Attribute("id").Value) == nextText
								  select opt.Value;
				//Sanity check
				if(optionQuery == null) {
					throw new Exception("Option Query must select something!");
				}
				
				foreach(var t in optionQuery) {
					res.Add(t);
				}
			}
		}
		
		return res.ToArray();
	}
	
	private void FillQueue(string dialogueID, int id = 0, bool isApproach = false) {
		//Fetch initial dialogues and fill queues
		string[] texts = QueryDialogue(
				dialogueID, 
				isApproach ? ON_APPROACH : ON_DEMAND,
				id == 0 ? 0 : 1 //Make sure its only 0 or 1
		);
		//throw new Exception(string.Join(", ", texts));
		for(int i = 0; i < texts.Length ; ++i) {
			if(id == 0) {
				target0Text.Enqueue(texts[i]);
			} else {
				target1Text.Enqueue(texts[i]);
			}
		}
	}
	
	/**
	 * @brief Starts the given dialogue and returns the first text
	 * @param dialogueId, the id of the dialogue that must be started 
	 * @returns the first text of the dialogue
	 */
	public string _StartDialogue(string dialogueId, bool isApproach = false, bool dualTargets = false) {
		//Check for lock
		if(IsOccupied) {
			return "Occupied";  
		} else {
			//Clear the queues
			target0Text.Clear();
			target1Text.Clear();
			
			//Grab lock if not onApproach
			if(!isApproach) {
				IsOccupied = true;
			}
			
			//Fetch initial dialogues and fill queues
			FillQueue(dialogueId, 0, isApproach);
			
			//Sanity Check
			if(target0Text.Count == 0) {
				throw new Exception("Queue not filled!");
			}
			
			//Check for second target
			if(dualTargets) {
				FillQueue(dialogueId, 1, isApproach);
			}
			
			//Target0 always starts the conversation
			try {	
				var txt = target0Text.Dequeue();
				if(txt == null) {
					throw new Exception("No text dequeued!");
				}
				return txt;
 			} catch {
				//If an exception ways thrown, the person has no dialogue
				return "Mince, j'ai oublié ce que je devais dire...";
			}
		}
	}
	
	/**
	 * @brief Fetches the next text in the dialogue for the given npc
	 * @param tragetId, the id of the npc requesting the text
	 * @returns the text required to continue the dialogue
	 */
	public string _NextDialogue(int targetId = 0) {
		var Q = targetId == 0 ? ref target0Text : ref target1Text;
		try {
			return Q.Dequeue();
		} catch(InvalidOperationException e) {
			if(targetId == 0) _EndDialogue();
			return null;
		}
	}  
	
	/**
	 * @brief Ends the currently playing dialogue
	 */
	public void _EndDialogue() {
		IsOccupied = false;
		
		//Clear the queues
		target0Text.Clear();
		target1Text.Clear();
	}
}
