using Godot;
using System;

public class FountainAudio : AudioStreamPlayer2D {
	private Player p;
	private StaticBody2D fountain;
	private float baseVolume;
	private const float RANDOM_EMPIRICAL_QUOTIENT = 15.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		p = GetNode<Player>("../YSort/Player");
		fountain = GetNode<StaticBody2D>("../YSort/Fountain");
		baseVolume = VolumeDb;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta) {
		//Honestly, this was devised completely at random
		VolumeDb = baseVolume - (fountain.Position.DistanceTo(p.Position)/RANDOM_EMPIRICAL_QUOTIENT);
	}
}
