using UnityEngine;
using System.Collections.Generic;

public class CompetenceInLevel : MonoBehaviour {
	// Note quel niveau utilis� pour une comp�tence pr�cise
	// Dictionaire avec les comp�tenc en clef et un tableau de boolean correspondant aux niveaux ou la comp�tence peux �tre utilis�e (true) ou non (false)
	public Dictionary<string, bool> competencPossible =
		new Dictionary<string, bool>() {
			{"1.6", false },
			{"3.1", false },
			{"3.2", false },
			{"3.3", false },
			{"3.4", false }
		};
}