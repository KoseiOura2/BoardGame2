using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Common;

public class EventManager : MonoBehaviour {

    private ParticleManager _particle_manager;
    private PlayerManager _player_manager;

    //アプリケーションマネージャーから必要なマネージャーを取得
    public void init( ref ParticleManager particle_manager, ref PlayerManager player_manager ) {
        _particle_manager = particle_manager;
        _player_manager   = player_manager;
    }

    /// <summary>
    /// イベント開始時の処理
    /// </summary>
    /// <param name="id"></param>
    /// <param name="_event_type"></param>
    public void startEvent( int id,  EVENT_TYPE _event_type ) {
        _player_manager.setEventStart( true );
        _player_manager.setEventType( id, _event_type );
    }

    /// <summary>
	/// DRAWイベントの処理
	/// </summary>
    /// <param value="drawnumber" ドロー枚数>The index.</param>
    public void drawEvent( int id, int value , EVENT_TYPE[ ] _event_type  )  {

         _player_manager.setEventType( id, _event_type[ id ] );

         value = _file_manager.getMassValue( mass_count )[ 0 ];
         if ( !_animation_running ) {
             _player_manager.setEventStart( true );
             for ( int j = 0; j < value; j++ ) {
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
         
         massAnimation( mass_count, _draw_card_list[ _anim_card_num ] );
         if ( _anim_card_num >= _draw_card_list.Count ) {
             _animation_end = true;
         }

         // カードリストを初期化
         if ( _animation_end ) {
             // カードを送信
             if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                 if ( !_host_data.isSendCard( id ) ) {
		            _host_data.refreshCardList( id );
                     _host_data.setSendCardlist( id, _player_manager.getDrawCard( ) );
                 }
             }
             _anim_card_num = 0;
             _draw_card_list.Clear( );
             _player_manager.setEventFinish( true );
             _animation_end = false;
             _animation_running = false;

        }
    }

    /// <summary>
	/// マスイベントの処理
	/// </summary>
	/// <param id="player" どちらのプレイヤーか>The index.</param>
	public void massEvent( int mass_count, int id ) {
		//_player_manager.setEventStart( id, true );

        int value = 0;
        int create_num = 0;
        List< int > particle_list = new List< int >( );

        switch ( _file_manager.getFileData( ).mass[ mass_count ].event_type ) {
            case EVENT_TYPE.EVENT_NONE:
            case EVENT_TYPE.EVENT_START:
                _event_type[ id ] = EVENT_TYPE.EVENT_START;
                if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                    _host_data.setSendEventType( _player_manager.getPlayerOrder( ), _event_type[ id ] );
                }
                _player_manager.setEventStart( true );
                _player_manager.setEventFinish( true );
                _player_manager.setEventType( id, _event_type[ id ] );
                break;
            case EVENT_TYPE.EVENT_DRAW:
                _event_type[ id ] = EVENT_TYPE.EVENT_DRAW;
                if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                    _host_data.setSendEventType( _player_manager.getPlayerOrder( ), _event_type[ id ] );
                }
                _player_manager.setEventType( id, _event_type[ id ] );

                value = _file_manager.getMassValue( mass_count )[ 0 ];
                if ( !_animation_running ) {
                    _player_manager.setEventStart( true );
                    for ( int j = 0; j < value; j++ ) {
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
                
                massAnimation( mass_count, _draw_card_list[ _anim_card_num ] );
                if ( _anim_card_num >= _draw_card_list.Count ) {
                    _animation_end = true;
                }

                // カードリストを初期化
                if ( _animation_end ) {
                    // カードを送信
                    if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                        if ( !_host_data.isSendCard( id ) ) {
			                _host_data.refreshCardList( id );
                            _host_data.setSendCardlist( id, _player_manager.getDrawCard( ) );
                            Debug.Log( "korosu" + _player_manager.getDrawCard( ).Count );
                        }
                    }
                    _anim_card_num = 0;
                    _draw_card_list.Clear( );
                    _player_manager.setEventFinish( true );
                    _animation_end = false;
                    _animation_running = false;
                }
			    break;
            case EVENT_TYPE.EVENT_MOVE:
                _event_type[ id ] = EVENT_TYPE.EVENT_MOVE;
                if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                    _host_data.setSendEventType( _player_manager.getPlayerOrder( ), _event_type[ id ] );
                }

                // パーティクルの生成
                create_num = _particle_manager.getParticlesForType( PARTICLE_TYPE.PARTICLE_OCEANCURRENT ).Length;
				if( create_num < _particle_manager.getLimitCreateNum( PARTICLE_TYPE.PARTICLE_OCEANCURRENT ) ) {
					_particle_manager.createParticle( PARTICLE_TYPE.PARTICLE_OCEANCURRENT );
				}
				_player_manager.setLimitValue( _file_manager.getMassValue( mass_count )[ 0 ] );
				_player_manager.setCurrentFlag( true );
				_player_manager.setAdvanceFlag( true );
				_player_manager.setEventStart( true );
                _before_player_count = _player_manager.getPlayerCount( id, _stage_manager.getMassCount( ) );
                _player_manager.setEventType( id, EVENT_TYPE.EVENT_MOVE );
                break;
                // カード捨てマス進む
		    case EVENT_TYPE.EVENT_TRAP_ONE:
                _event_type[ id ] = EVENT_TYPE.EVENT_TRAP_ONE;
                if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                    _host_data.setSendEventType( _player_manager.getPlayerOrder( ), _event_type[ id ] );
                }
                
                // パーティクルの生成
                create_num = _particle_manager.getParticlesForType( PARTICLE_TYPE.PARTICLE_OCEANCURRENT ).Length;
				if( create_num < _particle_manager.getLimitCreateNum( PARTICLE_TYPE.PARTICLE_OCEANCURRENT ) ) {
					_particle_manager.createParticle( PARTICLE_TYPE.PARTICLE_OCEANCURRENT );
				}
				_player_manager.setLimitValue( _file_manager.getMassValue ( mass_count )[ 0 ] );
				_player_manager.setCurrentFlag( true );
				_player_manager.setAdvanceFlag( true );
				_player_manager.setEventStart( true );
                _before_player_count = _player_manager.getPlayerCount( id, _stage_manager.getMassCount ( ) );
                _player_manager.setEventType( id, EVENT_TYPE.EVENT_TRAP_ONE );
                break;
            case EVENT_TYPE.EVENT_TRAP_TWO:
                // カードドロー
                _event_type[ id ] = EVENT_TYPE.EVENT_TRAP_TWO;
                if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                    _host_data.setSendEventType( _player_manager.getPlayerOrder( ), _event_type[ id ] );
                }
                _player_manager.setEventType( id, _event_type[ id ] );
                value = _file_manager.getMassValue( mass_count )[ 0 ];
                if ( !_animation_running ) {
                    for ( int j = 0; j < value; j++ ) {
                        // デッキのカード数が０になったらリフレッシュ
                        if ( _card_manager.getDeckCardNum( ) <= 0 ) {
                            _card_manager.createDeck( );
                        }
                        int num = _card_manager.distributeCard( ).id;
                        _draw_card_list.Add( num );
                        _player_manager.addDrawCard( num );
                    }

                    massAnimation( mass_count, _draw_card_list[ _anim_card_num ] );
                    _animation_running = true;
                }

                massAnimation( mass_count, _draw_card_list[ _anim_card_num ] );
                if ( _anim_card_num >= _draw_card_list.Count ) {
                    _animation_end = true;
                }

                // カードリストを初期化
                if ( _animation_end ) {
                    // カードを送信
                    if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                        if ( !_host_data.isSendCard( id ) ) {
			                _host_data.refreshCardList( id );
                            _host_data.setSendCardlist( id, _player_manager.getDrawCard( ) );
                        }
                    }
                    _anim_card_num = 0;
                    _draw_card_list.Clear( );
                    _animation_end = false;
                    _animation_running = false;

                    // パーティクルの生成
                    create_num = _particle_manager.getParticlesForType( PARTICLE_TYPE.PARTICLE_OCEANCURRENT ).Length;
				    if( create_num < _particle_manager.getLimitCreateNum( PARTICLE_TYPE.PARTICLE_OCEANCURRENT ) ) {
					    _particle_manager.createParticle( PARTICLE_TYPE.PARTICLE_OCEANCURRENT );
				    }
                    _player_manager.setEventStart( true );
                    _before_player_count = _player_manager.getPlayerCount( id, _stage_manager.getMassCount( ) );
				    _player_manager.setLimitValue( _file_manager.getMassValue( mass_count )[ 1 ] );
				    _player_manager.setCurrentFlag( true );
				    _player_manager.setAdvanceFlag( false );
                    _player_manager.setEventType( id, _event_type[ id ] );
                }
                break;
            case EVENT_TYPE.EVENT_GOAL:
				_player_manager.setEventStart( true );
                if ( _player_manager.getPlayerResult( id ) == BATTLE_RESULT.WIN ) {
                    Debug.Log ( "プレイヤー" + ( id + 1 ) + ":Goal!!" );
                    _goal_flag = true;
                    _player_manager.setEventFinish( true );
                    _player_manager.setEventType( id, EVENT_TYPE.EVENT_GOAL );
                } else if ( _player_manager.getPlayerResult( id ) == BATTLE_RESULT.LOSE ||
                            _player_manager.getPlayerResult( id ) == BATTLE_RESULT.DRAW ) {

                    // パーティクルの生成
                    create_num = _particle_manager.getParticlesForType( PARTICLE_TYPE.PARTICLE_OCEANCURRENT ).Length;
				    if( create_num < _particle_manager.getLimitCreateNum( PARTICLE_TYPE.PARTICLE_OCEANCURRENT ) ) {
					    _particle_manager.createParticle( PARTICLE_TYPE.PARTICLE_OCEANCURRENT );
				    }
					_player_manager.setLimitValue ( 1 );
					_player_manager.setCurrentFlag ( true );
					_player_manager.setAdvanceFlag ( false );
					_before_player_count = _player_manager.getPlayerCount( id, _stage_manager.getMassCount ( ) );
					_player_manager.setEventType( id, EVENT_TYPE.EVENT_MOVE );
                }
                break;
                /*
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
                */
            case EVENT_TYPE.EVENT_CHANGE:
                // パーティクルの生成
                create_num = _particle_manager.getParticlesForType( PARTICLE_TYPE.PARTICLE_SPIRAL ).Length;
				if( create_num < _particle_manager.getLimitCreateNum( PARTICLE_TYPE.PARTICLE_SPIRAL ) ) {
					_particle_manager.createParticle( PARTICLE_TYPE.PARTICLE_SPIRAL );
                    // イベント開始
                    _player_manager.setEventStart( true );
                    // プレイヤーの位置を保持
					_before_player_count = _player_manager.getPlayerCount( id, _stage_manager.getMassCount( ) );
                    _count_tmp           = _player_manager.getPlayerCount( id, _stage_manager.getMassCount( ) );
                    _vector_tmp          = _stage_manager.getTargetMassPos( _count_tmp );
				}

                particle_list = _particle_manager.getParticleNumsForType( PARTICLE_TYPE.PARTICLE_SPIRAL );
                // プレイヤー移動フェイズになったら
                if ( _particle_manager.isPhaseChange( particle_list[ 0 ] ) &&
                     _particle_manager.getParticlePhase( particle_list[ 0 ] ) == _particle_manager.PLAYER_MOVE_START_PHASE_ON_SPIRAL ) {
                    // 1Pの処理
					if ( id == ( int )PLAYER_ORDER.PLAYER_ONE ) {
						_player_manager.setPlayerPosition( ( int )PLAYER_ORDER.PLAYER_ONE,
                            _stage_manager.getTargetMassPos( _player_manager.getPlayerCount( ( int )PLAYER_ORDER.PLAYER_TWO,
                                _stage_manager.getMassCount( ) ) ) );
						_player_manager.setPlayerPosition( ( int )PLAYER_ORDER.PLAYER_TWO, _vector_tmp );
                        _player_manager.setPlayerCount( ( int )PLAYER_ORDER.PLAYER_ONE,
                                                        _player_manager.getPlayerCount( ( int )PLAYER_ORDER.PLAYER_TWO,
                                                                                        _stage_manager.getMassCount( ) ) );
						_player_manager.setPlayerCount( ( int )PLAYER_ORDER.PLAYER_TWO, _count_tmp );
					} 
                    // 2Pの処理
                    else if ( id == ( int )PLAYER_ORDER.PLAYER_TWO ) {
						_player_manager.setPlayerPosition( ( int )PLAYER_ORDER.PLAYER_TWO,
                            _stage_manager.getTargetMassPos( _player_manager.getPlayerCount( ( int )PLAYER_ORDER.PLAYER_ONE,
                                _stage_manager.getMassCount( ) ) ) );
						_player_manager.setPlayerPosition( ( int )PLAYER_ORDER.PLAYER_ONE, _vector_tmp );
                        _player_manager.setPlayerCount( ( int )PLAYER_ORDER.PLAYER_TWO,
                            _player_manager.getPlayerCount( ( int )PLAYER_ORDER.PLAYER_ONE, _stage_manager.getMassCount ( ) ) ); 
						_player_manager.setPlayerCount( ( int )PLAYER_ORDER.PLAYER_ONE, _count_tmp );
					}
                    // プレイヤーの順位を更新
					int[ ] count = getResideCount( );
					_player_manager.dicisionTopAndLowestPlayer( ref count );
				}
                _player_manager.setEventType( id, EVENT_TYPE.EVENT_CHANGE );
                break;
		    case EVENT_TYPE.EVENT_WORP:
                _player_manager.setEventType( id, EVENT_TYPE.EVENT_WORP );
                // パーティクルの生成
                create_num = _particle_manager.getParticlesForType( PARTICLE_TYPE.PARTICLE_SPIRAL ).Length;
				if( create_num < _particle_manager.getLimitCreateNum( PARTICLE_TYPE.PARTICLE_SPIRAL ) ) {
					_particle_manager.createParticle( PARTICLE_TYPE.PARTICLE_SPIRAL );
                    _player_manager.setEventStart( true );
                    // プレイヤーの位置を保持
					_before_player_count = _player_manager.getPlayerCount( id, _stage_manager.getMassCount( ) );
                    // ワープする場所を決定
                    _worp_position = _file_manager.getNomalValue( _player_manager.getPlayerCount( id, _stage_manager.getMassCount( ) ) );
				}

                particle_list = _particle_manager.getParticleNumsForType( PARTICLE_TYPE.PARTICLE_SPIRAL );
                // プレイヤー移動フェイズになったら
                if ( _particle_manager.isPhaseChange( particle_list[ 0 ] ) &&
                     _particle_manager.getParticlePhase( particle_list[ 0 ] ) == _particle_manager.PLAYER_MOVE_START_PHASE_ON_SPIRAL ) {
					_player_manager.setPlayerCount( id, _worp_position );
					_player_manager.setPlayerPosition( id, _stage_manager.getTargetMassPos( _worp_position ) );
					int[ ] count = getResideCount( );
					_player_manager.dicisionTopAndLowestPlayer( ref count );
				}
                break;
			case EVENT_TYPE.EVENT_DISCARD:
                _event_type[ id ] = EVENT_TYPE.EVENT_DISCARD;
                if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                    _host_data.setSendEventType( _player_manager.getPlayerOrder( ), _event_type[ id ] );
                }
                Debug.Log( "カード" + "捨てる" );
				//if ( _player_manager.getAnimationEnd( id ) == true ) {
					_player_manager.setEventStart( true );
					_player_manager.setEventFinish( true );
				//}
				_player_manager.setEventType( id, EVENT_TYPE.EVENT_DISCARD );
				break;
		}  
	}
    /// <summary>
    /// マス効果のコルーチン
    /// </summary>
    void massAnimation( int mass_count, int card_id ) {
        if ( _animation_time == 0.0f ) {
            GameObject treasure_chest = GameObject.Find( "TreasureChest:" + mass_count );

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
}
