﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine.Events;

public class ApplicationManager : Manager< ApplicationManager > {
    
    private const int CONNECT_WAIT_TIME        = 120;
	private const int SECOND_CONNECT_WAIT_TIME = 240;
	private const int MAX_DRAW_VALUE           = 4;

    private const int GOAL_WAIT_TIME           = 360;
    private const int GOAL_PARTICLE_WAIT_TIME  = 30;
	private const int MAX_EVENT_NUM            = 2;

	[ SerializeField ]
	private NetworkMNG _network_manager;
	[ SerializeField ]
	private PhaseManager _phase_manager;
    [ SerializeField ]
    private FileManager _file_manager;
	[ SerializeField ]
	private CardManager _card_manager;
    [ SerializeField ]
    private PlayerManager _player_manager;
    [ SerializeField ]
    private StageManager _stage_manager;
    [ SerializeField ]
	private ParticleManager _particle_manager;
	[ SerializeField ]
	private CameraManager _camera_manager;
	[ SerializeField ]
	private GraphicManager _graphic_manager;
	[ SerializeField ]
	private ResultUIManeger _result_UI_maneger;
    [ SerializeField ]
    private NetworkGUIControll _network_gui_controll;
    [ SerializeField ]
    private EventManager _event_manager;
    
	[ SerializeField ]
	private PROGRAM_MODE _mode = PROGRAM_MODE.MODE_NO_CONNECT;
	[ SerializeField ]
	private SCENE _scene = SCENE.SCENE_CONNECT;
	[ SerializeField ]
    private EVENT_TYPE[ ] _event_type = new EVENT_TYPE[ ]{ EVENT_TYPE.EVENT_NONE, EVENT_TYPE.EVENT_NONE };

	private GameObject _go_result_ui;

    private List< int > _draw_card_list = new List< int >( );
	private int[ ] _event_count = new int[ ]{ 0, 0 };        //イベントを起こす回数 
    private int[ ] _dice_value = new int[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
    private int _connect_wait_time   = 0;
    private int _before_player_count = 0;
    private int _worp_position       = 0;
	private int _goal_time           = 0;

    private bool _game_playing      = false;
    private bool _go_finish_scene   = false;
	private bool _refresh_card_list = false;
    private bool _network_init      = false;
    private bool _scene_init        = false;
    private bool _phase_init        = false;
    private bool _send_status       = false;
	private bool _battle            = true;
    
    // デバッグ用
    [ SerializeField ]
	private int _debug_dice_value = 0;
    [ SerializeField ]
	private int[ ] _debug_use_card = new int[ 6 ];
    [ SerializeField ]
	private bool _debug_mode = false;
    [ SerializeField ]
    private bool _immediately_goal = false;

	// Awake関数の代わり
	protected override void initialize( ) {
        if ( isFileManagerError( ) ) {
            return;
        }
        
        if ( isGraphicManagerError( ) ) {
            return;
        }
		referManager( );
	}

    private bool isFileManagerError( ) {
        bool error = false;

        if ( !_file_manager ) {
            try {
                error = true;
                _file_manager = FileManager.getInstance( );
            } catch {
                Debug.LogError( "ファイルマネージャーのインスタンスが取得できませんでした。" );
            }
        }

        return error;
    }

    private bool isGraphicManagerError( ) {
        bool error = false;

        if ( !_graphic_manager ) {
            try {
                error = true;
                _graphic_manager = GraphicManager.getInstance( );
            } catch {
                Debug.LogError( "グラフィックマネージャーのインスタンスが取得できませんでした。" );
            }
        }

        return error;
    }

	// Use this for initialization
	void Start( ) {
		referManager( );

        _graphic_manager.init( );
		_card_manager.init( );
        _particle_manager.init( ref _graphic_manager );
        _event_manager.init( ref _particle_manager, ref _player_manager, ref _network_manager, ref _card_manager, ref _stage_manager );
	}

    /// <summary>
    /// 各マネージャーの参照
    /// </summary>
	private void referManager( ) {
		if ( _network_manager == null ) {
            try {
			    _network_manager = referManager( "NetworkManager" ).GetComponent< NetworkMNG >( );
            } catch {
                Debug.Log( "NetworkManagerの参照に失敗しました。" );
            }
		}
		if ( _phase_manager == null ) {
            try {
			    _phase_manager = referManager( "PhaseManager" ).GetComponent< PhaseManager >( );
            } catch {
                Debug.Log( "PhaseManagerの参照に失敗しました。" );
            }
		}
		if ( _card_manager == null ) {
            try {
			    _card_manager = referManager( "CardManager" ).GetComponent< CardManager >( );
            } catch {
                Debug.Log( "CardManagerの参照に失敗しました。" );
            }
		}
		if ( _player_manager == null ) {
            try {
			    _player_manager = referManager( "PlayerManager" ).GetComponent< PlayerManager >( );
            } catch {
                Debug.Log( "PlayerManagerの参照に失敗しました。" );
            }
		}
		if ( _stage_manager == null ) {
            try {
			    _stage_manager = referManager( "StageManager" ).GetComponent< StageManager >( );
            } catch {
                Debug.Log( "StageManagerの参照に失敗しました。" );
            }
		}
        if ( _particle_manager == null ) {
            try {
			    _particle_manager = referManager( "ParticleManager" ).GetComponent< ParticleManager >( );
            } catch {
                Debug.Log( "ParticleManagerの参照に失敗しました。" );
            }
		}
		if ( _camera_manager == null ) {
            try {
                _camera_manager = Camera.main.GetComponent< CameraManager >( );
            } catch {
                Debug.Log( "CameraManagerの参照に失敗しました。" );
            }
		}
		if ( _network_gui_controll == null ) {
            try {
			    _network_gui_controll = referManager( "NetworkManager" ).GetComponent< NetworkGUIControll >( );
            } catch {
                Debug.Log( "NetworkGUIControllの参照に失敗しました。" );
            }
		}
		if ( _event_manager == null ) {
            try {
			    _event_manager = referManager( "EventManager" ).GetComponent< EventManager >( );
            } catch {
                Debug.Log( "EventManagerの参照に失敗しました。" );
            }
		}
	}

    private GameObject referManager( string manager_name ) {
        GameObject manager = null;
        GameObject pref    = null;

        manager = GameObject.Find( manager_name );
        // マネージャーがヒエラルキー内に存在しない場合
        if ( manager == null ) {
            pref = _graphic_manager.loadPrefab( "Manager/" + manager_name );
            manager = ( GameObject )Instantiate( pref );
        }

        return manager;
    }
	
	// Update is called once per frame
	void FixedUpdate( ) {

        if ( _network_manager != null && !_network_init ) {
            _network_manager.setProgramMode( _mode );
            _network_init = true;
        }

		switch( _scene ) {
		    case SCENE.SCENE_CONNECT:
			    updateConnectScene( );
			    break;
		    case SCENE.SCENE_TITLE:
			    updateTitleScene( );
			    break;
		    case SCENE.SCENE_GAME:
			    updateGameScene( );
			    break;
		    case SCENE.SCENE_FINISH:
			    updateFinishScene( );
			    break;
		}

        _network_manager.checkChangeScene( );
        _network_manager.checkChangePhase( );
        _network_manager.sendHostData( );
	}

	/// <summary>
	/// ConnectSceneの更新
	/// </summary>
	private void updateConnectScene( ) {
		if ( _mode == PROGRAM_MODE.MODE_NO_CONNECT ) {
			if ( Input.GetKeyDown( KeyCode.A ) ) {
				_scene = SCENE.SCENE_TITLE;
				_network_gui_controll.setShowGUI( false );
                _scene_init = false;
			}
		} else  {
			if ( _network_manager.getPlayerNum( ) >= ( int )_mode ) {
				_scene = SCENE.SCENE_TITLE;
				_network_gui_controll.setShowGUI( false );
				try {
                    _network_manager.changeScene( _scene );
				}
				catch {
					Debug.Log( "通信に失敗しまいました" );
				}
			}
		}
	}

	/// <summary>
	/// TitleSceneの更新
	/// </summary>
	private void updateTitleScene( ) {
        if ( !_scene_init ) {
            if ( _graphic_manager.getTitleObj( ) == null ) {
			    _graphic_manager.createTitle( );
            }
			_phase_manager.createPhaseText( MAIN_GAME_PHASE.GAME_PHASE_NO_PLAY );
            _scene_init = true;
        }

        if ( _mode == PROGRAM_MODE.MODE_NO_CONNECT ) {
		    if ( Input.GetKeyDown( KeyCode.A ) ) {
			    _scene = SCENE.SCENE_GAME;
                
                // オブジェクトの生成
                _graphic_manager.loadMainGameGraph( );
                _graphic_manager.createBackGroundObj( );
                GameObject player_manager_obj = _player_manager.transform.gameObject;
                _graphic_manager.createPlayerObj( player_manager_obj.transform );

			    //マスの生成
			    _stage_manager.init( ref _graphic_manager, ref _particle_manager );
			    for ( int i = 0; i < _file_manager.getMassCount( ); i++ ) {
				    int num = _stage_manager.getMassCount( );
				    _stage_manager.massCreate( num, _file_manager.getFileData( ).mass[ num ].mass_type,
                                               _file_manager.getFileData( ).mass[ num ].event_type, _file_manager.getMassCoordinate( num ) );
				    _stage_manager.increaseMassCount( );
			    }

                _stage_manager.createMiniMass( );

			    // ステージマネージャーの環境情報の設定
			    for ( int i = 0; i < _file_manager.getMassCount( ); i++ ) {
				    if ( _file_manager.getEnvironment( i ) != "" ) {
					    switch ( _file_manager.getEnvironment( i ) ) {
					    case "shallows":
						    _stage_manager.setEnvironmentID( i, FIELD_ENVIRONMENT.SHOAL_FIELD );
						    break;
					    case "shoal":
						    _stage_manager.setEnvironmentID( i, FIELD_ENVIRONMENT.OPEN_SEA_FIELD );
						    break;
					    case "deep":
						    _stage_manager.setEnvironmentID( i, FIELD_ENVIRONMENT.DEEP_SEA_FIELD );
						    break;
					    }
				    }
			    }

                // プレイヤーの初期化
                Vector3 pos = _file_manager.getMassCoordinate( 0 );
			    _player_manager.init( ref pos, ref _graphic_manager );

			    _network_gui_controll.setShowGUI( false );
                _graphic_manager.destroyTitleObj( );
                _scene_init = false;
		    }
        } else {
            if ( _network_manager.okStartGame( ) ) {
			    connectTitleUpdate( );
                _send_status = true;
            }
        }
	}

    private void connectTitleUpdate( ) {
        _scene = SCENE.SCENE_GAME;

        // ステージの生成
        _stage_manager.initMassCount( );
        _graphic_manager.loadMainGameGraph( );
        _graphic_manager.createBackGroundObj( );
        GameObject player_manager_obj = _player_manager.transform.gameObject;
        _graphic_manager.createPlayerObj( player_manager_obj.transform );
		//マスの生成
		_stage_manager.init( ref _graphic_manager, ref _particle_manager );
		for( int i = 0; i < _file_manager.getMassCount( ); i++ ) {
			int num = _stage_manager.getMassCount( );
			_stage_manager.massCreate( num, _file_manager.getFileData( ).mass[ num ].mass_type,
                                       _file_manager.getFileData( ).mass[ num ].event_type, _file_manager.getMassCoordinate( num ) );
			_stage_manager.increaseMassCount( );
		}

		// ステージマネージャーの環境情報の設定
		for ( int i = 0; i < _file_manager.getMassCount( ); i++ ) {
			if ( _file_manager.getEnvironment( i ) != "" ) {
				switch ( _file_manager.getEnvironment( i ) ) {
				case "shallows":
					_stage_manager.setEnvironmentID( i, FIELD_ENVIRONMENT.SHOAL_FIELD );
					break;
				case "shoal":
					_stage_manager.setEnvironmentID( i, FIELD_ENVIRONMENT.OPEN_SEA_FIELD );
					break;
				case "deep":
					_stage_manager.setEnvironmentID( i, FIELD_ENVIRONMENT.DEEP_SEA_FIELD );
					break;
				}
			}
		}

        // プレイヤーの初期化
        Vector3 pos = _file_manager.getMassCoordinate( 0 );
		_player_manager.init( ref pos, ref _graphic_manager );

		try {
			_network_manager.changeScene( _scene );
		} catch {
			Debug.Log( "通信に失敗しまいました" );
		}
        _graphic_manager.destroyTitleObj( );
		_network_gui_controll.setShowGUI( false );
        _scene_init = false;
    }

	/// <summary>
	/// FinishSceneの更新
	/// </summary>
	private void updateFinishScene( ) {
        if ( !_scene_init ) {
            _graphic_manager.createTitle( );
            _scene_init = true;
        }
        _connect_wait_time++;

        if ( _connect_wait_time >= CONNECT_WAIT_TIME ) {
            if ( _mode == PROGRAM_MODE.MODE_NO_CONNECT ) {
		        if ( Input.GetKeyDown( KeyCode.A ) ) {
			        _scene = SCENE.SCENE_TITLE;
                    _scene_init = false;
		        }
            } else {
		        if ( _network_manager.isReady( ) ) {
			        _scene = SCENE.SCENE_TITLE;
				    try {
					    _network_manager.changeScene( _scene );
				    } catch {
					    Debug.Log( "通信に失敗しまいました" );
				    }
                    _scene_init = false;
		        }
            }
            _connect_wait_time = 0;
        }
	}

	/// <summary>
	/// GameSceneの更新
	/// </summary>
	private void updateGameScene( ) {
        if ( !_scene_init ) {
            _game_playing = true;
            for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
                _network_manager.setMassCount( i, 0 );
            }

            _scene_init = true;
        }

        if ( _game_playing && !_go_finish_scene ) {
		    // フェイズごとの更新
		    switch( _phase_manager.getMainGamePhase( ) ) {
                case MAIN_GAME_PHASE.GAME_PHASE_NO_PLAY:
                    updateNoPlayPhase( );
                    break;
		        case MAIN_GAME_PHASE.GAME_PHASE_DICE:
			        updateDicePhase( );
			        break;
		        case MAIN_GAME_PHASE.GAME_PHASE_MOVE_CHARACTER:
			        updateMovePhase( );
			        break;
		        case MAIN_GAME_PHASE.GAME_PHASE_DRAW_CARD:
			        updateDrawPhase( );
			        break;
		        case MAIN_GAME_PHASE.GAME_PHASE_BATTLE:
			        updateButtlePhase( );
			        break;
		        case MAIN_GAME_PHASE.GAME_PHASE_RESULT:
			        updateResultPhase( );
			        break;
		        case MAIN_GAME_PHASE.GAME_PHASE_EVENT:
			        updateEventPhase( );
			        break;
		        case MAIN_GAME_PHASE.GAME_PHASE_FINISH:
			        updateFinishPhase( );
			        break;
		    }
        }

		// 通信データのセット
		if ( _phase_manager.isPhaseChanged( ) && _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
            _phase_init = false;
            _network_manager.changePhase( _phase_manager.getMainGamePhase( ) );
		}

        // プレイヤーのモーションを更新
        _player_manager.setPlayerMotion( );

        int[ ] count = getResideCount( );
        _player_manager.dicisionTopAndLowestPlayer( ref count );

        // カメラの位置更新
		_camera_manager.moveCameraPos( _player_manager.getTopPlayer( PLAYER_RANK.RANK_FIRST ).obj, _player_manager.getLastPlayer( ).obj );
		// カメラの障害物を消す
		_stage_manager.refreshRendBackObj( );
		_camera_manager.pointToRay( );


		int num = _player_manager.getTopPlayer( PLAYER_RANK.RANK_FIRST ).advance_count;
		switch ( _file_manager.getEnvironment( num ) ) {
		case "shallows":
			_stage_manager.setEnvironment( FIELD_ENVIRONMENT.SHOAL_FIELD );
			break;
		case "shoal":
			_stage_manager.setEnvironment( FIELD_ENVIRONMENT.OPEN_SEA_FIELD );
			break;
		case "deep":
			_stage_manager.setEnvironment( FIELD_ENVIRONMENT.DEEP_SEA_FIELD );
			break;
		}

        // ライトの更新
		_stage_manager.updateLightColor( _stage_manager.getEnvironment( ), num );

        // 泡パーティクルの更新
        _stage_manager.updateBubble( );

        if ( _send_status ) {
            _network_manager.setPlayerStatus( true );

            _send_status = false;
        }

        // タイトルへ戻るが送られて来たらタイトルへ
        if ( _game_playing ) {
            if ( _network_manager.isGoTitle( ) ) {
                goTitle( );
            }
        }

        // ゲーム終了時の処理
        if ( !_game_playing ) {
            _connect_wait_time++;
            if ( _connect_wait_time >= CONNECT_WAIT_TIME ) {
                _connect_wait_time = 0;
                _scene = SCENE.SCENE_TITLE;
                _graphic_manager.destroyMainGameObj( );
                if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
			        try {
				        _network_manager.changeScene( _scene );
			        } catch {
				        Debug.Log( "通信に失敗しまいました" );
			        }
                } else {
			        
                }
            }
        }

        if ( _go_finish_scene ) {
            _connect_wait_time++;
            if ( _connect_wait_time >= CONNECT_WAIT_TIME ) {
                _connect_wait_time = 0;
                _scene_init = false;
                _graphic_manager.destroyMainGameObj( );
		        _scene = SCENE.SCENE_FINISH;
		        try {
			        _network_manager.changeScene( _scene );
		        } catch {
			        Debug.Log( "通信に失敗しまいました" );
		        }
                _go_finish_scene = false;
            }
        }
	}

    /// <summary>
    /// タイトルへ戻るが送られて来たらタイトルへ
    /// </summary>
    private void goTitle( ) {
        _connect_wait_time++;
        if ( _connect_wait_time >= CONNECT_WAIT_TIME ) {
            _phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_NO_PLAY, "NoPlay" );
            _connect_wait_time = 0;
            _phase_init = false;
            _game_playing = false;
            _network_manager.setGameFinish( true );
        }
    }

	/// <summary>
	/// NoPlayPhaseの更新
	/// </summary>
	private void updateNoPlayPhase( ) {
        // サイコロフェイズへの移行
		StartCoroutine( "gameStart" );
        _send_status = true;
        if ( _phase_manager.isFinishMovePhaseImage( ) == false ) {
			_phase_manager.movePhaseImage( );
		} else {
            if ( !_immediately_goal ) {
                _phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_DICE, "DicePhase" );
                _phase_manager.deletePhaseImage( );
                _phase_manager.createPhaseText( MAIN_GAME_PHASE.GAME_PHASE_DICE );
            } else {
                _phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_FINISH, "FinishPhase" );
                _phase_manager.deletePhaseImage( );
                _phase_manager.createPhaseText( MAIN_GAME_PHASE.GAME_PHASE_FINISH );
            }
        }
	}
    
    private IEnumerator gameStart( ) {
        yield return new WaitForSeconds( 3.0f );
    }

	/// <summary>
	/// DicePhaseの更新
	/// </summary>
	private void updateDicePhase( ) {
		if ( _phase_manager.isFinishMovePhaseImage( ) == false ) {
			_phase_manager.movePhaseImage( );
		} else {
			_phase_manager.setPhaseImagePos( );
		}

		if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
            // 送られてきた賽の目の数
            int[ ] dice_value = new int[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
            dice_value[ 0 ] = _network_manager.getClientData( 0 ).dice_value;
            dice_value[ 1 ] = _network_manager.getClientData( 1 ).dice_value;
		    // ダイスを振ったら(通信)
		    if ( dice_value[ 0 ] > 0 && dice_value[ 1 ] > 0  ) {
                _dice_value[ 0 ] = dice_value[ 0 ];
                _dice_value[ 1 ] = dice_value[ 1 ];
                // uiの生成
                _graphic_manager.createPlayerLabel( 0, _dice_value[ 0 ] );
                _graphic_manager.createPlayerLabel( 1, _dice_value[ 1 ] );
                // キャラクター移動フェイズへの移行
                _phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_MOVE_CHARACTER, "MovePhase" );
				_phase_manager.deletePhaseImage( );
                _phase_init = false;
            }
		} else if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
            // 送られてきた賽の目の数
            int[ ] dice_value = new int[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
            dice_value[ 0 ] = _network_manager.getClientData( 0 ).dice_value;
		    // ダイスを振ったら(通信)
		    if ( dice_value[ 0 ] > 0 ) {
                _dice_value[ 0 ] = dice_value[ 0 ];
                _dice_value[ 1 ] = ( int )Random.Range( 1.0f, 3.0f );
                // uiの生成
                _graphic_manager.createPlayerLabel( 0, _dice_value[ 0 ] );
                _graphic_manager.createPlayerLabel( 1, _dice_value[ 1 ] );
                // キャラクター移動フェイズへの移行
				_phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_MOVE_CHARACTER, "MovePhase" );
				_phase_manager.deletePhaseImage( );
                _phase_init = false;
            }
		} else if ( _mode == PROGRAM_MODE.MODE_NO_CONNECT ) {
			if ( Input.GetKeyDown( KeyCode.A ) ) {
				// 賽の目の数
				int[ ] dice_value = new int[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
				for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
					dice_value[ i ] = _debug_dice_value;//( int )Random.Range( 1.0f, 4.0f );
					_dice_value[ i ] = dice_value[ i ];
					// uiの生成
					if ( _dice_value[ i ] <= 3 ) {
						_graphic_manager.createPlayerLabel( i, _dice_value[ i ] );
					}
				}
				// キャラクター移動フェイズへの移行
				_phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_MOVE_CHARACTER, "MovePhase" );
				_phase_manager.deletePhaseImage( );
                _phase_init = false;
			}
		}
	}

	/// <summary>
	/// MovePhaseの更新
	/// </summary>
	private void updateMovePhase( ) {
        if ( !_phase_init ) {
            // 行動順1Pをに設定する
            _player_manager.startPlayerOrder( );

            _phase_init = true;
        }

		if ( ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT || _mode == PROGRAM_MODE.MODE_NO_CONNECT ) &&
             _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER ) {
            if ( !_player_manager.isMoveStart( ) ) {
                // プレイヤーを動かす
				if ( _player_manager.getPlayerOnMove( ) ) {
                    if ( _debug_mode ) {
                        _player_manager.setLimitValue( _debug_dice_value );
                    } else {
					    _player_manager.setLimitValue( _dice_value[ ( int )_player_manager.getPlayerOrder( ) ] );
                    }
					_player_manager.setAdvanceFlag( true );
				} else {
					_player_manager.setPlayerOnMove( true );
				}
				_event_count[ ( int )_player_manager.getPlayerOrder( ) ] = 0;
			} 
            if ( _player_manager.isMoveFinish( ) ) {
                // プレイヤーを変える
				_player_manager.changePlayerOrder( );
            }
		} else if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT &&
                    _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER ) {
            if ( !_player_manager.isMoveStart( ) ) {
                // プレイヤーを動かす
				if ( _player_manager.getPlayerOnMove( ) ) {
                    if ( _debug_mode ) {
                        _player_manager.setLimitValue( _debug_dice_value );
                    } else {
					    _player_manager.setLimitValue( _dice_value[ ( int )_player_manager.getPlayerOrder( ) ] );
                    }
					_player_manager.setAdvanceFlag( true );
				} else {
					_player_manager.setPlayerOnMove( true );
				}
				_event_count[ ( int )_player_manager.getPlayerOrder( ) ] = 0;
			} 
            if ( _player_manager.isMoveFinish( ) ) {
                // プレイヤーを変える
				_player_manager.changePlayerOrder( );
            }
		}

        // プレイヤーの移動
        int[ ] reside_count = getResideCount( );
		_player_manager.movePhaseUpdate( ref reside_count,
            _stage_manager.getTargetMassPos( _player_manager.getTargetMassID( _stage_manager.getMassCount( ) ) ) );
        
        // 現在のマスをクライアントに送信
        if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
            if ( _player_manager.isChangeCount( PLAYER_ORDER.PLAYER_ONE ) ) {
                _network_manager.setMassCount( 0, _player_manager.getPlayerCount( 0, _stage_manager.getMassCount( ) ) );
            }
        } else if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
            if ( _player_manager.isChangeCount( PLAYER_ORDER.PLAYER_ONE ) ) {
                _network_manager.setMassCount( 0, _player_manager.getPlayerCount( 0, _stage_manager.getMassCount( ) ) );
            }
            if ( _player_manager.isChangeCount( PLAYER_ORDER.PLAYER_TWO ) ) {
                _network_manager.setMassCount( 1, _player_manager.getPlayerCount( 1, _stage_manager.getMassCount( ) ) );
            }
        }
       

        // プレイヤーの順番を更新
        _player_manager.updatePlayerOrder( );


        // 両方の移動が終わったら次のフェイズへ
        if ( _player_manager.isAllPlayerMoveFinish( ) ) {
            _player_manager.allMovedRefresh( );
            _phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_DRAW_CARD, "DrawPhase" );
            _phase_init = false;
        } 
	}

	/// <summary>
	/// DrawPhaseの更新
	/// </summary>
	private void updateDrawPhase( ) {
        if ( !_phase_init ) {
            _graphic_manager.createDrawPhaseUI( );
            // 行動順1Pをに設定する
            _player_manager.startPlayerOrder( );

            _phase_init = true;
        }
        
        int[ ] distribute_num = new int[ _dice_value.Length ];
        for ( int i = 0; i < _dice_value.Length; i++ ) {
            distribute_num[ i ] = MAX_DRAW_VALUE - _dice_value[ i ];
        }
        // カードをプレイヤーに配布
        if ( _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER ) {
            _graphic_manager.moveDrawCardUI( ( int )_player_manager.getPlayerOrder( ), distribute_num );
            // 配るプレイヤーを切り替える
            if ( _graphic_manager.isFinishDrawCardMove( ( int )_player_manager.getPlayerOrder( ) ) ) {
                _player_manager.changePlayerOrder( );
            }
        }

        // プレイヤーの順番を更新
        _player_manager.updatePlayerOrder( );

        List< int > card_list = new List< int >( );
		if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
            // ドローアニメーションが終了したら
            if ( _graphic_manager.isFinishDrawCardMove( ) ) {
                for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
                    // プレイヤーにカード配布
			        if ( !_network_manager.isSendCard( i ) ) {
				        for ( int j = 0; j < MAX_DRAW_VALUE - _dice_value[ i ]; j++ ) {
			                // デッキのカード数が０になったらリフレッシュ
			                if ( _card_manager.getDeckCardNum( ) <= 0 ) {
				                _card_manager.createDeck( );
			                }
                            card_list.Add( _card_manager.distributeCard( ).id );
				        }
                        _network_manager.refreshCard( i );
                        _network_manager.setCardList( i, card_list );
                        // カードリストを初期化
                        card_list.Clear( );
                    }
                }
            }

            // 両方の準備が終わったら次のフェイズへ
			if ( _network_manager.isReady( ) ) {
				if ( _connect_wait_time >= CONNECT_WAIT_TIME && !_refresh_card_list ) {
                    for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
                        _network_manager.refreshCard( i );
                    }
					_refresh_card_list = true;
				}
				_connect_wait_time++;
				if ( _connect_wait_time >= SECOND_CONNECT_WAIT_TIME ) {
					_phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_BATTLE, "BattlePhase" );
                    _phase_init = false;
					_connect_wait_time = 0;
					_refresh_card_list = false;
                    _send_status = true;
				}
            }
		} else if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
            // ドローアニメーションが終了したら
            if ( _graphic_manager.isFinishDrawCardMove( ) ) {
                // 1Pにカード配布
			    if ( !_network_manager.isSendCard( 0 ) ) {
				    for ( int i = 0; i < MAX_DRAW_VALUE - _dice_value[ 0 ]; i++ ) {
			            // デッキのカード数が０になったらリフレッシュ
			            if ( _card_manager.getDeckCardNum( ) <= 0 ) {
				            _card_manager.createDeck( );
			            }
                        card_list.Add( _card_manager.distributeCard( ).id );
		            }
                    _network_manager.refreshCard( 0 );
                    _network_manager.setCardList( 0, card_list );
                }
            }

            //Debug.Log( _client_data[ 0 ].getRecvData( ).ready );
            // 準備が終わったら次のフェイズへ
			if ( _network_manager.isReady( ) ) {
				if ( _connect_wait_time >= CONNECT_WAIT_TIME && !_refresh_card_list ) {
                    try {
					    _network_manager.refreshCard( 0 );
					    _refresh_card_list = true;
                    }
                    catch {
                        Debug.Log( "Failure Refresh CardList..." );
                    }
				}
				_connect_wait_time++;
				if ( _connect_wait_time >= SECOND_CONNECT_WAIT_TIME ) {
                    try {
					    _phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_BATTLE, "BattlePhase" );
                        _phase_init = false;
					    _connect_wait_time = 0;
					    _refresh_card_list = false;
                        _send_status = true;
                    }
                    catch {
                        Debug.Log( "Failure ChangePhase" );
                    }
				}
            }
		} else if ( _mode == PROGRAM_MODE.MODE_NO_CONNECT ) {
			// 準備が終わったら次のフェイズへ
			if ( Input.GetKeyDown( KeyCode.A ) ) {
				_phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_BATTLE, "BattlePhase" );
                _phase_init = false;
			}
		}
	}

	/// <summary>
	/// ButtlePhaseの更新
	/// </summary>
	private void updateButtlePhase( ) {
        if ( !_phase_init ) {
            _graphic_manager.destroyDrawPhaseUI( );
            if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
                    _network_manager.refreshCard( i );
                }
            }
            // ラベルUIの削除
            _graphic_manager.destroyAllPlayerLabels( );
            _player_manager.setDefalutStatus( );
            _phase_init = true;
        }
		if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
            if ( _network_manager.isBattleReady( ) )  {
				//バトルUIを作成する
				if ( _go_result_ui == null ) { 
					createResultUI( );
					// プレイヤーのステータスを設定
                    for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
					    for ( int j = 0; j < _network_manager.getClientData( i ).used_card_list.Length; j++ ) {
						    _player_manager.adaptaCard( 0, _card_manager.getCardData( _network_manager.getClientData( i ).used_card_list[ j ] ) );
					    }
					    _player_manager.endStatus( i );
					    Debug.Log( ( i + 1 ) + "Pのpower:" + _player_manager.getPlayerPower( )[ i ].ToString( ) );
                    }
					// プラスバリューの初期化
					_player_manager.allPlusValueInit( );

					// 攻撃力を比較
					_player_manager.attackTopAndLowestPlayer( _player_manager.getPlayerPower( ) );

					_result_UI_maneger.setCoroutine( 1, _player_manager.getPlayerResult( 0 ), _player_manager.getPlayerResult( 1 ) );
				}
				//UIの削除
                // 次のフェイズへ
                //_phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_RESULT, "ResultPhase" );
                //_phase_init = false;
            }
		} else if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
			if ( _network_manager.isBattleReady( ) )  {
				//バトルUIを作成する
				if ( _go_result_ui == null ) { 
					createResultUI( );
					// プレイヤーのステータスを設定
                    for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
					    for ( int j = 0; j < _network_manager.getClientData( 0 ).used_card_list.Length; j++ ) {
						    _player_manager.adaptaCard( 0, _card_manager.getCardData( _network_manager.getClientData( 0 ).used_card_list[ j ] ) );
					    }
					    _player_manager.endStatus( i );
					    Debug.Log( ( i + 1 ) + "Pのpower:" + _player_manager.getPlayerPower( )[ i ].ToString( ) );
                    }
					// プラスバリューの初期化
					_player_manager.allPlusValueInit( );

					// 攻撃力を比較
					int[ ] attack = new int[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
					for ( int i = 0; i < attack.Length; i++ ) {
						attack[ i ] = ( int )Random.Range( 10, 20 );
					}
					_player_manager.attackTopAndLowestPlayer( attack );
				}
				_result_UI_maneger.setCoroutine( 1, _player_manager.getPlayerResult( 0 ), _player_manager.getPlayerResult( 1 ) );
                // 次のフェイズへ
                //_phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_RESULT, "ResultPhase" );
                //_phase_init = false;
            }
		} else if ( _mode == PROGRAM_MODE.MODE_NO_CONNECT ) {
			if ( Input.GetKeyDown( KeyCode.A ) )  {
				//バトルUIを作成する
				if ( _go_result_ui == null ) { 
					createResultUI( );
				}
				// 次のフェイズへ
				BATTLE_RESULT[ ] result = new BATTLE_RESULT[ 2 ]{ BATTLE_RESULT.WIN, BATTLE_RESULT.LOSE };
				_result_UI_maneger.setCoroutine( 1, result[ 0 ], result[ 1 ] );
				//_phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_RESULT, "ResultPhase" );
                //_phase_init = false;
			}
		}



		if ( _result_UI_maneger != null && _result_UI_maneger.isEndResult( ) ) {
			_connect_wait_time++;
			if ( _connect_wait_time >= CONNECT_WAIT_TIME ) {
				_connect_wait_time = 0;
				_result_UI_maneger.destroyObj( );
				_phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_RESULT, "ResultPhase" );
				_phase_init = false;
			}
		}
	}

	public void setbattleFlag( bool battle ) {
		_battle = battle;
	}

	/// <summary>
	/// ResultPhaseの更新
	/// </summary>
	private void updateResultPhase( ) {
        // 初期化
        if ( !_phase_init ) {
            // 行動順1Pをに設定する
            _player_manager.setDefalutStatus( );
            _player_manager.startPlayerOrder( );
			_go_result_ui = null;
            _phase_init = true;
        }

        if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
            // 戦闘結果を送信
            if ( !_network_manager.isSendResult( ) ) {
                _connect_wait_time++;
                if ( _connect_wait_time > CONNECT_WAIT_TIME ) {
                    _connect_wait_time = 0;
                    BATTLE_RESULT[ ] result = new BATTLE_RESULT[ ]{ _player_manager.getPlayerResult( 0 ), _player_manager.getPlayerResult( 1 ) };
                    _network_manager.setResult( result, true );
                }
            }
        }

		if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
            if ( _network_manager.isReady( ) &&
                 _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER ) {
                if ( _network_manager.getClientData( ( int )_player_manager.getPlayerOrder( ) ).mass_adjust == MASS_ADJUST.ADVANCE &&
                     !_player_manager.isMoveStart( ) ) {
                    // Pを前に動かす
		            _player_manager.setLimitValue( 1 );
		            _player_manager.setAdvanceFlag( true );
					_event_count[ ( int )_player_manager.getPlayerOrder( ) ] = 0;
                } else if ( _network_manager.getClientData( ( int )_player_manager.getPlayerOrder( ) ).mass_adjust == MASS_ADJUST.BACK &&
                            !_player_manager.isMoveStart( ) ) {
                    // Pを後ろに動かす
		            _player_manager.setLimitValue( 1 );
		            _player_manager.setAdvanceFlag( false );
					_event_count[ ( int )_player_manager.getPlayerOrder( ) ] = 0;
                } else if ( _network_manager.getClientData( ( int )_player_manager.getPlayerOrder( ) ).mass_adjust == MASS_ADJUST.NO_ADJUST &&
                            !_player_manager.isMoveStart( ) ) {
                    // プレイヤーを変える
		            _player_manager.setLimitValue( 0 );
				    _player_manager.changePlayerOrder( );
                } else if ( _player_manager.isMoveFinish( ) ) {
                    // プレイヤーを変える
				    _player_manager.changePlayerOrder( );
                }
            }
		} else if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
            if ( _network_manager.isReady( ) &&
                 _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER )  {
                if ( _network_manager.getClientData( ( int )_player_manager.getPlayerOrder( ) ).mass_adjust == MASS_ADJUST.ADVANCE &&
                     !_player_manager.isMoveStart( ) ) {
                    // 1Pを前に動かす
		            _player_manager.setLimitValue( 1 );
		            _player_manager.setAdvanceFlag( true );
					_event_count[ ( int )_player_manager.getPlayerOrder( ) ] = 0;
                } else if ( _network_manager.getClientData( ( int )_player_manager.getPlayerOrder( ) ).mass_adjust == MASS_ADJUST.BACK &&
                            !_player_manager.isMoveStart( ) ) {
                    // 1Pを後ろに動かす
		            _player_manager.setLimitValue( 1 );
		            _player_manager.setAdvanceFlag( false );
					_event_count[ ( int )_player_manager.getPlayerOrder( ) ] = 0;
                } else if ( _network_manager.getClientData( ( int )_player_manager.getPlayerOrder( ) ).mass_adjust == MASS_ADJUST.NO_ADJUST &&
                            !_player_manager.isMoveStart( ) ) {
                    // プレイヤーを変える
		            _player_manager.setLimitValue( 0 );
				    _player_manager.changePlayerOrder( );
                } else if ( _player_manager.isMoveFinish( ) ) {
                    // プレイヤーを変える
				    _player_manager.changePlayerOrder( );
                }
            }
		} else if ( _mode == PROGRAM_MODE.MODE_NO_CONNECT &&
                    _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER ) {
			if ( Input.GetKeyDown( KeyCode.A ) ) {
				MASS_ADJUST[ ] adjust = new MASS_ADJUST[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
				for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
					adjust[ i ] = ( MASS_ADJUST )( ( int )Random.Range( 0.0f, 3.0f ) );
				}
                
                if ( adjust[ ( int )_player_manager.getPlayerOrder( ) ] == MASS_ADJUST.ADVANCE &&
                     !_player_manager.isMoveStart( ) ) {
                    // 1Pを前に動かす
		            _player_manager.setLimitValue( 1 );
		            _player_manager.setAdvanceFlag( true );
					_event_count[ ( int )_player_manager.getPlayerOrder( ) ] = 0;
                } else if ( adjust[ ( int )_player_manager.getPlayerOrder( ) ] == MASS_ADJUST.BACK &&
                            !_player_manager.isMoveStart( ) ) {
                    // 1Pを後ろに動かす
		            _player_manager.setLimitValue( 1 );
		            _player_manager.setAdvanceFlag( false );
					_event_count[ ( int )_player_manager.getPlayerOrder( ) ] = 0;
                } else if ( adjust[ ( int )_player_manager.getPlayerOrder( ) ] == MASS_ADJUST.NO_ADJUST &&
                            !_player_manager.isMoveStart( ) ) {
                    // プレイヤーを変える
		            _player_manager.setLimitValue( 0 );
				    _player_manager.changePlayerOrder( );
                } else if ( _player_manager.isMoveFinish( ) ) {
                    // プレイヤーを変える
				    _player_manager.changePlayerOrder( );
                }
			}
		}

        // プレイヤーの移動
        int[ ] num = getResideCount( );
        _player_manager.movePhaseUpdate( ref num, _stage_manager.getTargetMassPos( _player_manager.getTargetMassID( _stage_manager.getMassCount( ) ) ) );

        // プレイヤーの順番を更新
        _player_manager.updatePlayerOrder( );

        _connect_wait_time++;

        // 両方の移動が終わったら次のフェイズへ
        if ( _player_manager.isAllPlayerMoveFinish( ) == true && _connect_wait_time >= CONNECT_WAIT_TIME ) {
            _connect_wait_time = 0;
            _phase_init = false;
			if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                BATTLE_RESULT[ ] result = new BATTLE_RESULT[ ]{ BATTLE_RESULT.BATTLE_RESULT_NONE, BATTLE_RESULT.BATTLE_RESULT_NONE };
                _network_manager.setResult( result, false );
			}
            
            _phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_EVENT, "EventPhase" );
            _player_manager.allMovedRefresh( );

            _network_manager.setPlayerStatus( false );
        }
	}

	/// <summary>
	/// EventPhaseの更新
	/// </summary>
	private void updateEventPhase( ) {
        if ( !_phase_init ) {
            
            if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
                    _network_manager.refreshCard( i );
                }
            }
            // 行動順1Pに設定する
            _player_manager.startPlayerOrder( );

            _phase_init = true;
        }

        // 動作しているパーティクルの確保
        List< int > _particle_list;
        // ゴール処理
        if ( _player_manager.isAllPlayerEventFinish( ) && 
             _event_manager.isGoal( ) && _connect_wait_time >= CONNECT_WAIT_TIME ) {
            _event_manager.setNotGoal( );
            _connect_wait_time = 0;
			_player_manager.eventRefresh( );
            _player_manager.allMovedRefresh( );
            _phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_FINISH, "FinishPhase" );
            _phase_manager.createPhaseText( MAIN_GAME_PHASE.GAME_PHASE_FINISH );
            return;
        }

        // 通信待機時間の更新
        _connect_wait_time++;
        
        int id         = ( int )_player_manager.getPlayerOrder( );
        int mass_count = _player_manager.getPlayerCount( id, _stage_manager.getMassCount( ) );

		if ( _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER && !_player_manager.isEventFinish( ) && !_player_manager.isMoveStart( ) ) {
            // イベント開始時の処理
            if ( !_player_manager.isEventStart( ) ) {
                _event_manager.startEvent( id, _file_manager.getFileData( ).mass[ mass_count ].event_type,
                                           _file_manager.getMassValue( mass_count ) );
            }
            _event_manager.setResideCount( getResideCount( ) );
            // イベント処理
            if ( _player_manager.getEventType( ) != EVENT_TYPE.EVENT_WORP && 
                 _player_manager.getEventType( ) != EVENT_TYPE.EVENT_CHANGE ) {
                _event_manager.massEvent( mass_count );
			} else {
                mass_count = _before_player_count;
                _event_manager.massEvent( mass_count );
            }
            
			_before_player_count = _player_manager.getPlayerCount( id, _stage_manager.getMassCount( ) );
		} 
       
        // プレイヤーの移動
        int[ ] num = getResideCount( );
        _player_manager.movePhaseUpdate( ref num, _stage_manager.getTargetMassPos( _player_manager.getTargetMassID( _stage_manager.getMassCount( ) ) ) );

        // イベント終了処理
        if ( _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER ) {
            if ( _player_manager.isEventStart( ) && _player_manager.isEventFinish( ) ) {
                // 行動プレイヤーを変える
                _player_manager.changePlayerOrder( );
				_player_manager.setEventType( ( int )_player_manager.getPlayerOrder( ), EVENT_TYPE.EVENT_NONE );
		    }
        }

		// マス移動終了時にイベントフラグをfalseにしてもう一度イベントが発生するようにする
        if ( _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER ) {
            if ( _player_manager.getEventType( ) == EVENT_TYPE.EVENT_MOVE     ||
                 _player_manager.getEventType( ) == EVENT_TYPE.EVENT_TRAP_ONE ||
                 _player_manager.getEventType( ) == EVENT_TYPE.EVENT_TRAP_TWO ) {
			    if ( _player_manager.isMoveFinish( ) ) {
                    _player_manager.setEventType( ( int )_player_manager.getPlayerOrder( ), EVENT_TYPE.EVENT_NONE );
                    _player_manager.movedRefresh( );
					if ( _event_count[ ( int )_player_manager.getPlayerOrder( ) ] < MAX_EVENT_NUM - 1 ) {
						// イベント開始＆移動状態を初期化
						_player_manager.setEventStart( false );
						_event_count[ ( int )_player_manager.getPlayerOrder( ) ]++;
					} else {
						_player_manager.setEventFinish( true );
						_event_count[ ( int )_player_manager.getPlayerOrder( ) ] = 0;
					}
			    }
            }
        }

        // パーティクルの更新
        _particle_manager.particleUpdate( );
        _particle_manager.finishParticle( );
        
        // 動作しているパーティクルの確保
        _particle_list = _particle_manager.getParticleNumsForType( PARTICLE_TYPE.PARTICLE_SPIRAL );
        int[ ] _spiral_array = new int[ _particle_list.Count ];
        for ( int i = 0; i < _particle_list.Count; i++ ) {
            _spiral_array[ i ] = _particle_list[ i ];
        }


        if ( _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER &&
			( _player_manager.getEventType( ) == EVENT_TYPE.EVENT_CHANGE ||
		      _player_manager.getEventType( ) == EVENT_TYPE.EVENT_WORP ) ) {
            if( _particle_manager.isFinshParticle( _spiral_array ) ) {
                if( _player_manager.getEventType( ) == EVENT_TYPE.EVENT_CHANGE ) {
                    _player_manager.setEventAllFinish( true );
                } else {
                    _player_manager.setEventFinish( true );
                }
                _particle_list.Clear( );
			    _player_manager.setEventType( ( int )_player_manager.getPlayerOrder( ), EVENT_TYPE.EVENT_NONE );
            }
        }

        // プレイヤーの順番を更新
        _player_manager.updatePlayerOrder( );
        int player_one = ( int )PLAYER_ORDER.PLAYER_ONE;

        // イベント終了時の処理
		if ( _player_manager.isAllPlayerEventFinish( ) &&
             !_event_manager.isGoal( ) && _connect_wait_time >= CONNECT_WAIT_TIME ) {
            // カードドロー完了したら
            if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
                if ( !_network_manager.getClientData( 0 ).ok_event &&
                     _event_type[ player_one ] == EVENT_TYPE.EVENT_DRAW ) {
                    return;
                }
                if ( !_network_manager.getClientData( 0 ).ok_event &&
                     _event_type[ player_one ] == EVENT_TYPE.EVENT_DISCARD ) {
                    return;
                }
            } else if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
                for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
                    if ( !_network_manager.getClientData( i ).ok_event &&
                         _event_type[ i ] == EVENT_TYPE.EVENT_DRAW ) {
                        return;
                    }
                    if ( _network_manager.getClientData( i ).ok_event &&
                         _event_type[ i ] == EVENT_TYPE.EVENT_DISCARD ) {
                        return;
                    }
                }
            }

            // 各値を初期化
            _connect_wait_time = 0;
			_player_manager.eventRefresh( );
            _player_manager.allMovedRefresh( );

			for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
				_event_type[ i ] = EVENT_TYPE.EVENT_NONE;
				_player_manager.setEventType( i, _event_type[ i ] );
			}

            if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
                for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
                    if ( _network_manager.getClientData( i ).ok_event ) {
                        _network_manager.setEventType( i, _event_type[ i ] );
                        _network_manager.refreshCard( i );
                    }
                }
            } else if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
                if ( _network_manager.getClientData( player_one ).ok_event ) {
                    _network_manager.setEventType( player_one, _event_type[ player_one ] );
                    _network_manager.refreshCard( player_one );
                }
            }
            _player_manager.refreshPlayerResult( );
			_phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_DICE, "DisePhase" );
			_phase_manager.createPhaseText( MAIN_GAME_PHASE.GAME_PHASE_DICE );
		}

	}

    /// <summary>
    /// FinishPhaseの更新
    /// </summary>
    private void updateFinishPhase( ) {
        int create_num = 0;
        List< int > particle_list = new List< int >( );
        // 初期化処理    
		if ( !_phase_init ) {
			_connect_wait_time = 0;
			_phase_init = true;
		}

		if ( _goal_time < GOAL_WAIT_TIME ) {
            // パーティするを作動させる
			_phase_manager.setGoalImagePos( );
            // パーティクルの生成
            int rand = Random.Range( 0, 2 );
            PARTICLE_TYPE type = ( rand == 1 ) ? PARTICLE_TYPE.PARTICLE_FIREWORKS1 : PARTICLE_TYPE.PARTICLE_FIREWORKS2;
            create_num = _particle_manager.getParticlesForType( type ).Length;
            if( _particle_manager.isCretateFireTiming( ) ){
			    if( create_num < _particle_manager.getLimitCreateNum( type ) ) {
			        _particle_manager.createParticle( type );
			    }
            }
			//_particle_manager.setParticleType( PARTICLE_TYPE.PARTICLE_FIREWORKS );
			_particle_manager.particleUpdate( );
            _particle_manager.finishParticle( );
			_goal_time++;
		} else if ( _goal_time >= GOAL_WAIT_TIME || Input.GetKey( KeyCode.A ) ) {
			if ( _mode == PROGRAM_MODE.MODE_NO_CONNECT ) {
				if ( Input.GetKeyDown( KeyCode.A ) ) {
					_goal_time = 0;
					_phase_manager.deletePhaseImage( );
					_scene = SCENE.SCENE_FINISH;
					_graphic_manager.destroyMainGameObj( );
					_phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_NO_PLAY, "NoPlay" );
					_phase_init = false;
					_scene_init = false;
					_particle_manager.particleTypeDelete( PARTICLE_TYPE.PARTICLE_FIREWORKS1 );
                    _particle_manager.particleTypeDelete( PARTICLE_TYPE.PARTICLE_FIREWORKS2 );
				}
			} else {
				if ( _network_manager.isFinishGame( ) ) {
					_phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_NO_PLAY, "NoPlay" );
					_phase_init        = false;
					_connect_wait_time = 0;
					_go_finish_scene   = true;
					_goal_time         = 0;
					_phase_manager.deletePhaseImage( );
					_particle_manager.particleTypeDelete( PARTICLE_TYPE.PARTICLE_FIREWORKS1 );
                    _particle_manager.particleTypeDelete( PARTICLE_TYPE.PARTICLE_FIREWORKS2 );
				}
			}
			//_particle_manager.deleteParticle( );
		}
		//_particle_manager.enableParticle( );
	}

	public void OnGUI( ) {
		if ( _scene == SCENE.SCENE_CONNECT ) {
			drawConnectScene( );
		}
	}

	/// <summary>
	/// ConnectSceneの描画
	/// </summary>
	private void drawConnectScene( ) {
		if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
			_network_manager.hostStateDraw( );
		}
	}

	/// <summary>
	/// シーン情報を返す
	/// </summary>
	/// <returns>The scene.</returns>
	public SCENE getScene( ) {
		return _scene;
	}
   
    /// <summary>
    /// ゴールまでどれくらい残っているか取得
    /// </summary>
    /// <returns></returns>
    public int[ ] getResideCount( ) {
		int[ ] count = new int[ 2 ];
		for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
			count[ i ] = _file_manager.getMassCount( ) - 1 - _player_manager.getPlayerCount( i, _stage_manager.getMassCount( ) );
		}
		return count;
    }

	public void setEventCount( int id, int count ) {
		_event_count[ id ] = count;
	}

	private void createResultUI( ) { 
		if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
			_go_result_ui = ( GameObject )Resources.Load( "Prefabs/ResultUI" );
			GameObject go = ( GameObject )Instantiate( _go_result_ui, new Vector3( 0, 0, 0 ), Quaternion.identity );
			_result_UI_maneger = go.GetComponent< ResultUIManeger >( );
			List< int > use_card_id = new List< int >( );

			for ( var i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
				int player_id = i;
				for ( int j = 0; j < _network_manager.getClientData( player_id ).used_card_list.Length; j++ ) {
					if ( _network_manager.getClientData( player_id ).used_card_list[ j ] > 0 ) {
						use_card_id.Add( _network_manager.getClientData( player_id ).used_card_list[ j ] );
					}
				}
				_result_UI_maneger.Init( use_card_id , player_id );
				if ( use_card_id.Count > 0 ) {
					use_card_id.Clear( );
				}
			}
		} else {
			_go_result_ui = ( GameObject )Resources.Load( "Prefabs/ResultUI" );
			GameObject go = ( GameObject )Instantiate( _go_result_ui, new Vector3( 0, 0, 0 ),Quaternion.identity );
			_result_UI_maneger = go.GetComponent< ResultUIManeger >( );
			List< int > use_card_id = new List< int >( );
			for ( var i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
				int player_id = i;
				// debug用
				for ( int j = 1; j < 4; j++ ) {
					use_card_id.Add( j );
				}
				_result_UI_maneger.Init( use_card_id , player_id );
				if ( use_card_id.Count > 0 ) {
					use_card_id.Clear( );
				}
			}
		}
	}
}
