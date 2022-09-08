using UnityEngine;
using System.Collections.Generic;

public class FunctionalityInLevel : MonoBehaviour {
	// Note quel niveau utilis� pour une fonctionnalit� pr�cise
	// Dictionaire avec les fonctionnalit�s en clef et un tableau de boolean correspondant aux niveaux ou la fonctionnalit� peut �tre utilis�e (true) ou non (false)
	public Dictionary<string, List<string>> levelByFuncLevelDesign = new Dictionary<string, List<string>>();
	// Note quel niveau utiliser pour une comp�tence pr�cise autre qu'une fonctionnalit� de level design
	public Dictionary<string, List<string>> levelByFunc = new Dictionary<string, List<string>>();

}