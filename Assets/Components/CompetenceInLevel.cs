using UnityEngine;
using System.Collections.Generic;

public class CompetenceInLevel : MonoBehaviour {
	// Note quel niveau utilis� pour une comp�tence pr�cise
	// Dictionaire avec les comp�tenc en clef et un tableau de boolean correspondant aux niveaux ou la comp�tence peux �tre utilis�e (true) ou non (false)
	public Dictionary<string, bool[]> competencPossible;
}