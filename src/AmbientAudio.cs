using Godot;
using System;
using System.Diagnostics;

public class AmbientAudio : AudioStreamPlayer2D {
	Area2D audioArea;
	private Player p;

    [Export]
    private Vector2 lastPosition = null; // Initial lastPosition should be the closest point to the area2D
    [Export]
	private float falloff = 15.0f; // The amount by which the audio will fade with distance

    private bool inZone = false;
	private float baseVolume;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		audioArea = GetNode<Area2D>("Area2D");
		
		//Sanity check
		Debug.Assert(audioArea != null);

		// Set base volume
		baseVolume = VolumeDb;
		p = GetNode<Player>("../YSort/Player");

        // If we don't have an initial last position
        // the audio musn't play from the start
        if(lastPosition == null) {
            Stop();
        }
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta) {
        // Have the audio fade out based on the player's distance to the last position in the area
		VolumeDb = inZone ? baseVolume :
            baseVolume - (p.Position.DistanceTo(lastPosition) / falloff);
	}

    private void _on_Area2D_area_entered(Area2D hb) {
        // Check for player
        if(hb.Owner is Player) {
            if(!Playing) {
                Play();
            }
            inZone = true;
        }
    }

    // When the audio area is exited, save the exit location
    // and fade the audio out using the fountain formula from that point
    // which solves the problem of computing the shortest distance to an area.
    private void _on_Area2D_area_exited(Area2D hb) {
        // Check for player
        if(hb.Owner is Player) {
            // Update exitpoint
            lastPosition = p.Position; 
            inZone = false;
        }
    }
}


