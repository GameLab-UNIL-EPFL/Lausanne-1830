/*
Historically accurate educational video game based in 1830s Lausanne.
Copyright (C) 2021  GameLab UNIL-EPFL

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using Godot;
using System;

public class TextBox : Node2D {
	
	[Export]
	public string NPCName = "NPC";
	
	[Export]
	public int FontSize = 10;
	
	private NinePatchRect ATB;
	private NinePatchRect DTB;
	private MarginContainer TC;
	private Label Text;
	private AnimatedSprite E;
	private Label N;
	private DynamicFont F;
	
	private Vector2 DTCSize = new Vector2(114, 41);
	private Vector2 DTCPos = new Vector2(3, -16);
	private Vector2 ATCSize = new Vector2(114, 10);
	private Vector2 ATCPos = new Vector2(3, 9);
	private Vector2 DNamePos = new Vector2(10, -30);
	private Vector2 ANamePos = new Vector2(10, -12);
	
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
		ATB = GetNode<NinePatchRect>("ApproachTB");
		DTB = GetNode<NinePatchRect>("DemandTB");
		TC = GetNode<MarginContainer>("TextContainer");
		Text = GetNode<Label>("TextContainer/Text");
		E = GetNode<AnimatedSprite>("AnimatedSprite");
		N = GetNode<Label>("Name");
		F = (DynamicFont)Text.Get("custom_fonts/font");
		N.Text = NPCName;
		
		//Set default font size
		F.Size = FontSize;
		
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
		
		if(nLines > 3.0f) {
			F.Size = FontSize - 1;
		} else {
			F.Size = FontSize;
		}
		
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



