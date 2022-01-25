using Godot;
using System;

public class MapIcon : TextureButton
{
	[Export]
	public string id = "Brasserie";
	
	private AnimationPlayer AnimPlayer;
	private Label L;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		L = GetNode<Label>("Label");
	}

	private void _on_MapIcon_pressed()
	{
		SceneChanger SC = (SceneChanger)GetNode("/root/SceneChanger");
		SC.GotoScene("res://scenes/" + id + ".tscn");
	}
	
	private void _on_MapIcon_mouse_entered()
	{
		if(AnimPlayer == null) {
			_Ready();
		}
		L.Show();
		AnimPlayer.Play("Appear");
	}


	private void _on_MapIcon_mouse_exited()
	{
		L.Hide();
	}
	
}





