using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;//必要です。
using System.Net;//これもいるかもしれない
using Common;

public class NetworkMNG : NetworkManager  {
    
    [ SerializeField ]
    private HostData _host_data;
    [ SerializeField ]
    private List< ClientData > _client_data = new List< ClientData >( );

	//ファイヤーウォールを無効化してテスト
	//ファイヤーウォールの接続を許可すること.
    [ SerializeField ]
    PROGRAM_MODE _mode = PROGRAM_MODE.MODE_NO_CONNECT;
	[ SerializeField ]
	private static IPAddress _ip_address;
	private GameObject _object_prefab;
	private GameObject _object_prefab_2;
	private GameObject _host_obj   = null;
	private List< GameObject > _client_obj = new List< GameObject >( );
	private string _ip   = "localhost";
	private string _port = "5037";
    private int _player_num = 0;
	private bool _connected = false;

	void Awake( ) {
		try {
			_object_prefab   = ( GameObject )Resources.Load( "Prefabs/Player1" );
			_object_prefab_2 = ( GameObject )Resources.Load( "Prefabs/Player2" );
		}
		catch {
			Debug.Log( "resourceのロードに失敗しました。" );
		}
	}

	// Use this for initialization
	void Start( ) {
		try {
			// IPアドレスの取得
			_ip_address = IPAddress.Parse( Network.player.ipAddress );
            GetComponent< NetworkManager >( ).networkAddress = _ip_address.ToString( );
		}
		catch {
			Debug.Log( "IPの取得に失敗しまいました" );
		}
	}

    public void setProgramMode( PROGRAM_MODE mode ) {
        _mode = mode;
    }

	// Update is called once per frame
	void FixedUpdate( ) {
        int num = 0;

        if ( _mode == PROGRAM_MODE.MODE_ONE_CONNECT ) {
            num = 1;
        } else if ( _mode == PROGRAM_MODE.MODE_TWO_CONNECT ) {
            num = 2;
        }

		if ( _client_obj.Count < num ) { // 後でなおすToDo
			foreach( GameObject obj in GameObject.FindGameObjectsWithTag( "ClientObj" ) ) {
				if ( !_client_obj.Contains( obj ) ) {
					_client_obj.Add( obj );
				}
			}
		}

        // hostobjの検索
        if ( _host_obj == null ) {
            _host_obj = GameObject.FindWithTag( "HostObj" );
        }
        
        if ( _mode == PROGRAM_MODE.MODE_NO_CONNECT ) {
            _connected = true;
        }

        // host_dataとclient_dataの検索
        if ( !_connected ) {
		    if ( _host_data == null && _host_obj != null ) {
			    _host_data = _host_obj.GetComponent< HostData >( );
		    }
            for ( int i = 0; i < ( int )_mode; i++ ) {
                if ( _client_obj.Count > i && _client_obj[ i ] != null &&
                     _client_data.Count <= i ) {
                    _client_data.Add( _client_obj[ i ].GetComponent< ClientData >( ) );
                    if ( i == ( int )_mode - 1 ) {
                        _connected = true;
                    }
                }
            }
        }
        
	    // IPアドレスの取得
        GetComponent< NetworkManager >( ).networkAddress = _ip_address.ToString( );
	}
    
    //サーバーに接続したときクライアント上で呼び出されます。
    public override void OnServerConnect( NetworkConnection conn ) {
        if ( _host_obj != null ) {
            _player_num++;
            _host_obj.GetComponent< HostData >( ).increasePlayerNum( );
        }
    }

    /// <summary>
    /// サーバー時新しいクライアント接続で呼ばれる
    /// </summary>
    void OnPlayerConnected( ) {
        foreach( GameObject obj in GameObject.FindGameObjectsWithTag( "ClientObj" ) ) {
            if ( !_client_obj.Contains( obj ) ) {
                _client_obj.Add( obj );
            }
        }
    }

	/// <summary>
	/// 未接続時の描画
	/// </summary>
	public void noConnectDraw( ) {
		GUI.Label( new Rect( 40, 250, 100, 30 ), "HOST IP" );
		Rect rect1 = new Rect( 100, 250, 250, 30 );
		_ip = GUI.TextField( rect1, _ip, 32 );

		if( GUI.Button( new Rect( 10, 10, 90, 90 ), "Client" ) ) {    
			//( hostのIPアドレス,hostが接続を受け入れているポート番号 )
			Network.Connect( _ip, int.Parse( _port ) ); 
		}
		if( GUI.Button( new Rect( 10, 110, 90, 90 ), "Server" ) ) {    
			//(接続可能人数,接続を受け入れるポート番号,NATのパンチスルー機能の設定 )
			Network.InitializeServer( 10, int.Parse( _port ), false ) ;
			_ip = _ip_address.ToString( );
		}
	}

	/// <summary>
	/// ホスト側の描画
	/// </summary>
	public void hostStateDraw( ) {
        if ( _host_data != null && _host_data.getServerState( ) == SERVER_STATE.STATE_HOST ) {
		    // 文字の設定
		    string text = _ip_address.ToString( );
		    int width   = 500;
		    int height  = 30;
		    GUIStyle style = new GUIStyle( );
		    style.fontSize = 50;
		    GUIStyleState style_state = new GUIStyleState( );
		    style_state.textColor = Color.black;
		    style.normal = style_state;

		    // IPアドレスの表示
		    GUI.Label( new Rect( Screen.width / 2 - text.Length / 2, Screen.height / 2, width, height ), text, style );
        }
	}
    
    /// <summary>
    /// シーン移行したかチェック
    /// </summary>
    public void checkChangeScene( ) {
        if ( _host_data != null ) {
            int count = 0;
            for ( int i = 0; i < _client_data.Count; i++ ) {
                if ( _client_data[ i ] != null ) {
                    if ( _client_data[ i ].getRecvData( ).changed_scene ) {
                        count++;
                    }
                }
            }
            
            //  全てのクライアントの準備が完了したら
            if ( count == _client_data.Count ) {
                _host_data.setSendChangeFieldScene( false );
            }
 		}
    }

    /// <summary>
    /// フェイズ移行したかチェック
    /// </summary>
    public void checkChangePhase( ) {
        if ( _host_data != null ) {
            int count = 0;
            for ( int i = 0; i < _client_data.Count; i++ ) {
                if ( _client_data[ i ] != null ) {
                    if ( _client_data[ i ].getRecvData( ).changed_phase ) {
                        count++;
                    }
                }
            }
            
            //  全てのクライアントの準備が完了したら
            if ( count == _client_data.Count ) {
                _host_data.setSendChangeFieldPhase( false );
            }
 		}
    }

    /// <summary>
    /// ホストデータを送る処理
    /// </summary>
    public void sendHostData( ) {
        if ( _host_data != null ) {
            int count = 0;
            for ( int i = 0; i < _client_data.Count; i++ ) {
                if ( _client_data[ i ] != null ) {
                    if ( _client_data[ i ].getRecvData( ).connect_ready ) {
                        count++;
                    }
                }
            }
            
            //  全てのクライアントの準備が完了したら
            if ( count == _client_data.Count ) {
                _host_data.send( );
            }
 		}
    }

    public void changeScene( SCENE scene ) {
        _host_data.setSendScene( scene );
        _host_data.setSendChangeFieldScene( true );
    }

    public void changePhase( MAIN_GAME_PHASE phase ) {
		_host_data.setSendGamePhase( phase );
		_host_data.setSendChangeFieldPhase( true );
    }

    /// <summary>
    /// プレイヤーの現在のマスをhost_dataに設定
    /// </summary>
    /// <param name="player_num"></param>
    /// <param name="mass_count"></param>
    public void setMassCount( int player_num, int mass_count ) {
        if ( _host_data != null ) {
            _host_data.setSendMassCount( ( PLAYER_ORDER )player_num, mass_count );
        }
    }

    /// <summary>
    /// プレイヤーの現在のステータスをhost_dataに設定
    /// </summary>
    public void setPlayerStatus( bool flag ) {
        for ( int i = 0; i < _client_data.Count; i++ ) {
            if ( _client_data[ i ] != null ) {
                _host_data.setSendPlayerStatus( i, _client_data[ i ].getRecvData( ).player_power,
                                                _client_data[ i ].getRecvData( ).hand_num, flag );
            }
        }
    }

    public void setGameFinish( bool flag ) {
        _host_data.setSendGameFinish( flag );
    }

    public void refreshCard( int player_num ) {
        _host_data.refreshCardList( player_num );
    }

    public void setCardList( int player_num, List< int > card_list ) {
        _host_data.setSendCardlist( player_num, card_list );
    }

    public NETWORK_PLAYER_DATA getClientData( int player_num ) {
        return _client_data[ player_num ].getRecvData( );
    } 

    public void setResult( BATTLE_RESULT[ ] result, bool flag ) {
        _host_data.setSendBattleResult( result, flag );
    }

    public void setEventType( int player_num, EVENT_TYPE type ) {
        _host_data.setSendEventType( ( PLAYER_ORDER )player_num, type );
    }

    /// <summary>
    /// ゲームを開始してよいか
    /// </summary>
    /// <returns></returns>
    public bool okStartGame( ) {
        int count = 0;
        for ( int i = 0; i < _client_data.Count; i++ ) {
            if ( _client_data[ i ] != null ) {
                if ( _client_data[ i ].getRecvData( ).start_game ) {
                    count++;
                }
            }
        }

        //  全てのクライアントの準備が完了したら
        if ( count == _client_data.Count ) {
            if ( _host_data.getRecvData( ).game_finish ) {
                _host_data.setSendGameFinish( false );
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// 全てのクライアントの準備が完了したかどうか
    /// </summary>
    /// <returns></returns>
    public bool isReady( ) {
        int count = 0;
        for ( int i = 0; i < _client_data.Count; i++ ) {
            if ( _client_data[ i ] != null ) {
                if ( _client_data[ i ].getRecvData( ).ready ) {
                    count++;
                }
            }
        }

        //  全てのクライアントの準備が完了したら
        if ( count == _client_data.Count ) {
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// 全てのクライアントの戦闘準備が完了したかどうか
    /// </summary>
    /// <returns></returns>
    public bool isBattleReady( ) {
        int count = 0;
        for ( int i = 0; i < _client_data.Count; i++ ) {
            if ( _client_data[ i ] != null ) {
                if ( _client_data[ i ].getRecvData( ).battle_ready ) {
                    count++;
                }
            }
        }

        //  全てのクライアントの準備が完了したら
        if ( count == _client_data.Count ) {
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// 全てのクライアントの戦闘準備が完了したかどうか
    /// </summary>
    /// <returns></returns>
    public bool isFinishGame( ) {
        int count = 0;
        for ( int i = 0; i < _client_data.Count; i++ ) {
            if ( _client_data[ i ] != null ) {
                if ( _client_data[ i ].getRecvData( ).finish_game ) {
                    count++;
                }
            }
        }

        //  全てのクライアントの準備が完了したら
        if ( count == _client_data.Count ) {
            return true;
        }

        return false;
    }

    /// <summary>
    /// タイトルへ戻るかどうか
    /// </summary>
    /// <returns></returns>
    public bool isGoTitle( ) {
        for ( int i = 0; i < _client_data.Count; i++ ) {
            if ( _client_data[ i ] != null ) {
                if ( _client_data[ i ].getRecvData( ).go_title ) {
                    return true;
                }
            }
        }

        return false;
    }

    public bool isSendCard( int player_num ) {
        if ( _host_data.isSendCard( player_num ) ) {
            return true;
        }

        return false;
    }

    public bool isSendResult( ) {
        if ( _host_data.getRecvData( ).send_result ) {
            return true;
        }

        return false;
    }

	/// <summary>
	/// 接続されたかどうか返す
	/// </summary>
	/// <returns><c>true</c>, if connected was ised, <c>false</c> otherwise.</returns>
	public bool isConnected( ) {
		return _connected;
	}

	public GameObject getHostObj( ) {
		return _host_obj;
	}
    
	public GameObject getClientObj( int num ) {
        if ( num >= _client_obj.Count ) {
            return null;
        }

		return _client_obj[ num ];
	}

    public int getPlayerNum( ) {
        return _player_num;
    }
}

