using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Common;

public class ParticleManager : MonoBehaviour {
    
    // パーティクルの時間関係
    private const float OCEAN_CURRENT_STOP_TIME    = 1.0f;
    private const float OCEAN_CURRENT_DESTROY_TIME = 3.0f;
    private const float SPIRAL_TIME_ONE            = 0.25f;
    private const float SPIRAL_TIME_TWO            = 1.0f;
    private const float SPIRAL_TIME_THREE          = 1.2f;
    private const float SPIRAL_TIME_FOUR           = 3.0f;
    private const float GOAL_PARTICLE_WAIT_TIME    = 3.0f;
    private const float GOAL_PARTICLE_UPDATE_TIME  = 0.5f;
	private const float BUBBLE_TIME   			   = 10.0f;
    // パーティクルの生成座標関係
    private const float GOAL_PARTICLE_CREATE_POS_X_MIN =  90.0f;
    private const float GOAL_PARTICLE_CREATE_POS_X_MAX = 110.0f;
    private const float GOAL_PARTICLE_CREATE_POS_Y_MIN = 100.0f;
    private const float GOAL_PARTICLE_CREATE_POS_Y_MAX = 105.0f;
    private const float GOAL_PARTICLE_CREATE_POS_Z     = 165.0f;

    private const float BUBBLE_PARTICLE_CREATE_POS_X_MIN = -20.0f;
    private const float BUBBLE_PARTICLE_CREATE_POS_X_MAX = 100.0f;
    private const float BUBBLE_PARTICLE_CREATE_POS_Y_MIN =   0.1f;
    private const float BUBBLE_PARTICLE_CREATE_POS_Y_MAX =   1.5f;
    private const float BUBBLE_PARTICLE_CREATE_POS_Z     =  10.0f;

    private float[ ][ ] _particle_time_list = new float[ ( int )PARTICLE_TYPE.MAX_PARTICLE_NUM ][ ];

    /// <summary>
    /// パーティクルシステムを操作するクラス  
    /// </summary>
    public class ParticleOperate {
        private GameObject _particle;
        private PARTICLE_TYPE _particle_type;
        private List< float > _change_phase_time_list = new List< float >( );
        private int _particle_phase     = 0;
	    private float _particle_time    = 0.0f;
        private bool _phase_change      = false;
        private bool _particle_end      = false;

        public ParticleOperate( PARTICLE_TYPE type, ref float[ ] time_list, ref GameObject pref ) {
            // タイプを設定
            _particle_type = type;

            // フェイズ切り替え時間を登録
            for ( int i = 0; i < time_list.Length; i++ ) {
                _change_phase_time_list.Add( time_list[ i ] );
            }

            // パーティクルを生成
            _particle = ( GameObject )Instantiate( pref );
            Vector3 create_pos;
            // タイプによって生成位置を変える
            if ( _particle_type == PARTICLE_TYPE.PARTICLE_FIREWORKS1 || _particle_type == PARTICLE_TYPE.PARTICLE_FIREWORKS2 ) {
                create_pos = new Vector3( Random.Range( GOAL_PARTICLE_CREATE_POS_X_MIN, GOAL_PARTICLE_CREATE_POS_X_MAX ),
                                          Random.Range( GOAL_PARTICLE_CREATE_POS_Y_MIN, GOAL_PARTICLE_CREATE_POS_Y_MAX ),
                                          GOAL_PARTICLE_CREATE_POS_Z );
                // パーティクルの作動
                _particle.GetComponent< ParticleEmitter >( ).emit = true;
            } else if ( _particle_type == PARTICLE_TYPE.PARTICLE_BUBBLE ){
                create_pos = new Vector3( Random.Range( BUBBLE_PARTICLE_CREATE_POS_X_MIN, BUBBLE_PARTICLE_CREATE_POS_X_MAX ),
                                          Random.Range( BUBBLE_PARTICLE_CREATE_POS_Y_MIN, BUBBLE_PARTICLE_CREATE_POS_Y_MAX ),
                                          Random.Range( -BUBBLE_PARTICLE_CREATE_POS_Z, BUBBLE_PARTICLE_CREATE_POS_Z ) );
            } else {
                create_pos = pref.transform.position;
                // パーティクルの作動
                _particle.GetComponent< ParticleEmitter >( ).emit = true;
            }

            // ポジションの設定
            setParticlePos( create_pos );
        }

        /// <summary>
        /// パーティクルシステムの更新
        /// </summary>
        public void particleUpdate( ) {
            // パーティクルの更新
            _particle_time += Time.deltaTime;
            
            switch( _particle_type ) {
                case PARTICLE_TYPE.PARTICLE_OCEANCURRENT:
				    if( _particle_time > _change_phase_time_list[ 0 ] ) {
                        //　パーティクルの停止
					    _particle.GetComponent< ParticleEmitter >( ).emit = false;
				    }
                    break;
            }
				
            // パーティクル終了処理
            if ( _particle_time > _change_phase_time_list[ _change_phase_time_list.Count - 1 ] ) {
                // パーティクルの削除
				_particle_time = 0.0f;
                Destroy( _particle );
				_particle = null;
				_particle_phase = 0;
                _particle_end = true;
                
			}

        }

        /// <summary>
        /// パーティクル再生時間によってフェイズを切り替える
        /// </summary>
        public void particlePhaseUpdate( ) {
            if ( _particle_phase + 1 < _change_phase_time_list.Count ) { 
                // 更新時間が0秒の時
		        if ( _particle_time == 0.0f ) {
			        _particle_phase = 0;
		        }
                // 指定の区間に入ったらフェイズを更新
                else if ( _change_phase_time_list[ _particle_phase ] < _particle_time &&
                            _change_phase_time_list[ _particle_phase + 1 ] > _particle_time ) {
                    _phase_change = true;
			        _particle_phase += 1;
		        } else {
                    _phase_change = false;
                }
            }
	    }

        /// <summary>
        /// パーティクルの座標を設定
        /// </summary>
        /// <param name="pos"></param>
        public void setParticlePos( Vector3 pos ) {
            _particle.transform.position = pos;
        }

        /// <summary>
        /// パーティクルタイプを取得
        /// </summary>
        /// <returns></returns>
		public PARTICLE_TYPE getParticleType( ) {
			return _particle_type;
		}

        /// <summary>
        /// パーティクルオブジェクトを取得
        /// </summary>
        /// <returns></returns>
		public GameObject getParticleObject( ) {
			return _particle;
		}

        public int getParticlePhase( ) {
            return _particle_phase;
        }

        public bool isPhaseChange( ) {
            return _phase_change;
        }
        
        public bool isParticleEnd( ) {
            return _particle_end;
        }

	};
    
    // プレイヤーの移動を開始する渦潮パーティクルのフェイズ
    public int PLAYER_MOVE_START_PHASE_ON_SPIRAL = 2;

    private GraphicManager _graphic_manager;
    // パーティクルのプレハブ
	[ SerializeField ]
    private GameObject[ ] _particle_prefs = new GameObject[ ( int )PARTICLE_TYPE.MAX_PARTICLE_NUM ];
    // パーティクルクラスのリスト
    private List< ParticleOperate > _particle_operates = new List< ParticleOperate >( );
    // 削除予定のパーティクルの数
    private List< int > _delete_particle_num = new List< int >( );
    // 種別によるパーティクルの生成限界数
    private int[ ] _limit_create_nums = new int[ ( int )PARTICLE_TYPE.MAX_PARTICLE_NUM ] { 0, 1, 1, 7, 7, 3 };
    private float _fire_update_count  = 0;
    
    public void init( ref GraphicManager graphic_manager ) {
        _graphic_manager = graphic_manager;
        loadParticle( );
        timeArrayInsure( );
    }
	
    /// <summary>
    /// プレハブのロード
    /// </summary>
    private void loadParticle( ) {
        for ( int i = 1; i < ( int )PARTICLE_TYPE.MAX_PARTICLE_NUM; i++ ) {
			_particle_prefs[ i ] = _graphic_manager.loadPrefab( "Particle/Effect_" + i );
        }
    }

    /// <summary>
    /// 各パーティクルの時間を確保
    /// </summary>
    private void timeArrayInsure( ) {
        // 海流
        _particle_time_list[ ( int )PARTICLE_TYPE.PARTICLE_OCEANCURRENT ] = new float[ ] {
            OCEAN_CURRENT_STOP_TIME,
            OCEAN_CURRENT_DESTROY_TIME
        };
        // 渦潮
        _particle_time_list[ ( int )PARTICLE_TYPE.PARTICLE_SPIRAL ] = new float[ ] {
            SPIRAL_TIME_ONE,
            SPIRAL_TIME_TWO,
            SPIRAL_TIME_THREE,
            SPIRAL_TIME_FOUR
        };
        // 花火1
        _particle_time_list[ ( int )PARTICLE_TYPE.PARTICLE_FIREWORKS1 ] = new float[ ] {
            GOAL_PARTICLE_WAIT_TIME
        };
        // 花火2
        _particle_time_list[ ( int )PARTICLE_TYPE.PARTICLE_FIREWORKS2 ] = new float[ ] {
            GOAL_PARTICLE_WAIT_TIME
        };
        // 泡
        _particle_time_list[ ( int )PARTICLE_TYPE.PARTICLE_BUBBLE ] = new float[ ] {
            BUBBLE_TIME
        };
    }
    
    /// <summary>
    /// パーティクルの生成
    /// </summary>
    /// <param name="type"></param>
    public void createParticle( PARTICLE_TYPE type ) {
        ParticleOperate particle = new ParticleOperate( type, 
                                                        ref _particle_time_list[ ( int )type ],
                                                        ref _particle_prefs[ ( int )type ] );

        _particle_operates.Add( particle );
    }

    /// <summary>
    /// パーティクルの更新
    /// </summary>
    public void particleUpdate( ) {
        // 各パーティクルの更新
        for ( int i = 0; i < _particle_operates.Count; i++ ) {
            _particle_operates[ i ].particlePhaseUpdate( );
            _particle_operates[ i ].particleUpdate( );
        }

    }

    /// <summary>
    /// パーティクルオペレイターの削除
    /// </summary>
    public void finishParticle( ) {
        int count = 0;

        // パーティクルの削除
        if ( _delete_particle_num.Count > 0 ) {
            for ( int i = 0; i < _delete_particle_num.Count; i++ ) {
                _particle_operates.RemoveAt( _delete_particle_num[ i - count ] );
                count++;
            }
            _delete_particle_num.Clear( );
        }
    }
    
    /// <summary>
    /// 指定のパーティクルの座標を設定
    /// </summary>
    /// <param name="num"></param>
    /// <param name="pos"></param>
    public void setParticlePos( int num, Vector3 pos ) {
        _particle_operates[ num ].setParticlePos( pos );
    }

	/// <summary>
	/// 指定したパーティクルの取得
	/// </summary>
	public GameObject[ ] getParticlesForType( PARTICLE_TYPE type ) {

        int count = 0;
        List< int > obj_num = new List< int >( );

        // 同じタイプのオブジェクト番号を登録
		for( int i = 0; i < _particle_operates.Count; i++ ) {
			if ( _particle_operates[ i ].getParticleType ( ) == type ) {
                obj_num.Add( i );
                count++;
			}
		}
        
		GameObject[ ] obj = new GameObject[ count ];
        for( int i = 0; i < count; i++ ) {
			obj[ i ] = _particle_operates[ obj_num[ i ] ].getParticleObject( );
		} 
		return obj;
	}

    /// <summary>
    /// 生成制限回数を返す
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public int getLimitCreateNum( PARTICLE_TYPE type ) {
        return _limit_create_nums[ ( int )type ];
    }

    /// <summary>
    /// 指定したタイプの配列番号を返す
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public List< int > getParticleNumsForType( PARTICLE_TYPE type ) {
        List< int > num_list = new List< int >( );

        // 同じタイプのパーティクルを取得
        for( int i = 0; i < _particle_operates.Count; i++ ) {
			if ( _particle_operates[ i ].getParticleType ( ) == type ) {
				num_list.Add( i );
			}
		}

        return num_list;
    }

    /// <summary>
    /// 指定した配列番号のパーティクルのフェイズを返す
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public int getParticlePhase( int num ) {
        return _particle_operates[ num ].getParticlePhase( );
    }

    /// <summary>
    /// 指定した配列番号のパーティクルのフェイズが切り替わったかどうか
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public bool isPhaseChange( int num ) {
        return _particle_operates[ num ].isPhaseChange( );
    }

    /// <summary>
    /// パーティクルが終了したかどうかを返す
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public bool isFinshParticle( int[ ] num ) {
        int count = 0;

        for( int i = 0; i < _particle_operates.Count; i++ ) {
            if ( _particle_operates[ i ].isParticleEnd( ) ) {
                count++;
                if ( !_delete_particle_num.Contains( i ) ) {
                    _delete_particle_num.Add( i );
                }
            }
        }

        // 指定したすべてのパーティクルが終了したら
        if ( count == num.Length ) {
            return true;
        }

        return false;
    }

    /// <summary>
    /// パーティクルが存在してるかどうかを返す
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public bool isParticleOperates( int num ) {
        if( _particle_operates[ num ] == null ) {
            return false;
        } else { 
            return true;
        }
    }

	/// <summary>
	/// 指定したパーティクルの削除
	/// </summary>
	public void particleTypeDelete( PARTICLE_TYPE type ) {
        int count = 0;
        int length = _particle_operates.Count;

		for( int i = 0; i < length; i++ ) {
			if ( _particle_operates[ i - count ].getParticleType( ) == type ) {
                Destroy( _particle_operates[ i - count ].getParticleObject( ) );
				_particle_operates.RemoveAt( i - count );
                count++;
                
			}
		}
	}

    /// <summary>
    /// 花火の精製タイミング
    /// </summary>
    /// <returns></returns>
    public bool isCretateFireTiming( ) {
        _fire_update_count += Time.deltaTime;
        if( _fire_update_count > GOAL_PARTICLE_UPDATE_TIME ) {
            _fire_update_count = 0;
            return true;
        } else {
            return false;
        }

    }

    /*
    //　花火
    private int _particle_init_count = 0;
    if(_particle_init_time > GOAL_PARTICLE_WAIT_TIME) {
                    int rand = Random.Range(0, 2);
                    GameObject road_particle = ( rand == 1 ) ? _fireworks_1 : _fireworks_2;
                    _particle_init_count++;
                }
                _particle_time += Time.deltaTime;
*/
}

