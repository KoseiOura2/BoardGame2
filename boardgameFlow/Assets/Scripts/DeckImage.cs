using UnityEngine;
using System.Collections;

public class DeckImage : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void adjustSize( ) {
		GetComponent< RectTransform >( ).offsetMax = new Vector2( -Screen.width / 5, -Screen.height / 4 );
		GetComponent< RectTransform >( ).offsetMin = new Vector2( Screen.width / 5, Screen.height / 4 );
	}
}
