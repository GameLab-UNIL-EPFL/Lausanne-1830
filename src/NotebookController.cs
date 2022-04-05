using Godot;
using System;

public class NotebookController : TextureButton
{
	private AnimationPlayer AnimPlayer;
	private TextureButton MB;
	private Notebook NB;
	private Map M;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		MB = GetNode<TextureButton>("MapButton");
		NB = GetNode<Notebook>("../Notebook");
		
		if(!MB.IsConnected("pressed", NB, "_on_MapB_pressed")){
			MB.Connect("pressed", NB, "_on_MapB_pressed");
		}
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
	
	private void _on_Player_SlideInNotebookController() {
		if(AnimPlayer == null) {
			_Ready();
		}
		AnimPlayer.Play("Slide");
	}
}
