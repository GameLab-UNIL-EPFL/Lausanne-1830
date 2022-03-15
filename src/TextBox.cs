using Godot;
using System;

public class TextBox : Node2D {
	
	[Export]
	public string NPCName = "NPC";
	
	private Sprite ATB;
	private Sprite DTB;
	private MarginContainer TC;
	private Label Text;
	private AnimatedSprite E;
	private Label N;
	
	private Vector2 DTCSize = new Vector2(393, 140);
	private Vector2 DTCPos = new Vector2(12, -93);
	private Vector2 ATCSize = new Vector2(380, 50);
	private Vector2 ATCPos = new Vector2(15, 0);
	private Vector2 DNamePos = new Vector2(25, -125);
	private Vector2 ANamePos = new Vector2(25, -35);
	
	//Hides all nodes
	public void _HideAll() {
		ATB.Hide();
		DTB.Hide();
		TC.Hide();
		Text.Hide();
		E.Hide();
		N.Hide();
	}
	
	//Shows all nodes
	private void ShowAll() {
		TC.Show();
		Text.Show();
		N.Show();
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		//Fetch all usefull nodes
		ATB = GetNode<Sprite>("ApproachTB");
		DTB = GetNode<Sprite>("DemandTB");
		TC = GetNode<MarginContainer>("TextContainer");
		Text = GetNode<Label>("TextContainer/Text");
		E = GetNode<AnimatedSprite>("AnimatedSprite");
		N = GetNode<Label>("Name");
		N.Text = NPCName;
		
		
		Show();
		_HideAll();
	}
	
	public void _HideText() {
		Text.Text = "";
		_HideAll();
	}
	
	public void _ShowPressE() {
		E.Show();
	}
	
	public void _ShowText(string text) {
		Text.Text = text;
		float nLines = text.Length / 25.0f;
		ShowAll();
		
		//Pick which TB to show
		if(nLines > 1.0f)  {
			ATB.Hide();
			TC.Set("rect_size", DTCSize);
			TC.Set("rect_position", DTCPos);
			N.Set("rect_position", DNamePos);
			DTB.Show();
			N.Hide();
		} else {
			DTB.Hide();
			TC.Set("rect_size", ATCSize);
			TC.Set("rect_position", ATCPos);
			N.Set("rect_position", ANamePos);
			ATB.Show();
			N.Show();
		}
		ShowAll();
	}
}
