using Godot;
using System;

public class NPC : KinematicBody2D {
	
	private DialogueController DC;
	private string[] Dialogues;
	private int DialogueIdx = 0;
	
	//Used to display text
	private TextBox TB;
	
	[Export]
	public string DialogueID;
	[Export]
	public bool HasDialogue = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Show();
		//Fetch the scene's Dialogue controller and the TextBox
		DC = Owner.GetNode<DialogueController>("DialogueController");
		TB = GetNode<TextBox>("TextBox");
		
		//Sanity Check
		if(HasDialogue) {
			if(DC == null) {
				throw new Exception("Every scene must have its own dialogue controller!!");
			} 
			
			if(DialogueID == null) {
				throw new Exception("NPC doesn't have a dialogueID!");
			}
			//Load in the NPC's dialogue
			Dialogues = DC._QueryDialogue(DialogueID);
		}
	}

	private void _on_ListenBox_area_entered(Area2D tb) {
		if(tb.Owner is Player) {
			//Fetch the right dialogue
			string d = Dialogues[DialogueIdx];
			DialogueIdx = (DialogueIdx + 1) % Dialogues.Length;
			
			//Show it in the box
			TB._ShowText(d);
		} 
	}
	
	private void _on_ListenBox_area_exited(Area2D tb) {
		if(tb.Owner is Player) {
			//Hide the text box
			TB._HideText();
		}
	}
}



