# Lausanne-1830
Open-Source Historically accurate RPG based in 1830s Lausanne.  

# Dev  
Game currently being developped using the Open-Source [Godot](https://godotengine.org/download) game engine.  
__Godot version__: Godot Mono v3.4-stable  

Requirements:
- OpenGL 2.1 / OpenGL ES 2.0 compatible hardware
- For the Mono version: MSBuild
(from Visual Studio Build Tools or the [Mono SDK](https://www.mono-project.com/download/stable/))


  
## Input Map  
- __E__: Interact.   
- __Arrow keys__: Move around.  
- __Hold Shift__: Sprint (for 3 sec then 2 sec cooldown).  
  
## Dialogue File Format  
Dialogue is formatted using XML.  
Example:  
```xml
<scene name="palud">
	<dialogue type="onApproach" id="testApproach" target1="baker">
		<text id="1">
			<option id="1">Du pain frais de ce matin !</option>
			<option id="2">Venez goûter !</option>
		</text>
	</dialogue>
	<dialogue type="onDemand" id="testDemand" target1="baker">
		<text id="1">
			<option id="1">Bien le bonjour, qu'est-ce qui vous intéresse ?</option>
			<option id="2">Pain, croissant ?</option>
		</text>
		<text id="2">C'est fait ce matin avec la farine des Rochat !</text>
		<text id="3">Vous devriez aller visiter leur moulin au Flon.</text>
	</dialogue>
</scene>
```  
Here we have the following format:  
- __scene__: defines a scene, contains an attribute __name__ which is the name of the scene.  
- __dialogue__: defines a set of dialogue lines, contains the following attributes:  
    1. __type__: Defines how the dialogue is triggered (either _onDemand_ or _onApproach_).  
    2. __id__: A unique identifier used to locate the dialogue.  
    3. __target1__: The name of the person speaking.  
    4. __taget2__[Optional]: The name of the person listining in the case of a dialogue between NPCs.  
- __text__: The core text of the dialogue, contains and __id__ defining the index in the dialogue it is at.  
- __option__: In the case of random dialogue, an option is one of the options for a given text, __id__ is the index of the option.  
  


