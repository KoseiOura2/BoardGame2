using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Common;

public class ParticleManager : MonoBehaviour {
    
    // パーティクル関係
    private const float OCEAN_CURRENT_STOP_TIME    = 1.0f;
    private const float OCEAN_CURRENT_DESTROY_TIME = 1.5f;
    private const float SPIRAL_TIME_ONE            = 0.25f;
    private const float SPIRAL_TIME_TWO            = 1.0f;
    private const float SPIRAL_TIME_THREE          = 1.2f;
    private const float SPIRAL_TIME_FOUR           = 3.0f;
    private const float GOAL_PARTICLE_WAIT_TIME    = 0.5f;
	private const float LIGHTNING_TIME 			   = 1.0f;

    private float[ ][ ] _particle_time_list = new float[ ( int )PARTICLE_TYPE.MAX_PARTICLE_NUM ][ ];

    /// <summary>
    /// パーティクルシステムを操作するクラス  
    /// </summary>
    public class ParticleOperate {
        private GameObject _particle;
        private PARTICLE_TYPE _particle_type;
        private int _particle_phase  = 0;
        private List< float > _change_phase_time_list = new List< float >( );
	    private float _particle_time = 0.0f;
        private bool _particle_end   = false;

        public ParticleOperate( PARTICLE_TYPE type, ref float[ ] time_list, ref GameObject pref ) {
            // タイプを設定
            _particle_type = type;

            // フェイズ切り替え時間を登録
            for ( int i = 0; i < time_list.Length; i++ ) {
                _change_phase_time_list.Add( time_list[ i ] );
            }

            // パーティクルを生成
            _particle = ( GameObject )Instantiate( pref );
            _particle.transform.position = pref.transform.position;
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
			        _particle_phase += 1;
		        }
            }
	    }
        
        public bool isParticleEnd( ) {
            return _particle_end;
        }

		//パーティクルタイプを取得
		public PARTICLE_TYPE getParticleType( ) {
			return _particle_type;
		}

		//パーティクルオブジェクトを取得
		public GameObject getParticleObject( ) {
			return _particle;
		}

	};

	[ SerializeField ]
    private GameObject[ ] _particle_prefs = new GameObject[ ( int )PARTICLE_TYPE.MAX_PARTICLE_NUM ];
    private List< int > _delete_particle_num = new List< int >( );
    private List< ParticleOperate > _particle_operates = new List< ParticleOperate >( );
    
    public void init( ) {
        loadParticle( );
        timeArrayInsure( );
    }
	
    private void loadParticle( ) {
        for ( int i = 1; i < ( int )PARTICLE_TYPE.MAX_PARTICLE_NUM; i++ ) {
			_particle_prefs[ i ] = Resources.Load< GameObject >( "Prefabs/Particle/Effect_0" + i );
        }
    }

    /// <summary>
    /// 各パーティクルの時間を確保
    /// </summary>
    private void timeArrayInsure( ) {
        // 海流
        _particle_time_list[ ( int )PARTICLE_TYPE.PARTICLE_OCEANCURRENT ] = new float[ ]{
            OCEAN_CURRENT_STOP_TIME,
            OCEAN_CURRENT_DESTROY_TIME
        };
        // 渦潮
        _particle_time_list[ ( int )PARTICLE_TYPE.PARTICLE_SPIRAL ] = new float[ ]{
            SPIRAL_TIME_ONE,
            SPIRAL_TIME_TWO,
            SPIRAL_TIME_THREE,
            SPIRAL_TIME_FOUR
        };
        // 渦潮
        _particle_time_list[ ( int )PARTICLE_TYPE.PARTICLE_FIREWORKS ] = new float[ ]{
            GOAL_PARTICLE_WAIT_TIME
        };
		//点滅
		_particle_time_list[ ( int )PARTICLE_TYPE.PARTICLE_LIGHTNING ] = new float[ ]{
			LIGHTNING_TIME
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

            if ( _particle_operates[ i ].isParticleEnd( ) ) {
                _delete_particle_num.Add( i );
            }
        }

        // パーティクルの削除
        if ( _delete_particle_num.Count > 0 ) {
            for ( int i = 0; i < _delete_particle_num.Count; i++ ) {
                _particle_operates.RemoveAt( _delete_particle_num[ i ] );
            }
            _delete_particle_num.Clear( );
        }
    }

	/// <summary>
	/// 指定したパーティクルの取得
	/// </summary>
	public GameObject[ ] getParticle( PARTICLE_TYPE type ){
		GameObject[ ] obj = new GameObject[ _particle_operates.Count ];

		for( int i = 0; i < _particle_operates.Count; i++ ) {
			if ( _particle_operates [ i ].getParticleType ( ) == type ) {
				obj[ i ] = _particle_operates [ i ].getParticleObject( );
			}
		}
		return obj;
	}
}

