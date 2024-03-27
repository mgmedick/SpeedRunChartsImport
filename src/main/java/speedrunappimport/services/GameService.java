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
	private Logger _logger;

	public GameService(IGameRepository gameRepo, Logger logger) {
		_gameRepo = gameRepo;
		_logger = logger;
	}

	public boolean ProcessGames(Instant lastImportRefDateUtc, boolean isReload) {
		boolean result = true;

		try {
			_logger.info("Started ProcessGames: {@lastImportRefDateUtc}, {@isReload}", lastImportRefDateUtc, isReload);
			var results = new ArrayList<GameResponse>();
			List<GameResponse> games = new ArrayList<GameResponse>();
			var prevTotal = 0;

			do {
				games = GetGameResponses(isReload, results.size() + prevTotal);
				results.addAll(games);
				Thread.sleep(super.getPullDelayMS());
				_logger.info("Pulled games: {@New}, total games: {@Total}", games.size(), results.size() + prevTotal);

				var memorySize = Runtime.getRuntime().totalMemory();
				if (memorySize > super.getMaxMemorySizeBytes()) {
					prevTotal += results.size();
					_logger.info("Saving to clear memory, results: {@Count}, size: {@Size}", results.size(),
							memorySize);
					SaveGames(results, isReload);
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
				SaveGames(results, isReload);
				// var lastUpdateDate = results.stream().map(i -> i.created != null ?
				// Instant.parse(i.created) :
				// super.sqlMinDateTime).max(Instant::compareTo).get();
				// _settingService.UpdateSetting("GameLastImportRefDate", lastUpdateDate);
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
			parameters.put("max", Integer.toString(super.getMaxPageLimitSM()));
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
				_logger.info("Retrying pull games: {@New}, total games: {@Total}, retry: {@RetryCount}",
						super.getMaxPageLimitSM(), offset, retryCount);
				data = GetGameResponses(isReload, offset, retryCount);
			} else {
				throw ex;
			}
		}

		return data;
	}

	public void SaveGames(List<GameResponse> games, boolean isReload) {
		_logger.info("Started SaveGames: {@count}", games.size());

		if (!isReload) {
			games = games.reversed();
		}

		games = games.stream()
				.collect(Collectors.toMap(GameResponse::id, Function.identity(), (u1, u2) -> u1))
				.values()
				.stream()
				.collect(Collectors.toList());

		var existingGamesVWs = _gameRepo.GetGamesByCode(games.stream().map(x -> x.id()).toList());

		var gameEntities = games.stream().map(i -> {
				var game = new Game();

				var existingGame = existingGamesVWs.stream().filter(x -> x.getCode() == i.id()).findFirst().orElse(null);			
				game.setId(existingGame != null ? existingGame.getId() : 0);
				game.setName(i.names().international());
				game.setCode(i.id());
				game.setAbbr(i.abbreviation());
				game.setShowMilliseconds(i.ruleset() != null ? i.ruleset().showMilliseconds() : false);
				game.setReleaseDate(i.releaseDate());
				game.setImportRefDate(i.created());

				var categoryTypes = i.categories().data().stream()
										.map(x -> { 
											var categoryType = new CategoryType();
											categoryType.setId(Enums.CategoryType.valueOf(StringExtensions.KebabToUpperCamelCase(x.type())).getValue());
											categoryType.setName(Enums.CategoryType.valueOf(StringExtensions.KebabToUpperCamelCase(x.type())).name());
											return categoryType;
										}).collect(Collectors.groupingBy(CategoryType::getId, Collectors.collectingAndThen(
											Collectors.toList(), 
											values -> values.get(0)))).values().stream().toList();
				game.setCategoryTypes(categoryTypes);

				var categories = i.categories().data().stream()
									.map(x -> { 
										var category = new Category();
										category.setId(existingGame != null ? existingGame.getCategories().stream().filter(g ->  g.getCode() == x.id()).map(g -> g.getId()).findFirst().orElse(0) : 0);
										category.setName(x.name());
										category.setCode(x.id());
										category.setGameId(game.getId());
										category.setCategoryTypeId(Enums.CategoryType.valueOf(StringExtensions.KebabToUpperCamelCase(x.type())).getValue());
										category.setMiscellaneous(x.miscellaneous());
										category.setIsTimerAscending(false);
										return category;
									}).toList();
				game.setCategories(categories);

				var levels = i.levels().data().stream()
									.map(x -> { 
										var level = new Level();
										level.setId(existingGame != null ? existingGame.getLevels().stream().filter(g ->  g.getCode() == x.id()).map(g -> g.getId()).findFirst().orElse(0) : 0);
										level.setName(x.name());
										level.setCode(x.id());
										level.setGameId(game.getId());
										return level;
									}).toList();
				game.setLevels(levels);

				var variables = i.variables().data().stream()
									.map(x -> { 
										var variable = new Variable();
										variable.setId(existingGame != null ? existingGame.getVariables().stream().filter(g ->  g.getCode() == x.id()).map(g -> g.getId()).findFirst().orElse(0) : 0);
										variable.setName(x.name());
										variable.setCode(x.id());
										variable.setGameId(game.getId());
										variable.setVariableScopeTypeId(Enums.VariableScopeType.valueOf(StringExtensions.KebabToUpperCamelCase(x.scope().type())).getValue());
										variable.setCategoryCode(x.category());
										variable.setLevelCode(x.scope().level());
										return variable;
									}).toList();
				game.setVariables(variables);			

				var variableValues = i.variables().data().stream()
									.flatMap(x -> x.values().values().entrySet().stream().map(g -> {
										var variableValue = new VariableValue();
										variableValue.setId(existingGame != null ? existingGame.getVariableValues().stream().filter(h ->  h.getCode() == g.getKey()).map(h -> h.getId()).findFirst().orElse(0) : 0);
										variableValue.setName(g.getValue().label());
										variableValue.setCode(g.getKey());		
										variableValue.setGameId(game.getId());
										variableValue.setVariableId(existingGame != null ? existingGame.getVariables().stream().filter(h ->  h.getCode() == x.id()).map(h -> h.getId()).findFirst().orElse(0) : 0);
										variableValue.setVariableCode(x.id());
										return variableValue;							
									})).toList();
				game.setVariableValues(variableValues);
				
				if (existingGame != null) {
					var removeCategories = existingGame.getCategories().stream().filter(g -> !categories.stream().anyMatch(h -> h.getCode() == g.getCode())).map(g -> g.getId()).toList();
					game.setCategoriesToRemove(removeCategories);

					var removeLevels = existingGame.getLevels().stream().filter(g -> !levels.stream().anyMatch(h -> h.getCode() == g.getCode())).map(g -> g.getId()).toList();
					game.setLevelsToRemove(removeLevels);	
					
					var removeVariables = existingGame.getVariables().stream().filter(g -> !levels.stream().anyMatch(h -> h.getCode() == g.getCode())).map(g -> g.getId()).toList();
					game.setVariablesToRemove(removeVariables);	
					
					var removeVariableValues = existingGame.getVariableValues().stream().filter(g -> !levels.stream().anyMatch(h -> h.getCode() == g.getCode())).map(g -> g.getId()).toList();
					game.setVariableValuesToRemove(removeVariableValues);						
				}
				
				return game;
			}).toList();

		_gameRepo.SaveGames(gameEntities);
		
		_logger.info("Completed SaveGames");
	}
}
