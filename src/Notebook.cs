using Godot;
using System;

public class Notebook : Node2D {
	
	private bool hidden = true;
	private AudioStreamPlayer ASP;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Hide();
		ASP = GetNode<AudioStreamPlayer>("../NotebookClick");
	}
	
	public void _on_NotebookController_pressed() {
		if(ASP.Playing == false){
			ASP.Play();
		}
		if(hidden) {
			Show();
		} else {
			Hide();
		}
		hidden = !hidden;
	}
}



