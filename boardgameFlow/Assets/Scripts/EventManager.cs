using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Common;

public class EventManager : MonoBehaviour {
    
	[ SerializeField ]
    private ParticleManager _particle_manager;
	[ SerializeField ]
    private PlayerManager _player_manager;
	[ SerializeField ]
    private StageManager _stage_manager;
	[ SerializeField ]
    private NetworkMNG _network_manager;
	[ SerializeField ]
	private CardManager _card_manager;
    
    private GameObject _anim_draw_card;
    private PROGRAM_MODE _mode     = PROGRAM_MODE.MODE_NO_CONNECT;
    private EVENT_TYPE _event_type = EVENT_TYPE.EVENT_NONE;
    private int[ ] _mass_value = new int[ ] { 0, 0 };
    private int[ ] _reside_count = new int[ ] { 0, 0 };
    private List< int > _draw_card_list = new List< int >( );
    private int _mass_count = 0;
    private int _player_id  = 0;
    private int _anim_card_num       = 0;
    private int _before_player_count = 0;
    private int _worp_position       = 0;
    private float _animation_time = 0.0f;
    private bool _animation_running = false;
    private bool _animation_end     = false;
    private bool _goal              = false;

    /// <summary>
    /// アプリケーションマネージャーから必要なマネージャーを取得
    /// </summary>
    /// <param name="particle_manager"></param>
    /// <param name="player_manager"></param>
    /// <param name="network_manager"></param>
    /// <param name="card_manager"></param>
    public void init( ref ParticleManager particle_manager, ref PlayerManager player_manager,
                      ref NetworkMNG network_manager, ref CardManager card_manager, ref StageManager stage_manager ) {
        _particle_manager = particle_manager;
        _player_manager   = player_manager;
        _stage_manager    = stage_manager;
        _network_manager  = network_manager;
        _card_manager     = card_manager;
    }

    /// <summary>
    /// イベント開始時の処理
    /// </summary>
    /// <param name="id"></param>
    /// <param name="event_type"></param>
    /// <param name="value"></param>
    public void startEvent( int id,  EVENT_TYPE event_type, int[ ] value ) {
        _event_type = event_type;
        _player_manager.setEventStart( true );
        _player_manager.setEventType( id, event_type );
        if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
            _network_manager.setEventType( id, event_type );
        }
        _mass_value = value;
        _player_id  = id;
    }
    
    public delegate void eventDelegate( );

    /// <summary>
    /// マスイベントの処理
    /// </summary>
    /// <param name="mass_count"></param>
	public void massEvent( int mass_count ) {
        _mass_count = mass_count;

        /*
        public enum EVENT_TYPE {
            EVENT_NONE,           //0
            EVENT_START,          //1
            EVENT_GOAL,           //2
            EVENT_DRAW,           //3
            EVENT_MOVE,           //4
            EVENT_TRAP_ONE,       //5
            EVENT_TRAP_TWO,       //6
            EVENT_WORP,           //7
            EVENT_CHANGE,         //8
		    EVENT_DISCARD,        //9
        }
        */
        eventDelegate[ ] method = {
            new eventDelegate( eventNone ),
            new eventDelegate( eventNone ),
            new eventDelegate( eventGoal ),
            new eventDelegate( eventDraw ),
            new eventDelegate( eventMove ),
            new eventDelegate( eventMoveAndDisCard ),
            new eventDelegate( eventDrawAndBackMove ),
            new eventDelegate( eventWorp ),
            new eventDelegate( eventChange ),
            new eventDelegate( eventDisCard ),
        };

        // 任意のイベントを呼び出す
        method[ ( int )_event_type ]( );

        if ( _animation_end ) {
            _animation_end = false;
        }
    }

    /// <summary>
    /// EventNone or EventStart時の処理
    /// </summary>
    private void eventNone( ) {
        _player_manager.setEventFinish( true );
    }

    /// <summary>
    /// EventGoal時の処理
    /// </summary>
    private void eventGoal( ) {
        // カードバトルに勝利した場合
        if ( _player_manager.getPlayerResult( _player_id ) == BATTLE_RESULT.WIN ) {
            _goal = true;
            _player_manager.setEventFinish( true );
            _player_manager.setEventType( _player_id, EVENT_TYPE.EVENT_GOAL );
        } 
        // カードバトルに引き分け・負けた場合
        else if ( _player_manager.getPlayerResult( _player_id ) == BATTLE_RESULT.LOSE ||
                    _player_manager.getPlayerResult( _player_id ) == BATTLE_RESULT.DRAW ) {
            // パーティクルの生成
            int create_num = _particle_manager.getParticlesForType( PARTICLE_TYPE.PARTICLE_OCEANCURRENT ).Length;
			if ( create_num < _particle_manager.getLimitCreateNum( PARTICLE_TYPE.PARTICLE_OCEANCURRENT ) ) {
				_particle_manager.createParticle( PARTICLE_TYPE.PARTICLE_OCEANCURRENT );
			}
			_player_manager.setLimitValue( 1 );
			_player_manager.setCurrentFlag( true );
			_player_manager.setAdvanceFlag( false );
			_player_manager.setEventType( _player_id, EVENT_TYPE.EVENT_MOVE );
        }
    }

    /// <summary>
	/// eventDrawの処理
	/// </summary>
    public void eventDraw( ) {
        if ( !_animation_running ) {
            _player_manager.setEventStart( true );
            for ( int j = 0; j < _mass_value[ 0 ]; j++ ) {
                // デッキのカード数が０になったらリフレッシュ
                if ( _card_manager.getDeckCardNum( ) <= 0 ) {
                    _card_manager.createDeck( );
                }
                int num = _card_manager.distributeCard( ).id;
                _draw_card_list.Add( num );
                _player_manager.addDrawCard( num );
            }
            _animation_running = true;
        }
                
        eventAnimation( _draw_card_list[ _anim_card_num ] );
        if ( _anim_card_num >= _draw_card_list.Count ) {
            _animation_end = true;
        }

        // カードリストを初期化
        if ( _animation_end ) {
            // カードを送信
            if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                if ( !_network_manager.isSendCard( _player_id ) ) {
                    _network_manager.refreshCard( _player_id );
                    _network_manager.setCardList( _player_id, _player_manager.getDrawCard( ) );
                }
            }
            _anim_card_num = 0;
            _draw_card_list.Clear( );
            _player_manager.setEventFinish( true );
            _animation_running = false;
        }
    }
    
    /// <summary>
	/// eventMoveの処理
	/// </summary>
    private void eventMove( ) {
        // パーティクルの生成
        int create_num = _particle_manager.getParticlesForType( PARTICLE_TYPE.PARTICLE_OCEANCURRENT ).Length;
		if ( create_num < _particle_manager.getLimitCreateNum( PARTICLE_TYPE.PARTICLE_OCEANCURRENT ) ) {
			_particle_manager.createParticle( PARTICLE_TYPE.PARTICLE_OCEANCURRENT );
		}

        int value = 0;

        if ( _event_type == EVENT_TYPE.EVENT_MOVE || _event_type == EVENT_TYPE.EVENT_TRAP_ONE ) {
            value = _mass_value[ 0 ];
        } else if ( _event_type == EVENT_TYPE.EVENT_TRAP_TWO ) {
            value = _mass_value[ 1 ];
        }

		_player_manager.setLimitValue( value );
		_player_manager.setCurrentFlag( true );
		_player_manager.setAdvanceFlag( true );
    }
    
    /// <summary>
	/// eventMoveAndDisCardの処理
	/// </summary>
    private void eventMoveAndDisCard( ) {
        // 移動処理
        eventMove( );
    }
    
    /// <summary>
	/// eventDrawAndBackMoveの処理
	/// </summary>
    private void eventDrawAndBackMove( ) {
        if ( _animation_end ) {
            // 移動処理
            eventMove( );
        } else {
            // ドロー処理
            eventDraw( );
        }
    }
    
    /// <summary>
	/// eventWorpの処理
	/// </summary>
    private void eventWorp( ) {
        // パーティクルの生成
        int create_num = _particle_manager.getParticlesForType( PARTICLE_TYPE.PARTICLE_SPIRAL ).Length;
	    if ( create_num < _particle_manager.getLimitCreateNum( PARTICLE_TYPE.PARTICLE_SPIRAL ) ) {
		    _particle_manager.createParticle( PARTICLE_TYPE.PARTICLE_SPIRAL );
            // ワープする場所を決定
            _worp_position = _mass_value[ 0 ];
	    }

        List< int > particle_list = _particle_manager.getParticleNumsForType( PARTICLE_TYPE.PARTICLE_SPIRAL );
        // プレイヤー移動フェイズになったら
        if ( _particle_manager.isPhaseChange( particle_list[ 0 ] ) &&
             _particle_manager.getParticlePhase( particle_list[ 0 ] ) == _particle_manager.PLAYER_MOVE_START_PHASE_ON_SPIRAL ) {
		    _player_manager.setPlayerCount( _player_id, _worp_position );
		    _player_manager.setPlayerPosition( _player_id, _stage_manager.getTargetMassPos( _worp_position ) );
		    _player_manager.dicisionTopAndLowestPlayer( ref _reside_count );
	    }
    }
    
    /// <summary>
	/// eventChangeの処理
	/// </summary>
    private void eventChange( ) {
        int count_tmp = 0;
        Vector3 vector_tmp = Vector3.zero;

        // パーティクルの生成
        int create_num = _particle_manager.getParticlesForType( PARTICLE_TYPE.PARTICLE_SPIRAL ).Length;
		if ( create_num < _particle_manager.getLimitCreateNum( PARTICLE_TYPE.PARTICLE_SPIRAL ) ) {
			_particle_manager.createParticle( PARTICLE_TYPE.PARTICLE_SPIRAL );
            // プレイヤーの位置を保持
            count_tmp  = _player_manager.getPlayerCount( _player_id, _stage_manager.getMassCount( ) );
            vector_tmp = _stage_manager.getTargetMassPos( count_tmp );
		}

        List< int > particle_list = _particle_manager.getParticleNumsForType( PARTICLE_TYPE.PARTICLE_SPIRAL );
        // プレイヤー移動フェイズになったら
        if ( _particle_manager.isPhaseChange( particle_list[ 0 ] ) &&
                _particle_manager.getParticlePhase( particle_list[ 0 ] ) == _particle_manager.PLAYER_MOVE_START_PHASE_ON_SPIRAL ) {
            // 1Pの処理
			if ( _player_id == ( int )PLAYER_ORDER.PLAYER_ONE ) {
				_player_manager.setPlayerPosition( ( int )PLAYER_ORDER.PLAYER_ONE,
                    _stage_manager.getTargetMassPos( _player_manager.getPlayerCount( ( int )PLAYER_ORDER.PLAYER_TWO,
                        _stage_manager.getMassCount( ) ) ) );
				_player_manager.setPlayerPosition( ( int )PLAYER_ORDER.PLAYER_TWO, vector_tmp );
                _player_manager.setPlayerCount( ( int )PLAYER_ORDER.PLAYER_ONE,
                                                _player_manager.getPlayerCount( ( int )PLAYER_ORDER.PLAYER_TWO,
                                                                                _stage_manager.getMassCount( ) ) );
				_player_manager.setPlayerCount( ( int )PLAYER_ORDER.PLAYER_TWO, count_tmp );
			} 
            // 2Pの処理
            else if ( _player_id == ( int )PLAYER_ORDER.PLAYER_TWO ) {
				_player_manager.setPlayerPosition( ( int )PLAYER_ORDER.PLAYER_TWO,
                    _stage_manager.getTargetMassPos( _player_manager.getPlayerCount( ( int )PLAYER_ORDER.PLAYER_ONE,
                        _stage_manager.getMassCount( ) ) ) );
				_player_manager.setPlayerPosition( ( int )PLAYER_ORDER.PLAYER_ONE, vector_tmp );
                _player_manager.setPlayerCount( ( int )PLAYER_ORDER.PLAYER_TWO,
                    _player_manager.getPlayerCount( ( int )PLAYER_ORDER.PLAYER_ONE, _stage_manager.getMassCount ( ) ) ); 
				_player_manager.setPlayerCount( ( int )PLAYER_ORDER.PLAYER_ONE, count_tmp );
			}
            // プレイヤーの順位を更新
			_player_manager.dicisionTopAndLowestPlayer( ref _reside_count );
		}
    }

    /// <summary>
	/// eventDisCardの処理
	/// </summary>
    private void eventDisCard( ) {
		//if ( _player_manager.getAnimationEnd( id ) == true ) {
			_player_manager.setEventStart( true );
			_player_manager.setEventFinish( true );
		//}
		_player_manager.setEventType( _player_id, EVENT_TYPE.EVENT_DISCARD );
    }

    /// <summary>
    /// マス効果のコルーチン
    /// </summary>
    private void eventAnimation( int card_id ) {
        if ( _animation_time == 0.0f ) {
            GameObject treasure_chest = GameObject.Find( "TreasureChest:" + _mass_count );

            _anim_draw_card = Instantiate( ( GameObject )Resources.Load( "Prefabs/AnimationCard" ) );
            _anim_draw_card.GetComponent< Card >( ).setCardData( _card_manager.getCardData( card_id ) );
            _anim_draw_card.transform.parent = treasure_chest.transform;
            _anim_draw_card.transform.position = treasure_chest.transform.position;
            _anim_draw_card.transform.localScale = Vector3.one;
        }
        _animation_time += Time.deltaTime;
        
        if ( Mathf.Approximately( _animation_time, 3.0f ) ) {
            Destroy( _anim_draw_card.GetComponent< Animator >( ) );
            //カメラの前に表示
            Vector3 returnScale = _anim_draw_card.transform.localScale;
            _anim_draw_card.transform.localScale = returnScale;
            _anim_draw_card.transform.rotation = Camera.main.transform.rotation;
            _anim_draw_card.transform.parent = Camera.main.transform;
            _anim_draw_card.transform.localPosition = new Vector3( 0, 0, 5 );
        } else if ( Mathf.Approximately( _animation_time, 5.0f ) ) {
            _animation_time = 0.0f;
            Destroy( _anim_draw_card );
            _anim_card_num++;
        }
    }

    /// <summary>
    /// ゴール成功したかどうか
    /// </summary>
    /// <returns></returns>
    public bool isGoal( ) {
        if ( _goal ) {
            return true;
        }

        return false;
    }

    public void setNotGoal( ) {
        _goal = false;
    }
   
    /// <summary>
    /// ゴールまでどれくらい残っているか設定
    /// </summary>
    /// <returns></returns>
    public void setResideCount( int[ ] count ) {
		_reside_count = count;
    }
        /*
         switch ( _event_type ) {
                case "selectDraw":
                    int cardType = _file_manager.getCardID ( i );
                    _card_manager.getCardData ( cardType );
					_player_manager.setEventStart ( id, true );
                    _player_manager.setEventFinish ( id, true );
                    _reset_mass_update[ id ] = true;
                    break;
                case "Buff":
                    int buffValue = _file_manager.getMassValue ( i )[ 0 ];
                    Debug.Log ( "プレイヤーのパラメーターを" + buffValue.ToString ( ) + "上昇" );
					_player_manager.setEventStart ( id, true );
                    _player_manager.setEventFinish ( id, true );
                    _reset_mass_update[ id ] = true;
                    break;
                case "MoveSeal":
                    Debug.Log ( "行動停止" );
                    _player_manager.setPlayerOnMove ( id, false );
					_player_manager.setEventStart ( id, true );
                    _player_manager.setEventFinish ( id, true );
                    _reset_mass_update[ id ] = true;
                    _player_manager.setEventType ( id, EVENT_TYPE.EVENT_DRAW );
                    break;
		}  
        */
}
