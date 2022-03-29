using Godot;
using System;

public class Menu : Control
{
	Context context;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		context = GetNode<Context>("/root/Context");
		MusicPlayer MP = (MusicPlayer)GetNode("/root/MusicPlayer");
		MP.PlayMusic("Schubert_Sonata13_2.mp3");
	}

	private void _on_Button_pressed() {
		SceneChanger SC = (SceneChanger)GetNode("/root/SceneChanger");
		SC.GotoScene("res://scenes/Palud/ProtoPalud.tscn");
		MusicPlayer MP = (MusicPlayer)GetNode("/root/MusicPlayer");
		MP.PlayMusic("Schubert_Sonata13.mp3", -5);
	}

}


