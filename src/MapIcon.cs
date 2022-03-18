using Godot;
using System;

public class MapIcon : TextureButton {
	[Export]
	public string id = "Brasserie";
	
	[Export]
	public string text = "Brasserie";
	
	private AnimationPlayer AnimPlayer;
	private Label L;
	private ReferenceRect RR;
	private Context context;
	
	private Vector2 PALUD_POS = new Vector2(194, 124);
	private Vector2 BRASSERIE_POS = new Vector2(110, 90);
	private Vector2 CASINO_POS = new Vector2(260, 206);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		AnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		L = GetNode<Label>("Label");
		L.Text = text;
		context = GetNode<Context>("/root/Context");
		RR = GetNode<ReferenceRect>("../YouAreHere");
		
		//Update You are Here
		SetYouAreHerePos(context._GetLocation());
	}
	
	private void SetYouAreHerePos(Locations l) {
		switch(l) {
			case Locations.PALUD:
				RR.RectPosition = PALUD_POS;
				break;
			case Locations.CASINO:
				RR.RectPosition = CASINO_POS;
				break;
			case Locations.BRASSERIE:
				RR.RectPosition = BRASSERIE_POS;
				break;
			default:
				break;
		}
	}

	private void _on_MapIcon_pressed() {
		SceneChanger SC = (SceneChanger)GetNode("/root/SceneChanger");
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





