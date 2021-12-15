using Godot;
using System;

public class NotebookController : Button
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	private Texture I;
	private Texture Hover;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		 Hover = (Texture)GD.Load("res://assets/04_notebook/journal2.png");
		 I = (Texture)GD.Load("res://assets/04_notebook/journal.png");
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
	private void _on_NotebookController_mouse_entered() {
	// Replace with function body.
		Icon = Hover;
	}


	private void _on_NotebookController_mouse_exited() {
		Icon = I;
	}

}


