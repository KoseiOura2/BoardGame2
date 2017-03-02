using UnityEngine;
using System.Collections;

public class BackgroundObj : MonoBehaviour {

	private bool _render = true;

	// Use this for initialization
	void Start( ) {
	
	}
	
	// Update is called once per frame
	void Update( ) {
	
	}

	public void rendRefresh( ) {
		if ( !_render ) {
			_render = true;
			GetComponent< Renderer >( ).enabled = true;
			this.gameObject.tag = "BackgroundObj";
		}
	}

	public void notRend( ) {
		_render = false;
		GetComponent< Renderer >( ).enabled = false;
		this.gameObject.tag = "NoRendObj";
	}
}
