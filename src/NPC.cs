using Godot;
using System;

public class NPC : KinematicBody2D {
	
	[Signal]
	public delegate void EndDialogue();
	
	[Signal]
	public delegate void StartDialogue();
	
	private DialogueController DC;
	
	private string[] AutoDialogues;
	private int AutoDialogueIdx = 0;
	
	private string[] DemandDialogues;
	private int DemandDialogueIdx = 0;
	
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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Show();
		//Fetch the scene's Dialogue controller and the TextBox
		DC = Owner.GetNode<DialogueController>("DialogueController");
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
	
	/**
	 * @brief Called by the player when the NPC should be notified of an interaction.
	 */
	public void _Notify(Player player) {
		if(HasDemandDialogue) {
			//Check if there is any dialogue left
			if(DemandDialogueIdx < DemandDialogues.Length) {
				player._StartDialogue();
				//Fetch the right dialogue
				string d = DemandDialogues[DemandDialogueIdx++];
				
				//Show it in the box
				TB._ShowText(d);
			} else {
				TB._HideText();
				player._EndDialogue();
			}
		}
	}
}

