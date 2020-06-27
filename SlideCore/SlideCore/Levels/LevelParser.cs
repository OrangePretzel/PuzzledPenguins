using Newtonsoft.Json.Linq;
using SlideCore.Data;
using SlideCore.Entities;
using SlideCore.Math;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlideCore.Levels
{
	public partial class Level
	{
		/// <summary>Class to parse levels from a serialized string</summary>
		public static class Parser
		{
			/// <summary>Parse the given string as a Level</summary>
			public static Level ParseLevel(string serializedLevel)
			{
				try
				{
					// Parse the string as JSON
					JObject levelJSON = JObject.Parse(serializedLevel);
					if (levelJSON == null) throw new InvalidSerializedContentException("Level was not valid JSON");

					// Get the parser version
					// In the future additional version can be added for new features without breaking compatibility
					var parserVersion = levelJSON.Value<string>("Parser");
					switch (parserVersion)
					{
						case "STD1":
							return ParseLevel_STD1(levelJSON);
						default:
							throw new Exception($"Unknown parser version [{parserVersion}]");
					}
				}
				catch (Exception ex)
				{
					throw new InvalidSerializedContentException("Unable to parse level", ex);
				}
			}

			private static Level ParseLevel_STD1(JObject levelJSON)
			{
				var levelWidth = levelJSON.Value<int>("Width");
				if (levelWidth <= 0) throw new InvalidSerializedContentException($"Invalid level width specified [{levelWidth}]");

				var levelHeight = levelJSON.Value<int>("Height");
				if (levelHeight <= 0) throw new InvalidSerializedContentException($"Invalid level height specified [{levelHeight}]");

				var targetMovesAttribute = levelJSON["TargetMoves"];
				int levelTargetMoves = 0;
				if (targetMovesAttribute != null) levelTargetMoves = targetMovesAttribute.Value<int>();
				if (levelTargetMoves < 0) throw new InvalidSerializedContentException($"Invalid target moves specified [{levelTargetMoves}]");

				var entities = (JArray)levelJSON["Entities"];
				if (entities == null || entities.Count < 1) throw new InvalidSerializedContentException($"Invalid level entities specified [{entities}]");

				var level = new Level()
				{
					LevelWidth = levelWidth,
					LevelHeight = levelHeight,
					TargetMoves = levelTargetMoves,
				};

				bool containsFinish = false,
					containsPlayer = false;

				List<Action> delayedParseActions = new List<Action>();

				// Parse all entities
				foreach (var jsonEntity in entities)
				{
					var entity = JSONToEntity(level, jsonEntity, delayedParseActions);
					if (entity == null) continue;

					if (entity is StaticEntity)
					{
						level.StaticEntities.Add((StaticEntity)entity);
						if (entity.EntityType == EntityTypes.Sushi)
							// Sushi is also tracked in an additional list for easy checking
							level.SushiEntities.Add((SushiEntity)entity);
					}
					if (entity is DynamicEntity)
						if (entity.EntityType == EntityTypes.Player)
							level.PlayerEntities.Add((PlayerEntity)entity);
						else
							level.DynamicEntities.Add((DynamicEntity)entity);

					switch (entity.EntityType)
					{
						case EntityTypes.FinishFlag:
							containsFinish = true;
							break;
						case EntityTypes.Player:
							containsPlayer = true;
							break;
					}
				}

				delayedParseActions.ForEach(a => a.Invoke());

				// Valid levels must contain a finish flag and a player 
				if (!containsPlayer)
					throw new InvalidSerializedContentException($"No player was present in level [{levelJSON}]");
				if (!containsFinish)
					throw new InvalidSerializedContentException($"No finish flag was present in level [{levelJSON}]");

				return level;
			}

			private static Entity JSONToEntity(Level level, JToken entity, List<Action> delayedParseActions)
			{
				var entityType = entity["Type"]?.ToObject<EntityTypes>();
				if (entityType == null) throw new InvalidSerializedContentException($"Invalid entity type specified [{entityType}]");

				var entityX = entity.Value<int>("X");
				if (entityX < 0 || entityX >= level.LevelWidth) throw new InvalidSerializedContentException($"Invalid entity X specified [{entityX}]");

				var entityY = entity.Value<int>("Y");
				if (entityY < 0 || entityY >= level.LevelHeight) throw new InvalidSerializedContentException($"Invalid entity Y specified [{entityY}]");

				switch (entityType.Value)
				{
					case EntityTypes.Pit:
					case EntityTypes.HaltTile:
					case EntityTypes.FinishFlag:
					case EntityTypes.JumpTile:
						var genericEntity = new StaticEntity(entityType.Value, level.GetNextEntityID(), entityX, entityY);
						return genericEntity;

					case EntityTypes.Wall:
						var adjacentWalls = entity.Value<byte>("AdjacentWalls");
						var wallEntity = new WallEntity(level.GetNextEntityID(), entityX, entityY, adjacentWalls);
						return wallEntity;
					case EntityTypes.RedirectTile:
						var redirectDir = entity.Value<int>("RedirectDir");
						if (redirectDir < 0 || redirectDir > 3) throw new InvalidSerializedContentException($"Invalid redirect direction [{redirectDir}]");
						var redirectEntity = new RedirectTile(level.GetNextEntityID(), entityX, entityY, redirectDir);
						return redirectEntity;
					case EntityTypes.Portal:
						var portalTargetX = entity["PortalTarget"]?.Value<int>("X");
						var portalTargetY = entity["PortalTarget"]?.Value<int>("Y");

						if (!portalTargetX.HasValue || portalTargetX < 0 || portalTargetX >= level.LevelWidth)
							throw new InvalidSerializedContentException($"Invalid portal target X specified [{portalTargetX}]");
						if (!portalTargetY.HasValue || portalTargetY < 0 || portalTargetY >= level.LevelHeight)
							throw new InvalidSerializedContentException($"Invalid portal target Y specified [{portalTargetY}]");

						var portalEntity = new PortalEntity(level.GetNextEntityID(), entityX, entityY, new IntVector2(portalTargetX.Value, portalTargetY.Value));
						return portalEntity;

					case EntityTypes.Player:
						var playerEntity = new PlayerEntity(-(level.PlayerEntities.Count + 1), entityX, entityY);
						return playerEntity;
					case EntityTypes.SlidingCrate:
						var slidingCrateEntity = new SlidingCrate(level.GetNextEntityID(), entityX, entityY);
						return slidingCrateEntity;

					case EntityTypes.WireButton:
						var buttonMode = entity["Mode"]?.ToObject<WireButtonEntity.ButtonModes>();
						if (buttonMode == null) throw new InvalidSerializedContentException($"Invalid button mode specified [{buttonMode}]");

						var wireButtonEntity = new WireButtonEntity(level.GetNextEntityID(), entityX, entityY, buttonMode.Value);
						delayedParseActions.Add(() =>
						{
							var wireInteractionsJSON = (JArray)entity["Wire"];
							if (wireInteractionsJSON == null || wireInteractionsJSON.Count < 1)
								throw new InvalidSerializedContentException($"Invalid wire interactions specified for [{wireButtonEntity.EntityType}] at [{wireButtonEntity.Position}]");
							foreach (var wireInteractionJSON in wireInteractionsJSON)
							{
								var wireX = wireInteractionJSON.Value<int>("X");
								if (wireX < 0 || wireX >= level.LevelWidth) throw new InvalidSerializedContentException($"Invalid wire X specified [{wireX}]");

								var wireY = wireInteractionJSON.Value<int>("Y");
								if (wireY < 0 || wireY >= level.LevelHeight) throw new InvalidSerializedContentException($"Invalid entity Y specified [{wireY}]");

								var invert = wireInteractionJSON.Value<bool>("Invert");

								var interactables = level
									.GetEntitiesAtPosition(wireX, wireY)
									.Where(i => i is IWireInteractable);
								foreach (var interactable in interactables)
								{
									wireButtonEntity.WireInteractions.Add(new WireInteraction()
									{
										Interactable = (IWireInteractable)interactable,
										Invert = invert
									});
								}
							}
						});
						return wireButtonEntity;

					case EntityTypes.WireDoor:
						var wireDoorEntity = new WireDoorEntity(level.GetNextEntityID(), entityX, entityY, false);
						return wireDoorEntity;

					case EntityTypes.Sushi:
						var sushiEntity = new SushiEntity(level.GetNextEntityID(), entityX, entityY, true);
						return sushiEntity;

					case EntityTypes.None:
						return null;
					default:
						throw new NotSupportedException($"Entity type [{entityType}] is not parsable. Please implement JSON to entity.");
				}
			}
		}
	}
}
