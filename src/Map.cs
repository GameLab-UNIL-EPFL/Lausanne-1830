using Godot;
using System;

public class Map : Node2D
{
	private bool hidden = true;
	
	private AudioStreamPlayer ASP;
	private Sprite S;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ASP = GetNode<AudioStreamPlayer>("../ButtonSound");
		S = GetNode<Sprite>("../TabMap");
	}

	public void _on_MapButton_pressed() {
		if(ASP.Playing == false) {
			ASP.Play();
		}
		if(hidden) {
			Show();
			S.Frame = 0;
		} else {
			Hide();
			S.Frame = 1;
		}
		hidden = !hidden;
	}

}


