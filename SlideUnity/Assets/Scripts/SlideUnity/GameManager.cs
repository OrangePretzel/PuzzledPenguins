using SlideCore;
using SlideCore.Data;
using SlideCore.Entities;
using SlideCore.Levels;
using SlideUnity.Entities;
using SlideUnity.Entities.Behaviours;
using SlideUnity.Game;
using SlideUnity.UI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Analytics;

namespace SlideUnity
{
	public enum GameState
	{
		MainMenu,
		PauseMenu,
		Playing,
		LevelPackMenu,
		LevelSelectMenu,
		OptionsMenu,
		LevelCompleteMenu,
		HelpMenu
	}

	public class SettingsManager
	{
		public System.Guid PlayerID;

		public const string PLAYER_ID_KEY = "PLAYER_ID";
		public const string PLAYER_SETTINGS_KEY = "PLAYER_SETTINGS";
		private const int DEFAULT_SETTINGS_INT = 1;

		public System.Action<SettingsManager> OnSettingsChanged;

		// TODO: Should make these readonly properties
		public bool ParticlesEnabled;

		public void LoadPlayerSettings()
		{
			// Get the player ID
			var playerIDString = PlayerPrefs.GetString(PLAYER_ID_KEY, null);
			if (string.IsNullOrWhiteSpace(playerIDString) || !System.Guid.TryParse(playerIDString, out PlayerID))
			{
				// No player ID (probably first launch), so generate a new player ID
				PlayerID = System.Guid.NewGuid();
				PlayerPrefs.SetString(PLAYER_ID_KEY, PlayerID.ToString());
			}

			var settingsInt = PlayerPrefs.GetInt(PLAYER_SETTINGS_KEY, DEFAULT_SETTINGS_INT);
			ParticlesEnabled = (settingsInt & (1 << 0)) != 0;
		}

		public void StorePlayerSettings()
		{
			int settingsInt = (ParticlesEnabled ? 1 : 0);
			PlayerPrefs.SetInt(PLAYER_SETTINGS_KEY, settingsInt);

			OnSettingsChanged?.Invoke(this);
		}

		public void ToggleParticlesEnabled()
		{
			ParticlesEnabled = !ParticlesEnabled;
			StorePlayerSettings();
		}
	}

	public class GameManager : MonoBehaviour
	{
		public class StaticSpriteObjectCollection : KeyedCollection<Vector2Int, StaticSpriteObject>
		{
			protected override Vector2Int GetKeyForItem(StaticSpriteObject item) =>
				new Vector2Int((int)item.transform.position.x, (int)item.transform.position.y);
		}

		public static class Events
		{
			public static System.Action<Level> OnLevelChanged;
		}

		private const float ANIMATION_RATE = 0.2f;

		private static GameManager Instance => _instance;
		private static GameManager _instance;

		public static GraphicsDictionary GraphicsDictionary => _instance?.GraphicsDictionaryObject;
		public static SettingsManager Settings => _instance?._settingsManager;
		public static bool Paused => _instance?._gameState == GameState.PauseMenu;

		[SerializeField]
		private InputManager InputManager = null;
		public static InputManager GetInputManager() => _instance?.InputManager;
		[SerializeField]
		public UIManager UIManager = null;

		[SerializeField]
		private TutorialController TutorialController;
		[SerializeField]
		private GameController GameController;

		[SerializeField]
		private GraphicsDictionary GraphicsDictionaryObject = null;
		[SerializeField]
		private Object StaticSpriteObjectPrefab = null;
		[SerializeField]
		private Object DynamicSpriteObjectPrefab = null;
		[SerializeField]
		private Object PlayerSpriteObjectPrefab = null;
		[SerializeField]
		private Object ParticleObjectPrefab = null;

		[SerializeField]
		private GameObject MainGameContainer = null;

		[SerializeField]
		private SpriteRenderer BackgroundRenderer = null;
		[SerializeField]
		private StaticSpriteObject[] CornerObjects = new StaticSpriteObject[4];
		[SerializeField]
		private StaticSpriteObject[] EdgeObjects = new StaticSpriteObject[4];

		private SettingsManager _settingsManager = new SettingsManager();

		private StaticSpriteObjectCollection _staticSpriteObjects = new StaticSpriteObjectCollection();
		private List<DynamicSpriteObject> _dynamicSpriteObjects = new List<DynamicSpriteObject>();
		private ObjectPool _staticSpriteObjectPool;
		private ObjectPool _dynamicSpriteObjectPool;
		private ObjectPool _playerSpriteObjectPool;
		private ObjectPool _particleObjectPool;
		private GameObject _levelContainer;

		public static Level CurrentLevel => _instance?._currentLevel;
		public static LevelPack CurrentLevelPack => _instance?._currentLevelPack;
		public static Vector3 LevelCenter => _instance?.GetLevelCenter() ?? Vector3.zero;
		public static GameState GameState => _instance?._gameState ?? GameState.MainMenu;

		private System.Action OnLevelStabilized;
		private LevelPack _currentLevelPack;
		private Level _currentLevel;

		private bool _updateFinished = true;
		private GameState _gameState = GameState.MainMenu;
		private bool _gameHidden = false;

		private GameController _currentGameController;

		#region Unity Hooks

		private void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Destroy(gameObject);
				return;
			}

			if (_instance == this)
				return;

			_instance = this;
			_settingsManager.LoadPlayerSettings();
			_settingsManager.OnSettingsChanged += settingsManager => ApplyPlayerSettings();

			DataManager.RegisterDataStore(new UnityDataStore());
			ContentManager.RegisterContentStore(new UnityContentStore());

			LevelManager.LoadLevelPacks();

			// Deactivate all game controllers
			TutorialController.DisableController();
			GameController.DisableController();
			SetGameController(GameController);

			SetupObjectPools();
			_levelContainer = new GameObject("Level Container");
			_levelContainer.transform.parent = MainGameContainer.transform;

			ApplyPlayerSettings();
		}

		private void Start()
		{
			GotoState(GameState.MainMenu);
		}

		private void FixedUpdate()
		{
			if (_gameState != GameState.Playing) return;

			if (!_updateFinished)
			{
				bool hasStabilized = true;
				foreach (var dynamicSpriteObject in _dynamicSpriteObjects)
					if (!dynamicSpriteObject.HasSettled)
					{
						hasStabilized = false;
						break;
					}

				if (!hasStabilized)
					foreach (var staticSpriteObject in _staticSpriteObjects)
						if (!staticSpriteObject.HasSettled)
						{
							hasStabilized = false;
							break;
						}

				if (hasStabilized) OnLevelStabilized?.Invoke();
			}
			else
			{
				var action = _currentGameController.GetNextPlayerAction();
				if (action != PlayerActions.None)
				{
					PerformPlayerAction(action);
					_currentGameController.ClearNextAction();
				}
			}
		}

		private void Update()
		{
			HandleDevHacks();
		}

		#endregion

		private void HandleDevHacks()
		{
			if (Input.GetKeyDown(KeyCode.P))
			{
				Debug.Log("DEV HACK: Locking All Levels");
				foreach (var levelPack in LevelManager.LevelPacks)
				{
					foreach (var level in levelPack.Levels)
					{
						if (level.ID == "Basics\\Level_01_1") level.SetStatus(Level.LevelInfo.LevelInfoStatus.Unsolved, true);
						else level.SetStatus(Level.LevelInfo.LevelInfoStatus.Locked, true);
					}
					levelPack.SaveProgressForPack();
				}
			}

			if (Input.GetKeyDown(KeyCode.O))
			{
				Debug.Log("DEV HACK: Solving All Levels");
				foreach (var levelPack in LevelManager.LevelPacks)
				{
					foreach (var level in levelPack.Levels)
					{
						level.SetStatus(Level.LevelInfo.LevelInfoStatus.Solved);
					}
					levelPack.SaveProgressForPack();
				}
			}

		}

		private void SetGameController(GameController gameController)
		{
			if (gameController == _currentGameController) return;

			_currentGameController?.DisableController();
			_currentGameController = gameController;
			_currentGameController?.EnableController();
		}

		private void ApplyPlayerSettings()
		{
			UIManager.ToggleParticles(_settingsManager.ParticlesEnabled);
		}

		private void PerformPlayerAction(PlayerActions action)
		{
			var aggregateUpdateResult = _currentLevel.DoPlayerAction(action);
			if (action == PlayerActions.Undo)
				ClearLevelStabilizedCallbacks();

			HandleUpdateResult(aggregateUpdateResult, action);
		}

		private void UpdateLevel()
		{
			if (_updateFinished) return;

			var aggregateUpdateResult = _currentLevel.UpdateLevel();
			HandleUpdateResult(aggregateUpdateResult);
		}

		private void InvokeEndOfUpdateCallbacks()
		{
			foreach (var dynamicSpriteObject in _dynamicSpriteObjects)
				dynamicSpriteObject.OnEndOfUpdate.Invoke();
			foreach (var staticSpriteObject in _staticSpriteObjects)
				staticSpriteObject.OnEndOfUpdate.Invoke();
		}

		private void HandleUpdateResult(AggregateUpdateResult aggregateUpdateResult, PlayerActions causalAction = PlayerActions.None)
		{
			switch (aggregateUpdateResult.Result)
			{
				case AggregateUpdateResult.ResultTypes.Cancelled:
					foreach (var dynamicSpriteObject in _dynamicSpriteObjects)
						dynamicSpriteObject.CancelAction(causalAction);
					foreach (var staticSpriteObject in _staticSpriteObjects)
						staticSpriteObject.CancelAction(causalAction);
					_updateFinished = true;
					break;
				case AggregateUpdateResult.ResultTypes.RequiresUpdate:
					_updateFinished = false;
					// TODO: Find a way to cache the following update loops?
					foreach (var dynamicSpriteObject in _dynamicSpriteObjects)
					{
						var updateResult = aggregateUpdateResult.EntityUpdateResults[dynamicSpriteObject.ID];
						dynamicSpriteObject.UpdateToResult(updateResult);
					}
					foreach (var staticSpriteObject in _staticSpriteObjects)
						if (aggregateUpdateResult.EntityUpdateResults.ContainsKey(staticSpriteObject.ID))
						{
							var updateResult = aggregateUpdateResult.EntityUpdateResults[staticSpriteObject.ID];
							staticSpriteObject.UpdateToResult(updateResult);
						}

					RegisterLevelStabilizedCallback(() =>
					{
						InvokeEndOfUpdateCallbacks();
						UpdateLevel();
					});
					break;
				case AggregateUpdateResult.ResultTypes.DoneUpdating:
					_updateFinished = false;
					foreach (var dynamicSpriteObject in _dynamicSpriteObjects)
					{
						var updateResult = aggregateUpdateResult.EntityUpdateResults[dynamicSpriteObject.ID];
						dynamicSpriteObject.UpdateToResult(updateResult);
					}
					foreach (var staticSpriteObject in _staticSpriteObjects)
						if (aggregateUpdateResult.EntityUpdateResults.ContainsKey(staticSpriteObject.ID))
						{
							var updateResult = aggregateUpdateResult.EntityUpdateResults[staticSpriteObject.ID];
							staticSpriteObject.UpdateToResult(updateResult);
						}

					RegisterLevelStabilizedCallback(() =>
					{
						InvokeEndOfUpdateCallbacks();
						_updateFinished = true;
					});
					break;
				case AggregateUpdateResult.ResultTypes.LevelComplete:
					_updateFinished = false;
					foreach (var dynamicSpriteObject in _dynamicSpriteObjects)
					{
						var updateResult = aggregateUpdateResult.EntityUpdateResults[dynamicSpriteObject.ID];
						dynamicSpriteObject.UpdateToResult(updateResult);
					}
					foreach (var staticSpriteObject in _staticSpriteObjects)
						if (aggregateUpdateResult.EntityUpdateResults.ContainsKey(staticSpriteObject.ID))
						{
							var updateResult = aggregateUpdateResult.EntityUpdateResults[staticSpriteObject.ID];
							staticSpriteObject.UpdateToResult(updateResult);
						}

					RegisterLevelStabilizedCallback(() =>
					{
						AnalyticsHelper.SendLevelCompleteAnalyticsEventAsync(_currentLevel);

						LevelManager.SolveLevel(_currentLevelPack, _currentLevel.Info.ID, _currentLevel.IsWithinTargetMoves, _currentLevel.HasCollectedAllSushi);
						_updateFinished = true;
						UIManager.SetLevelCompleteStars(_currentLevel.HasCollectedAllSushi, _currentLevel.IsWithinTargetMoves);
						UIManager.PlayLevelCompleteAnimation();
					});
					break;
				case AggregateUpdateResult.ResultTypes.UndoPerformed:
					foreach (var dynamicSpriteObject in _dynamicSpriteObjects)
					{
						var updateResult = aggregateUpdateResult.EntityUpdateResults[dynamicSpriteObject.ID];
						dynamicSpriteObject.UndoToResult(updateResult);
					}
					foreach (var staticSpriteObject in _staticSpriteObjects)
						if (aggregateUpdateResult.EntityUpdateResults.ContainsKey(staticSpriteObject.ID))
						{
							var updateResult = aggregateUpdateResult.EntityUpdateResults[staticSpriteObject.ID];
							staticSpriteObject.UndoToResult(updateResult);
						}

					_updateFinished = true;
					break;
				case AggregateUpdateResult.ResultTypes.FatalResult:
					_updateFinished = false;
					foreach (var dynamicSpriteObject in _dynamicSpriteObjects)
					{
						var updateResult = aggregateUpdateResult.EntityUpdateResults[dynamicSpriteObject.ID];
						dynamicSpriteObject.UpdateToResult(updateResult);
					}
					foreach (var staticSpriteObject in _staticSpriteObjects)
						if (aggregateUpdateResult.EntityUpdateResults.ContainsKey(staticSpriteObject.ID))
						{
							var updateResult = aggregateUpdateResult.EntityUpdateResults[staticSpriteObject.ID];
							staticSpriteObject.UpdateToResult(updateResult);
						}

					RegisterLevelStabilizedCallback(() =>
					{
						_updateFinished = true;
						Undo();
					});
					break;
				default:
					Debug.LogError($"Unimplemented AggregateUpdateResult {aggregateUpdateResult.Result}");
					break;
			}
		}

		private void RegisterLevelStabilizedCallback(System.Action callback)
		{
			System.Action OnLevelStabilizedCallback = null;
			OnLevelStabilizedCallback = () =>
			{
				callback?.Invoke();
				OnLevelStabilized -= OnLevelStabilizedCallback;
			};
			OnLevelStabilized += OnLevelStabilizedCallback;
		}

		private void ClearLevelStabilizedCallbacks()
		{
			OnLevelStabilized = null;
		}

		#region Object Pooling

		private void SetupObjectPools()
		{
			// Particle Pool
			var particleObjectPool = new GameObject("Particle Object Pool");
			_particleObjectPool = particleObjectPool.AddComponent<ObjectPool>();
			_particleObjectPool.SetAllocationFunction(() =>
			{
				var gObj = (GameObject)Instantiate(ParticleObjectPrefab, _particleObjectPool.transform);
				gObj.name = "Particle Object";
				var poolable = gObj.GetComponent<IPoolableObject>();
				return poolable;
			});
			_particleObjectPool.AllocateObjects(8);

			// Static Pool
			var staticObjectPool = new GameObject("Static Sprite Object Pool");
			_staticSpriteObjectPool = staticObjectPool.AddComponent<ObjectPool>();
			_staticSpriteObjectPool.SetAllocationFunction(() =>
			{
				var gObj = (GameObject)Instantiate(StaticSpriteObjectPrefab, _staticSpriteObjectPool.transform);
				gObj.name = "Static Sprite Object";
				var poolable = gObj.GetComponent<IPoolableObject>();
				return poolable;
			});
			_staticSpriteObjectPool.AllocateObjects(20);

			// Dynamic Pool
			var dynamicObjectPool = new GameObject("Dynamic Sprite Object Pool");
			_dynamicSpriteObjectPool = dynamicObjectPool.AddComponent<ObjectPool>();
			_dynamicSpriteObjectPool.SetAllocationFunction(() =>
			{
				var gObj = (GameObject)Instantiate(DynamicSpriteObjectPrefab, _dynamicSpriteObjectPool.transform);
				gObj.name = "Dynamic Sprite Object";
				var poolable = gObj.GetComponent<IPoolableObject>();
				return poolable;
			});
			_dynamicSpriteObjectPool.AllocateObjects(5);

			// Player Pool
			var playerObjectPool = new GameObject("Player Sprite Object Pool");
			_playerSpriteObjectPool = playerObjectPool.AddComponent<ObjectPool>();
			_playerSpriteObjectPool.SetAllocationFunction(() =>
			{
				var gObj = (GameObject)Instantiate(PlayerSpriteObjectPrefab, _playerSpriteObjectPool.transform);
				gObj.name = "Player Sprite Object";
				var poolable = gObj.GetComponent<IPoolableObject>();
				return poolable;
			});
			if (_settingsManager.ParticlesEnabled) // Only allocate if we should be using particles (we can always allocate more later)
				_playerSpriteObjectPool.AllocateObjects(2);
		}

		private StaticSpriteObject GetStaticSpriteObjectForEntity(StaticEntity entity)
		{
			var poolable = _staticSpriteObjectPool.GetObjectFromPool();
			var poolObj = (StaticSpriteObject)poolable;
			poolObj.transform.parent = _levelContainer.transform;
			poolObj.transform.localEulerAngles = Vector3.zero;
			poolObj.transform.localScale = Vector3.one;
			return poolObj;
		}

		private DynamicSpriteObject GetDynamicSpriteObjectForEntity(DynamicEntity entity)
		{
			var poolable = _dynamicSpriteObjectPool.GetObjectFromPool();
			var poolObj = (DynamicSpriteObject)poolable;
			poolObj.transform.parent = _levelContainer.transform;
			poolObj.transform.localEulerAngles = Vector3.zero;
			poolObj.transform.localScale = Vector3.one;
			return poolObj;
		}

		private PlayerSpriteObject GetPlayerSpriteObjectForEntity(PlayerEntity entity)
		{
			var poolable = _playerSpriteObjectPool.GetObjectFromPool();
			var poolObj = (PlayerSpriteObject)poolable;
			poolObj.transform.parent = _levelContainer.transform;
			poolObj.transform.localEulerAngles = Vector3.zero;
			poolObj.transform.localScale = Vector3.one;
			return poolObj;
		}

		public static ParticleObject GetSnowParticleSystem()
		{
			if (!_instance._settingsManager.ParticlesEnabled) return null;
			return ((ParticleObject)_instance._particleObjectPool.GetObjectFromPool());
		}

		#endregion

		private void InitializeStaticLevelEntities()
		{
			foreach (var entity in _currentLevel.StaticEntities)
			{
				StaticSpriteObject spriteObject;
				switch (entity.EntityType)
				{
					case EntityTypes.HaltTile:
					case EntityTypes.Portal:
					case EntityTypes.FinishFlag:
					case EntityTypes.Pit:
						// Get Object
						spriteObject = GetStaticSpriteObjectForEntity(entity);
						// Set Sprite
						SetGraphicForEntity(spriteObject, entity.EntityType);
						// Set Transform
						spriteObject.transform.position = entity.Position.ToVector2();
						break;

					case EntityTypes.Wall:
						// Convert Entity
						var wallEntity = (WallEntity)entity;
						// Get Object
						spriteObject = GetStaticSpriteObjectForEntity(entity);
						// Set Sprite
						var wallSprite = GraphicsDictionary.SeamlessWallGraphics.GetSpriteForLayout(wallEntity.AdjacentWalls);
						spriteObject.SetSprite(wallSprite.Sprite);
						// Set Transform
						spriteObject.transform.position = entity.Position.ToVector2();
						spriteObject.transform.localEulerAngles = new Vector3(
							spriteObject.transform.localEulerAngles.x,
							spriteObject.transform.localEulerAngles.y,
							wallSprite.Rotation * 90);
						spriteObject.transform.localScale = wallSprite.Scale;
						break;

					case EntityTypes.RedirectTile:
						// Convert Entity
						var redirectTileEntity = (RedirectTile)entity;
						// Get Object
						spriteObject = GetStaticSpriteObjectForEntity(entity);
						spriteObject.EntityBehaviour = FlashEntityBehaviour.Behaviour;
						// Set Sprite
						SetGraphicForEntity(spriteObject, entity.EntityType);
						// Set Transform
						spriteObject.transform.position = entity.Position.ToVector2();
						spriteObject.transform.localEulerAngles = new Vector3(
							spriteObject.transform.localEulerAngles.x,
							spriteObject.transform.localEulerAngles.y,
							redirectTileEntity.RedirectDir * 90);
						break;

					case EntityTypes.WireButton:
						// Get Object
						spriteObject = GetStaticSpriteObjectForEntity(entity);
						spriteObject.EntityBehaviour = WireButtonBehaviour.Behaviour;
						// Set Sprite
						SetGraphicForEntity(spriteObject, entity.EntityType);
						// Set Transform
						spriteObject.transform.position = entity.Position.ToVector2();
						break;

					case EntityTypes.WireDoor:
						// Get Object
						spriteObject = GetStaticSpriteObjectForEntity(entity);
						spriteObject.EntityBehaviour = WireDoorBehaviour.Behaviour;
						// Set Sprite
						SetGraphicForEntity(spriteObject, entity.EntityType);
						// Set Transform
						spriteObject.transform.position = entity.Position.ToVector2();
						break;

					case EntityTypes.JumpTile:
						// Get Object
						spriteObject = GetStaticSpriteObjectForEntity(entity);
						spriteObject.EntityBehaviour = JumpTileBehaviour.Behaviour;
						// Set Sprite
						SetGraphicForEntity(spriteObject, entity.EntityType);
						// Set Transform
						spriteObject.transform.position = entity.Position.ToVector2();
						break;

					case EntityTypes.Sushi:
						// Get Object
						spriteObject = GetStaticSpriteObjectForEntity(entity);
						spriteObject.EntityBehaviour = SushiBehaviour.Behaviour;
						// Set Transform
						spriteObject.transform.position = entity.Position.ToVector2();
						break;

					default:
						throw new System.NotSupportedException($"Static entity type [{entity.EntityType}] is not supported. Please implement UnityObject for entity.");
				}

				// Initialize
				spriteObject.EntityBehaviour?.InitializeEntity(spriteObject, entity);
				// Set ID, Entity, & Name
				spriteObject.ID = entity.ID;
				spriteObject.BackingEntity = entity;
				spriteObject.name = $"{entity.EntityType} at {entity.Position}";
				// Add to list
				_staticSpriteObjects.Add(spriteObject);
			}
		}

		private void InitializeDynamicEntities()
		{
			var allEntities = new List<DynamicEntity>();
			allEntities.AddRange(_currentLevel.PlayerEntities);
			allEntities.AddRange(_currentLevel.DynamicEntities);
			foreach (var entity in allEntities)
			{
				DynamicSpriteObject spriteObject;
				switch (entity.EntityType)
				{
					case EntityTypes.Player:
						// Get Object
						spriteObject = GetPlayerSpriteObjectForEntity((PlayerEntity)entity);
						spriteObject.EntityBehaviour = PlayerEntityBehaviour.Behaviour;
						break;

					case EntityTypes.SlidingCrate:
						// Get Object
						spriteObject = GetDynamicSpriteObjectForEntity(entity);
						spriteObject.EntityBehaviour = SlidingCrateBehaviour.Behaviour;
						break;

					default:
						throw new System.NotSupportedException($"Dynamic entity type [{entity.EntityType}] is not supported. Please implement UnityObject for entity.");
				}

				// Initialize
				spriteObject.EntityBehaviour.InitializeEntity(spriteObject, entity);
				// Update
				spriteObject.UpdateToEntity(entity);
				// Set ID, Entity, & Name
				spriteObject.ID = entity.ID;
				spriteObject.BackingEntity = entity;
				spriteObject.name = $"{entity.EntityType} ({entity.ID})";
				// Add to list
				_dynamicSpriteObjects.Add(spriteObject);
			}
		}

		// TODO: Move this logic to entity behaviours
		private void SetGraphicForEntity(StaticSpriteObject spriteObject, EntityTypes entityType)
		{
			switch (entityType)
			{
				case EntityTypes.Wall:
					spriteObject.SetSprite(GraphicsDictionary.SeamlessWallGraphics.NoAdjacentSides);
					break;
				case EntityTypes.RedirectTile:
					spriteObject.SetSprite(GraphicsDictionary.RedirectTileSprite);
					break;
				case EntityTypes.HaltTile:
					spriteObject.SetSprite(GraphicsDictionary.HaltTileSprite);
					break;
				case EntityTypes.Portal:
					spriteObject.SetSprite(GraphicsDictionary.PortalSprites, ANIMATION_RATE);
					break;
				case EntityTypes.FinishFlag:
					spriteObject.SetSprite(GraphicsDictionary.FinishFlagSprites, ANIMATION_RATE);
					break;
				case EntityTypes.Pit:
					spriteObject.SetSprite(GraphicsDictionary.PitSprite);
					break;
				case EntityTypes.WireDoor:
					spriteObject.SetSprite(GraphicsDictionary.PitSprite);
					break;
				case EntityTypes.WireButton:
					spriteObject.SetSprite(GraphicsDictionary.PitSprite);
					break;
				case EntityTypes.JumpTile:
					spriteObject.SetSprite(GraphicsDictionary.JumpTileSprites.JumpTileSprite_Normal);
					break;
				case EntityTypes.Player: // Graphic already set
				case EntityTypes.None: // No graphic
					break;
				default:
					throw new System.NotSupportedException($"Entity type [{entityType}] is not supported. Please implement graphic for entity.");
			}
		}

		private void LoadLevel(Level level)
		{
			if (_currentLevel != null)
				CleanupLevel();

			// Set new level
			_currentLevel = level;
			_currentLevel.InitializeLevelForGame();

			// Setup Game Controller
			const string TUTORIAL_LEVEL_ID = @"Basics\Level_01_1";
			// If it is the first level and it has never been solved before
			if (_currentLevel.Info.ID == TUTORIAL_LEVEL_ID && _currentLevel.Info.Status == Level.LevelInfo.LevelInfoStatus.Unsolved)
				SetGameController(TutorialController);
			else
				SetGameController(GameController);
			_currentGameController.Reset();

			// Setup Game
			ShowGame();
			SetupSceneForLevel();
			_updateFinished = true;

			AnalyticsEvent.LevelStart(level.Info.ID);
		}

		public Vector3 GetLevelCenter()
		{
			return new Vector3(
				(_currentLevel.LevelWidth / 2f) - 0.5f,
				(_currentLevel.LevelHeight / -2f) + 0.5f);
		}

		public static StaticSpriteObject GetStaticSpriteObjectFromPosition(Vector2Int position)
		{
			return _instance._staticSpriteObjects[position];
		}

		private void SetupSceneForLevel()
		{
			var levelCenter = GetLevelCenter();

			BackgroundRenderer.transform.position = levelCenter;
			BackgroundRenderer.size = new Vector2(_currentLevel.LevelWidth, _currentLevel.LevelHeight);

			PlaceEdges(levelCenter);
			InitializeStaticLevelEntities();
			InitializeDynamicEntities();

			Events.OnLevelChanged?.Invoke(_currentLevel);
		}

		// TODO: Make Edges just like regular static walls
		private void PlaceEdges(Vector3 levelCenter)
		{
			int[] cornerDirs = new int[] { -1, -1, 1, -1, 1, 1, -1, 1 };
			int[] edgeDirs = new int[] { -1, 0, 0, -1, 1, 0, 0, 1 };

			var halfWidth = _currentLevel.LevelWidth / 2f + 0.5f;
			var halfHeight = _currentLevel.LevelHeight / 2f + 0.5f;
			for (int i = 0; i < 4; i++)
			{
				CornerObjects[i].transform.position = new Vector3(
					cornerDirs[2 * i] * halfWidth,
					cornerDirs[2 * i + 1] * halfHeight) + levelCenter;
				EdgeObjects[i].transform.position = new Vector3(
					edgeDirs[2 * i] * halfWidth,
					edgeDirs[2 * i + 1] * halfHeight) + levelCenter;
				var width = _currentLevel.LevelWidth;
				var height = _currentLevel.LevelHeight;

				EdgeObjects[i].SpriteRenderer.size = new Vector2(i % 2 == 0 ? 1 : width, i % 2 == 0 ? height : 1);
			}
		}

		private void CleanupLevel()
		{
			ClearLevelStabilizedCallbacks();

			for (int i = _dynamicSpriteObjects.Count - 1; i >= 0; i--)
				_dynamicSpriteObjects[i].ReturnToPool();
			_dynamicSpriteObjects.Clear();

			for (int i = _staticSpriteObjects.Count - 1; i >= 0; i--)
				_staticSpriteObjects[i].ReturnToPool();
			_staticSpriteObjects.Clear();

			_currentLevel = null;
		}

		private PlayerActions GetPlayerAction() => _currentGameController?.GetNextPlayerAction() ?? PlayerActions.None;

		private void HideGame()
		{
			if (_gameHidden) return;
			MainGameContainer.transform.position = new Vector3(-100, -100, 100);
			_currentGameController?.DisableController();
			_gameHidden = true;
		}

		private void ShowGame()
		{
			if (!_gameHidden) return;
			MainGameContainer.transform.position = Vector3.zero;
			_currentGameController?.EnableController();
			_gameHidden = false;
		}

		public static void LoadLevelPack(string levelPackID)
		{
			try
			{
				_instance._currentLevelPack = LevelManager.GetLevelPack(levelPackID);
			}
			catch (System.Exception ex)
			{
				Debug.LogException(ex);
				return;
			}
		}

		public static void SetCurrentLevelPack(LevelPack levelPack)
		{
			if (levelPack == null) throw new System.Exception($"The provided level pack is null");
			_instance._currentLevelPack = levelPack;
		}

		public static void GotoLevel(Level.LevelInfo levelInfo)
		{
			GotoLevel(LevelManager.GetLevel(_instance._currentLevelPack, levelInfo.ID));
		}

		public static void GotoLevel(Level level)
		{
			Debug.Log($"Loading level {level.Info.ID}");
			_instance.LoadLevel(level);
			GotoState(GameState.Playing);
		}

		public async void GotoDailyLevel()
		{
			var level = await LevelManager.GetDailyLevelAsync(System.DateTime.Now);
			GotoLevel(level);
		}

		public static void Undo()
		{
			_instance.PerformPlayerAction(PlayerActions.Undo);
		}

		public static void RestartLevel()
		{
			AnalyticsHelper.SendCustomAnalyticsEventForLevel("action_restart", _instance._currentLevel);

			GotoState(GameState.Playing);
			var aggregateUpdateResult = CurrentLevel.ResetLevel();
			_instance.ClearLevelStabilizedCallbacks();
			_instance.HandleUpdateResult(aggregateUpdateResult, PlayerActions.Undo);
		}

		public static void GotoPreviousLevel()
		{
			Level.LevelInfo nextLevelInfo = CurrentLevel.Info;
			if (LevelManager.GetPreviousLevel(ref _instance._currentLevelPack, ref nextLevelInfo))
			{
				var prevLevel = LevelManager.GetLevel(CurrentLevelPack, nextLevelInfo.ID);
				GotoLevel(prevLevel);
				return;
			}

			GotoState(GameState.MainMenu);
		}

		public static void GotoNextLevel()
		{
			Level.LevelInfo nextLevelInfo = CurrentLevel.Info;
			if (LevelManager.GetNextLevel(ref _instance._currentLevelPack, ref nextLevelInfo))
			{
				var nextLevel = LevelManager.GetLevel(CurrentLevelPack, nextLevelInfo.ID);
				GotoLevel(nextLevel);
				return;
			}

			GotoState(GameState.MainMenu);
		}

		public static void HardResetProgress()
		{
			PlayerPrefs.DeleteAll();

			LevelManager.LoadLevelPacks();
		}

		public static void QuitGame()
		{
			if (Application.isEditor) Debug.Log("No exiting for you!!!!");
			else Application.Quit();
		}

		public static void GotoState(GameState gameState)
		{
			_instance._gameState = gameState;
			_instance.UIManager.GotoScreen(gameState);

			if (_instance._gameState == GameState.Playing)
				_instance.ShowGame();
			else
				_instance.HideGame();
		}

		public static void GotoLevelSelectMenu(LevelPack levelPack)
		{
			SetCurrentLevelPack(levelPack);
			GotoState(GameState.LevelSelectMenu);
		}
	}
}