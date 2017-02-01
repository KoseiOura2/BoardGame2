﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine.Events;

public class ApplicationManager : Manager< ApplicationManager > {
    
    private const int CONNECT_WAIT_TIME        = 120;
	private const int SECOND_CONNECT_WAIT_TIME = 180;
	private const int MAX_DRAW_VALUE           = 4;

    // パーティクル関係
    private const float OCEAN_CURRENT_STOP_TIME    = 60.0f;
    private const float OCEAN_CURRENT_DESTROY_TIME = 90.0f;
    private const float SPIRAL_TIME_ONE            = 10.0f;
    private const float SPIRAL_TIME_TWO            = 360.0f;
    private const float SPIRAL_TIME_THREE          = 362.0f;
    private const float SPIRAL_TIME_FOUR           = 480.0f;

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
	private CameraManager _camera_manager;

    [ SerializeField ]
    private NetworkGUIControll _network_gui_controll;
    [ SerializeField ]
    private HostData _host_data;
    [ SerializeField ]
    private ClientData[ ] _client_data = new ClientData[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
    
	[ SerializeField ]
	private PROGRAM_MODE _mode = PROGRAM_MODE.MODE_NO_CONNECT;
	[ SerializeField ]
	private SCENE _scene = SCENE.SCENE_CONNECT;
    private EVENT_TYPE[ ] _event_type = new EVENT_TYPE[ ]{ EVENT_TYPE.EVENT_NONE, EVENT_TYPE.EVENT_NONE };

    private GameObject _title_back_pref;
    private GameObject _title_back_obj;
    private GameObject _anim_draw_card;

    public int _debug_dice_value = 0;
    private List< int > _draw_card_list = new List< int >( );
	private int[ ] _event_count = new int[ ]{ 0, 0 };        //イベントを起こす回数 
    [ SerializeField ]
    private int[ ] _dice_value = new int[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
    private int _connect_wait_time  = 0;
    private int _anim_card_num = 0;
    private float _animation_time = 0.0f;
    private bool _game_playing      = false;
    private bool _goal_flag         = false;
	private bool _refresh_card_list = false;
    private bool _network_init      = false;
    [ SerializeField ]
    private bool _animation_running = false;
    private bool _animation_end     = false;

    private bool _scene_init  = false;
    private bool _phase_init  = false;
    [ SerializeField ]
    private bool _send_status = false;
    private int _before_player_count;
    
	[ SerializeField ]
	private GameObject _particle;
	[ SerializeField ]
	private float _particle_time = 0;

	public Text _scene_text;
	public Text[ ] _reside_text = new Text[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];    //残りマス用テキスト
	public Text[ ] _environment = new Text[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];    //環境情報用テキスト

	// Awake関数の代わり
	protected override void initialize( ) {
		init( );
	}

    void init( ) {
        if ( isError( ) ) {
            return;
        }

		referManager( );
        loadGraph( );
	}

    
    bool isError( ) {
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

	// Use this for initialization
	void Start( ) {


		referManager( );

		_card_manager.init( );
	}

	void referManager( ) {
		try {
			if ( _network_manager == null ) {
				_network_manager = GameObject.Find( "NetworkManager" ).GetComponent< NetworkMNG >( );
			}
			if ( _phase_manager == null ) {
				_phase_manager = GameObject.Find( "PhaseManager" ).GetComponent< PhaseManager >( );
			}
			if ( _card_manager == null ) {
				_card_manager = GameObject.Find( "CardManager" ).GetComponent< CardManager >( );
			}
			if ( _player_manager == null ) {
				_player_manager = GameObject.Find( "PlayerManager" ).GetComponent< PlayerManager >( );
			}
			if ( _stage_manager == null ) {
				_stage_manager = GameObject.Find( "StageManager" ).GetComponent< StageManager >( );
			}
			if ( _camera_manager == null ) {
				_camera_manager = Camera.main.GetComponent< CameraManager >( );
			}
			_network_gui_controll = GameObject.Find( "NetworkManager" ).GetComponent< NetworkGUIControll >( );
		}
		catch {
			Debug.Log( "参照に失敗しました。" );
		}
	}

    void loadGraph( ) {
        _title_back_pref = Resources.Load< GameObject >( "Prefabs/BackGroundObj/TitleBack" );
    }
	
	// Update is called once per frame
	void FixedUpdate( ) {

        if ( _network_manager != null && !_network_init ) {
            _network_manager.setProgramMode( _mode );
            _network_init = true;
        }
        
		if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
			if ( _host_data == null && _network_manager.getHostObj( ) != null ) {
				_host_data = _network_manager.getHostObj( ).GetComponent< HostData >( );
			}
			if ( _client_data[ 0 ] == null && _network_manager.getClientObj( 0 ) != null ) {
				_client_data[ 0 ] = _network_manager.getClientObj( 0 ).GetComponent< ClientData >( );
			}
		} else if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
			if ( _host_data == null && _network_manager.getHostObj( ) != null ) {
				_host_data = _network_manager.getHostObj( ).GetComponent< HostData >( );
			}
			if ( _client_data[ 0 ] == null && _network_manager.getClientObj( 0 ) != null ) {
				_client_data[ 0 ] = _network_manager.getClientObj( 0 ).GetComponent< ClientData >( );
			}
		    if ( _client_data[ 1 ] == null && _network_manager.getClientObj( 1 ) != null ) {
			    _client_data[ 1 ] = _network_manager.getClientObj( 1 ).GetComponent< ClientData >( );
		    }
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

		if ( _host_data != null ) {
			if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
                if ( _client_data[ 0 ] != null ) {
				    // player側のシーン変更が完了したかどうか
				    if ( _client_data[ 0 ].getRecvData( ).changed_scene == true ) {
					    _host_data.setSendChangeFieldScene( false );
				    }
				    // player側のフェイズ変更が完了したかどうか
				    if ( _client_data[ 0 ].getRecvData( ).changed_phase == true ) {
					    _host_data.setSendChangeFieldPhase( false );
				    }

                    if ( _client_data[ 0 ].getRecvData( ).connect_ready ) {
                        _host_data.send( );
                    }
                }
			} else if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
                if ( _client_data[ 0 ] != null && _client_data[ 1 ] != null ) {
				    // player側のシーン変更が完了したかどうか
				    if ( _client_data[ 0 ].getRecvData( ).changed_scene == true &&
                         _client_data[ 1 ].getRecvData( ).changed_scene == true ) {
					    _host_data.setSendChangeFieldScene( false );
				    }
				    // player側のフェイズ変更が完了したかどうか
				    if ( _client_data[ 0 ].getRecvData( ).changed_phase == true &&
                         _client_data[ 1 ].getRecvData( ).changed_phase == true ) {
					    _host_data.setSendChangeFieldPhase( false );
				    }

                    if ( _client_data[ 0 ].getRecvData( ).connect_ready && _client_data[ 1 ].getRecvData( ).connect_ready ) {
                        _host_data.send( );
                    }
                }
            }
 		}
	}

	/// <summary>
	/// ConnectSceneの更新
	/// </summary>
	private void updateConnectScene( ) {
		if ( _mode == PROGRAM_MODE.MODE_NO_CONNECT ) {
			if ( Input.GetKeyDown( KeyCode.A ) ) {
				_scene = SCENE.SCENE_TITLE;
				_scene_text.text = "SCENE_TITLE";
				_network_gui_controll.setShowGUI( false );
                _scene_init = false;
			}
		} else if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
			if ( _network_manager.getPlayerNum( ) >= 1 ) {
				_scene = SCENE.SCENE_TITLE;
				_scene_text.text = "SCENE_TITLE";
				_network_gui_controll.setShowGUI( false );
				try {
					_host_data.setSendScene( _scene );
	            	_host_data.setSendChangeFieldScene( true );
				}
				catch {
					Debug.Log( "通信に失敗しまいました" );
				}
			}
		} else if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
			if ( _network_manager.getPlayerNum( ) >= 2 ) {
				_scene = SCENE.SCENE_TITLE;
				_scene_text.text = "SCENE_TITLE";
				_network_gui_controll.setShowGUI( false );
				try {
					_host_data.setSendScene( _scene );
	            	_host_data.setSendChangeFieldScene( true );
				}
				catch {
					Debug.Log( "通信に失敗しまいました" );
				}
			}
		}
	}

    private void createTitle( ) {
        _title_back_obj = ( GameObject )Instantiate( _title_back_pref );

        Vector3 pos = _title_back_pref.GetComponent< RectTransform >( ).localPosition;
        _title_back_obj.transform.SetParent( GameObject.Find( "Canvas" ).transform );
        _title_back_obj.GetComponent< RectTransform >( ).localScale = new Vector3( 1, 1, 1 );


        _title_back_obj.GetComponent< RectTransform >( ).localPosition = pos;
        _title_back_obj.GetComponent< RectTransform >( ).offsetMax = new Vector2( 0, 0 );
        _title_back_obj.GetComponent< RectTransform >( ).offsetMin = new Vector2( 0, 0 );
    }

    private void destroyTitleObj( ) {
        Destroy( _title_back_obj );
        _title_back_obj = null;
    }

	/// <summary>
	/// TitleSceneの更新
	/// </summary>
	private void updateTitleScene( ) {
        if ( !_scene_init ) {
            createTitle( );
            _scene_init = true;
        }

        if ( _mode == PROGRAM_MODE.MODE_NO_CONNECT ) {
		    if ( Input.GetKeyDown( KeyCode.A ) ) {
			    _scene = SCENE.SCENE_GAME;
			    _scene_text.text = "SCENE_GAME";

                Vector3 pos = _file_manager.getMassCoordinate( 0 );
			    _player_manager.init( ref pos );
                
                // ステージの生成
                _stage_manager.loadGraph( );
                _stage_manager.createBackGroundObj( );

			    //マスの生成
			    for( int i = 0; i < _file_manager.getMassCount( ); i++ ) {
				    int num = _stage_manager.getMassCount( );
				    _stage_manager.massCreate( num, _file_manager.getFileData( ).mass[ num ].mass_type,
                                               _file_manager.getFileData( ).mass[ num ].event_type, _file_manager.getMassCoordinate( num ) );
				    _stage_manager.increaseMassCount( );
			    }
			    _stage_manager.init( );

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

			    _network_gui_controll.setShowGUI( false );
                destroyTitleObj( );
                _scene_init = false;
		    }
        } else if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
            if ( _client_data[ 0 ] != null ) {
		        if ( _client_data[ 0 ].getRecvData( ).ready ) {
			       connectTitleUpdate( );
                   _send_status = true;
		        }
            }
        } else if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
            if ( _client_data[ 0 ] != null && _client_data[ 1 ] != null ) {
		        if ( _client_data[ 0 ].getRecvData( ).ready && 
                     _client_data[ 1 ].getRecvData( ).ready ) {
			       connectTitleUpdate( );
                   _send_status = true;
		        }
            }
        }
	}

    private void connectTitleUpdate( ) {
        _scene = SCENE.SCENE_GAME;
		_scene_text.text = "SCENE_GAME";
        
        Vector3 pos = _file_manager.getMassCoordinate( 0 );
		_player_manager.init( ref pos );


        // ステージの生成
        _stage_manager.loadGraph( );
        _stage_manager.createBackGroundObj( );
		//マスの生成
		for( int i = 0; i < _file_manager.getMassCount( ); i++ ) {
			int num = _stage_manager.getMassCount( );
			_stage_manager.massCreate( num, _file_manager.getFileData( ).mass[ num ].mass_type,
                                       _file_manager.getFileData( ).mass[ num ].event_type, _file_manager.getMassCoordinate( num ) );
			_stage_manager.increaseMassCount( );
		}
		_stage_manager.init( );

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

		try {
			_host_data.setSendScene( _scene );
			_host_data.setSendChangeFieldScene( true );
		} catch {
			Debug.Log( "通信に失敗しまいました" );
		}
        destroyTitleObj( );
		_network_gui_controll.setShowGUI( false );
        _scene_init = false;
    }

	/// <summary>
	/// FinishSceneの更新
	/// </summary>
	private void updateFinishScene( ) {
		if ( Input.GetKeyDown( KeyCode.A ) ) {
			_scene = SCENE.SCENE_TITLE;
			_scene_text.text = "SCENE_TITLE";
			if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
				try {
					_host_data.setSendScene( _scene );
					_host_data.setSendChangeFieldScene( true );
				} catch {
					Debug.Log( "通信に失敗しまいました" );
				}
			}
            _scene_init = false;
		}
	}

	/// <summary>
	/// GameSceneの更新
	/// </summary>
	private void updateGameScene( ) {
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

		// 通信データのセット
		if ( _phase_manager.isPhaseChanged( ) && _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
            _phase_init = false;
			_host_data.setSendGamePhase( _phase_manager.getMainGamePhase( ) );
			_host_data.setSendChangeFieldPhase( true );
		}

        // プレイヤーのモーションを更新
        _player_manager.setPlayerMotion( );

        // playerの環境情報を更新
		for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
			if ( _file_manager.getEnvironment( _player_manager.getPlayerCount( i, _stage_manager.getMassCount( ) ) ) != "" ) {
				string environment = _file_manager.getEnvironment ( _player_manager.getPlayerCount( i, _stage_manager.getMassCount( ) ) );
				playerEnvironment( environment, i );
			}
		}

        int[ ] count = getResideCount( );
        _player_manager.dicisionTopAndLowestPlayer( ref count );

        // カメラの位置更新
		_camera_manager.moveCameraPos( _player_manager.getTopPlayer( PLAYER_RANK.RANK_FIRST ).obj, _player_manager.getLastPlayer( ).obj );
        


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

		_stage_manager.updateLightColor( _stage_manager.getEnvironment( ), num );

        if ( _send_status ) {
            if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
                _host_data.setSendPlayerStatus( 0, _client_data[ 0 ].getRecvData( ).player_power,
                                                _client_data[ 0 ].getRecvData( ).hand_num, true );
            } else if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
                _host_data.setSendPlayerStatus( 0, _client_data[ 0 ].getRecvData( ).player_power,
                                                _client_data[ 0 ].getRecvData( ).hand_num, true );
                _host_data.setSendPlayerStatus( 1, _client_data[ 1 ].getRecvData( ).player_power,
                                                _client_data[ 1 ].getRecvData( ).hand_num, true );
            }

            _send_status = false;
        }
	}

	/// <summary>
	/// NoPlayPhaseの更新
	/// </summary>
	private void updateNoPlayPhase( ) {
        // サイコロフェイズへの移行
		StartCoroutine( "gameStart" );
        _send_status = true;
        _phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_DICE, "DicePhase" );
        _phase_manager.createPhaseText( MAIN_GAME_PHASE.GAME_PHASE_DICE );
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
            dice_value[ 0 ] = _client_data[ 0 ].getRecvData( ).dice_value;
            dice_value[ 1 ] = _client_data[ 1 ].getRecvData( ).dice_value;
		    // ダイスを振ったら(通信)
		    if ( dice_value[ 0 ] > 0 && dice_value[ 1 ] > 0  ) {
                _dice_value[ 0 ] = dice_value[ 0 ];
                _dice_value[ 1 ] = dice_value[ 1 ];
                // キャラクター移動フェイズへの移行
                _phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_MOVE_CHARACTER, "MovePhase" );
				_phase_manager.deletePhaseImage( );
                _phase_init = false;
            }
		} else if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
            // 送られてきた賽の目の数
            int[ ] dice_value = new int[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
            dice_value[ 0 ] = _client_data[ 0 ].getRecvData( ).dice_value;
		    // ダイスを振ったら(通信)
		    if ( dice_value[ 0 ] > 0 ) {
                _dice_value[ 0 ] = dice_value[ 0 ];
                _dice_value[ 1 ] = ( int )Random.Range( 1.0f, 3.0f );
                // キャラクター移動フェイズへの移行
				_phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_MOVE_CHARACTER, "MovePhase" );
				_phase_manager.deletePhaseImage( );
                _phase_init = false;
            }
		} else if ( _mode == PROGRAM_MODE.MODE_NO_CONNECT ) {
			if ( Input.GetKeyDown( KeyCode.A ) ) {
				// 送られてきた賽の目の数
				int[ ] dice_value = new int[ ( int )PLAYER_ORDER.MAX_PLAYER_NUM ];
				for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
					dice_value[ i ] = _debug_dice_value;//( int )Random.Range( 1.0f, 4.0f );
                    _dice_value[ i ] = dice_value[ i ];
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
					_player_manager.setLimitValue( _dice_value[ ( int )_player_manager.getPlayerOrder( ) ] );
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
					_player_manager.setLimitValue( _dice_value[ ( int )_player_manager.getPlayerOrder( ) ] );
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
            _stage_manager.getTargetMass( _player_manager.getTargetMassID( _stage_manager.getMassCount( ) ) ) );
        
        // 現在のマスをクライアントに送信
        if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
            if ( _player_manager.isChangeCount( PLAYER_ORDER.PLAYER_ONE ) ) {
                _host_data.setSendMassCount( PLAYER_ORDER.PLAYER_ONE,
                                             _player_manager.getPlayerCount( 0, _stage_manager.getMassCount( ) ) );
            }
        } else if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
            if ( _player_manager.isChangeCount( PLAYER_ORDER.PLAYER_ONE ) ) {
                _host_data.setSendMassCount( PLAYER_ORDER.PLAYER_ONE,
                                             _player_manager.getPlayerCount( ( int )PLAYER_ORDER.PLAYER_ONE,
                                                                             _stage_manager.getMassCount( ) ) );
            }
            if ( _player_manager.isChangeCount( PLAYER_ORDER.PLAYER_TWO ) ) {
                _host_data.setSendMassCount( PLAYER_ORDER.PLAYER_TWO,
                                             _player_manager.getPlayerCount( ( int )PLAYER_ORDER.PLAYER_TWO,
                                                                             _stage_manager.getMassCount( ) ) );
            }
        }
       

        // プレイヤーの順番を更新
        _player_manager.updatePlayerOrder( );

        // ゴールまでの残りマスを表示
		resideCount( );

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
        List< int > card_list = new List< int >( );

		if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
            // 1Pにカード配布
			if ( !_host_data.isSendCard( 0 ) ) {
				for ( int i = 0; i < MAX_DRAW_VALUE - _dice_value[ 0 ]; i++ ) {
			        // デッキのカード数が０になったらリフレッシュ
			        if ( _card_manager.getDeckCardNum( ) <= 0 ) {
				        _card_manager.createDeck( );
			        }
                    card_list.Add( _card_manager.distributeCard( ).id );
				}
				_host_data.refreshCardList( 0 );
                _host_data.setSendCardlist( ( int )PLAYER_ORDER.PLAYER_ONE, card_list );
                // カードリストを初期化
                card_list.Clear( );
            }
            
            // 2Pにカード配布
			if ( !_host_data.isSendCard( 1 ) ) {
				for ( int i = 0; i < MAX_DRAW_VALUE - _dice_value[ 1 ]; i++ ) {
			        // デッキのカード数が０になったらリフレッシュ
			        if ( _card_manager.getDeckCardNum( ) <= 0 ) {
				        _card_manager.createDeck( );
			        }
                    card_list.Add( _card_manager.distributeCard( ).id );
				}
				_host_data.refreshCardList( 1 );
                _host_data.setSendCardlist( ( int )PLAYER_ORDER.PLAYER_TWO, card_list );
            }
            // 両方の準備が終わったら次のフェイズへ
			if ( _client_data[ 0 ].getRecvData( ).ready == true && _client_data[ 1 ].getRecvData( ).ready == true ) {
				if ( _connect_wait_time >= CONNECT_WAIT_TIME && !_refresh_card_list ) {
					_host_data.refreshCardList( 0 );
					_host_data.refreshCardList( 1 );
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
            // 1Pにカード配布
			if ( !_host_data.isSendCard( 0 ) ) {
				for ( int i = 0; i < MAX_DRAW_VALUE - _dice_value[ 0 ]; i++ ) {
			        // デッキのカード数が０になったらリフレッシュ
			        if ( _card_manager.getDeckCardNum( ) <= 0 ) {
				        _card_manager.createDeck( );
			        }
                    card_list.Add( _card_manager.distributeCard( ).id );
		        }
				_host_data.refreshCardList( 0 );
                _host_data.setSendCardlist( ( int )PLAYER_ORDER.PLAYER_ONE, card_list );
            }

            //Debug.Log( _client_data[ 0 ].getRecvData( ).ready );
            // 準備が終わったら次のフェイズへ
			if ( _client_data[ 0 ].getRecvData( ).ready == true ) {
				if ( _connect_wait_time >= CONNECT_WAIT_TIME && !_refresh_card_list ) {
                    try {
					    _host_data.refreshCardList( 0 );
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
            if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                _host_data.refreshCardList( 0 );
                _host_data.refreshCardList( 1 );
            }
            _phase_init = true;
        }
		if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
            if ( _client_data[ 0 ].getRecvData( ).battle_ready == true &&
				 _client_data[ 1 ].getRecvData( ).battle_ready == true )  {
				// 1Pのステータスを設定
				for ( int i = 0; i < _client_data[ 0 ].getRecvData( ).used_card_list.Length; i++ ) {
					_player_manager.adaptaCard( 0, _card_manager.getCardData( _client_data[ 0 ].getRecvData( ).used_card_list[ i ] ) );
				}
				_player_manager.endStatus( 0 );
				Debug.Log( "1Pのpower:" + _player_manager.getPlayerPower( )[ 0 ].ToString( ) );

				// 2Pのステータスを設定
				for ( int i = 0; i < _client_data[ 1 ].getRecvData( ).used_card_list.Length; i++ ) {
					_player_manager.adaptaCard( 1, _card_manager.getCardData( _client_data[ 1 ].getRecvData( ).used_card_list[ i ] ) );
				}
				_player_manager.endStatus( 1 );
				Debug.Log( "2Pのpower:" + _player_manager.getPlayerPower( )[ 1 ].ToString( ) );
				// プラスバリューの初期化
				_player_manager.allPlusValueInit( );

                // 攻撃力を比較
				_player_manager.attackTopAndLowestPlayer( _player_manager.getPlayerPower( ) );
                // 次のフェイズへ
                _phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_RESULT, "ResultPhase" );
                _phase_init = false;
            }
		} else if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
			if ( _client_data[ 0 ].getRecvData( ).battle_ready == true )  {
				// 1Pのステータスを設定
				for ( int i = 0; i < _client_data[ 0 ].getRecvData( ).used_card_list.Length; i++ ) {
					_player_manager.adaptaCard( 0, _card_manager.getCardData( _client_data[ 0 ].getRecvData( ).used_card_list[ i ] ) );
				}
				_player_manager.endStatus( 0 );
				Debug.Log( "1Pのpower:" + _player_manager.getPlayerPower( )[ 0 ].ToString( ) );

				// 2Pのステータスを設定
				for ( int i = 0; i < _client_data[ 0 ].getRecvData( ).used_card_list.Length; i++ ) {
					_player_manager.adaptaCard( 1, _card_manager.getCardData( _client_data[ 0 ].getRecvData( ).used_card_list[ i ] ) );
				}
				_player_manager.endStatus( 1 );
				Debug.Log( "2Pのpower:" + _player_manager.getPlayerPower( )[ 1 ].ToString( ) );
				// プラスバリューの初期化
				_player_manager.allPlusValueInit( );

                // 攻撃力を比較
				_player_manager.attackTopAndLowestPlayer( _player_manager.getPlayerPower( ) );
                // 次のフェイズへ
                _phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_RESULT, "ResultPhase" );
                _phase_init = false;
            }
		} else if ( _mode == PROGRAM_MODE.MODE_NO_CONNECT ) {
			if ( Input.GetKeyDown( KeyCode.A ) )  {
				// 次のフェイズへ
				_phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_RESULT, "ResultPhase" );
                _phase_init = false;
			}
		}
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

            _phase_init = true;
        }

        if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
            // 戦闘結果を送信
            if ( _host_data.getRecvData( ).send_result == false ) {
                _connect_wait_time++;
                if ( _connect_wait_time > CONNECT_WAIT_TIME ) {
                    _connect_wait_time = 0;
                    BATTLE_RESULT[ ] result = new BATTLE_RESULT[ ]{ _player_manager.getPlayerResult( 0 ), _player_manager.getPlayerResult( 1 ) };
                    _host_data.setSendBattleResult( result, true );
                }
            }
        }

		if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
            if ( _client_data[ 0 ].getRecvData( ).ready &&
                 _client_data[ 1 ].getRecvData( ).ready &&
                 _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER ) {
                if ( _client_data[ ( int )_player_manager.getPlayerOrder( ) ].getRecvData( ).mass_adjust == MASS_ADJUST.ADVANCE &&
                     !_player_manager.isMoveStart( ) ) {
                    // Pを前に動かす
		            _player_manager.setLimitValue( 1 );
		            _player_manager.setAdvanceFlag( true );
					_event_count[ ( int )_player_manager.getPlayerOrder( ) ] = 0;
                } else if ( _client_data[ ( int )_player_manager.getPlayerOrder( ) ].getRecvData( ).mass_adjust == MASS_ADJUST.BACK &&
                            !_player_manager.isMoveStart( ) ) {
                    // Pを後ろに動かす
		            _player_manager.setLimitValue( 1 );
		            _player_manager.setAdvanceFlag( false );
					_event_count[ ( int )_player_manager.getPlayerOrder( ) ] = 0;
                } else if ( _client_data[ ( int )_player_manager.getPlayerOrder( ) ].getRecvData( ).mass_adjust == MASS_ADJUST.NO_ADJUST &&
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
            if ( _client_data[ 0 ].getRecvData( ).ready  &&
                 _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER )  {
                if ( _client_data[ 0 ].getRecvData( ).mass_adjust == MASS_ADJUST.ADVANCE &&
                     !_player_manager.isMoveStart( ) ) {
                    // 1Pを前に動かす
		            _player_manager.setLimitValue( 1 );
		            _player_manager.setAdvanceFlag( true );
					_event_count[ ( int )_player_manager.getPlayerOrder( ) ] = 0;
                } else if ( _client_data[ 0 ].getRecvData( ).mass_adjust == MASS_ADJUST.BACK &&
                            !_player_manager.isMoveStart( ) ) {
                    // 1Pを後ろに動かす
		            _player_manager.setLimitValue( 1 );
		            _player_manager.setAdvanceFlag( false );
					_event_count[ ( int )_player_manager.getPlayerOrder( ) ] = 0;
                } else if ( _client_data[ 0 ].getRecvData( ).mass_adjust == MASS_ADJUST.NO_ADJUST &&
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
        _player_manager.movePhaseUpdate( ref num, _stage_manager.getTargetMass( _player_manager.getTargetMassID( _stage_manager.getMassCount( ) ) ) );

        // ゴールまでの残りマスを表示
		resideCount( );
        
        // プレイヤーの順番を更新
        _player_manager.updatePlayerOrder( );

        _connect_wait_time++;

        // 両方の移動が終わったら次のフェイズへ
        if ( _player_manager.isAllPlayerMoveFinish( ) == true && _connect_wait_time >= CONNECT_WAIT_TIME ) {
            _connect_wait_time = 0;
            _phase_init = false;
			if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                BATTLE_RESULT[ ] result = new BATTLE_RESULT[ ]{ BATTLE_RESULT.BATTLE_RESULT_NONE, BATTLE_RESULT.BATTLE_RESULT_NONE };
				_host_data.setSendBattleResult( result, false );
			}
            _player_manager.refreshPlayerResult( );
            _phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_EVENT, "EventPhase" );
            _player_manager.allMovedRefresh( );

            if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
                _host_data.setSendPlayerStatus( 0, _client_data[ 0 ].getRecvData( ).player_power,
                                                _client_data[ 0 ].getRecvData( ).hand_num, false );
            } else if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
                _host_data.setSendPlayerStatus( 0, _client_data[ 0 ].getRecvData( ).player_power,
                                                _client_data[ 0 ].getRecvData( ).hand_num, false );
                _host_data.setSendPlayerStatus( 1, _client_data[ 1 ].getRecvData( ).player_power,
                                                _client_data[ 1 ].getRecvData( ).hand_num, false );
            }
        }
	}

	/// <summary>
	/// EventPhaseの更新
	/// </summary>
	private void updateEventPhase( ) {
        if ( !_phase_init ) {
            
            // 行動順1Pに設定する
            _player_manager.startPlayerOrder( );

            _phase_init = true;
        }

        // ゴール処理
        if ( _player_manager.isAllPlayerEventFinish( ) && 
             _goal_flag && _connect_wait_time >= CONNECT_WAIT_TIME && _particle == null ) {
            _connect_wait_time = 0;
			_player_manager.eventRefresh( );
            _player_manager.allMovedRefresh( );
            _phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_FINISH, "FinishPhase" );
            return;
        }

        // 通信待機時間の更新
        _connect_wait_time++;

        int player_one = ( int )PLAYER_ORDER.PLAYER_ONE;
        int player_two = ( int )PLAYER_ORDER.PLAYER_TWO;

		if ( _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER && !_player_manager.isEventFinish( ) ) {
            // 1Pのイベント処理
            if ( _player_manager.getEventType( ) != EVENT_TYPE.EVENT_WORP && 
                 _player_manager.getEventType( ) != EVENT_TYPE.EVENT_CHANGE ) {
				massEvent( _player_manager.getPlayerCount( ( int )_player_manager.getPlayerOrder( ), _stage_manager.getMassCount( ) ) );
			} else {
				massEvent( _before_player_count );
            }
		} 

        if ( _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER ) {
            Debug.Log( ( int )_player_manager.getPlayerOrder( ) + "move_start:" + _player_manager.isMoveFinish( ) );
            Debug.Log( ( int )_player_manager.getPlayerOrder( ) + "move_finish:" + _player_manager.isMoveFinish( ) );
            Debug.Log( ( int )_player_manager.getPlayerOrder( ) + "event_type:" + _player_manager.getEventType( ) );
            Debug.Log( ( int )_player_manager.getPlayerOrder( ) + "event_start:" + _player_manager.isEventStart( ) );
            Debug.Log( ( int )_player_manager.getPlayerOrder( ) + "event_finish:" + _player_manager.isEventFinish( ) );
        }
       
        // プレイヤーの移動
        int[ ] num = getResideCount( );
        _player_manager.movePhaseUpdate( ref num, _stage_manager.getTargetMass( _player_manager.getTargetMassID( _stage_manager.getMassCount( ) ) ) );

		// マス移動終了時にイベントフラグをfalseにしてもう一度イベントが発生するようにする
        if ( _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER ) {
            if ( _player_manager.getEventType( ) == EVENT_TYPE.EVENT_MOVE        ||
                 _player_manager.getEventType( ) == EVENT_TYPE.EVENT_TRAP_ONE ||
                 _player_manager.getEventType( ) == EVENT_TYPE.EVENT_TRAP_TWO ) {
			    if ( _player_manager.isMoveFinish( ) ) {
                    // イベント開始＆移動状態を初期化
				    _player_manager.setEventStart( false );
				    _player_manager.movedRefresh( );
			    }
            }
        }

        // ゴールまでの残りマスを表示
        resideCount( );

        // パーティクルの更新
		if( _particle != null ) {
			if( _particle.gameObject.name == "OceanCurrent" ) {
				_particle_time++;
				if( _particle_time > OCEAN_CURRENT_STOP_TIME ) {
                    //　パーティクルの停止
					_particle.GetComponent< ParticleEmitter >( ).emit = false;
				}
				if( _particle_time > OCEAN_CURRENT_DESTROY_TIME ) {
                    // パーティクルの削除
					_particle_time = 0.0f;
					_particle = null;
				}
			} else if( _particle.gameObject.name == "Spiral" ) {
				_particle_time++;
                if ( _particle_time > SPIRAL_TIME_FOUR ) {
					_player_manager.setEventFinish( true );
					_particle_time = 0.0f;
					_particle = null;
                }
			}
		}

        // イベント終了処理
        if ( _player_manager.getPlayerOrder( ) != PLAYER_ORDER.NO_PLAYER ) {
            if ( _player_manager.isEventStart( ) && _player_manager.isEventFinish( ) ) {
                // 行動プレイヤーを変える
                _player_manager.changePlayerOrder( );
		    }
        }
        // プレイヤーの順番を更新
        _player_manager.updatePlayerOrder( );

        // イベント終了時の処理
		if ( _player_manager.isAllPlayerEventFinish( ) &&
             _goal_flag == false && _connect_wait_time >= CONNECT_WAIT_TIME && _particle == null ) {
            // カードドロー完了したら
            if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
                if ( !_client_data[ player_one ].getRecvData( ).ok_event &&
                     _event_type[ player_one ] == EVENT_TYPE.EVENT_DRAW ) {
                    return;
                }
                if ( !_client_data[ player_one ].getRecvData( ).ok_event &&
                     _event_type[ player_one ] == EVENT_TYPE.EVENT_DISCARD ) {
                    return;
                }
            } else if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
                if ( ( !_client_data[ player_one ].getRecvData( ).ok_event && _event_type[ player_one ] == EVENT_TYPE.EVENT_DRAW ) ||
                     ( !_client_data[ player_two ].getRecvData( ).ok_event && _event_type[ player_two ] == EVENT_TYPE.EVENT_DRAW ) ) {
                    return;
                }
                if ( ( !_client_data[ player_one ].getRecvData( ).ok_event && _event_type[ player_one ] == EVENT_TYPE.EVENT_DISCARD ) ||
                     ( !_client_data[ player_two ].getRecvData( ).ok_event && _event_type[ player_two ] == EVENT_TYPE.EVENT_DISCARD ) ) {
                    return;
                }
            }

            // 各値を初期化
            _connect_wait_time = 0;
			_player_manager.eventRefresh( );
            _player_manager.allMovedRefresh( );
            if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                if ( _client_data[ player_one ] != null && _client_data[ player_one ].getRecvData( ).ok_event ) {
                    _event_type[ player_one ] = EVENT_TYPE.EVENT_NONE;
                    _player_manager.setEventType( player_one, _event_type[ player_one ] );
                    _host_data.setSendEventType( PLAYER_ORDER.PLAYER_ONE, _event_type[ player_one ] );
                    _host_data.refreshCardList( player_one );
                }
                if ( _client_data[ player_two ] != null && _client_data[ player_two ].getRecvData( ).ok_event ) {
                    _event_type[ player_two ] = EVENT_TYPE.EVENT_NONE;
                    _player_manager.setEventType( player_two, _event_type[ player_two ] );
                    _host_data.setSendEventType( PLAYER_ORDER.PLAYER_ONE, _event_type[ player_two ] );
                    _host_data.refreshCardList( player_two );
                }
            }

			_phase_manager.changeMainGamePhase( MAIN_GAME_PHASE.GAME_PHASE_DICE, "DisePhase" );
			_phase_manager.createPhaseText( MAIN_GAME_PHASE.GAME_PHASE_DICE );
		}

	}

	/// <summary>
	/// マスイベントの処理
	/// </summary>
	/// <param name="mass_count" 何マス目か>The index.</param>
	public void massEvent( int mass_count ) {
		//_player_manager.setEventStart( id, true );

        int value = 0;
        int id = ( int )_player_manager.getPlayerOrder( );

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
                Debug.Log( "マスの数は" + mass_count );
                Debug.Log( "入手カード数は" + value );
                if ( !_animation_running ) {
                    _player_manager.setEventStart( true );
                    for ( int j = 0; j < value; j++ ) {
                        // デッキのカード数が０になったらリフレッシュ
                        if ( _card_manager.getDeckCardNum( ) <= 0 ) {
                            _card_manager.createDeck( );
                        }
                        int num = _card_manager.distributeCard( ).id;
                        _draw_card_list.Add( num );
                        Debug.Log( "cardID:" + num );
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
			    break;
            case EVENT_TYPE.EVENT_MOVE:
                _event_type[ id ] = EVENT_TYPE.EVENT_MOVE;
                if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
                    _host_data.setSendEventType( _player_manager.getPlayerOrder( ), _event_type[ id ] );
                }
                Debug.Log( _file_manager.getMassValue( mass_count )[ 0 ] + "マス進む" );
				if ( _particle == null ) {
					_particle = GameObject.Find( "OceanCurrent" );
				}
                _particle.GetComponent< ParticleEmitter >( ).emit = true;
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

				if( _particle == null ) {
					_particle = GameObject.Find("OceanCurrent");
				}
				_player_manager.setEventStart( true );
				_particle.GetComponent< ParticleEmitter >( ).emit = true;
				_player_manager.setLimitValue( _file_manager.getMassValue ( mass_count )[ 0 ] );
				_player_manager.setCurrentFlag( true );
				_player_manager.setAdvanceFlag( true );
				_player_manager.setEventStart( true );
                _before_player_count = _player_manager.getPlayerCount( id, _stage_manager.getMassCount ( ) );
                _player_manager.setEventType( id, EVENT_TYPE.EVENT_TRAP_ONE );
                break;
            case EVENT_TYPE.EVENT_TRAP_TWO:
                // カードドロー
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
				if ( _particle == null ) {
					_particle = GameObject.Find( "OceanCurrent" );
				}
				_player_manager.setEventStart( true );
				_particle.GetComponent< ParticleEmitter >( ).emit = true;
				_player_manager.setLimitValue( _file_manager.getMassValue( mass_count )[ 1 ] );
				_player_manager.setCurrentFlag( true );
				_player_manager.setAdvanceFlag( false );
				_player_manager.setEventStart( true );
                _before_player_count = _player_manager.getPlayerCount( id, _stage_manager.getMassCount( ) );
                _player_manager.setEventType( id, EVENT_TYPE.EVENT_TRAP_TWO );
                break;
            case EVENT_TYPE.EVENT_GOAL:
				_player_manager.setEventStart( true );
                if ( _player_manager.getPlayerResult( id ) == BATTLE_RESULT.WIN ) {
                    _phase_manager.changeMainGamePhase ( MAIN_GAME_PHASE.GAME_PHASE_FINISH, "FinishPhase" );
                    Debug.Log ( "プレイヤー" + ( id + 1 ) + ":Goal!!" );
                    _goal_flag = true;
                    _player_manager.setEventFinish( true );
                    _player_manager.setEventType( id, EVENT_TYPE.EVENT_GOAL );
                } else if ( _player_manager.getPlayerResult( id ) == BATTLE_RESULT.LOSE ||
                            _player_manager.getPlayerResult( id ) == BATTLE_RESULT.DRAW ) {
                    if( _particle == null ) {
						_particle = GameObject.Find( "OceanCurrent" );
					}
					_particle.GetComponent< ParticleEmitter >( ).emit = true;
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
                Debug.Log ( "チェンジ" );
					
				int count_tmp      = _player_manager.getPlayerCount( id, _stage_manager.getMassCount( ) );
				Vector3 vector_tmp = _stage_manager.getTargetMass( count_tmp ).transform.localPosition;
                // パーティクルを検索
				if( _particle == null ) {
					_particle = GameObject.Find( "Spiral" );
				}
				if( _particle_time == 0 ) {
                    // プレイヤーの位置を保持
					_before_player_count = _player_manager.getPlayerCount( id, _stage_manager.getMassCount( ) );
					// パーティクルの開始
					_particle.GetComponent< ParticleEmitter >( ).emit = true;

				} else if ( SPIRAL_TIME_ONE < _particle_time && _particle_time < SPIRAL_TIME_TWO ) {
                    // パーティクルを停止
					_particle.GetComponent< ParticleEmitter >( ).emit = false;
				} else if (  SPIRAL_TIME_TWO < _particle_time && _particle_time< SPIRAL_TIME_THREE ) {
					if ( id == ( int )PLAYER_ORDER.PLAYER_ONE ) {
						_player_manager.setPlayerPosition( ( int )PLAYER_ORDER.PLAYER_ONE,
                            _stage_manager.getTargetMass( _player_manager.getPlayerCount( ( int )PLAYER_ORDER.PLAYER_TWO,
                                _stage_manager.getMassCount( ) ) ).transform.localPosition );
						_player_manager.setPlayerPosition( ( int )PLAYER_ORDER.PLAYER_TWO, vector_tmp );
						_player_manager.setPlayerCount( ( int )PLAYER_ORDER.PLAYER_ONE,
                                                        _player_manager.getPlayerCount( ( int )PLAYER_ORDER.PLAYER_TWO,
                                                                                        _stage_manager.getMassCount( ) ) );
						_player_manager.setPlayerCount( ( int )PLAYER_ORDER.PLAYER_TWO, count_tmp );
                        _player_manager.setEventStart( true );
					} else if ( id == ( int )PLAYER_ORDER.PLAYER_TWO ) {
						_player_manager.setPlayerPosition( ( int )PLAYER_ORDER.PLAYER_TWO,
                            _stage_manager.getTargetMass( _player_manager.getPlayerCount( ( int )PLAYER_ORDER.PLAYER_ONE,
                                _stage_manager.getMassCount( ) ) ).transform.localPosition );
						_player_manager.setPlayerPosition( ( int )PLAYER_ORDER.PLAYER_ONE, vector_tmp );
						_player_manager.setPlayerCount( ( int )PLAYER_ORDER.PLAYER_TWO,
                            _player_manager.getPlayerCount( ( int )PLAYER_ORDER.PLAYER_ONE, _stage_manager.getMassCount ( ) ) ); 
						_player_manager.setPlayerCount( ( int )PLAYER_ORDER.PLAYER_ONE, count_tmp );
					}
					int[ ] count = getResideCount( );
					_player_manager.dicisionTopAndLowestPlayer( ref count );
				}
                _player_manager.setEventType( id, EVENT_TYPE.EVENT_CHANGE );
                break;
            case EVENT_TYPE.EVENT_WORP:
                // ワープする場所を決定
                int worp_position = _file_manager.getNomalValue( _player_manager.getPlayerCount( id, _stage_manager.getMassCount( ) ) );
				// パーティクルを検索
                if ( _particle == null ) {
					_particle = GameObject.Find( "Spiral" );
				}
				if ( _particle_time == 0 ) {
                    // プレイヤーの位置を保持
					_before_player_count = _player_manager.getPlayerCount( id, _stage_manager.getMassCount( ) );
                    // パーティクルの開始
					_particle.GetComponent< ParticleEmitter >( ).emit = true;
                    _player_manager.setEventStart( true );
				} else if( SPIRAL_TIME_ONE < _particle_time && _particle_time < SPIRAL_TIME_TWO ) {
                    // パーティクルを停止
					_particle.GetComponent< ParticleEmitter >( ).emit = false;

				} else if( _particle_time < SPIRAL_TIME_FOUR && _particle_time > SPIRAL_TIME_TWO ) {
					_player_manager.setPlayerCount( id, worp_position );
					_player_manager.setPlayerPosition( id, _stage_manager.getTargetMass( worp_position ).transform.localPosition );
					int[ ] count = getResideCount( );
					_player_manager.dicisionTopAndLowestPlayer( ref count );
				}
                _player_manager.setEventType( id, EVENT_TYPE.EVENT_WORP );
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

    /// <summary>
    /// FinishPhaseの更新
    /// </summary>
    private void updateFinishPhase( ) {
		if ( Input.GetKeyDown( KeyCode.A ) ) {
			_scene = SCENE.SCENE_FINISH;
			_scene_text.text = "SCENEFINISH";
            _player_manager.destroyObj( );
            _stage_manager.destroyObj( );
			if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
				try {
					_host_data.setSendScene( _scene );
					_host_data.setSendChangeFieldScene( true );
				} catch {
					Debug.Log( "通信に失敗しまいました" );
				}
			}
            _scene_init = false;
		}
	}

	public void OnGUI( ) {
		if ( _host_data != null && _scene == SCENE.SCENE_CONNECT ) {
			drawConnectScene( );
		}
	}

	/// <summary>
	/// ConnectSceneの描画
	/// </summary>
	private void drawConnectScene( ) {
		if ( _mode != PROGRAM_MODE.MODE_NO_CONNECT ) {
			if ( !_network_manager.isConnected( ) && _host_data.getServerState( ) != SERVER_STATE.STATE_HOST ) {
				//_network_manager.noConnectDraw( );
			}

			if ( _host_data.getServerState( ) == SERVER_STATE.STATE_HOST ) {
				_network_manager.hostStateDraw( );
			}
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
	/// プレイヤーの現在位置（環境）
	/// </summary>
	/// <param name="environment"></param>
	/// <param name="num"></param>
	public void playerEnvironment( string environment, int num ) {
		_environment[ num ].text = "プレイヤー" + ( num + 1 ) + ":" + environment;
	}

	/// <summary>
	/// ゴールまでの残りマスを表示
	/// </summary>
	public void resideCount( ) {
		for ( int i = 0; i < ( int )PLAYER_ORDER.MAX_PLAYER_NUM; i++ ) {
			_reside_text[ i ].text = "プレイヤー" + i.ToString( ) + "：残り" + getResideCount( )[ i ].ToString( ) + "マス";
		}
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

}
