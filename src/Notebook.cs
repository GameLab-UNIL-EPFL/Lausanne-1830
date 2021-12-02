using Godot;
using System;

public class Notebook : Node2D {
	
	private Panel p;
	bool hidden = true;
	
	public void _HideAll() {
		p.Hide();
		Hide();
	}
	
	public void _ShowAll() {
		p.Show();
		Show();
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		p = GetNode<Panel>("Panel");
		_HideAll();
	}
	
	public void _on_NotebookController_pressed() {
		if(hidden) {
			_ShowAll();
		} else {
			_HideAll();
		}
		hidden = !hidden;
	}
}



