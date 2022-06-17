using Godot;
using System;

public class Credits : Control
{
	SceneChanger SC;
	MusicPlayer MP;
	private Control E;
	private Control A;
	private Label L;
	private Label L2;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SC = (SceneChanger)GetNode("/root/SceneChanger");
		MP = (MusicPlayer)GetNode("/root/MusicPlayer");
		E = GetNode<Control>("Equipe");
		A = GetNode<Control>("Assets");
		L = GetNode<Label>("TextureButton2/Label");
		L2 = GetNode<Label>("TextureButton2/Label2");
	}

	private void _on_TextureButton_pressed() {
		SC.GotoScene("res://scenes/Interaction/Menu.tscn");
	}
	private void _on_LinkButton_pressed() {
		OS.ShellOpen("https://retrocademedia.itch.io/buttonprompts4");
	}
	
	private void _on_LinkButton2_pressed() {
		OS.ShellOpen("https://arcade.itch.io/newsgeek");
	}
	
	private void _on_TextureButton2_pressed() {
		E.Visible = !E.Visible;
		A.Visible = !A.Visible;
		L.Visible = !L.Visible;
		L2.Visible = !L2.Visible;
	}
}










