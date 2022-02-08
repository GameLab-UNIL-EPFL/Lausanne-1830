using Godot;
using System;

public class Menu : Control
{
	private AnimationPlayer AP;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AP = GetNode<AnimationPlayer>("AnimationPlayer");
		AP.Play("Loop");
	}

	private void _on_Button_pressed() {
		SceneChanger SC = (SceneChanger)GetNode("/root/SceneChanger");
		SC.GotoScene("res://scenes/Palud/ProtoPalud.tscn");
	}

}


