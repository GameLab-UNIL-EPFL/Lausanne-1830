using Godot;
using System;

public class TextTextureRect : TextureRect {
	[Export]
	public string resourceName; //includine the extension, e.g. /normal.png
	[Export]
	public string resourcePath; //excluding the language and the filename, e.g. "06_UI_menus/"
	
	private const string resourceBase = "res://assets/";
	
	Context context;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Call the base class's ready
		base._Ready();
		
		// Get the context
		context = GetNode<Context>("/root/Context");
		
		// Update the ressource with the correct language
		if(resourceName != "") {
			UpdateRessource(context._GetLanguage());
		}
		
		// Connect the language update signal to the class
		context.Connect("UpdateLanguage", this, nameof(UpdateRessource));
	}
	
	private void UpdateRessource(Language l) {
		// Update the sprite
		string path = resourceBase + resourcePath + context._GetLanguageAbbrv(l);
		
		// Load in both new textures
		this.Texture = (Texture) ResourceLoader.Load(path + resourceName);
	}
}
