using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Common;

public class GraphicManager : Manager< GraphicManager > {

    private const float DISTRIBUTE_TIME = 1.0f;

    private Sprite[ ] _big_num_image = new Sprite[ 10 ];

    // タイトルシーン用
    private GameObject _title_back_pref;
    private GameObject _title_back_obj;

    // ステージ用グラフィック
    private GameObject _plane_pref;
    private GameObject _back_ground_objs_pref;
	private GameObject _mass_prefab;	
    private GameObject _chest_prefab;
	private GameObject[ ] _player_pref = new GameObject[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
    // ステージ用オブジェクト
    private GameObject _plane_obj;
    private GameObject _background_objs;
	private List< GameObject > _mass_obj_list = new List< GameObject >( );	
	private List< GameObject > _chest_obj_list = new List< GameObject >( );	
    private GameObject[ ] _player_objs = new GameObject[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
    
    // UI
    private GameObject[ ] _player_label_prefs = new GameObject[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
    private GameObject[ ] _player_label_objs  = new GameObject[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
    private GameObject _deck_ui_pref;
    private GameObject _draw_card_ui_pref;
    private GameObject _deck_ui_obj;
    private GameObject[ ][ ] _draw_card_ui_objs = new GameObject[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ][ ];
    
    private int[ ] _distribution_count = new int[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
    private float _distribution_time = 0.0f;
    [ SerializeField ]
    private bool[ ][ ] _start_draw_card_move  = new bool[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ][ ];
    private bool[ ] _finish_draw_card_move = new bool[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];

	// Use this for initialization
	void Start( ) {
	
	}
    // Awake関数の代わり
	protected override void initialize( ) {
		
	}

    public void init( ) {
        loadAlwaysGraph( );
        // 配列の確保
        for ( int i = 0; i < _draw_card_ui_objs.Length; i++ ) {
            _draw_card_ui_objs[ i ] = new GameObject[ 3 ];
        }
        
        for ( int i = 0; i < _start_draw_card_move.Length; i++ ) {
            _start_draw_card_move[ i ] = new bool[ 3 ];
        }
        for ( int i = 0; i < _finish_draw_card_move.Length; i++ ) {
            for ( int j = 0; j < _start_draw_card_move[ i ].Length; j++ ) {
                _start_draw_card_move[ i ][ j ] = false;
            }
            _finish_draw_card_move[ i ] = false;
            _distribution_count[ i ] = 0;
        }
    }

    /// <summary>
    /// プレハブロード用関数
    /// </summary>
    /// <param name="data_path"></param>
    /// <returns></returns>
    public GameObject loadPrefab( string data_path ) {
        GameObject pref = Resources.Load< GameObject >( "Prefabs/" + data_path );

        return pref;
    }
    
    /// <summary>
    /// スプライトロード用関数
    /// </summary>
    /// <param name="data_path"></param>
    /// <returns></returns>
    public Sprite loadSprite( string data_path ) {
        Sprite sprite = Resources.Load< Sprite >( "Graphics/" + data_path );

        return sprite;
    }

    private void loadAlwaysGraph( ) {
        _title_back_pref = loadPrefab( "BackGroundObj/TitleBack" );

        for ( int i = 0; i < _big_num_image.Length; i++ ) {
            _big_num_image[ i ] = loadSprite( "UI/number/number_buff_" + i );
        }
    }

    public void loadMainGameGraph( ) {
        _plane_pref            = Resources.Load< GameObject >( "Prefabs/BackGroundObj/Terrain" );
        _back_ground_objs_pref = Resources.Load< GameObject >( "Prefabs/BackGroundObj/BackGroundObj" );
        _chest_prefab          = Resources.Load< GameObject >( "Prefabs/BackGroundObj/object_chest" );
        for ( int i = 0; i < _player_pref.Length; i++ ) {
            // プレイヤープレハブのロード
            _player_pref[ i ] = Resources.Load< GameObject >( "Prefabs/Player/Player" + i );
        }

        _deck_ui_pref      = Resources.Load< GameObject >( "Prefabs/UI/Deck" );
        _draw_card_ui_pref = Resources.Load< GameObject >( "Prefabs/UI/DrawCard" );
    }
	
    /// <summary>
    /// タイトル画面の生成
    /// </summary>
    public void createTitle( ) {
        // 生成
        _title_back_obj = ( GameObject )Instantiate( _title_back_pref );

        // キャンバスの子に設定
        _title_back_obj.transform.SetParent( GameObject.Find( "Canvas" ).transform );
        // スケールの単位化
        _title_back_obj.GetComponent< RectTransform >( ).localScale = new Vector3( 1, 1, 1 );

        // ポジションの設定
        Vector3 pos = _title_back_pref.GetComponent< RectTransform >( ).localPosition;
        _title_back_obj.GetComponent< RectTransform >( ).localPosition = pos;
        _title_back_obj.GetComponent< RectTransform >( ).offsetMax = new Vector2( 0, 0 );
        _title_back_obj.GetComponent< RectTransform >( ).offsetMin = new Vector2( 0, 0 );
        
        // タイトルロゴの生成
        GameObject logo = _title_back_obj.transform.GetChild( 0 ).transform.gameObject;
        logo.GetComponent< RectTransform >( ).offsetMax = new Vector2( -Screen.width / 5, -Screen.height / 3 );
        logo.GetComponent< RectTransform >( ).offsetMin = new Vector2( Screen.width / 5, Screen.height / 3 );
    }
    
    public void destroyTitleObj( ) {
        Destroy( _title_back_obj );
        _title_back_obj = null;
    }

    public GameObject getTitleObj( ) {
        return _title_back_obj;
    }
    
    public void destroyMainGameObj( ) {
        destroyBackGroundObjs( );
        destroyMassObjs( );
        destroyPlayerObj( );
    }

    /// <summary>
    /// メインゲームの背景オブジェクトを生成
    /// </summary>
    public void createBackGroundObj( ) {
        _plane_obj = ( GameObject )Instantiate( _plane_pref );
        _plane_obj.transform.position = _plane_pref.transform.position;
        _background_objs = ( GameObject )Instantiate( _back_ground_objs_pref );
        _background_objs.transform.position = _back_ground_objs_pref.transform.position;
    }

    private void destroyBackGroundObjs( ) {
        Destroy( _plane_obj );
        Destroy( _background_objs );
    }

    public void createMassObj( int num, MASS_TYPE type, EVENT_TYPE event_type, Vector3 pos ) {
		// タイプによるリソース分け
		switch ( type ) {
            case MASS_TYPE.MASS_NORMAL:
                _mass_prefab = ( GameObject )Resources.Load( "Prefabs/Mass/mass_normal" );
                break;
			case MASS_TYPE.MASS_START:
			case MASS_TYPE.MASS_GOAL:
				_mass_prefab = ( GameObject )Resources.Load( "Prefabs/Mass/mass_yellow" );
                break;
			case MASS_TYPE.MASS_NONE:
                _mass_prefab = ( GameObject )Resources.Load( "Prefabs/Mass/mass_blue" );
                break;
			case MASS_TYPE.MASS_DENGER:
				_mass_prefab = ( GameObject )Resources.Load( "Prefabs/Mass/mass_red" );
				break;
			case MASS_TYPE.MASS_EVENT:
				_mass_prefab = ( GameObject )Resources.Load( "Prefabs/Mass/mass_green" );
                break;
        }
        
		// 生成
		GameObject obj = ( GameObject )Instantiate( _mass_prefab, pos, _mass_prefab.transform.localRotation );
		obj.name = "Mass:ID" + num;
        
        // マネージャーの配下に設定
        obj.transform.parent = transform;
        _mass_obj_list.Add( obj );

        // チェストの生成
        switch( event_type ) {
            case EVENT_TYPE.EVENT_DRAW:
            case EVENT_TYPE.EVENT_TRAP_TWO:
                Vector3 mass_local_pos = obj.transform.localPosition;
                GameObject chest = ( GameObject )Instantiate( _chest_prefab );
                chest.transform.position = new Vector3( mass_local_pos.x, mass_local_pos.y, mass_local_pos.z + 2 );
                chest.name = "TreasureChest:" + num;
                _chest_obj_list.Add( chest );
                break; 
        }
    }

    public GameObject getMassObjPos( int num ) {
        return _mass_obj_list[ num ];
    }

    public void createMiniMassObj( ) {
        // マスとマスのつなぎを生成
		for( int i = 0; i < _mass_obj_list.Count - 1; i++ ) {
			_mass_prefab = ( GameObject )Resources.Load( "Prefabs/Mass/mass_mini" );
			Vector3 mini_pos = Vector3.Lerp( _mass_obj_list[ i ].transform.localPosition, _mass_obj_list[ i + 1 ].transform.localPosition, 0.5f );
			GameObject mini_obj = ( GameObject )Instantiate( _mass_prefab, mini_pos, _mass_prefab.transform.localRotation );
			mini_obj.transform.SetParent( _mass_obj_list[ i ].transform );
		}
    }

    private void destroyMassObjs( ) {
        for ( int i = 0; i < _mass_obj_list.Count; i++ ) {
            if ( i < _mass_obj_list.Count - 1 ) {
                Destroy( _mass_obj_list[ i ].transform.GetChild( 0 ).gameObject );
            }
            Destroy( _mass_obj_list[ i ] );
        }
        _mass_obj_list.Clear( );

        for ( int i =0; i < _chest_obj_list.Count; i++ ) {
            Destroy( _chest_obj_list[ i ] );
        }
        _chest_obj_list.Clear( );
    }

    public void createPlayerObj( Transform trans ) {
        // オブジェクト生成
        for ( int i = 0; i < _player_objs.Length; i++ ) {
		    _player_objs[ i ] = ( GameObject )Instantiate( _player_pref[ i ] );
            _player_objs[ i ].transform.parent = trans;
            _player_objs[ i ].name   = "Player" + i;
        }
    }

    public void destroyPlayerObj( ) {
        for ( int i = 0; i < _player_objs.Length; i++ ) {
            Destroy( _player_objs[ i ] );
        }
    }

    public GameObject getPlayerObj( int num ) {
        return _player_objs[ num ];
    }

    public void movePlayerObj( int num, Vector3 pos ) {
        _player_objs[ num ].transform.position = pos;
    }

    /// <summary>
    /// プレイヤーラベルの生成
    /// </summary>
    /// <param name="player_num"></param>
    /// <param name="dice_value"></param>
    public void createPlayerLabel( int player_num, int dice_value ) {
        // 生成
        _player_label_prefs[ player_num ] = ( GameObject )Resources.Load( "Prefabs/UI/PlayerLabel_" + player_num );
        _player_label_objs[ player_num ] = ( GameObject )Instantiate( _player_label_prefs[ player_num ] );
        
        // キャンバスの子に設定
        _player_label_objs[ player_num ].transform.SetParent( GameObject.Find( "Canvas" ).transform );
        // スケールの単位化
        _player_label_objs[ player_num ].GetComponent< RectTransform >( ).localScale = new Vector3( 1, 1, 1 );
        
        // ポジションの設定
        Vector3 pos = _player_label_prefs[ player_num ].GetComponent< RectTransform >( ).localPosition;
        _player_label_objs[ player_num ].GetComponent< RectTransform >( ).localPosition = pos;
        _player_label_objs[ player_num ].GetComponent< PlayerLabel >( ).dicisionPlayerOrder( ( PLAYER_ORDER )player_num );
        _player_label_objs[ player_num ].GetComponent< PlayerLabel >( ).setPos( );
        
        // さいの目の設定
        GameObject dice_value_obj = _player_label_objs[ player_num ].transform.GetChild( 0 ).transform.gameObject;
        dice_value_obj.GetComponent< Image >( ).sprite = _big_num_image[ dice_value ];
    }

    public void destroyAllPlayerLabels( ) {
        for ( int i = 0; i < _player_label_objs.Length; i++ ) {
            Destroy( _player_label_objs[ i ] );
            _player_label_objs[ i ] = null;
            _player_label_prefs[ i ] = null;
        }
    }

    /// <summary>
    /// ドローフェイズ時のUIを生成
    /// </summary>
    public void createDrawPhaseUI( ) {
        // ドローカード
        for ( int i = 0; i < _draw_card_ui_objs.Length; i++ ) {
            for ( int j = 0; j < _draw_card_ui_objs[ i ].Length; j++ ) {
                // 生成
                _draw_card_ui_objs[ i ][ j ] = ( GameObject )Instantiate( _draw_card_ui_pref );
        
                // キャンバスの子に設定
                _draw_card_ui_objs[ i ][ j ].transform.SetParent( GameObject.Find( "Canvas" ).transform );
                // スケールの単位化
                _draw_card_ui_objs[ i ][ j ].GetComponent< RectTransform >( ).localScale = new Vector3( 1, 1, 1 );
        
                // ポジションの設定
                Vector3 draw_card_pos = _draw_card_ui_pref.GetComponent< RectTransform >( ).localPosition;
                _draw_card_ui_objs[ i ][ j ].GetComponent< RectTransform >( ).localPosition = draw_card_pos;
            }
        }

        // デッキ
        // 生成
        _deck_ui_obj = ( GameObject )Instantiate( _deck_ui_pref );
        
        // キャンバスの子に設定
        _deck_ui_obj.transform.SetParent( GameObject.Find( "Canvas" ).transform );
        // スケールの単位化
        _deck_ui_obj.GetComponent< RectTransform >( ).localScale = new Vector3( 1, 1, 1 );
        
        // ポジションの設定
        Vector3 deck_pos = _deck_ui_pref.GetComponent< RectTransform >( ).localPosition;
        _deck_ui_obj.GetComponent< RectTransform >( ).localPosition = deck_pos;
    }

    public void destroyDrawPhaseUI( ) {
        Destroy( _deck_ui_obj );
        for ( int i = 0; i < _draw_card_ui_objs.Length; i++ ) {
            for ( int j = 0; j < _draw_card_ui_objs[ i ].Length; j++ ) {
                Destroy( _draw_card_ui_objs[ i ][ j ] );
            }
        }
    }

    public void moveDrawCardUI( int player_order, int[ ] distribute_value ) {
        _distribution_time += Time.deltaTime;

        if ( _distribution_time >= DISTRIBUTE_TIME * _distribution_count[ player_order ] &&
            _distribution_count[ player_order ] < distribute_value[ player_order ] ) {
            if ( !_start_draw_card_move[ player_order ][ _distribution_count[ player_order ] ] ) {
                // アニメーションの再生
                _draw_card_ui_objs[ player_order ][ _distribution_count[ player_order ] ].GetComponent< Animator >( ).Play( "CardDraw" + player_order );
                _start_draw_card_move[ player_order ][ _distribution_count[ player_order ] ] = true;
                _distribution_count[ player_order ]++;
            }
        }
        // アニメーション終了処理
        if ( _draw_card_ui_objs[ player_order ][ distribute_value[ player_order ] - 1 ].GetComponent< DrawCard >( ).isFinishAnim( ) ) {
            _finish_draw_card_move[ player_order ] = true;
            _distribution_count[ player_order ] = 0;
            _distribution_time = 0.0f;
        }
    }

    public bool isFinishDrawCardMove( ) {
        if ( _finish_draw_card_move[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM - 1 ] ) {
            for ( int i = 0; i < _finish_draw_card_move.Length; i++ ) {
                for ( int j = 0; j < _start_draw_card_move[ i ].Length; j++ ) {
                    _start_draw_card_move[ i ][ j ] = false;
                }
                _finish_draw_card_move[ i ] = false;
            }
            _distribution_time = 0.0f;

            return true;
        }

        return false;
    }

    public bool isFinishDrawCardMove( int num ) {
        return _finish_draw_card_move[ num ];
    }
}
