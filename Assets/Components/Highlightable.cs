using UnityEngine;

public class Highlightable : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
    public Color baseColor = Color.white;
    public Color highlightedColor =  Color.yellow;
    // TODO : Virer �a de l� => Highlightable est aussi utilis� pour tous les objets de la sc�ne comme le sol ou le robot dropZoneChild est propre aux objets de programme
    public GameObject dropZoneChild; // Drop zone g�n�ral de l'objet qui permetra d'activer la red bar ou l'outline lors du survole de l'�l�ment (selon condition)
}