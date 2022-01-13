using Godot;
using System;

public class SceneChanger : CanvasLayer
{
	private AnimationPlayer AnimPlayer;
	
	public Node CurrentScene { get; set; }

	public override void _Ready()
	{
		Viewport root = GetTree().Root;
		CurrentScene = root.GetChild(root.GetChildCount() - 1);
		GD.Print(CurrentScene.Name);
		AnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	private void _FadeOut() {
		AnimPlayer.Play("FadeOut");
		//TODO Fade Music and sounds
	}
	
	private void _EaseIn() {
		AnimPlayer.PlayBackwards("FadeOut");
	}
	
	public void GotoScene(string path) {
	// This function will usually be called from a signal callback,
	// or some other function from the current scene.
	// Deleting the current scene at this point is
	// a bad idea, because it may still be executing code.
	// This will result in a crash or unexpected behavior.

	// The solution is to defer the load to a later time, when
	// we can be sure that no code from the current scene is running:

		CallDeferred(nameof(DeferredGotoScene), path, true);
	}

	public void DeferredGotoScene(string path, bool animate = true) {
		// It is now safe to remove the current scene
		CurrentScene.Free();

		// Load a new scene.
		var nextScene = (PackedScene)GD.Load(path);
		
		if(nextScene == null) {
			throw new Exception("Cannot open path!");
		} 
		else if(!nextScene.CanInstance()) {
			throw new Exception("Cannot instance path!");
		}
			
		if(animate) {
			_FadeOut();
		}
		// Instance the new scene.
		CurrentScene = nextScene.Instance();

		// Add it to the active scene, as child of root.
		GetTree().Root.AddChild(CurrentScene);

		// Optionally, to make it compatible with the SceneTree.change_scene() API.
		GetTree().CurrentScene = CurrentScene;
		
		if(animate) {
			_EaseIn();
		}
	}

}
