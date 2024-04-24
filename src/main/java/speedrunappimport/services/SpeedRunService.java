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
import java.util.stream.IntStream;

import org.slf4j.Logger;

import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.PropertyNamingStrategies;
import com.fasterxml.jackson.databind.node.ArrayNode;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;

import speedrunappimport.common.StringExtensions;
import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.interfaces.services.*;
import speedrunappimport.model.entity.*;
import speedrunappimport.model.json.*;
import speedrunappimport.model.Enums.*;

public class SpeedRunService extends BaseService implements ISpeedRunService {
	private ISpeedRunRepository _speedRunRepo;
	private IGameRepository _gameRepo;
	private IPlatformRepository _platformRepo;
	private ISettingService _settingService;
	private Logger _logger;

	public SpeedRunService(ISpeedRunRepository speedRunRepo, IGameRepository gameRepo, IPlatformRepository platformRepo, ISettingService settingService, Logger logger) {
		_speedRunRepo = speedRunRepo;
		_platformRepo = platformRepo;
		_gameRepo = gameRepo;
		_settingService = settingService;
		_logger = logger;
	}

	public boolean ProcessSpeedRuns(boolean isReload) {
		boolean result = false;
		
		if (isReload) {
			result = ProcessSpeedRunsByGame(isReload);
		} else {
			result = ProcessSpeedRunsByDate(isReload);
		}

		return result;
	}

	public boolean ProcessSpeedRunsByGame(boolean isReload) {
		boolean result = false;

		try {
			var stLastImportDateUtc = _settingService.GetSetting("LastImportDate");
			var lastImportDateUtc = stLastImportDateUtc != null && stLastImportDateUtc.getDte() != null ? stLastImportDateUtc.getDte() : this.getSqlMinDateTime();	
			_logger.info("Started ProcessSpeedRunsByGame: {}, {}", lastImportDateUtc, isReload);

			var results = new ArrayList<SpeedRunResponse>();
			List<SpeedRunResponse> runs = new ArrayList<SpeedRunResponse>();
			var prevTotal = 0;
			var limit = super.getMaxPageLimit();
			var games = _gameRepo.GetGamesModifiedAfter(lastImportDateUtc);

			for (var game : games) {
				do {
					try {
						runs = GetSpeedRunResponses(limit, ((int)results.stream().filter(i->i.game().equals(game.getCode())).count()) + prevTotal, game.getCode());
					} catch (PaginationException ex) {
						results.removeIf(i -> i.game().equals(game.getCode()));
						for (var category : game.getCategories()) {
							try {
							runs = GetSpeedRunResponses(limit, ((int)results.stream().filter(i -> i.game().equals(game.getCode()) && i.category().equals(category.getCode())).count()) + prevTotal, game.getCode(), category.getCode(), SpeedRunOrderBy.DateSubmitted);
							} catch (PaginationException ex1) {
								runs = GetSpeedRunResponses(limit, ((int)results.stream().filter(i -> i.game().equals(game.getCode()) && i.category().equals(category.getCode())).count()) + prevTotal, game.getCode(), category.getCode(), SpeedRunOrderBy.DateSubmittedDesc);
							}
						}
					}
					results.addAll(runs);
					Thread.sleep(super.getPullDelayMS());
					_logger.info("Pulled games: {}, total games: {}", runs.size(), results.size() + prevTotal);
				}
				while (runs.size() == limit);
				//while (1 == 0);

				var memorySize = Runtime.getRuntime().totalMemory();
				if (memorySize > super.getMaxMemorySizeBytes()) {
					prevTotal += results.size();
					_logger.info("Saving to clear memory, results: {}, size: {}", results.size(), memorySize);
					//SaveSpeedRunResponses(results, isReload);
					results.clear();
					results.trimToSize();
				}				
			}

			if (results.size() > 0) {
				//SaveSpeedRunResponses(results, isReload);
				results.clear();
				results.trimToSize();
			}

			result = true;	
			_logger.info("Completed ProcessSpeedRunsByGame");
		} catch (Exception ex) {
			result = false;
			_logger.error("ProcessSpeedRunsByGame", ex);
		}

		return result;
	}

	public boolean ProcessSpeedRunsByDate(boolean isReload) {
		boolean result = false;

		try {
			var stSpeedRunLastImportRefDateUtc = _settingService.GetSetting("SpeedRunLastImportRefDate");
			var lastImportRefDateUtc = stSpeedRunLastImportRefDateUtc != null && stSpeedRunLastImportRefDateUtc.getDte() != null ? stSpeedRunLastImportRefDateUtc.getDte() : this.getSqlMinDateTime();	
			_logger.info("Started ProcessSpeedRuns: {}, {}", lastImportRefDateUtc, isReload);

			var results = new ArrayList<SpeedRunResponse>();
			List<SpeedRunResponse> runs = new ArrayList<SpeedRunResponse>();
			var prevTotal = 0;
			var limit = super.getMaxPageLimit();

			do {
				runs = GetSpeedRunResponses(limit, results.size() + prevTotal, SpeedRunOrderBy.VerifyDateDesc);
				results.addAll(runs);
				Thread.sleep(super.getPullDelayMS());
				_logger.info("Pulled games: {}, total games: {}", runs.size(), results.size() + prevTotal);

				var memorySize = Runtime.getRuntime().totalMemory();
				if (memorySize > super.getMaxMemorySizeBytes()) {
					prevTotal += results.size();
					_logger.info("Saving to clear memory, results: {}, size: {}", results.size(), memorySize);
					//SaveSpeedRunResponses(results, isReload);
					results.clear();
					results.trimToSize();
				}
			}
			while (runs.size() == limit && (isReload || runs.stream().map(i -> i.status().verifyDate() != null ? i.status().verifyDate() : super.getSqlMinDateTime()).max(Instant::compareTo).get().compareTo(lastImportRefDateUtc) > 0));
			//while (1 == 0);

			if (!isReload) {
				results.removeIf(i -> (i.status().verifyDate() != null ? i.status().verifyDate() : super.getSqlMinDateTime())
						.compareTo(lastImportRefDateUtc) <= 0);
			}

			if (results.size() > 0) {
				//SaveSpeedRunResponses(results, isReload);
				var lastUpdateDate = results.stream().map(i -> i.status().verifyDate() != null ? i.status().verifyDate() : super.getSqlMinDateTime()).max(Instant::compareTo).get();
				_settingService.UpdateSetting("SpeedRunLastImportRefDate", lastUpdateDate);
				results.clear();
				results.trimToSize();
			}

			result = true;	
			_logger.info("Completed ProcessSpeedRuns");
		} catch (Exception ex) {
			result = false;
			_logger.error("ProcessSpeedRuns", ex);
		}

		return result;
	}

	public List<SpeedRunResponse> GetSpeedRunResponses(int limit, int offset, SpeedRunOrderBy orderBy) throws Exception {
		return GetSpeedRunResponses(limit, offset, null, null, orderBy, 0);
	}

	public List<SpeedRunResponse> GetSpeedRunResponses(int limit, int offset, String gameCode) throws Exception {
		return GetSpeedRunResponses(limit, offset, gameCode, null, null, 0);
	}	

	public List<SpeedRunResponse> GetSpeedRunResponses(int limit, int offset, String gameCode, String categoryCode, SpeedRunOrderBy orderBy) throws Exception {
		return GetSpeedRunResponses(limit, offset, gameCode, categoryCode, orderBy, 0);
	}		

	public List<SpeedRunResponse> GetSpeedRunResponses(int limit, int offset, String gameCode, String categoryCode, SpeedRunOrderBy orderBy, int retryCount) throws Exception {
		List<SpeedRunResponse> data = new ArrayList<SpeedRunResponse>();

		try (var client = HttpClient.newHttpClient()) {
			var parameters = new HashMap<String, String>();
			parameters.put("embed", "players");
			parameters.put("max", Integer.toString(limit));
			parameters.put("offset", Integer.toString(offset));

			if (gameCode != null && !gameCode.isEmpty()) {
				parameters.put("game", gameCode);						
			}

			if (categoryCode != null && !categoryCode.isEmpty()) {
				parameters.put("category", categoryCode);						
			}

			if (orderBy != null) {
				var orderByString = "game";
				var isDesc = false;

				switch (orderBy) {
					case SpeedRunOrderBy.DateSubmitted:
					case SpeedRunOrderBy.DateSubmittedDesc:
						orderByString = "date-submitted";
						isDesc = (orderBy == SpeedRunOrderBy.DateSubmittedDesc);
						break;	
					case SpeedRunOrderBy.VerifyDate:
					case SpeedRunOrderBy.VerifyDateDesc:
						orderByString = "verify-date";
						isDesc = (orderBy == SpeedRunOrderBy.VerifyDateDesc);
						break;										
				}

				parameters.put("orderby", orderByString);
				
				if (isDesc) {
					parameters.put("direction", "desc");
				}				
			}

			String paramString = String.join("&", parameters.entrySet().stream().map(i -> i.getKey() + "=" + i.getValue()).toList());

			var request = HttpRequest.newBuilder()
					.uri(URI.create("https://www.speedrun.com/api/v1/runs?" + paramString))
					.build();

			var mapper = new ObjectMapper().configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false)
										.setPropertyNamingStrategy(PropertyNamingStrategies.KEBAB_CASE)
										.registerModule(new JavaTimeModule());					

			var response = client.send(request, BodyHandlers.ofString());

			if (response.statusCode() == 200) {
				var runs = Arrays.asList(mapper.readerFor(SpeedRunResponse[].class)
								.readValue(mapper.readTree(response.body()).get("data"), SpeedRunResponse[].class));
				data = new ArrayList<SpeedRunResponse>(runs);
			} else {
				var errorMsg = mapper.readTree(response.body()).get("message").toString();
				if (errorMsg != null && errorMsg.contains("Invalid pagination values")) {
					throw new PaginationException();
				}
			}
		} catch (PaginationException ex) {
			throw ex;
		} catch (Exception ex) {
			Thread.sleep(super.getErrorPullDelayMS());
			retryCount++;
			if (retryCount <= super.getMaxRetryCount()) {
				_logger.info("Retrying pull runs: {}, total runs: {}, retry: {}", limit, offset, retryCount);
				data = GetSpeedRunResponses(limit, offset, gameCode, categoryCode, orderBy, retryCount);
			} else {
				throw ex;
			}
		}

		return data;
	}

	/*
	public void SaveGameResponses(List<GameResponse> gameResponses, boolean isReload) {
		_logger.info("Started SaveGameResponses: {}", gameResponses.size());

		if (!isReload) {
			gameResponses = gameResponses.reversed();
		}

		gameResponses = gameResponses.stream()
				.collect(Collectors.toMap(GameResponse::id, Function.identity(), (u1, u2) -> u1))
				.values()
				.stream()
				.collect(Collectors.toList());

		var existingGamesVWs = _gameRepo.GetGameViewsByCode(gameResponses.stream().map(x -> x.id()).toList());
		var games = GetGamesFromResponses(gameResponses, existingGamesVWs);
		games = GetNewOrChangedGames(games, existingGamesVWs);

		_gameRepo.SaveGames(games);

		_logger.info("Completed SaveGameResponses");
	}

	private List<Game> GetGamesFromResponses(List<GameResponse> games, List<GameView> existingGamesVWs) {
		var categoryTypes = _gameRepo.GetCategoryTypes();
		var platforms = _platformRepo.GetAllPlatforms();

		var gameEntities = games.stream().map(i -> {
			var game = new Game();

			var existingGameVW = existingGamesVWs.stream().filter(x -> x.getCode().equals(i.id())).findFirst().orElse(null);			
			game.setId(existingGameVW != null ? existingGameVW.getId() : 0);
			game.setName(i.names().international());
			game.setCode(i.id());
			game.setAbbr(i.abbreviation());
			game.setShowMilliseconds(i.ruleset() != null ? i.ruleset().showMilliseconds() : false);
			game.setReleaseDate(i.releaseDate());
			game.setImportRefDate(i.created());

			var gameLink = new GameLink();
			gameLink.setId(existingGameVW != null ? existingGameVW.getGameLinkId() : 0);
			gameLink.setGameId(game.getId());
			gameLink.setCoverImageUrl(i.assets().coverLarge().uri());
			gameLink.setSpeedRunComUrl(i.weblink());
			game.setGameLink(gameLink);
			
			var gameCategoryTypes = categoryTypes.stream()
									.filter(g -> i.categories().data().stream().anyMatch(x -> StringExtensions.KebabToUpperCamelCase(x.type()).equals(g.getName())))
									.map(x -> { 
										var gameCategoryType = new GameCategoryType();
										gameCategoryType.setId(existingGameVW != null ? existingGameVW.getGameCategoryTypes().stream().filter(g ->  g.getCategoryTypeId() == x.getId()).map(g -> g.getId()).findFirst().orElse(0) : 0);
										gameCategoryType.setCategoryTypeId(x.getId());
										gameCategoryType.setGameId(game.getId());
										return gameCategoryType;
							}).toList();
			game.setGameCategoryTypes(gameCategoryTypes);	

			var categories = i.categories().data().stream()
								.map(x -> { 
									var category = new Category();
									category.setId(existingGameVW != null ? existingGameVW.getCategories().stream().filter(g ->  g.getCode().equals(x.id())).map(g -> g.getId()).findFirst().orElse(0) : 0);
									category.setName(x.name());
									category.setCode(x.id());
									category.setGameId(game.getId());
									category.setCategoryTypeId(Enums.CategoryType.valueOf(StringExtensions.KebabToUpperCamelCase(x.type())).getValue());
									category.setMiscellaneous(x.miscellaneous());
									category.setIsTimerAscending(x.rules() != null && x.rules().contains("long as possible"));
									return category;
								}).toList();
			game.setCategories(categories);

			var levels = i.levels().data().stream()
								.map(x -> { 
									var level = new Level();
									level.setId(existingGameVW != null ? existingGameVW.getLevels().stream().filter(g ->  g.getCode().equals(x.id())).map(g -> g.getId()).findFirst().orElse(0) : 0);
									level.setName(x.name());
									level.setCode(x.id());
									level.setGameId(game.getId());
									return level;
								}).toList();
			game.setLevels(levels);

			var variablesList = i.variables().data();
			var variables = IntStream.range(0, variablesList.size())
									.mapToObj(x -> { 
										var variable = new Variable();
										variable.setId(existingGameVW != null ? existingGameVW.getVariables().stream().filter(g ->  g.getCode().equals(variablesList.get(x).id())).map(g -> g.getId()).findFirst().orElse(0) : 0);
										variable.setName(variablesList.get(x).name());
										variable.setCode(variablesList.get(x).id());
										variable.setGameId(game.getId());
										variable.setVariableScopeTypeId(Enums.VariableScopeType.valueOf(StringExtensions.KebabToUpperCamelCase(variablesList.get(x).scope().type())).getValue());
										variable.setCategoryId(existingGameVW != null ? existingGameVW.getCategories().stream().filter(h ->  h.getCode().equals(variablesList.get(x).category())).map(h -> h.getId()).findFirst().orElse(0) : 0);
										variable.setCategoryCode(variablesList.get(x).category());
										variable.setLevelId(existingGameVW != null ? existingGameVW.getLevels().stream().filter(h ->  h.getCode().equals(variablesList.get(x).scope().level())).map(h -> h.getId()).findFirst().orElse(0) : 0);
										variable.setLevelCode(variablesList.get(x).scope().level());
										variable.setSortOrder(x);
										return variable;
									}).toList();			
			game.setVariables(variables);			

			var variableValues = i.variables().data().stream()
								.flatMap(x -> x.values().values().entrySet().stream().map(g -> {
									var variableValue = new VariableValue();
									variableValue.setId(existingGameVW != null ? existingGameVW.getVariableValues().stream().filter(h ->  h.getCode().equals(g.getKey())).map(h -> h.getId()).findFirst().orElse(0) : 0);
									variableValue.setName(g.getValue().label());
									variableValue.setCode(g.getKey());		
									variableValue.setGameId(game.getId());
									variableValue.setVariableId(existingGameVW != null ? existingGameVW.getVariables().stream().filter(h ->  h.getCode().equals(x.id())).map(h -> h.getId()).findFirst().orElse(0) : 0);
									variableValue.setVariableCode(x.id());
									return variableValue;							
								})).toList();
			game.setVariableValues(variableValues);

			var gamePlatforms = platforms.stream()
								.filter(g -> i.platforms().contains(g.getCode()))
								.map(x -> { 
									var gamePlatform = new GamePlatform();
									gamePlatform.setId(existingGameVW != null ? existingGameVW.getGamePlatforms().stream().filter(g ->  g.getPlatformId() == x.getId()).map(g -> g.getId()).findFirst().orElse(0) : 0);
									gamePlatform.setPlatformId(x.getId());
									gamePlatform.setGameId(game.getId());
									return gamePlatform;
								}).toList();
			game.setGamePlatforms(gamePlatforms);				
			
			if (existingGameVW != null) {
				if (existingGameVW.getGameCategoryTypes() != null) {
					var removeGameCategoryTypes = existingGameVW.getGameCategoryTypes().stream().filter(g -> !gameCategoryTypes.stream().anyMatch(h -> h.getId() == g.getId())).map(g -> g.getId()).toList();
					game.setGameCategoryTypesToRemove(removeGameCategoryTypes);		
				}

				if (existingGameVW.getCategories() != null) {
					var removeCategories = existingGameVW.getCategories().stream().filter(g -> !categories.stream().anyMatch(h -> h.getCode().equals(g.getCode()))).map(g -> g.getId()).toList();
					game.setCategoriesToRemove(removeCategories);
				}

				if (existingGameVW.getLevels() != null) {
					var removeLevels = existingGameVW.getLevels().stream().filter(g -> !levels.stream().anyMatch(h -> h.getCode().equals(g.getCode()))).map(g -> g.getId()).toList();
					game.setLevelsToRemove(removeLevels);	
				}

				if (existingGameVW.getVariables() != null) {
					var removeVariables = existingGameVW.getVariables().stream().filter(g -> !variables.stream().anyMatch(h -> h.getCode().equals(g.getCode()))).map(g -> g.getId()).toList();
					game.setVariablesToRemove(removeVariables);	
				}

				if (existingGameVW.getVariableValues() != null) {					
					var removeVariableValues = existingGameVW.getVariableValues().stream().filter(g -> !variableValues.stream().anyMatch(h -> h.getCode().equals(g.getCode()))).map(g -> g.getId()).toList();
					game.setVariableValuesToRemove(removeVariableValues);		
				}	

				if (existingGameVW.getGamePlatforms() != null) {										
					var removeGamePlatforms = existingGameVW.getGamePlatforms().stream().filter(g -> !gamePlatforms.stream().anyMatch(h -> h.getId() == g.getId())).map(g -> g.getId()).toList();
					game.setGamePlatformsToRemove(removeGamePlatforms);	
				}					
			}
			
			return game;
		}).toList();

		return gameEntities;
	}

	private List<Game> GetNewOrChangedGames(List<Game> games, List<GameView> existingGamesVWs) {	
		var results = new ArrayList<Game>();
		var newCount = 0;
		var changedCount = 0;

		for (var game : games) {
			var isNew = false;	
			var isChanged = false;
		
			if (game.getId() == 0) {
				isNew = true;
				newCount++;
			} else {
				var existingGameVW = existingGamesVWs.stream().filter(x -> x.getId() == game.getId()).findFirst().orElse(null);			
				if (existingGameVW != null) {
					isChanged = (!game.getName().equals(existingGameVW.getName())
						|| !game.getAbbr().equals(existingGameVW.getAbbr())
						|| !game.getReleaseDate().isEqual(existingGameVW.getReleaseDate())
						|| !game.getGameLink().getCoverImageUrl().equals(existingGameVW.getCoverImageUrl())
						|| !game.getGameLink().getSpeedRunComUrl().equals(existingGameVW.getSpeedRunComUrl()));

					if (!isChanged) {
						var categoryTypeIds = game.getGameCategoryTypes().stream().map(i -> i.getCategoryTypeId()).toList();
						var existingCategoryTypeIds = existingGameVW.getGameCategoryTypes().stream().map(i -> i.getCategoryTypeId()).toList();
						isChanged = categoryTypeIds.stream().anyMatch(i -> !existingCategoryTypeIds.contains(i))
									|| existingCategoryTypeIds.stream().anyMatch(i -> !categoryTypeIds.contains(i));
					}

					if (!isChanged) {
						var categoryCodes = game.getCategories().stream().map(i -> i.getCode()).toList();
						var existingCategoryCodes = existingGameVW.getCategories().stream().map(i -> i.getCode()).toList();
						isChanged = categoryCodes.stream().anyMatch(i -> !existingCategoryCodes.contains(i))
									|| existingCategoryCodes.stream().anyMatch(i -> !categoryCodes.contains(i));
					}

					if (!isChanged) {
						var levelCodes = game.getLevels().stream().map(i -> i.getCode()).toList();
						var existingLevelCodes = existingGameVW.getLevels().stream().map(i -> i.getCode()).toList();
						isChanged = levelCodes.stream().anyMatch(i -> !existingLevelCodes.contains(i))
									|| existingLevelCodes.stream().anyMatch(i -> !levelCodes.contains(i));
					}		
					
					if (!isChanged) {
						var variableCodes = game.getVariables().stream().map(i -> i.getCode()).toList();
						var existingVariableCodes = existingGameVW.getVariables().stream().map(i -> i.getCode()).toList();
						isChanged = variableCodes.stream().anyMatch(i -> !existingVariableCodes.contains(i))
									|| existingVariableCodes.stream().anyMatch(i -> !variableCodes.contains(i));
					}
					
					if (!isChanged) {
						var variableIndex = 0;
						for (var variable : game.getVariables()) {
							isChanged = !variable.getCode().equals(existingGameVW.getVariables().get(variableIndex).getCode());
							variableIndex++;
						}
					}

					if (!isChanged) {
						var variableValueCodes = game.getVariableValues().stream().map(i -> i.getCode()).toList();
						var existingVariableValueCodes = existingGameVW.getVariableValues().stream().map(i -> i.getCode()).toList();
						isChanged = variableValueCodes.stream().anyMatch(i -> !existingVariableValueCodes.contains(i))
									|| existingVariableValueCodes.stream().anyMatch(i -> !variableValueCodes.contains(i));
					}		
					
					if (!isChanged) {
						var platformIds = game.getGamePlatforms().stream().map(i -> i.getPlatformId()).toList();
						var existingPlatformIds = existingGameVW.getGamePlatforms().stream().map(i -> i.getPlatformId()).toList();
						isChanged = platformIds.stream().anyMatch(i -> !existingPlatformIds.contains(i))
									|| existingPlatformIds.stream().anyMatch(i -> !platformIds.contains(i));
					}					

					if (isChanged){
						changedCount++;
					}
				}
			}

			if (isNew || isChanged) {
				results.add(game);
			}
		}

		_logger.info("Found New: {}, Changed: {}, Total: {}", newCount, changedCount, results.size());	
		return results;
	}	
	*/
}
