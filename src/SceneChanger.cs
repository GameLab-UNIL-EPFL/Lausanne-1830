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

public class SceneChanger : CanvasLayer
{
	private AnimationPlayer AnimPlayer;
	
	public Node CurrentScene { get; set; }

	public override void _Ready()
	{
		Viewport root = GetTree().Root;
		CurrentScene = root.GetChild(root.GetChildCount() - 1);
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
			
		if(animate) {
			_FadeOut();
		}
		// Instance the new scene.
		CurrentScene = nextScene.Instance();

		// Add it to the active scene, as child of root.
		var tree = GetTree();
		tree.Root.AddChild(CurrentScene);

		// Optionally, to make it compatible with the SceneTree.change_scene() API.
		tree.CurrentScene = CurrentScene;
		
		if(animate) {
			_EaseIn();
		}
	}

}
