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
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

public struct CharacterInfo_t {
	public CharacterInfo_t(
		string pnm="", string nm="", string addr="", int numb=0,
		string cnjt="", int enfts=0, string mtr=""
	) {
		prenom = pnm;
		nom = nm;
		adresse = addr;
		num = numb;
		conjoint = cnjt;
		enfants = enfts;
		metier = mtr;
		valid = true;
	}
	public CharacterInfo_t(int _illegalVal) {
		prenom = "";
		nom = "";
		adresse = "";
		num = _illegalVal;
		conjoint = "";
		enfants = _illegalVal;
		metier = "";
		valid = false;
	}
	public bool valid;
	
	public string prenom;
	public string nom;
	public string adresse;
	public int num;
	public string conjoint;
	public int enfants;
	public string metier;
	
	public bool IsValid() {
		return valid;
	}
};

//////////////////////////////////////////////////////////////////////////////
/////////////////////////////////InfoValue_t//////////////////////////////////
//////////////////////////////////////////////////////////////////////////////

public struct InfoValue_t {
	public InfoValue_t(bool initVal0, bool initVal1, bool initVal2, 
						bool initVal3, bool initVal4, bool initVal5, bool initVal6) {
		prenomCorrect = initVal0;
		nomCorrect = initVal1;
		adresseCorrect = initVal2;
		numCorrect = initVal3;
		conjointCorrect = initVal4;
		enfantsCorrect = initVal5;
		metierCorrect = initVal6;
		valid = false;
	}
	
	public InfoValue_t(bool initVal) {
		prenomCorrect = initVal;
		nomCorrect = initVal;
		adresseCorrect = initVal;
		numCorrect = initVal;
		conjointCorrect = initVal;
		enfantsCorrect = initVal;
		metierCorrect = initVal;
		valid = false;
	}
	public bool valid;
	public bool prenomCorrect;
	public bool nomCorrect;
	public bool adresseCorrect;
	public bool numCorrect;
	public bool conjointCorrect;
	public bool enfantsCorrect;
	public bool metierCorrect;
	
	public bool IsCorrect() {
		return prenomCorrect && nomCorrect && adresseCorrect &&
			numCorrect && conjointCorrect && enfantsCorrect && metierCorrect;
	}
	
	public bool IsValid() {
		return valid;
	}
	
	// Returns the attribute names of all attributes that were found
	public List<string> FoundAttributes() {
		List<string> res = new List<string>();
		if(prenomCorrect) {
			res.Add("prenom");
		}
		if(nomCorrect) {
			res.Add("nom");
		}
		if(adresseCorrect) {
			res.Add("adresse");
		}
		if(numCorrect) {
			res.Add("num");
		}
		if(conjointCorrect) {
			res.Add("conjoint");
		}
		if(enfantsCorrect) {
			res.Add("enfants");
		}
		if(metierCorrect) {
			res.Add("metier");
		}
		return res;
	}
	
	public List<string> Outliers() {
		List<string> res = new List<string>();
		if(!prenomCorrect) {
			res.Add("prenom");
		}
		if(!nomCorrect) {
			res.Add("nom");
		}
		if(!adresseCorrect) {
			res.Add("adresse");
		}
		if(!numCorrect) {
			res.Add("num");
		}
		if(!conjointCorrect) {
			res.Add("conjoint");
		}
		if(!enfantsCorrect) {
			res.Add("enfants");
		}
		if(!metierCorrect) {
			res.Add("metier");
		}
		return res;
	}
};

/////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////XMLAttributeInfo_t//////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////

public struct XMLAttributeInfo_t {
	public XMLAttributeInfo_t(string _name, string _val) {
		name = _name;
		val = _val;
	}
	
	public string name;
	public string val;
};

///////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////QUEST_CONTROLLER//////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////

public class QuestController : Node {

	//File at which the scene's dialogue is stored
	[Export]
	public string SceneCharacterFileName = "notebookCharacterList.xml";
	public string SceneCharacterFileBasePath = "res://db/characters/";
	public string SceneCharacterFilePath;
	
	//Local XDocument containing a parsed version of the dialogue
	private XDocument characterList;
	
	//Buffer storing the dialogue being used by a questNPC
	private Stack<string> QuestBuffer = new Stack<string>();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		SceneCharacterFilePath = SceneCharacterFileBasePath + SceneCharacterFileName;
		DialogueController._ParseXML(ref characterList, SceneCharacterFilePath);
	}
	
	public void _InitBuffer(string[] text) {
		for(int i = 0; i < text.Length; ++i) {
			QuestBuffer.Push(text[text.Length - i - 1]);
		}
	}
	
	public string _NextLine() {
		if(QuestBuffer.Count == 0) {
			return null;
		}
		return QuestBuffer.Pop();
	}
	
	public InfoValue_t _CompareCharInfo(CharacterInfo_t solution, CharacterInfo_t val) {
		InfoValue_t res = new InfoValue_t(
			solution.prenom.Equals(val.prenom),
			solution.nom.Equals(val.nom),
			solution.adresse.Equals(val.adresse),
			solution.num == val.num,
			solution.conjoint.Equals(val.conjoint),
			solution.enfants == val.enfants,
			solution.metier.Equals(val.metier)
		);
		
		return res;
	}
	
	/**
	 * @brief Queries the local XDocument for a given solution
	 * @return a CharacterInfo_t struct containing all of the solution data
	 */
	public CharacterInfo_t _QueryQuestSolution(int tabId) {
		
		//Query a new solution
		var querySolution = from solution in characterList.Root.Descendants("solution")
							where Int32.Parse(solution.Attribute("id").Value) == tabId
							select solution.Attributes();
		
		// Extract attribute name and value
		var solutionList = querySolution.Select(x => 
			x.Select(y => new XMLAttributeInfo_t(y.Name.LocalName, y.Value))
		);
		
		CharacterInfo_t res = new CharacterInfo_t(-1);
		
		foreach(var e in solutionList) {
			foreach(var xmlAttr in e) {
				var inf = xmlAttr.val;
				switch(xmlAttr.name) {
					case "prenom":
						res.prenom = inf;
						break;
					case "nom":
						res.nom = inf;
						break;
					case "adresse":
						res.adresse = inf;
						break;
					case "num":
						res.num = Int32.Parse(inf);
						break;
					case "conjoint":
						res.conjoint = inf;
						break;
					case "enfant":
						res.enfants = Int32.Parse(inf);
						break;
					case "metier":
						res.metier = inf;
						break;
					default:
						break;
				}
			}
		}
		
		return res;
	}
}
