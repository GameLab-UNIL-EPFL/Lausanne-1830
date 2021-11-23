using Godot;
using System;

public class TextBox : Node2D {
	private MarginContainer PC;
	private Panel P;
	private MarginContainer TC;
	private Label Text;
	
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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		
		//Fetch all usefull nodes
		PC = GetNode<MarginContainer>("PanelContainer");
		P = GetNode<Panel>("PanelContainer/Panel");
		TC = GetNode<MarginContainer>("PanelContainer/Panel/TextContainer");
		Text = GetNode<Label>("PanelContainer/Panel/TextContainer/Text");
		
		Show();
		HideAll();
	}
	
	public void _HideText() {
		Text.Text = "";
		HideAll();
	}
	
	public void _ShowText(string text) {
		Text.Text = text;
		ShowAll();
	}
}
