using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionEngine;
using MotionEngine.Res;

namespace MotionGame
{
	/// <summary>
	/// 音频层级
	/// </summary>
	public enum EAudioLayer
	{
		Music, // 音乐
		Ambient, // 环境
		Voice, //语音
		Sound, // 音效
	}

	/// <summary>
	/// 音频管理器
	/// </summary>
	public sealed class AudioManager : IModule
	{
		/// <summary>
		/// 音频源封装类
		/// </summary>
		private class AudioSourceWrapper
		{
			public GameObject Go { private set; get; }
			public AudioSource Source { private set; get; }
			public AudioSourceWrapper(string name, Transform emitter)
			{
				// Create an empty game object
				Go = new GameObject(name);
				Go.transform.position = emitter.position;
				Go.transform.parent = emitter;

				// Create the source
				Source = Go.AddComponent<AudioSource>();
				Source.volume = 1.0f;
				Source.pitch = 1.0f;
			}
		}


		public static readonly AudioManager Instance = new AudioManager();

		/// <summary>
		/// 资源集合
		/// </summary>
		private readonly Dictionary<string, AssetAudio> _assets = new Dictionary<string, AssetAudio>(500);

		/// <summary>
		/// 音频源集合
		/// </summary>
		private readonly Dictionary<EAudioLayer, AudioSourceWrapper> _audioSourceWrappers = new Dictionary<EAudioLayer, AudioSourceWrapper>(200);

		/// <summary>
		/// 音频源的ROOT
		/// </summary>
		private GameObject _root;

		/// <summary>
		/// Resources文件夹下音频文件夹的相对路径
		/// </summary>
		public string BaseFolderPath = "Audio/";


		private AudioManager()
		{
		}
		public void Awake()
		{
			_root = new GameObject("[AudioManager]");
			UnityEngine.Object.DontDestroyOnLoad(_root);

			foreach (int value in System.Enum.GetValues(typeof(EAudioLayer)))
			{
				EAudioLayer layer = (EAudioLayer)value;
				_audioSourceWrappers.Add(layer, new AudioSourceWrapper(layer.ToString(), _root.transform));
			}
		}
		public void Start()
		{
		}
		public void Update()
		{
			// 背景音乐的淡入淡出
			UpdateFadeEffect(Time.deltaTime);
		}
		public void LateUpdate()
		{
		}
		public void OnGUI()
		{
			Engine.GUILable($"Audio total count : {_assets.Count}");
		}

		/// <summary>
		/// 预加载音频资源
		/// </summary>
		public void PreloadAsset(string name, int audioType)
		{
			if (_assets.ContainsKey(name) == false)
			{
				AssetAudio asset = new AssetAudio(audioType);
				_assets.Add(name, asset);
				asset.Load(BaseFolderPath + name, null);
			}
		}

		/// <summary>
		/// 释放所有音频资源
		/// </summary>
		public void ReleaseAll()
		{
			foreach (KeyValuePair<string, AssetAudio> pair in _assets)
			{
				pair.Value.UnLoad();
			}
			_assets.Clear();
		}

		/// <summary>
		/// 释放指定类型音频资源
		/// </summary>
		/// <param name="audioType">音频类型</param>
		public void Release(EAudioLayer audioType)
		{
			List<string> removeList = new List<string>();
			foreach (KeyValuePair<string, AssetAudio> pair in _assets)
			{
				if (pair.Value.AudioTag == (int)audioType)
					removeList.Add(pair.Key);
			}

			for (int i = 0; i < removeList.Count; i++)
			{
				string key = removeList[i];
				_assets[key].UnLoad();
				_assets.Remove(key);
			}
		}

		/// <summary>
		/// 播放背景音乐
		/// </summary>
		/// <param name="name">资源名称</param>
		/// <param name="loop">是否循环播放</param>
		public void PlayMusic(string name, bool loop)
		{
			if (string.IsNullOrEmpty(name))
				return;

			if (_assets.ContainsKey(name))
			{
				if (_assets[name].Result == EAssetResult.OK)
				{
					PlayAudioClipInternal(EAudioLayer.Music, _assets[name].Clip, loop);
					PlayFadeEffect(EFadeMode.FadeIn);
				}
			}
			else
			{
				// 新建加载资源
				AssetAudio assetAudio = new AssetAudio((int)EAudioLayer.Music);
				_assets.Add(name, assetAudio);
				assetAudio.Load(BaseFolderPath + name, (Asset asset, EAssetResult result) =>
				{
					if (result == EAssetResult.OK)
					{
						PlayAudioClipInternal(EAudioLayer.Music, _assets[name].Clip, loop);
						PlayFadeEffect(EFadeMode.FadeIn);
					}
				});
			}
		}

		/// <summary>
		/// 播放环境音效
		/// </summary>
		/// <param name="name">资源名称</param>
		/// <param name="loop">是否循环播放</param>
		public void PlayAmbient(string name, bool loop)
		{
			if (string.IsNullOrEmpty(name))
				return;

			if (_assets.ContainsKey(name))
			{
				if (_assets[name].Result == EAssetResult.OK)
					PlayAudioClipInternal(EAudioLayer.Ambient, _assets[name].Clip, loop);
			}
			else
			{
				// 新建加载资源
				AssetAudio assetAudio = new AssetAudio((int)EAudioLayer.Ambient);
				_assets.Add(name, assetAudio);
				assetAudio.Load(BaseFolderPath + name, (Asset asset, EAssetResult result) =>
				{
					if (result == EAssetResult.OK)
						PlayAudioClipInternal(EAudioLayer.Ambient, _assets[name].Clip, loop);
				});
			}
		}

		/// <summary>
		/// 播放语音
		/// </summary>
		/// <param name="name">资源名称</param>
		public void PlayVoice(string name)
		{
			if (string.IsNullOrEmpty(name))
				return;

			// 如果静音状态直接跳过播放
			if (IsMute(EAudioLayer.Voice))
				return;

			if (_assets.ContainsKey(name))
			{
				if (_assets[name].Result == EAssetResult.OK)
					PlayAudioClipInternal(EAudioLayer.Voice, _assets[name].Clip, false);
			}
			else
			{
				// 新建加载资源
				AssetAudio assetAudio = new AssetAudio((int)EAudioLayer.Voice);
				_assets.Add(name, assetAudio);
				assetAudio.Load(BaseFolderPath + name, (Asset asset, EAssetResult result) =>
				{
					if (result == EAssetResult.OK)
						PlayAudioClipInternal(EAudioLayer.Voice, _assets[name].Clip, false);
				});
			}
		}

		/// <summary>
		/// 播放音效
		/// </summary>
		/// <param name="name">资源名称</param>
		public void PlaySound(string name)
		{
			if (string.IsNullOrEmpty(name))
				return;

			// 如果静音状态直接跳过播放
			if (IsMute(EAudioLayer.Sound))
				return;

			if (_assets.ContainsKey(name))
			{
				if (_assets[name].Result == EAssetResult.OK)
					PlayAudioClipInternal(EAudioLayer.Sound, _assets[name].Clip, false);
			}
			else
			{
				// 新建加载资源
				AssetAudio assetAudio = new AssetAudio((int)EAudioLayer.Sound);
				_assets.Add(name, assetAudio);
				assetAudio.Load(BaseFolderPath + name, (Asset asset, EAssetResult result) =>
				{
					if (result == EAssetResult.OK)
						PlayAudioClipInternal(EAudioLayer.Sound, _assets[name].Clip, false);
				});
			}
		}

		/// <summary>
		/// 使用外部音频源播放音效
		/// </summary>
		/// <param name="audio">外部的音频源</param>
		/// <param name="name">资源名称</param>
		public void PlaySound(AudioSource audio, string name)
		{
			if (audio == null) return;
			if (audio.isActiveAndEnabled == false) return;
			if (string.IsNullOrEmpty(name)) return;

			// 如果静音状态直接跳过播放
			if (IsMute(EAudioLayer.Sound))
				return;

			if (_assets.ContainsKey(name))
			{
				if (_assets[name].Result == EAssetResult.OK)
				{
					if (audio != null)
						audio.PlayOneShot(_assets[name].Clip);
				}
			}
			else
			{
				// 新建加载资源
				AssetAudio assetAudio = new AssetAudio((int)EAudioLayer.Sound);
				_assets.Add(name, assetAudio);
				assetAudio.Load(BaseFolderPath + name, (Asset asset, EAssetResult result) =>
				{
					if (result == EAssetResult.OK)
					{
						if (audio != null)
							audio.PlayOneShot(_assets[name].Clip);
					}
				});
			}
		}

		/// <summary>
		/// 暂停播放
		/// </summary>
		public void Stop(EAudioLayer layer)
		{
			_audioSourceWrappers[layer].Source.Stop();
		}

		/// <summary>
		/// 设置静音
		/// </summary>
		public void Mute(bool isMute)
		{
			foreach (KeyValuePair<EAudioLayer, AudioSourceWrapper> pair in _audioSourceWrappers)
			{
				pair.Value.Source.mute = isMute;
			}
		}

		/// <summary>
		/// 设置静音
		/// </summary>
		public void Mute(EAudioLayer layer, bool isMute)
		{
			_audioSourceWrappers[layer].Source.mute = isMute;
		}

		/// <summary>
		/// 查询是否静音
		/// </summary>
		public bool IsMute(EAudioLayer layer)
		{
			return _audioSourceWrappers[layer].Source.mute;
		}

		/// <summary>
		/// 设置音量
		/// </summary>
		public void Volume(EAudioLayer layer, float volume)
		{
			volume = Mathf.Clamp01(volume);
			_audioSourceWrappers[layer].Source.volume = volume;
		}


		private void PlayAudioClipInternal(EAudioLayer layer, AudioClip clip, bool isLoop)
		{
			if (clip == null)
				return;

			if (layer == EAudioLayer.Music || layer == EAudioLayer.Ambient || layer == EAudioLayer.Voice)
			{
				_audioSourceWrappers[layer].Source.clip = clip;
				_audioSourceWrappers[layer].Source.loop = isLoop;
				_audioSourceWrappers[layer].Source.Play();
			}
			else if (layer == EAudioLayer.Sound)
			{
				_audioSourceWrappers[layer].Source.PlayOneShot(clip);
			}
			else
			{
				throw new NotImplementedException($"{layer}");
			}
		}

		#region 背景音乐淡入淡出相关
		private enum EFadeMode
		{
			None,
			FadeIn,
			FadeOut,
		}
		private const float FADE_IN_TIME = 6f;
		private const int FADE_OUT_FRAME = 60;

		private EFadeMode _fadeMode = EFadeMode.None;
		private float _fadeTimer = 0f;
		private int _fadeFrame = 0;

		private void PlayFadeEffect(EFadeMode fadeMode)
		{
			_fadeTimer = 0f;
			_fadeFrame = 0;
			_fadeMode = fadeMode;
		}
		private void UpdateFadeEffect(float deltaTime)
		{
			if (_fadeMode == EFadeMode.None)
				return;

			AudioSource audioSource = _audioSourceWrappers[EAudioLayer.Music].Source;
			if (audioSource == null) return;

			if (_fadeMode == EFadeMode.FadeIn)
			{
				_fadeTimer += deltaTime;
				if (_fadeTimer < FADE_IN_TIME + 0.5f)
					audioSource.volume = _fadeTimer / FADE_IN_TIME;
				else
					_fadeMode = EFadeMode.None;
			}
			else if (_fadeMode == EFadeMode.FadeOut)
			{
				_fadeFrame++;
				if (_fadeFrame <= FADE_OUT_FRAME)
					audioSource.volume = 1f - (float)_fadeFrame / FADE_OUT_FRAME;
				else
					_fadeMode = EFadeMode.None;
			}
		}
		#endregion
	}
}