using UnityEngine;
using System.Collections;

public class DrawCard : MonoBehaviour {

    [ SerializeField ]
    private bool _finish_anim = false;

	// Use this for initialization
	void Start( ) {
	
	}
	
	// Update is called once per frame
	void Update( ) {
	
	}

    public void finishAnim( ) {
        _finish_anim = true;
    }

    public bool isFinishAnim( ) {
        return _finish_anim;
    }
}
