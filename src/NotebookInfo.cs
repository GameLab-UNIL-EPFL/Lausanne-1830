using Godot;
using System;

public class NotebookInfo : Node2D
{
	// Declare member variables here. Examples:
	private MarginContainer TC;
	private Label Text;
	private Color Hover = new Color(1,1,1);
	private Color C = new Color("#876853");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TC = GetNode<MarginContainer>("TextContainer");
		Text = GetNode<Label>("TextContainer/Text");
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

	private void _on_TextContainer_mouse_entered()
	{
		Text.Set("custom_colors/font_color", Hover);
	}
	

	private void _on_TextContainer_mouse_exited()
	{
		Text.Set("custom_colors/font_color", C);
	}

}
