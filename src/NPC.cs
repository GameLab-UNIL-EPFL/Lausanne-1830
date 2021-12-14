using Godot;
using System;
using System.Collections.Generic;

public class NPC : KinematicBody2D {
	
	[Signal]
	public delegate void EndDialogue();
	
	[Signal]
	public delegate void StartDialogue();
	
	private const int MAX_CHAR_PER_LINE = 35;
	private const int MAX_LINES = 3;
	
	private DialogueController DC;
	private QuestController QC;
	
	private string[] AutoDialogues;
	private int AutoDialogueIdx = 0;
	
	private string[] DemandDialogues;
	private string[] InnerLines;
	private int DemandDialogueIdx = 0;
	private int InnerLinesCount = 0;
	private int QuestLinesCount = 0;
	
	//Used to display text
	private TextBox TB;
	
	[Export]
	public string AutoDialogueID;
	[Export]
	public string DemandDialogueID;
	[Export]
	public bool HasAutoDialogue = true;
	[Export]
	public bool HasDemandDialogue = true;
	[Export]
	public bool isQuestNPC = false;
	
	private bool inDialogue = false;
	
	private string[] QuestText(InfoValue_t res) {
		string[] outliers = res.Outliers().ToArray();
		if(res.IsCorrect()) {
			return FormatText("Alors...¢" +
			"Voyons voir ce registre.¢"+
			"Bravo ! Vous avez fait du bon travail !¢"+
			"Je vais garder ça dans nos documents importants.¢"+
			"Peut-être qu'un jour des historiens pourront utiliser ces informations¢"+
			"et en faire un jeu vidéo.");
		}
		
		string d = "Alors...¢" +
		"Voyons voir ce registre.¢" +
		//"Il y a encore plusieurs données qui sont eronnées, comme:¢";
		"Il y a encore ";
		var i = 0;
		foreach(var o in outliers) {
			//d += o + ", ";
			i++;
		}
		d += i;
		//d += "et ...¢" + "Je crois que les ai tous cités.¢";
		d += " données qui sont eronnées.¢";
		d += "Revenez me voir lorsque vous les aurez corrigées.";
		return FormatText(d);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Show();
		//Fetch the scene's Dialogue controller and the TextBox
		DC = Owner.GetNode<DialogueController>("DialogueController");
		QC = Owner.GetNode<QuestController>("QuestController");
		TB = GetNode<TextBox>("TextBox");
		
		//Sanity Check
		if(HasDemandDialogue || HasAutoDialogue) {
			if(DC == null) {
				throw new Exception("Every scene must have its own dialogue controller!!");
			} 
			
			//Check for auto dialogue
			if(HasAutoDialogue) {
				if(AutoDialogueID == null) {
					throw new Exception("NPC doesn't have a dialogueID!");
				}
				//Load in the NPC's dialogue
				AutoDialogues = DC._QueryDialogue(AutoDialogueID);
			}
			
			//Check for onDemand dialogue
			if(HasDemandDialogue) {
				if(DemandDialogueID == null) {
					throw new Exception("NPC doesn't have a dialogueID!");
				}
				DemandDialogues = DC._QueryDialogue(DemandDialogueID);
			}
		}
	}
	
	/**
	 * @brief Handles what happens when the player enters the ListenBox area,
	 * meaning that the player has entered the zone where they should be able to 
	 * hear the NPC's dialogue. This also causes the NPC to subscribe to the player
	 * allowing for onDemand dialogue to take place.
	 * @param tb, the TalkBox of the player that has entered the zone.
	 */
	private void _on_ListenBox_area_entered(Area2D tb) {
		if(tb.Owner is Player) {
			Player p = (Player)tb.Owner;
			//Subscribe to the player
			p._Subscribe(this);
			
			//Show auto dialogue if the NPC has one
			if(HasAutoDialogue) {
				//Fetch the right dialogue
				string d = AutoDialogues[AutoDialogueIdx];
				AutoDialogueIdx = (AutoDialogueIdx + 1) % AutoDialogues.Length;
				
				//Show it in the box
				TB._ShowText(d);
			}
		} 
	}
	
	/**
	 * @brief Handles what happens the the player is no longer in range to hear dialogue.
	 * This causes the NPC to unsubscribe to the player, making it no longer
	 * possible to generate onDemand dialogue.
	 * @param tb, the TalkBox of the player who has left the zone.
	 */
	private void _on_ListenBox_area_exited(Area2D tb) {
		if(tb.Owner is Player) {
			Player p = (Player)tb.Owner;
			//Unsubscribe to the player
			p._Unsubscribe(this);
			
			//Reset dialogue counter
			DemandDialogueIdx = 0;
			
			//Hide the text box
			TB._HideText();
		}
	}
	
	private string[] FormatText(string text) {
		string newText = "";
		int count = MAX_CHAR_PER_LINE;
		int lines = MAX_LINES;
		List<string> textLines = new List<string>();
		foreach(char c in text) {
			// Ignore new lines
			if(c == '\n') continue;
			
			// Only 3 lines per entry
			if(lines == 0 || c == '¢') {
				textLines.Add(newText);
				newText = "";
				lines = MAX_LINES;
				continue;
			}
			
			// Max 25 characters per line
			if(count-- == 0) {
				count = MAX_CHAR_PER_LINE;
				lines--;
			} 
			newText += c;
		}
		if(textLines.Count > 1) {
			textLines.Add(newText);
		}
		return textLines.ToArray();
	}
	
	/**
	 * @brief Called by the player when the NPC should be notified of an interaction.
	 */
	public void _Notify(Player player) {
		TB._HideAll();
		if(HasDemandDialogue) {
			//Check if there is any dialogue left
			if(DemandDialogueIdx < DemandDialogues.Length) {
				player._StartDialogue();
				
				//Fetch the right dialogue
				string d;
				if(InnerLinesCount == 0) {
					d = DemandDialogues[DemandDialogueIdx++];
					InnerLines = FormatText(d);
					InnerLinesCount = InnerLines.Length;
				} else {
					d = InnerLines[InnerLines.Length - InnerLinesCount--];
				}
				
				//Show it in the box
				TB._ShowText(d);
			} else {
				TB._HideText();
				player._EndDialogue();
			}
		}
	}
	
	public InfoValue_t _CompareSolutions(CharacterInfo_t characterInfo) {
		CharacterInfo_t solution = QC._QueryQuestSolution();
		return QC._CompareCharInfo(solution, characterInfo);
	}
	
	public InfoValue_t _EvaluateQuest(Player player, CharacterInfo_t characterInfo) {
		CharacterInfo_t solution = QC._QueryQuestSolution();
		InfoValue_t res = QC._CompareCharInfo(solution, characterInfo);
		
		if(!inDialogue) {
			inDialogue = true;
			player._StartDialogue();
			QC._InitBuffer(QuestText(res));
			TB._ShowText(QC._NextLine());
		} else {
			string l = QC._NextLine();
			//Check that the dialogue isn't over
			if(l == null) {
				TB._HideText();
				player._EndDialogue();
				inDialogue = false;
				QC._ClearCache();
			} else {
				TB._ShowText(l);
			}
		}
		return res;
	}
}

