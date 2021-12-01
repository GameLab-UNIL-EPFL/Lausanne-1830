# Using the parseDialogue script  
This folder contains a `parseDialogue.py` python script, which converts a `.tsv` file into a `.xml` file.  
  
The script takes a single command line argument in the form of a path to the `.tsv` file that we want to convert.  
  
## Example  
Given the following `example.tsv` file found in the `/tsv/` directory:
```tsv
scene name	dialogue type id target1 target2	text id	option id	
palud	onApproach testApproach baker	0	0	Du pain frais de ce matin !
palud	onApproach testApproach baker 	0	1	Venez goûter !
palud	onDemand testDemand baker	0	0	Bien le bonjour, qu'est-ce qui vous intéresse ?
palud	onDemand testDemand baker	0	0	Pain, croissant ?
palud	onDemand testDemand baker	1	0	C'est fait ce matin avec la farine des Rochat !
palud	onDemand testDemand baker	2	0	Vous devriez aller visiter leur moulin au Flon.
```  
We can convert it using the following command:  
```sh
python3 parseDialogue.py tsv/example.tsv
```  
This will give us the following result, stored in `xml/examples.xml`:  
```xml  
<scene name="palud">
    <dialogue type="onApproach" id="testApproach" target1="baker">
        <text id="0">
            <option id="0"><>Du pain frais de ce matin !</></option>
        </text>
    </dialogue>
    <dialogue type="onApproach" id="testApproach" target1="baker" target2="">
        <text id="0">
            <option id="1"><>Venez go&#251;ter !</></option>
        </text>
    </dialogue>
    <dialogue type="onDemand" id="testDemand" target1="baker">
        <text id="0">
            <option id="0"><>Bien le bonjour, qu'est-ce qui vous int&#233;resse ?</></option>
        </text>
    </dialogue>
    <dialogue type="onDemand" id="testDemand" target1="baker">
        <text id="0">
            <option id="0"><>Pain, croissant ?</></option>
        </text>
    </dialogue>
    <dialogue type="onDemand" id="testDemand" target1="baker">
        <text id="1">
            <option id="0"><>C'est fait ce matin avec la farine des Rochat !</></option>
        </text>
    </dialogue>
    <dialogue type="onDemand" id="testDemand" target1="baker">
        <text id="2">
            <option id="0"><>Vous devriez aller visiter leur moulin au Flon.</></option>
        </text>
    </dialogue>
</scene>
```  
> Note that this script is perfectly generic and works with any xml format.  
