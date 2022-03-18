using Godot;
using System;

public class NotebookController : Button
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	private Texture I;
	private Texture Hover;
	private AnimationPlayer AnimPlayer;
	private Button MB;
	private Notebook NB;
	private Map M;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hover = (Texture)GD.Load("res://assets/04_notebook/journal2.png");
		I = (Texture)GD.Load("res://assets/04_notebook/journal.png");
		AnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		MB = GetNode<Button>("MapButton");
		NB = GetNode<Notebook>("../Notebook");
		M = GetNode<Map>("../Notebook/Map");
		
		MB.Connect("pressed", NB, "_on_MapB_pressed");
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
	
	private void _on_Player_SlideInNotebookController() {
		if(AnimPlayer == null) {
			_Ready();
		}
		AnimPlayer.Play("Slide");
	}
}
