using Godot;
using System;
using System.Diagnostics;

public class WalkingSoundTrigger : Area2D {
	[Export]
	private AudioTypes EnterType;
	
	[Export]
	private AudioTypes ExitType;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		this.Connect("area_entered", this, nameof(_on_area_entered));
		this.Connect("area_exited", this, nameof(_on_area_exited));
	}

	private void _on_area_entered(Area2D hb) {
		//Check for player
		if(hb.Owner is Player) {
			Player p = (Player)hb.Owner;
			p._UpdateAudioType(EnterType);
		}
	}
	
	private void _on_area_exited(Area2D hb) {
		//Check for player
		if(hb.Owner is Player) {
			Player p = (Player)hb.Owner;
			
			// Check for update conflict
			if(p._GetAudioType() == EnterType) {
				p._UpdateAudioType(ExitType);
			}
		}
	}
}


