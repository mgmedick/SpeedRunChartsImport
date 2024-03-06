package speedrunappimport.services;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse.BodyHandlers;
import java.time.Instant;
import java.util.ArrayList;
import java.util.List;
import java.util.Arrays;
import java.util.HashMap;
import java.util.stream.Collectors;
import java.util.function.Function;
import java.util.Comparator;

import org.slf4j.Logger;

import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;

import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.interfaces.services.*;
import speedrunappimport.model.entity.Game;
import speedrunappimport.model.json.*;

public class GameService extends BaseService implements IGameService
{
	private IGameRepository _gameRepo;
	private Logger _logger;

	public GameService(IGameRepository gameRepo, Logger logger) {
		_gameRepo = gameRepo;
		_logger = logger;
	}

	public boolean ProcessGames(Instant lastImportRefDateUtc, boolean isReload)
	{	
		boolean result = true;

		try
		{
			_logger.info("Started ProcessGames: {@lastImportRefDateUtc}, {@isReload}", lastImportRefDateUtc, isReload);
			var results = new ArrayList<GameResponse>();
			var games = new ArrayList<GameResponse>();
			var prevTotal = 0;

			do
			{
				games = GetGameResponses("created", !isReload, results.size() + prevTotal);
				Thread.sleep(super.pullDelayMS);
				results.addAll(games);
				_logger.info("Pulled games: {@New}, total games: {@Total}", games.size(), results.size() + prevTotal);

				var memorySize = Runtime.getRuntime().totalMemory();
				if (memorySize > super.maxMemorySizeBytes)
				{
					prevTotal += results.size();
					_logger.info("Saving to clear memory, results: {@Count}, size: {@Size}", results.size(), memorySize);
					//SaveGames(results);
					results.clear();
					results.trimToSize();
				}
			}
			//while (games.size() == super.maxPageLimit && (isReload || games.stream().map(i -> i.created != null ? i.created : super.sqlMinDateTime).max(Instant::compareTo).get().compareTo(lastImportDateUtc) > 0));                
			while (1 == 0);

			if (!isReload)
			{
				results.removeIf(i -> (i.created != null ? i.created : super.sqlMinDateTime).compareTo(lastImportRefDateUtc) <= 0);
			}

			if (results.size() > 0)
			{
				//SaveGames(results);
				//var lastUpdateDate = results.stream().map(i -> i.created != null ?  Instant.parse(i.created) : super.sqlMinDateTime).max(Instant::compareTo).get();
				//_settingService.UpdateSetting("GameLastImportRefDate", lastUpdateDate);
				results.clear();
				results.trimToSize();
			}
			
			_logger.info("Completed ProcessGames");
		}
		catch (Exception ex)
		{
			result = false;
			_logger.error("ProcessGames", ex);
		}

		return result;
	}
	
	public ArrayList<GameResponse> GetGameResponses(String sort, Boolean isDesc, int offset) throws Exception
	{
		return GetGameResponses(sort, isDesc, offset, 0);
	}

	public ArrayList<GameResponse> GetGameResponses(String sort, Boolean isDesc, int offset, int retryCount) throws Exception
	{
		var data = new ArrayList<GameResponse>();

		try (var client = HttpClient.newHttpClient())
		{
			var parameters = new HashMap<String, String>();
			parameters.put("embed", "levels");
			parameters.put("orderby", sort);
			parameters.put("max", Integer.toString(super.maxPageLimitSM));
			parameters.put("offset", Integer.toString(offset));

			if (isDesc)
			{
				parameters.put("direction", "desc");
			}

			String paramString = String.join("&", parameters.entrySet().stream().map(i -> i.getKey() + "=" + i.getValue()).toList());

			var request = HttpRequest.newBuilder()
			.uri(URI.create("https://www.speedrun.com/api/v1/games?" + paramString))
			.build();

			var response = client.send(request, BodyHandlers.ofString());
			if (response.statusCode() == 200)
			{
				var mapper = new ObjectMapper().configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false)
											   .registerModule(new JavaTimeModule());
				var games = mapper.readerFor(GameResponse[].class).readValue(mapper.readTree(response.body()).get("data"), GameResponse[].class);
				data = new ArrayList<>(Arrays.asList(games));
			}
		}
		catch (Exception ex)
		{
			Thread.sleep(super.errorPullDelayMS);
			retryCount++;
			if (retryCount <= super.maxRetryCount)
			{
				_logger.info("Retrying pull games: {@New}, total games: {@Total}, retry: {@RetryCount}", super.maxPageLimit, offset, retryCount);
				data = GetGameResponses(sort, isDesc, offset, retryCount);
			}
			else
			{
				throw ex;
			}
		}

		return data;
	}

	public void SaveGames(List<GameResponse> games)
	{
		_logger.info("Started SaveGames: {@count}", games.size());
		
		games =  games.stream()
					  .collect(Collectors.toMap(GameResponse::getId, Function.identity(), (u1, u2) -> u1))
					  .values()
					  .stream()
					  .sorted(Comparator.comparing(GameResponse::getCreated, Comparator.nullsFirst(Instant::compareTo)))
					  .collect(Collectors.toList());

		var existingGames = _gameRepo.GetGamesByCode(games.stream().map(x -> x.id).toList());			 

		games.stream().map(i -> { var game = new Game();
								  var existingGame = existingGames.stream().filter(x -> x.code == i.id).findFirst().orElse(null);
								  game.id = existingGame != null ? existingGame.id : null;
								  game.name = i.names.international;
								  game.code = i.id;
								  game.abbr = i.abbreviation;
								  game.isromhack = i.romhack;
								  game.yearofrelease = i.released;
								  game.importrefdate = i.created;

								  return game;
								});


		_logger.info(Integer.toString(existingGames.size()));
	}

	/*
	public void SaveGames(IEnumerable<Game> games, bool isBulkReload)
	{
		_logger.Information("Started SaveGames: {@Count}, {@IsBulkReload}", games.Count(), isBulkReload);

		_gameRepo.RemoveObsoleteGameSpeedRunComIDs();

		games = games.GroupBy(g => new { g.ID })
						.Select(i => i.First())
						.ToList();

		games = games.OrderBy(i => i.CreationDate).ToList();
		var gameIDs = games.Select(i => i.ID).ToList();
		var gameSpeedRunComIDs = _gameRepo.GetGameSpeedRunComIDs();
		gameSpeedRunComIDs = gameSpeedRunComIDs.Join(gameIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

		var userIDs = games.SelectMany(i => i.ModeratorUsers.Select(i => i.ID)).Distinct().ToList();
		var userSpeedRunComIDs = _userRepo.GetUserSpeedRunComIDs();
		userSpeedRunComIDs = userSpeedRunComIDs.Join(userIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

		var levelIDs = games.SelectMany(i => i.Levels.Select(i => i.ID)).Distinct().ToList();
		var levelSpeedRunComIDs = _gameRepo.GetLevelSpeedRunComIDs();
		levelSpeedRunComIDs = levelSpeedRunComIDs.Join(levelIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

		var categoryIDs = games.SelectMany(i => i.Categories.Select(i => i.ID)).Distinct().ToList();
		var categorySpeedRunComIDs = _gameRepo.GetCategorySpeedRunComIDs();
		categorySpeedRunComIDs = categorySpeedRunComIDs.Join(categoryIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

		var variableIDs = games.SelectMany(i => i.Variables.Select(i => i.ID)).Distinct().ToList();
		var variableSpeedRunComIDs = _gameRepo.GetVaraibleSpeedRunComIDs();
		variableSpeedRunComIDs = variableSpeedRunComIDs.Join(variableIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

		var variableValueIDs = games.SelectMany(i => i.Variables.SelectMany(g => g.Values.Select(h => h.ID))).Distinct().ToList();
		var variableValueSpeedRunComIDs = _gameRepo.GetVariableValueSpeedRunComIDs();
		variableValueSpeedRunComIDs = variableValueSpeedRunComIDs.Join(variableValueIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

		var platformIDs = games.SelectMany(i => i.PlatformIDs).Distinct().ToList();
		var platformSpeedRunComIDs = _platformRepo.GetPlatformSpeedRunComIDs(i => platformIDs.Contains(i.SpeedRunComID)).ToList();

		var regionIDs = games.SelectMany(i => i.RegionIDs).Distinct().ToList();
		var regionSpeedRunComIDs = _gameRepo.GetRegionSpeedRunComIDs(i => regionIDs.Contains(i.SpeedRunComID)).ToList();

		var moderators = games.Where(i => i.ModeratorUsers != null)
								//.SelectMany(i => i.ModeratorUsers.Where(i => !userSpeedRunComIDs.Any(g => g.SpeedRunComID == i.ID)))
								.SelectMany(i => i.ModeratorUsers)
								.GroupBy(g => new { g.ID })
								.Select(i => i.First())
								.ToList();
		if (moderators.Any())
		{
			_userService.SaveUsers(moderators, isBulkReload, userSpeedRunComIDs);
			userSpeedRunComIDs = _userRepo.GetUserSpeedRunComIDs().Where(i => userIDs.Contains(i.SpeedRunComID)).ToList();
		}

		var gameEntities = games.Select(i => new GameEntity()
		{
			ID = gameSpeedRunComIDs.Where(g => g.SpeedRunComID == i.ID).Select(g => g.GameID).FirstOrDefault(),
			SpeedRunComID = i.ID,
			Name = i.Name,
			YearOfRelease = i.YearOfRelease,
			IsRomHack = i.IsRomHack,
			Abbr = i.Abbreviation,
			CreatedDate = i.CreationDate
		}).ToList();
		var gameLinkEntities = games.Select(i => new GameLinkEntity()
		{
			GameSpeedRunComID = i.ID,
			SpeedRunComUrl = i.WebLink.ToString(),
			CoverImageUrl = i.Assets?.CoverLarge?.Uri.ToString()
		}).ToList();
		var levelEntities = games.SelectMany(i => i.Levels.Select(g => new LevelEntity
		{
			ID = levelSpeedRunComIDs.Where(h => h.SpeedRunComID == g.ID).Select(o => o.LevelID).FirstOrDefault(),
			SpeedRunComID = g.ID,
			Name = g.Name,
			GameSpeedRunComID = i.ID
		})).ToList();
		var levelRuleEntities = games.SelectMany(i => i.Levels.Select(g => new LevelRuleEntity
		{
			LevelSpeedRunComID = g.ID,
			Rules = g.Rules
		})).Where(i => !string.IsNullOrWhiteSpace(i.Rules))
		.ToList();
		var categoryEntities = games.SelectMany(i => i.Categories.Select(g => new CategoryEntity
		{
			ID = categorySpeedRunComIDs.Where(h => h.SpeedRunComID == g.ID).Select(o => o.CategoryID).FirstOrDefault(),
			SpeedRunComID = g.ID,
			Name = g.Name,
			GameSpeedRunComID = i.ID,
			CategoryTypeID = (int)g.Type,
			IsMiscellaneous = g.IsMiscellaneous,
			IsTimerAscending = g.IsTimerAscending
		})).ToList();
		var categoryRuleEntities = games.SelectMany(i => i.Categories.Select(g => new CategoryRuleEntity
		{
			CategorySpeedRunComID = g.ID,
			Rules = g.Rules
		})).Where(i => !string.IsNullOrWhiteSpace(i.Rules))
		.ToList();
		var variableEntities = games.SelectMany(i => i.Variables.Select(g => new VariableEntity
		{
			ID = variableSpeedRunComIDs.Where(h => h.SpeedRunComID == g.ID).Select(o => o.VariableID).FirstOrDefault(),
			SpeedRunComID = g.ID,
			Name = g.Name,
			VariableScopeTypeID = (int)g.Scope.Type,
			GameSpeedRunComID = i.ID,
			CategorySpeedRunComID = g.CategoryID,
			LevelSpeedRunComID = g.Scope.LevelID,
			IsSubCategory = g.IsSubCategory
		})).Where(i => (string.IsNullOrWhiteSpace(i.CategorySpeedRunComID) || categoryEntities.Any(g => g.GameSpeedRunComID == i.GameSpeedRunComID && g.SpeedRunComID == i.CategorySpeedRunComID))
						&& (string.IsNullOrWhiteSpace(i.LevelSpeedRunComID) || levelEntities.Any(g => g.GameSpeedRunComID == i.GameSpeedRunComID && g.SpeedRunComID == i.LevelSpeedRunComID)))
			.ToList();
		var variableValueEntities = games.SelectMany(i => i.Variables.SelectMany(g => g.Values.Select(h => new VariableValueEntity
		{
			ID = variableValueSpeedRunComIDs.Where(n => n.SpeedRunComID == h.ID).Select(o => o.VariableValueID).FirstOrDefault(),
			SpeedRunComID = h.ID,
			GameSpeedRunComID = g.GameID,
			VariableSpeedRunComID = h.VariableID,
			Value = h.Value,
			IsCustomValue = h.IsCustomValue
		}))).Where(i => variableEntities.Any(g => g.SpeedRunComID == i.VariableSpeedRunComID))
		.ToList();
		var gamePlatformEntities = games.SelectMany(i => i.PlatformIDs.Select(g => new GamePlatformEntity
		{
			GameSpeedRunComID = i.ID,
			PlatformID = platformSpeedRunComIDs.Where(h => h.SpeedRunComID == g).Select(o => o.PlatformID).FirstOrDefault(),
			PlatformSpeedRunComID = g
		}))
		.Where(i => i.PlatformID != 0)
		.ToList();
		var gameRegionEntities = games.SelectMany(i => i.RegionIDs.Select(g => new GameRegionEntity
		{
			GameSpeedRunComID = i.ID,
			RegionID = regionSpeedRunComIDs.Where(h => h.SpeedRunComID == g).Select(o => o.RegionID).FirstOrDefault()
		}))
		.Where(i => i.RegionID != 0)
		.ToList();
		var gameModeratorEntities = games.SelectMany(i => i.ModeratorUsers.Select(g => new GameModeratorEntity
		{
			GameSpeedRunComID = i.ID,
			UserID = userSpeedRunComIDs.Where(h => h.SpeedRunComID == g.ID).Select(o => o.UserID).FirstOrDefault(),
			UserSpeedRunComID = g.ID
		}))
		.Where(i => i.UserID != 0)
		.ToList();
		var gameRulesetEntities = games.Select(i => new GameRulesetEntity
		{
			GameSpeedRunComID = i.ID,
			ShowMilliseconds = i.Ruleset.ShowMilliseconds,
			RequiresVerification = i.Ruleset.RequiresVerification,
			RequiresVideo = i.Ruleset.RequiresVideo,
			DefaultTimingMethodID = (int)i.Ruleset.DefaultTimingMethod,
			EmulatorsAllowed = i.Ruleset.EmulatorsAllowed
		}).ToList();
		var gameTimingMethodEntities = games.SelectMany(i => i.Ruleset.TimingMethods.Select(g => new GameTimingMethodEntity
		{
			GameSpeedRunComID = i.ID,
			TimingMethodID = (int)g
		})).ToList();

		if (isBulkReload)
		{
			_gameRepo.InsertGames(gameEntities, gameLinkEntities, levelEntities, levelRuleEntities, categoryEntities, categoryRuleEntities, variableEntities, variableValueEntities, gamePlatformEntities, gameRegionEntities, gameModeratorEntities, gameRulesetEntities, gameTimingMethodEntities);
		}
		else
		{
			var newGameEntities = gameEntities.Where(i => i.ID == 0).ToList();
			SetChangedGames(gameEntities, gameLinkEntities, categoryEntities, levelEntities, variableEntities, variableValueEntities, gamePlatformEntities, gameModeratorEntities);
			var changedGameEntities = gameEntities.Where(i => i.IsChanged == true).ToList();
			var totalGames = gameEntities.Count();
			gameEntities = newGameEntities.Concat(changedGameEntities).ToList();
			var saveGameSpeedRunComIDs = gameEntities.Select(i => i.SpeedRunComID).ToList();

			_logger.Information("Found NewGames: {@New}, ChangedGames: {@Changed}, TotalGames: {@Total}", newGameEntities.Count(), changedGameEntities.Count(), totalGames);

			gameLinkEntities = gameLinkEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
			levelEntities = levelEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
			levelRuleEntities = levelRuleEntities.Where(i => levelEntities.Any(g => g.SpeedRunComID == i.LevelSpeedRunComID)).ToList();
			categoryEntities = categoryEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
			categoryRuleEntities = categoryRuleEntities.Where(i => categoryEntities.Any(g => g.SpeedRunComID == i.CategorySpeedRunComID)).ToList();
			variableEntities = variableEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
			variableValueEntities = variableValueEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
			gamePlatformEntities = gamePlatformEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
			gameModeratorEntities = gameModeratorEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
			gameRulesetEntities = gameRulesetEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
			gameTimingMethodEntities = gameTimingMethodEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();

			_gameRepo.SaveGames(gameEntities, gameLinkEntities, levelEntities, levelRuleEntities, categoryEntities, categoryRuleEntities, variableEntities, variableValueEntities, gamePlatformEntities, gameRegionEntities, gameModeratorEntities, gameRulesetEntities, gameTimingMethodEntities);
		}

		if (IsProcessGameCoverImages)
		{
			ProcessGameCoverImages(gameLinkEntities, gameEntities, isBulkReload);
		}

		_logger.Information("Completed SaveGames");
	}
	*/	
}
