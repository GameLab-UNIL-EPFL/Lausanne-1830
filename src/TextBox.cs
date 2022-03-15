using Godot;
using System;

public class TextBox : Node2D {
	private Sprite ATB;
	private Sprite DTB;
	private MarginContainer TC;
	private Label Text;
	private AnimatedSprite E;
	
	private Vector2 DTCSize = new Vector2(393, 140);
	private Vector2 DTCPos = new Vector2(12, -93);
	private Vector2 ATCSize = new Vector2(380, 50);
	private Vector2 ATCPos = new Vector2(15, 0);
	
	//Hides all nodes
	public void _HideAll() {
		ATB.Hide();
		DTB.Hide();
		TC.Hide();
		Text.Hide();
		E.Hide();
	}
	
	//Shows all nodes
	private void ShowAll() {
		TC.Show();
		Text.Show();
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		//Fetch all usefull nodes
		ATB = GetNode<Sprite>("ApproachTB");
		DTB = GetNode<Sprite>("DemandTB");
		TC = GetNode<MarginContainer>("TextContainer");
		Text = GetNode<Label>("TextContainer/Text");
		E = GetNode<AnimatedSprite>("PressE");
		
		
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
			DTB.Show();
		} else {
			DTB.Hide();
			TC.Set("rect_size", ATCSize);
			TC.Set("rect_position", ATCPos);
			ATB.Show();
		}
		ShowAll();
	}
}
