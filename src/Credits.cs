using Godot;
using System;

public class Credits : Control
{
	SceneChanger SC;
	MusicPlayer MP;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SC = (SceneChanger)GetNode("/root/SceneChanger");
		MP = (MusicPlayer)GetNode("/root/MusicPlayer");
	}

	private void _on_TextureButton_pressed() {
		SC.GotoScene("res://scenes/Interaction/Menu.tscn");
		MP.ChangeMusic("Schubert_Sonata13_2.mp3", -10);
	}
}

