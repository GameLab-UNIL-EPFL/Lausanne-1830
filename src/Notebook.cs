using Godot;
using System;

public class Notebook : Node2D {
	
	bool hidden = true;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		Hide();
	}
	
	public void _on_NotebookController_pressed() {
		if(hidden) {
			Show();
		} else {
			Hide();
		}
		hidden = !hidden;
	}
}



