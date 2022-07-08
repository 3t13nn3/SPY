using UnityEngine;
using System.Collections.Generic;

public class FunctionalityInLevel : MonoBehaviour {
	// Note quel niveau utilis� pour une comp�tence pr�cise
	// Dictionaire avec les function en clef et un tableau de boolean correspondant aux niveaux ou la fonction peux �tre utilis�e (true) ou non (false)
	public Dictionary<string, List<string>> levelByFunc =
		new Dictionary<string, List<string>>() {
			{"F6", new List<string>() },
			{"F7", new List<string>() },
			{"F10", new List<string>() },
			{"F11", new List<string>() },
			{"F12", new List<string>() },
			{"F13", new List<string>() },
			{"F18", new List<string>() },
			{"F19", new List<string>() },
			{"F20", new List<string>() },
			{"F21", new List<string>() },
			{"F24", new List<string>() }
		};
}