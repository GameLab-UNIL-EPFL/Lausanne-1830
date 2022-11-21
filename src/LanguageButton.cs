using Godot;
using System;

public class LanguageButton : Button {
	
	private Context context;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		context = GetNode<Context>("/root/Context");
	}
	
	// Update the language and button text
	private void _on_language_pressed() {
		// Go to the next language
		context._NextLanguage();
		Text = context._LanguageToString();
	}
}



