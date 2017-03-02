using UnityEngine;
using System.Collections;
using Common;

public class PlayerLabel : MonoBehaviour {

    private PLAYER_ORDER _player_order = PLAYER_ORDER.NO_PLAYER;

	// Use this for initialization
	void Start( ) {
	
	}
	
    public void dicisionPlayerOrder( PLAYER_ORDER order ) {
        _player_order = order;
    }

    public void setPos( ) {
        if ( _player_order == PLAYER_ORDER.PLAYER_ONE ) {
            Vector3 pos = this.gameObject.GetComponent< RectTransform >( ).localPosition;
            this.gameObject.transform.SetParent( GameObject.Find( "Canvas" ).transform );
            this.gameObject.GetComponent< RectTransform >( ).localScale = new Vector3( 1, 1, 1 );

            this.gameObject.GetComponent< RectTransform >( ).localPosition = pos;
            this.gameObject.GetComponent< RectTransform >( ).offsetMax = new Vector2( -Screen.width * 3 / 4, -Screen.height / 3 );
            this.gameObject.GetComponent< RectTransform >( ).offsetMin = new Vector2( 0, 0 );

            // ダイスの値のポジションを設定
            GameObject dice_value = this.gameObject.transform.GetChild( 0 ).transform.gameObject;
            float obj_width  = Screen.width / 4;
            float obj_height = Screen.height / 3;

            dice_value.GetComponent< RectTransform >( ).offsetMax = new Vector2( -obj_width / 5, -obj_height * 6 / 5 );
            dice_value.GetComponent< RectTransform >( ).offsetMin = new Vector2( 0, 0 );
        } else if ( _player_order == PLAYER_ORDER.PLAYER_TWO ) {
            Vector3 pos = this.gameObject.GetComponent< RectTransform >( ).localPosition;
            this.gameObject.transform.SetParent( GameObject.Find( "Canvas" ).transform );
            this.gameObject.GetComponent< RectTransform >( ).localScale = new Vector3( 1, 1, 1 );

            this.gameObject.GetComponent< RectTransform >( ).localPosition = pos;
            this.gameObject.GetComponent< RectTransform >( ).offsetMax = new Vector2( 0, -Screen.height / 3 );
            this.gameObject.GetComponent< RectTransform >( ).offsetMin = new Vector2( Screen.width * 3 / 4, 0 );
            
            // ダイスの値のポジションを設定
            float obj_width  = Screen.width / 4;
            float obj_height = Screen.height / 3;

            GameObject dice_value = this.gameObject.transform.GetChild( 0 ).transform.gameObject;
            dice_value.GetComponent< RectTransform >( ).offsetMax = new Vector2( 0, -obj_height * 6 / 5 );
            dice_value.GetComponent< RectTransform >( ).offsetMin = new Vector2( obj_width / 5, 0 );
        }

    }

	// Update is called once per frame
	void Update( ) {
	
	}

}
