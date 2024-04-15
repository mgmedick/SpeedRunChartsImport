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
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;

import speedrunappimport.common.StringExtensions;
import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.interfaces.services.*;
import speedrunappimport.model.entity.*;
import speedrunappimport.model.json.*;
import speedrunappimport.model.Enums;

public class GameService extends BaseService implements IGameService {
	private IGameRepository _gameRepo;
	private IPlatformRepository _platformRepo;
	private ISettingService _settingService;
	private Logger _logger;

	public GameService(IGameRepository gameRepo, IPlatformRepository platformRepo, ISettingService settingService, Logger logger) {
		_gameRepo = gameRepo;
		_platformRepo = platformRepo;
		_settingService = settingService;
		_logger = logger;
	}

	public boolean ProcessGames(boolean isReload) {
		boolean result = true;

		try {
			var stGameLastImportRefDateUtc = _settingService.GetSetting("GameLastImportRefDate");
			var lastImportRefDateUtc = stGameLastImportRefDateUtc != null && stGameLastImportRefDateUtc.getDte() != null ? stGameLastImportRefDateUtc.getDte() : this.getSqlMinDateTime();	
			_logger.info("Started ProcessGames: {}, {}", lastImportRefDateUtc, isReload);

			var results = new ArrayList<GameResponse>();
			List<GameResponse> games = new ArrayList<GameResponse>();
			var prevTotal = 0;

			do {
				games = GetGameResponses(isReload, results.size() + prevTotal);
				results.addAll(games);
				Thread.sleep(super.getPullDelayMS());
				_logger.info("Pulled games: {}, total games: {}", games.size(), results.size() + prevTotal);

				var memorySize = Runtime.getRuntime().totalMemory();
				if (memorySize > super.getMaxMemorySizeBytes()) {
					prevTotal += results.size();
					_logger.info("Saving to clear memory, results: {}, size: {}", results.size(), memorySize);
					SaveGameResponses(results, isReload);
					results.clear();
					results.trimToSize();
				}
			}
			//while (games.size() == super.getMaxPageLimit() && (isReload || games.stream().map(i -> i.created() != null ? i.created() : super.getSqlMinDateTime()).max(Instant::compareTo).get().compareTo(lastImportRefDateUtc) > 0));
			while (1 == 0);

			if (!isReload) {
				results.removeIf(i -> (i.created() != null ? i.created() : super.getSqlMinDateTime())
						.compareTo(lastImportRefDateUtc) <= 0);
			}

			if (results.size() > 0) {
				SaveGameResponses(results, isReload);
				var lastUpdateDate = results.stream().map(i -> i.created() != null ? i.created() : super.getSqlMinDateTime()).max(Instant::compareTo).get();
				_settingService.UpdateSetting("GameLastImportRefDate", lastUpdateDate);
				results.clear();
				results.trimToSize();
			}

			_logger.info("Completed ProcessGames");
		} catch (Exception ex) {
			result = false;
			_logger.error("ProcessGames", ex);
		}

		return result;
	}

	public List<GameResponse> GetGameResponses(Boolean isReload, int offset) throws Exception {
		return GetGameResponses(isReload, offset, 0);
	}

	public List<GameResponse> GetGameResponses(Boolean isReload, int offset, int retryCount) throws Exception {
		List<GameResponse> data = new ArrayList<GameResponse>();

		try (var client = HttpClient.newHttpClient()) {
			var parameters = new HashMap<String, String>();
			parameters.put("orderby", "created");
			parameters.put("embed", "levels,categories,variables");
			parameters.put("max", Integer.toString(super.getMaxPageLimit()));
			parameters.put("offset", Integer.toString(offset));

			if (!isReload) {
				parameters.put("direction", "desc");
			}

			String paramString = String.join("&",
					parameters.entrySet().stream().map(i -> i.getKey() + "=" + i.getValue()).toList());

			var request = HttpRequest.newBuilder()
					.uri(URI.create("https://www.speedrun.com/api/v1/games?" + paramString))
					.build();

			var response = client.send(request, BodyHandlers.ofString());
			if (response.statusCode() == 200) {
				var mapper = new ObjectMapper().configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false)
											.setPropertyNamingStrategy(PropertyNamingStrategies.KEBAB_CASE)
											.registerModule(new JavaTimeModule());
				var games = Arrays.asList(mapper.readerFor(GameResponse[].class)
								.readValue(mapper.readTree(response.body()).get("data"), GameResponse[].class));
				data = new ArrayList<GameResponse>(games);
			}
		} catch (Exception ex) {
			Thread.sleep(super.getErrorPullDelayMS());
			retryCount++;
			if (retryCount <= super.getMaxRetryCount()) {
				_logger.info("Retrying pull games: {}, total games: {}, retry: {}",
						super.getMaxPageLimit(), offset, retryCount);
				data = GetGameResponses(isReload, offset, retryCount);
			} else {
				throw ex;
			}
		}

		return data;
	}

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
}
