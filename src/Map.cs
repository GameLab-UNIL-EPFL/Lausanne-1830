using Godot;
using System;

public class Map : Node2D
{
	private TextureButton office;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		office = GetNode<TextureButton>("office_button");
	}

	private void _on_Map_visibility_changed() {
		office.GrabFocus();
	}
}



