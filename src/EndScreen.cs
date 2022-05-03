using Godot;
using System;

public class EndScreen : Node2D {
	Context context;
	SceneChanger SC;
	
	public override void _Ready() {
		context = GetNode<Context>("/root/Context");
		SC = GetNode<SceneChanger>("/root/SceneChanger");
	}
	
	private void _on_Button_pressed() {
		//Clear context and restart game
		context._Clear();
		SC.GotoScene("res://scenes/Interaction/Menu.tscn");
	}
}

