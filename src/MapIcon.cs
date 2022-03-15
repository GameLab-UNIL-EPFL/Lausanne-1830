using Godot;
using System;

public class MapIcon : TextureButton {
	[Export]
	public string id = "Brasserie";
	
	[Export]
	public string text = "Brasserie";
	
	private AnimationPlayer AnimPlayer;
	private Label L;
	private Context context;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		AnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		L = GetNode<Label>("Label");
		L.Text = text;
		context = GetNode<Context>("/root/Context");
	}

	private void _on_MapIcon_pressed() {
		SceneChanger SC = (SceneChanger)GetNode("/root/SceneChanger");
		SC.GotoScene("res://scenes/" + id + ".tscn");
			
		if(id == "Palud/ProtoPalud") {
			context._StartGame();
		} else {
			context._SwitchScenes();
		}
	}
	
	private void _on_MapIcon_mouse_entered() {
		if(AnimPlayer == null) {
			_Ready();
		}
		if(L.Text != "") {
			L.Show();
			AnimPlayer.Play("Appear");	
		}
	}


	private void _on_MapIcon_mouse_exited() {
		L.Hide();
	}
	
}





