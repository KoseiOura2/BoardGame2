using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Common;

public class StageManager : MonoBehaviour {

    // 浅瀬の色
	private Color SHOAL_COLOR       = new Color( 116.0f / 255.0f, 193.0f / 255.0f, 223.0f / 255.0f );	
	// 中間の色
	private Color MESOPELAGIC_COLOR = new Color(  46.0f / 255.0f, 206.0f / 255.0f, 231.0f / 255.0f );	
	// 深海の色
	private Color DEEP_SEA_COLOR    = new Color(  52.0f / 255.0f, 137.0f / 255.0f, 225.0f / 255.0f );	
	// ｺﾞｰﾙの色
	private Color GOAL_COLOR        = new Color(  46.0f / 255.0f,  55.0f / 255.0f, 167.0f / 255.0f );		

	private const float SHOAL_SEA_DEPTH   =    0.0f;
	private const float MESOPELAGIC_DEPTH =  200.0f;
	private const float DEEP_SEA_DEPTH    = 1000.0f;
	private const float GOAL_DEPTH        = 1200.0f;
    
    private const int MAX_CREATE_BUBBLE_NUM = 3;
    private const float CREATE_BUBBLE_TIME  = 5.0f;

    private GraphicManager _graphic_manager;
    private ParticleManager _particle_manager;

	[ SerializeField ]
	private Light _main_light;
	private Color[ ] _sea_color = new Color[ 4 ];
    private List< Vector3 > _mass_pos_list = new List< Vector3 >( );
    
    // 環境情報
	private FIELD_ENVIRONMENT _environment = FIELD_ENVIRONMENT.NO_FIELD;

    private int _create_mass_count = 0;
	private int[ ] _sea_environment_id = new int[ ( int )FIELD_ENVIRONMENT.FIELD_ENVIRONMENT_NUM ]{ 0, 0, 0 };
	[ SerializeField ]
    private float _create_bubble_time = 0.0f;

	public void init( ref GraphicManager graphic_manager, ref ParticleManager particle_manager ) {
        // グラフィックマネージャーの参照
        _graphic_manager = graphic_manager;
        // パーティクルマネージャーの参照
        _particle_manager = particle_manager;
        
        // カラーの読み込み
		_sea_color = new Color[ ]{ SHOAL_COLOR, MESOPELAGIC_COLOR, DEEP_SEA_COLOR, GOAL_COLOR };
        
        // ライトのカラーを初期化
		GameObject light = GameObject.Find( "MainLight" ); 
		_main_light = light.GetComponent< Light >( );

		_main_light.color = SHOAL_COLOR;

        // マスリストの初期化
        _mass_pos_list.Clear( );
	}

    /// <summary>
    /// ゲーム開始時マスを生成
    /// </summary>
    /// <param name="num" "マス番号"></param>
    /// <param name="type" "マスのタイプ"></param>
    /// <param name="event_type" "イベントのタイプ"></param>
    /// <param name="pos" "生成する座標"></param>
	public void massCreate( int num, MASS_TYPE type, EVENT_TYPE event_type, Vector3 pos ) {
        // オブジェクトの生成
		_graphic_manager.createMassObj( num, type, event_type, pos );

        // マスのリストに追加
        _mass_pos_list.Add( pos );
    }

    public void createMiniMass( ) {
        _graphic_manager.createMiniMassObj( );
    }

    /// <summary>
    /// ライトカラーの更新
    /// </summary>
    /// <param name="environment"></param>
    /// <param name="mass_id"></param>
	public void updateLightColor( FIELD_ENVIRONMENT environment, int mass_id ) {
		int mass_num = 0;
		int player_pos = 0;
		float r = 0.0f;
		float g = 0.0f;
		float b = 0.0f;

		// 区間のマスの数と現在地を測定
		if ( environment == FIELD_ENVIRONMENT.DEEP_SEA_FIELD ) {
			mass_num   = getMassCount( ) - _sea_environment_id[ ( int )environment ];
			player_pos = getMassCount( ) - mass_id;
			r = ( _sea_color[ _sea_color.Length - 1 ].r - _sea_color[ ( int )environment ].r ) / mass_num;
			g = ( _sea_color[ _sea_color.Length - 1 ].g - _sea_color[ ( int )environment ].g ) / mass_num;
			b = ( _sea_color[ _sea_color.Length - 1 ].b - _sea_color[ ( int )environment ].b ) / mass_num;
		} else {
			mass_num   = _sea_environment_id[ ( int )environment + 1 ] - _sea_environment_id[ ( int )environment ] + 1;
			player_pos = _sea_environment_id[ ( int )environment + 1 ] - mass_id;
			r = ( _sea_color[ ( int )environment + 1 ].r - _sea_color[ ( int )environment ].r ) / mass_num;
			g = ( _sea_color[ ( int )environment + 1 ].g - _sea_color[ ( int )environment ].g ) / mass_num;
			b = ( _sea_color[ ( int )environment + 1 ].b - _sea_color[ ( int )environment ].b ) / mass_num;
		}

		// カラーの設定
		_main_light.color = new Color( _sea_color[ ( int )environment + 1 ].r - r * player_pos,
									   _sea_color[ ( int )environment + 1 ].g - g * player_pos,
									   _sea_color[ ( int )environment + 1 ].b - b * player_pos );

	}

    /// <summary>
    /// 泡パーティクルの更新
    /// </summary>
    public void updateBubble( ) {
        // timeの更新
        _create_bubble_time += Time.deltaTime;
        _particle_manager.particleUpdate( );

        // 動作しているパーティクルの確保
        List< int > bubble_list = _particle_manager.getParticleNumsForType( PARTICLE_TYPE.PARTICLE_BUBBLE );
        int[ ] bubble_array = new int[ bubble_list.Count ];
        for ( int i = 0; i < bubble_list.Count; i++ ) {
            bubble_array[ i ] = bubble_list[ i ];
        }

        if ( _particle_manager.isFinshParticle( bubble_array ) ) {
            _particle_manager.finishParticle( );
        }

        if ( _create_bubble_time >= CREATE_BUBBLE_TIME ) {
            // 現在生成されているパーティクル数を取得
            int created_num = _particle_manager.getParticlesForType( PARTICLE_TYPE.PARTICLE_BUBBLE ).Length;
		    if( created_num < _particle_manager.getLimitCreateNum( PARTICLE_TYPE.PARTICLE_BUBBLE ) ) {
                // 生成する数を計算
                int create_num = Random.Range( 1, MAX_CREATE_BUBBLE_NUM );
                if ( create_num + created_num > MAX_CREATE_BUBBLE_NUM ) {
                    create_num = MAX_CREATE_BUBBLE_NUM - created_num;
                }
                for ( int i = 0; i < create_num; i++ ) {
                    // パーティクルを生成
                    _particle_manager.createParticle( PARTICLE_TYPE.PARTICLE_BUBBLE );
                }
            }

            // timeの初期化
            _create_bubble_time = 0.0f;
        }
    }

	/// <summary>
	/// 背景オブジェのレンダラーをリフレッシュする
	/// </summary>
	public void refreshRendBackObj( ) {
		foreach ( GameObject obj in GameObject.FindGameObjectsWithTag( "NoRendObj" ) ) {
			obj.GetComponent< BackgroundObj >( ).rendRefresh( );
		}
	}

	public void setEnvironmentID( int id, FIELD_ENVIRONMENT environment ) {
		_sea_environment_id[ ( int )environment ] = id;
	}

    /// <summary>
    /// 進む先のマスを取得
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public Vector3 getTargetMassPos( int num ) {
        return _mass_pos_list[ num ];
    }

	public void increaseMassCount( ) {
		_create_mass_count++;
	}

	public int getMassCount( ) {
		return _create_mass_count;
	}

    public void initMassCount( ) {
        _create_mass_count = 0;
    }

	public void setEnvironment( FIELD_ENVIRONMENT environment ) {
		_environment = environment;
	}

	public FIELD_ENVIRONMENT getEnvironment( ) {
		return _environment;
	}
}
