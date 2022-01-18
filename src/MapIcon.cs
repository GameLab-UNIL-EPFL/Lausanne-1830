using Godot;
using System;

public class MapIcon : TextureButton
{
	[Export]
	public string id = "Brasserie";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	private void _on_MapIcon_pressed()
	{
		SceneChanger SC = (SceneChanger)GetNode("/root/SceneChanger");
		SC.GotoScene("res://scenes/" + id + ".tscn");
	}
	
}


