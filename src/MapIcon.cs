using Godot;
using System;

public class MapIcon : TextureButton {
	[Export]
	public string id = "Brasserie";
	
	[Export]
	public string text = "Brasserie";
	
	private AnimationPlayer AnimPlayer;
	private Label L;
	private Sprite RR;
	private Context context;
	
	private Vector2 PALUD_POS = new Vector2(208, 152);
	private Vector2 BRASSERIE_POS = new Vector2(138, 108);
	private Vector2 CASINO_POS = new Vector2(278, 234);
	private Vector2 MOULIN_POS = new Vector2(184, 223);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		AnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		L = GetNode<Label>("Label");
		L.Text = text;
		context = GetNode<Context>("/root/Context");
		RR = GetNode<Sprite>("../YouAreHere");
		
		//Update You are Here
		SetYouAreHerePos(context._GetLocation());
	}
	
	private void SetYouAreHerePos(Locations l) {
		switch(l) {
			case Locations.PALUD:
				RR.Position = PALUD_POS;
				break;
			case Locations.CASINO:
				RR.Position = CASINO_POS;
				break;
			case Locations.BRASSERIE:
				RR.Position = BRASSERIE_POS;
				break;
			default:
				break;
		}
	}

	private void _on_MapIcon_pressed() {
		SceneChanger SC = GetNode<SceneChanger>("/root/SceneChanger");
		SC.GotoScene("res://scenes/" + id + ".tscn");
			
		//Update the context
		context._UpdateLocation(id);
	}
	
	private void _on_MapIcon_mouse_entered() {
		if(AnimPlayer == null) {
			_Ready();
		}
		if(L.Text != "") {
			L.Show();
		}
	}


	private void _on_MapIcon_mouse_exited() {
		L.Hide();
	}
	
}





