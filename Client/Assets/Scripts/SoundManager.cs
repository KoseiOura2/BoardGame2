using UnityEngine;
using Common;
using System.Collections;
using System;

// 音量クラス
[ Serializable ]
public class SoundVolume {
	public float bgm = 1.0f;
	public float voice = 1.0f;
	public float se = 1.0f;
	public bool mute = false;
	
	public void init( ) {
		bgm = 1.0f;
		voice = 1.0f;
		se = 1.0f;
		mute = false;
    }
}

public class SoundManager : Manager< SoundManager > {

    // 音量設定
    public SoundVolume _volume = new SoundVolume( );

    // === AudioSource ===
    // BGM
    private AudioSource _bgm_source;
    // SE
    private AudioSource[ ] _se_sources = new AudioSource[ 10 ];
    // 音声(多分使わない）
    private AudioSource[ ] _voice_sources = new AudioSource[ 2 ];
     
    // === AudioClip ===
    // BGM
    [SerializeField]
    private AudioClip[ ] _bgm;
    // SE   
    [SerializeField]          
    private AudioClip[ ] _se;
    // 音声(多分使わない）
    [SerializeField]
    private AudioClip[ ] _voice;

    // Awake関数の代わり
	protected override void initialize( ) {
		init( );
	}

    void init( ) {
        loadAudioOption( );
        loadAudioSource( );
	}

    void loadAudioOption( ) {
        // BGM AudioSource
        _bgm_source = gameObject.AddComponent< AudioSource >( );
        // BGMはループを有効にする
        _bgm_source.loop = true;
        // SE AudioSource
        for( int i = 0 ; i < _se_sources.Length ; i++ ) {
         _se_sources[ i ] = gameObject.AddComponent< AudioSource >( );
        }
         
        // 音声 AudioSource
        for( int i = 0 ; i < _voice_sources.Length ; i++ ) {
         _voice_sources[ i ] = gameObject.AddComponent< AudioSource >( );
        }
    }

    void loadAudioSource( ) {
        // === BGMファイル設定 ===
        Array bgm_file = Enum.GetValues( typeof( BGM_TYPE ) );
        
        //BGMリスト数を設定
        _bgm = new AudioClip[ bgm_file.Length ];
        
        //BGMファイル設定
        for(int i = 1; i < bgm_file.Length; i++ ) {
            //こんな感じにする予定 BGMはBGM_01などにファイル名を変更します
            if( i <= 9 ) {
                //_bgm[ i ] = ( AudioClip )Resources.Load( "Audio/BGM/BGM_0" + i );
            } else {
                //_bgm[ i ] = ( AudioClip )Resources.Load( "Audio/BGM/BGM_" + i );
            }
        }
        _bgm[ ( int )BGM_TYPE.BGM_FIELD ] = ( AudioClip )Resources.Load( "Audio/BGM/field" );
        _bgm[ ( int )BGM_TYPE.BGM_TILTE ] = ( AudioClip )Resources.Load( "Audio/BGM/title" );
        _bgm[ ( int )BGM_TYPE.BGM_GAME_END2 ] = ( AudioClip )Resources.Load( "Audio/BGM/gameend2" );

        // === SEファイル設定 ===
        Array se_file = Enum.GetValues( typeof( SE_TYPE ) );

        //SEリスト数を設定
        _se = new AudioClip[ se_file.Length ];

        //SEファイル設定
        _se[ ( int )SE_TYPE.SE_BUFF ] = ( AudioClip )Resources.Load( "Audio/SE/buff" );
        _se[ ( int )SE_TYPE.SE_BUTTLE ] = ( AudioClip )Resources.Load( "Audio/SE/buttle" );
        _se[ ( int )SE_TYPE.SE_CHOICE1 ] = ( AudioClip )Resources.Load( "Audio/SE/choice1" );
        _se[ ( int )SE_TYPE.SE_CHOICE2 ] = ( AudioClip )Resources.Load( "Audio/SE/choice2" );
        _se[ ( int )SE_TYPE.SE_DRAW ] = ( AudioClip )Resources.Load( "Audio/SE/draw" );
        _se[ ( int )SE_TYPE.SE_FISH1 ] = ( AudioClip )Resources.Load( "Audio/SE/fish" );
        _se[ ( int )SE_TYPE.SE_FISH2 ] = ( AudioClip )Resources.Load( "Audio/SE/fish2" );
        _se[ ( int )SE_TYPE.SE_MASS ] = ( AudioClip )Resources.Load( "Audio/SE/mass" );
        _se[ ( int )SE_TYPE.SE_PHASE ] = ( AudioClip )Resources.Load( "Audio/SE/phase" );
        _se[ ( int )SE_TYPE.SE_TRESURE ] = ( AudioClip )Resources.Load( "Audio/SE/tresure" );
		_se[ ( int )SE_TYPE.SE_RESULT ] = ( AudioClip )Resources.Load( "Audio/SE/result" );

        // === VOICEファイル設定 ===
    }

    void soundConfig( ) {
        // ミュート設定
        _bgm_source.mute = _volume.mute;
        foreach( AudioSource source in _se_sources ) {
            source.mute = _volume.mute;
        }
        foreach( AudioSource source in _voice_sources ) {
            source.mute = _volume.mute;
        }
   
        // ボリューム設定
        _bgm_source.volume = _volume.bgm;
        foreach( AudioSource source in _se_sources ) {
            source.volume = _volume.se;
        }
        foreach( AudioSource source in _voice_sources ) {
            source.volume = _volume.voice;
        }
    }

    void Update( ) {
        soundConfig( );
    }

    // ***** BGM再生 *****
	/// <summary>
	/// BGM再生( 実行したいＢＧＭリストの中の曲 )
	/// </summary>
    public void playBGM( BGM_TYPE play_bgm ) {
        int play_index = ( int )play_bgm;

        if ( BGM_TYPE.BGM_NONE > play_bgm ) {
         return;
        }
        // 同じBGMの場合は何もしない
        if( _bgm_source.clip == _bgm[ play_index ] ) {
         return;
        }
        _bgm_source.Stop( );
        _bgm_source.clip = _bgm[ play_index ];
        _bgm_source.Play( );
    }
    
	/// <summary>
	/// 全てのBGMを停止します
	/// </summary>
     public void stopBGM( ) {
     	_bgm_source.Stop( );
     	_bgm_source.clip = null;
     }
     
     // ***** SE再生 *****
	/// <summary>
	/// SE再生( 実行したいSEリストの中の曲 )
	/// </summary>
     public void playSE( SE_TYPE play_se ) {
        int play_index = ( int )play_se;

        if( SE_TYPE.SE_NONE > play_se ) {
            return;
        }

        // 再生中で無いAudioSouceで鳴らす
        foreach ( AudioSource source in _se_sources ) {
            if ( false == source.isPlaying ) {
                source.clip = _se[ play_index ];
                source.Play( );
                return;
            }
        }
    }
  
	/// <summary>
	/// 全てのＳＥを停止します
	/// </summary>
    public void stopSE( ) {
        // 全てのSE用のAudioSouceを停止する
        foreach( AudioSource source in _se_sources ) {
           source.Stop( );
           source.clip = null;
        }
    }
      
     // ***** 音声再生 *****
	/// <summary>
	/// 音声再生( 実行したい音声リストの中の曲 )
	/// </summary>
     public void playVoice( VOICE_TYPE play_voice ) {
        int play_index = ( int )play_voice;

        if( VOICE_TYPE.VOICE_NONE > play_voice ){
            return;
        }
        // 再生中で無いAudioSouceで鳴らす
        foreach( AudioSource source in _voice_sources ) {
            if( false == source.isPlaying ){
                source.clip = _voice[ play_index ];
                source.Play( );
                return;
            }
        }
     }
      
	/// <summary>
	/// 全ての音声を停止します
	/// </summary>
     public void stopVoice( ) {
        // 全ての音声用のAudioSouceを停止する
        foreach( AudioSource source in _voice_sources ) {
            source.Stop( );
            source.clip = null;
        }
     }
}