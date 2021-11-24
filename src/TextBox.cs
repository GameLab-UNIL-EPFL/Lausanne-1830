using Godot;
using System;

public class TextBox : Node2D {
	private MarginContainer PC;
	private Panel P;
	private MarginContainer TC;
	private Label Text;
	
	private Vector2 InitLocation = new Vector2(-44, -43);
	private Vector2 MediumLocation = new Vector2(-62, -44);
	private Vector2 LargeLocation = new Vector2(-67, -52);
	
	private Vector2 InitSize = new Vector2(350, 64);
	private Vector2 MediumSize = new Vector2(495, 75);
	private Vector2 LargeSize = new Vector2(550, 100);
	
	private const int SMALL_CHAR_COUNT = 30;
	private const int MEDIUM_CHAR_COUNT = 50; 
	
	//Hides all nodes
	private void HideAll() {
		PC.Hide();
		P.Hide();
		TC.Hide();
		Text.Hide();
	}
	
	//Shows all nodes
	private void ShowAll() {
		PC.Show();
		P.Show();
		TC.Show();
		Text.Show();
	}
	
	private void ResetPC() {
		PC.Set("rect_size", InitSize);
	}
	
	private void SetSize(Vector2 location, Vector2 size) {
		PC.Set("rect_size", size);
		Set("position", location);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		
		//Fetch all usefull nodes
		PC = GetNode<MarginContainer>("PanelContainer");
		PC = GetNode<MarginContainer>("PanelContainer");
		P = GetNode<Panel>("PanelContainer/Panel");
		TC = GetNode<MarginContainer>("PanelContainer/TextContainer");
		Text = GetNode<Label>("PanelContainer/TextContainer/Text");
		
		SetSize(InitLocation, InitSize);
		
		Show();
		HideAll();
	}
	
	public void _HideText() {
		Text.Text = "";
		HideAll();
	}
	
	public void _ShowText(string text) {
		Text.Text = text;
		int size = text.Length;
		if(size > SMALL_CHAR_COUNT) {
			SetSize(MediumLocation, MediumSize);
		} else if(size > MEDIUM_CHAR_COUNT) {
			SetSize(LargeLocation, LargeSize);
		}
		ShowAll();
	}
}
