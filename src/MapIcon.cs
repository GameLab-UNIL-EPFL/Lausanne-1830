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

public class MapIcon : TextureButton {
	[Export]
	public string id = "Brasserie";
	
	[Export]
	public string text = "Brasserie";
	
	[Export]
	public string resourceNameHover; //including the extension
	[Export]
	public string resourceNameNormal; //includine the extension, e.g. normal.png
	[Export]
	public string resourcePath; //excluding the language and the filename, e.g. "06_UI_menus"
	
	private const string resourceBase = "res://assets/";
	

	private Sprite RR;
	private Context context;
	
	private Vector2 INTRO_POS = new Vector2(260, 120);
	private Vector2 PALUD_POS = new Vector2(208, 152);
	private Vector2 BRASSERIE_POS = new Vector2(138, 108);
	private Vector2 CASINO_POS = new Vector2(278, 234);
	private Vector2 MOULIN_POS = new Vector2(184, 223);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Call the base class's ready
		base._Ready();
		
		context = GetNode<Context>("/root/Context");
		RR = GetNode<Sprite>("../YouAreHere");
		
		//Update You are Here
		SetYouAreHerePos(context._GetLocation());
		// Update the ressource with the correct language
		UpdateRessource(context._GetLanguage());
		
		// Connect the language update signal to the class
		context.Connect("UpdateLanguage", this, nameof(UpdateRessource));
	}
	
	private void SetYouAreHerePos(Locations l) {
		switch(l) {
			case Locations.INTRO:
				RR.Position = INTRO_POS;
				break;
			case Locations.PALUD:
				RR.Position = PALUD_POS;
				break;
			case Locations.CASINO:
				RR.Position = CASINO_POS;
				break;
			case Locations.BRASSERIE:
				RR.Position = BRASSERIE_POS;
				break;
			case Locations.MOULIN:
			case Locations.FLON:
				RR.Position = MOULIN_POS;
				break;
			default:
				break;
		}
	}
	
	private void UpdateRessource(Language l) {
		// Update the sprite
		string path = string.Format("{0}/{1}/{2}/", resourceBase, resourcePath, Context._GetLanguageAbbrv(l));
		
		// Load in both new textures
		TextureHover = (Texture) ResourceLoader.Load(path +resourceNameHover);
		TextureNormal = (Texture) ResourceLoader.Load(path +resourceNameNormal);
		TextureFocused = (Texture) ResourceLoader.Load(path +resourceNameHover);
	}

	private void _on_MapIcon_pressed() {
		SceneChanger SC = GetNode<SceneChanger>("/root/SceneChanger");
		SC.GotoScene("res://scenes/" + id + ".tscn");
		
		MusicPlayer MP = (MusicPlayer)GetNode("/root/MusicPlayer");
		MP.ChangeMusic("Schubert_Sonata13.mp3", -5);
		
		
			
		//Update the context
		context._UpdateLocation(id);
	}
	
}





