# Using the parseDialogue script  
This folder contains a `parseDialogue.py` python script, which converts a `.tsv` file into a `.xml` file.  
  
The script takes a single command line argument in the form of a path to the `.tsv` file that we want to convert.  
  
## Example  
Given the following `example.tsv` file found in the `/tsv/` directory:
```tsv
scene name	dialogue type id target1 target2	
palud	onApproach testApproach baker	Du pain frais de ce matin !|Venez goûter !
palud	onDemand testDemand baker	Bien le bonjour, qu'est-ce qui vous intéresse ?|Pain, croissant ?&C'est fait ce matin avec la farine des Rochat !&Vous devriez aller visiter leur moulin au Flon.
```  
> Note that in the body section of the tsv, the options are separated using the `|` symbol and the different text lines are separated using the `&` symbol.  

We can convert it using the following command:  
```sh
python3 parseDialogue.py tsv/example.tsv
```  
This will give us the following result, stored in `xml/examples.xml`:  
```xml  
<scene name="palud">
    <dialogue type="onApproach" id="testApproach" target1="baker">
        <text>
            <option id="0">Du pain frais de ce matin !</option>
            <option id="1">Venez go&#251;ter !</option>
        </text>
    </dialogue>
    <dialogue type="onDemand" id="testDemand" target1="baker">
        <text>
            <option id="0">Bien le bonjour, qu'est-ce qui vous int&#233;resse ?</option>
            <option id="1">Pain, croissant ?</option>
        </text>
        <text>C'est fait ce matin avec la farine des Rochat !</text>
        <text>Vous devriez aller visiter leur moulin au Flon.</text>
    </dialogue>
</scene>
```  
> Note that this script is perfectly generic and works with any xml format containing the text and option fields.    
