﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Common;

public class StageManager : MonoBehaviour {

	private Color SHOAL_COLOR       = new Color( 116.0f / 255.0f, 193.0f / 255.0f, 223.0f / 255.0f );		// 浅瀬の色
	private Color MESOPELAGIC_COLOR = new Color(  46.0f / 255.0f, 206.0f / 255.0f, 231.0f / 255.0f );		// 中間の色
	private Color DEEP_SEA_COLOR    = new Color(  52.0f / 255.0f, 137.0f / 255.0f, 225.0f / 255.0f );		// 深海の色
	private Color GOAL_COLOR        = new Color(  46.0f / 255.0f,  55.0f / 255.0f, 167.0f / 255.0f );		// ｺﾞｰﾙの色

	private const float SHOAL_SEA_DEPTH   =    0.0f;
	private const float MESOPELAGIC_DEPTH =  200.0f;
	private const float DEEP_SEA_DEPTH    = 1000.0f;
	private const float GOAL_DEPTH        = 1200.0f;

    // シーンに配置するオブジェクト
    private GameObject _ground;
    private GameObject _background;
	[ SerializeField ]
	private List< GameObject > _mass_list = new List< GameObject >( );	//マスデータをリストとして保持しておく
	private List< GameObject > _chest_list = new List< GameObject >( );	//マスデータをリストとして保持しておく
    // シーンに配置するオブジェクトのプレハブ
    private GameObject _ground_pref;
    private GameObject _back_ground_pref;
	private GameObject _mass_prefab;	//マスデータロード用関数
    private GameObject _arrangement_prefab;

	[ SerializeField ]
	private Light _main_light;
    private int _create_mass_count = 0;
	private Color[ ] _sea_color = new Color[ 4 ];
	private int[ ] _sea_environment_id = new int[ ( int )FIELD_ENVIRONMENT.FIELD_ENVIRONMENT_NUM ]{ 0, 0, 0 };
	private FIELD_ENVIRONMENT _environment = FIELD_ENVIRONMENT.NO_FIELD;

	public void init( ) {
		_sea_color = new Color[ ]{ SHOAL_COLOR, MESOPELAGIC_COLOR, DEEP_SEA_COLOR, GOAL_COLOR };

		for( int i = 0; i < _mass_list.Count - 1; i++ ) {
			_mass_prefab = ( GameObject )Resources.Load( "Prefabs/Mass/mass_mini" );
			Vector3 pos = Vector3.Lerp( _mass_list[ i ].transform.localPosition, _mass_list[ i + 1 ].transform.localPosition, 0.5f );
			GameObject obj = ( GameObject )Instantiate( _mass_prefab, pos, _mass_prefab.transform.localRotation );
			obj.transform.SetParent( _mass_list[ i ].transform );
		}

		GameObject light = GameObject.Find( "MainLight" ); 
		_main_light = light.GetComponent< Light >( );

		_main_light.color = SHOAL_COLOR;
	}

    public void loadGraph( ) {
        _ground_pref = Resources.Load< GameObject >( "Prefabs/BackGroundObj/Terrain" );
        _back_ground_pref = Resources.Load< GameObject >( "Prefabs/BackGroundObj/BackGroundObj" );
        _arrangement_prefab = Resources.Load< GameObject >( "Prefabs/BackGroundObj/object_chest" );
    }

    public void createBackGroundObj( ) {
        _ground = ( GameObject )Instantiate( _ground_pref );
        _ground.transform.position = _ground_pref.transform.position;
        _background = ( GameObject )Instantiate( _back_ground_pref );
        _background.transform.position = _back_ground_pref.transform.position;
    }

	// Use this for initialization
	void Start( ) {
	
	}

    void massUpdate( ) {
        
    }

    //ゲーム開始時マスを生成
	public void massCreate( int num, MASS_TYPE type, EVENT_TYPE event_type, Vector3 pos ) {
		// タイプによるリソース分け
		switch ( type ) {
			case MASS_TYPE.MASS_START:
			case MASS_TYPE.MASS_GOAL:
				_mass_prefab = ( GameObject )Resources.Load( "Prefabs/Mass/mass_yellow" );
                break;
			case MASS_TYPE.MASS_NONE:
            case MASS_TYPE.MASS_NORMAL:
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

        switch( event_type ) {
            case EVENT_TYPE.EVENT_DRAW:
            case EVENT_TYPE.EVENT_TRAP_TWO:
                GameObject obj_arrangement = ( GameObject )Instantiate( _arrangement_prefab, new Vector3(obj.transform.localPosition.x,obj.transform.localPosition.y,obj.transform.localPosition.z + 2), _arrangement_prefab.transform.rotation );
                obj_arrangement.name = "TreasureChest:" + num;
                _chest_list.Add( obj_arrangement );
                break; 
        }

        // マネージャーの配下に設定
        obj.transform.parent = transform;
        _mass_list.Add( obj );
    }

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

	public void setEnvironmentID( int id, FIELD_ENVIRONMENT environment ) {
		_sea_environment_id[ ( int )environment ] = id;
	}

    //進む先のマスを取得
    public GameObject getTargetMass( int i ) {
        return _mass_list[ i ];
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
	
	// Update is called once per frame
	void Update( ) {
	
	}

    public bool resetMassColor ( int i ) {
        bool flag = false;

        if ( getTargetMass( i ).transform.localScale.x > 0.3f || getTargetMass( i ).transform.localScale.z > 0.3f ) {
            getTargetMass( i ).GetComponent< Renderer >( ).material.SetColor( "_Color", new Color ( 0.4f, 0.4f, 0.4f, 1f ) );
            getTargetMass( i ).transform.localScale = new Vector3 ( getTargetMass ( i ).transform.localScale.x - 0.05f,
                                                                     getTargetMass ( i ).transform.localScale.y,
                                                                     getTargetMass ( i ).transform.localScale.z - 0.05f );
        } else {
            getTargetMass( i ).transform.localScale = new Vector3 ( 0.3f, getTargetMass ( i ).transform.localScale.y, 0.3f );
            flag = false;
        }

        return flag;
    }

    public void destroyObj( ) {
        Destroy( _ground );
        Destroy( _background );

        for ( int i = 0; i < _mass_list.Count; i++ ) {
            if ( i < _mass_list.Count - 1 ) {
                Destroy( _mass_list[ i ].transform.GetChild( 0 ).gameObject );
            }
            Destroy( _mass_list[ i ] );
        }
        _mass_list.Clear( );

        for ( int i =0; i < _chest_list.Count; i++ ) {
            Destroy( _chest_list[ i ] );
        }
        _chest_list.Clear( );
    }
}
