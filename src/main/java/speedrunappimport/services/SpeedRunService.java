package speedrunappimport.services;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse.BodyHandlers;
import java.time.Instant;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;
import java.util.Arrays;
import java.util.HashMap;
import java.util.stream.Collectors;
import java.util.function.Function;
import java.time.Duration;

import org.slf4j.Logger;

import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.PropertyNamingStrategies;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;

import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.interfaces.services.*;
import speedrunappimport.model.entity.*;
import speedrunappimport.model.json.*;
import speedrunappimport.model.Enums.*;
import speedrunappimport.common.*;

public class SpeedRunService extends BaseService implements ISpeedRunService {
	private ISpeedRunRepository _speedRunRepo;
	private IGameRepository _gameRepo;
	private IPlatformRepository _platformRepo;
	private IPlayerRepository _playerRepo;
	private ISettingService _settingService;
	private Logger _logger;

	public SpeedRunService(ISpeedRunRepository speedRunRepo, IGameRepository gameRepo, IPlatformRepository platformRepo, IPlayerRepository playerRepo, ISettingService settingService, Logger logger) {
		_speedRunRepo = speedRunRepo;
		_gameRepo = gameRepo;
		_platformRepo = platformRepo;
		_playerRepo = playerRepo;
		_settingService = settingService;
		_logger = logger;
	}

	public boolean ProcessSpeedRuns(boolean isReload) {
		boolean result = false;
		
		if (isReload) {
			result = ProcessSpeedRunsByGame();
		} else {
			result = ProcessSpeedRunsByDate();
		}

		return result;
	}

	public boolean ProcessSpeedRunsByGame() {
		boolean result = false;

		try {
			var stLastImportDateUtc = _settingService.GetSetting("LastImportDate");
			var lastImportDateUtc = stLastImportDateUtc != null && stLastImportDateUtc.getDte() != null ? stLastImportDateUtc.getDte() : this.getSqlMinDateTime();				
			_logger.info("Started ProcessSpeedRunsByGame: {}", lastImportDateUtc);

			var results = new ArrayList<SpeedRunResponse>();
			List<SpeedRunResponse> runs = new ArrayList<SpeedRunResponse>();
			var prevTotal = 0;
			var limit = super.getMaxPageLimit();
			var games = _gameRepo.GetGameViewsModifiedAfter(lastImportDateUtc);
			var isSaved = false;

			for (var game : games) {
				do {
					try {
						runs = GetSpeedRunResponses(limit, ((int)results.stream().filter(i->i.game().equals(game.getCode())).count()), game.getCode(), null, null);
						results.addAll(runs);
						_logger.info("GameCode: {}, pulled runs: {}, game total: {}, total runs: {}", game.getCode(), runs.size(), ((int)results.stream().filter(i->i.game().equals(game.getCode())).count()), results.size() + prevTotal);						
						Thread.sleep(super.getPullDelayMS());
					} catch (PaginationException ex) {
						results.removeIf(i -> i.game().equals(game.getCode()));
						runs = ProcessSpeedRunsByCategory(game, results.size() + prevTotal);
						results.addAll(runs);
						break;
					}
				}
				while (runs.size() == limit);
				//while (1 == 0);

				var memorySize = Runtime.getRuntime().totalMemory() - Runtime.getRuntime().freeMemory();
				if (results.size() > 0 && memorySize > super.getMaxMemorySizeBytes()) {
					_logger.info("Saving to clear memory, results: {}, size: {}", results.size(), memorySize);
					prevTotal += results.size();
					SaveSpeedRunResponses(results, true);
					isSaved = true;
					results = new ArrayList<SpeedRunResponse>();
					System.gc();
				}
			}

			if (results.size() > 0) {
				SaveSpeedRunResponses(results, true);
				isSaved = true;
				results = new ArrayList<SpeedRunResponse>();
				System.gc();			
			}

			if (isSaved) {
				FinalizeSavedSpeedRuns(true);				
			}

			result = true;	
			_logger.info("Completed ProcessSpeedRunsByGame");
		} catch (Exception ex) {
			result = false;
			_logger.error("ProcessSpeedRunsByGame", ex);
		}

		return result;
	}

	public List<SpeedRunResponse> ProcessSpeedRunsByCategory(GameView game, int prevTotal) throws Exception {
		var limit = super.getMaxPageLimit();
		var results = new ArrayList<SpeedRunResponse>();	
		List<SpeedRunResponse> runs = new ArrayList<SpeedRunResponse>();
	
		for (var category : game.getCategories()) {							
				do {
					try {
						runs = GetSpeedRunResponses(limit, ((int)results.stream().filter(i -> i.game().equals(game.getCode()) && i.category().equals(category.getCode())).count()), game.getCode(), category.getCode(), SpeedRunsOrderBy.SUBMITTED);
						results.addAll(runs);	
						_logger.info("GameCode: {}, CategoryCode: {}, pulled runs: {}, order: asc, category total: {}, total runs: {}", game.getCode(), category.getCode(), runs.size(), ((int)results.stream().filter(i -> i.game().equals(game.getCode()) && i.category().equals(category.getCode())).count()), results.size() + prevTotal);
						Thread.sleep(super.getPullDelayMS());
					} catch (PaginationException ex) {
						runs = ProcessSpeedRunsByCategoryDesc(game, category, results.size() + prevTotal);
						results.addAll(runs);	
						break;
					}
				}
				while (runs.size() == limit);
		}

		return results;
	}

	public List<SpeedRunResponse> ProcessSpeedRunsByCategoryDesc(GameView game, Category category, int prevTotal) throws Exception {
		var limit = super.getMaxPageLimit();
		var results = new ArrayList<SpeedRunResponse>();	
		List<SpeedRunResponse> runs = new ArrayList<SpeedRunResponse>();

		do {
			try {
				runs = GetSpeedRunResponses(limit, ((int)results.stream().filter(i -> i.game().equals(game.getCode()) && i.category().equals(category.getCode())).count()), game.getCode(), category.getCode(), SpeedRunsOrderBy.SUBMITTED_DESC);
				results.addAll(runs);	
				_logger.info("GameCode: {}, CategoryCode: {}, pulled runs: {}, order: desc, category total: {}, total runs: {}", game.getCode(), category.getCode(), runs.size(), ((int)results.stream().filter(i -> i.game().equals(game.getCode()) && i.category().equals(category.getCode())).count()), results.size() + prevTotal);
				Thread.sleep(super.getPullDelayMS());
			} catch (PaginationException ex) {
				_logger.error("ProcessSpeedRunsByCategoryDesc", ex);
				break;
			}
		}
		while (runs.size() == limit);

		return results;
	}	

	public boolean ProcessSpeedRunsByDate() {
		boolean result = false;

		try {
			var stSpeedRunLastImportRefDateUtc = _settingService.GetSetting("SpeedRunLastImportRefDate");
			var lastImportRefDateUtc = stSpeedRunLastImportRefDateUtc != null && stSpeedRunLastImportRefDateUtc.getDte() != null ? stSpeedRunLastImportRefDateUtc.getDte() : this.getSqlMinDateTime();	
			var currImportRefDateUtc = lastImportRefDateUtc;
			_logger.info("Started ProcessSpeedRunsByDate: {}", lastImportRefDateUtc);

			List<SpeedRunResponse> results = new ArrayList<SpeedRunResponse>();
			List<SpeedRunResponse> runs = new ArrayList<SpeedRunResponse>();
			var prevTotal = 0;
			var limit = super.getMaxPageLimit();
			var isSaved = false;

			do {
				runs = GetSpeedRunResponses(limit, results.size() + prevTotal, null, null, SpeedRunsOrderBy.VERIFY_DATE_DESC);
				results.addAll(runs);
				Thread.sleep(super.getPullDelayMS());
				_logger.info("Pulled runs: {}, total runs: {}", runs.size(), results.size() + prevTotal);

				var memorySize = Runtime.getRuntime().totalMemory() - Runtime.getRuntime().freeMemory();
				if (results.size() > 0 && memorySize > super.getMaxMemorySizeBytes()) {
					results = results.reversed();
					results.removeIf(i -> (i.status().verifyDate() != null ? i.status().verifyDate() : super.getSqlMinDateTime()).compareTo(lastImportRefDateUtc) <= 0);
					
					if (results.size() > 0) {
						_logger.info("Saving to clear memory, results: {}, size: {}", results.size(), memorySize);
						prevTotal += results.size();
						currImportRefDateUtc = results.stream().map(i -> i.status().verifyDate() != null ? i.status().verifyDate() : super.getSqlMinDateTime()).max(Instant::compareTo).get();					
						SaveSpeedRunResponses(results, false);
						isSaved = true;
						results = new ArrayList<SpeedRunResponse>();
						System.gc();
					}
				}
			}
			while (runs.size() == limit && runs.stream().map(i -> i.status().verifyDate() != null ? i.status().verifyDate() : super.getSqlMinDateTime()).max(Instant::compareTo).get().compareTo(lastImportRefDateUtc) > 0);
			//while (1 == 0);

			results = results.reversed();
			results.removeIf(i -> (i.status().verifyDate() != null ? i.status().verifyDate() : super.getSqlMinDateTime()).compareTo(lastImportRefDateUtc) <= 0);

			if (results.size() > 0) {			
				currImportRefDateUtc = results.stream().map(i -> i.status().verifyDate() != null ? i.status().verifyDate() : super.getSqlMinDateTime()).max(Instant::compareTo).get();
				SaveSpeedRunResponses(results, false);
				isSaved = true;
				results = new ArrayList<SpeedRunResponse>();
				System.gc();
			}

			if (isSaved) {
				FinalizeSavedSpeedRuns(false);				
				_settingService.UpdateSetting("SpeedRunLastImportRefDate", currImportRefDateUtc);	
			}
			
			result = true;	
			_logger.info("Completed ProcessSpeedRuns");
		} catch (Exception ex) {
			result = false;
			_logger.error("ProcessSpeedRuns", ex);
		}

		return result;
	}

	public List<SpeedRunResponse> GetSpeedRunResponses(int limit, int offset, String gameCode, String categoryCode, SpeedRunsOrderBy orderBy) throws Exception {
		return GetSpeedRunResponses(limit, offset, gameCode, categoryCode, orderBy, 0);
	}		

	public List<SpeedRunResponse> GetSpeedRunResponses(int limit, int offset, String gameCode, String categoryCode, SpeedRunsOrderBy orderBy, int retryCount) throws Exception {
		List<SpeedRunResponse> data = new ArrayList<SpeedRunResponse>();

		try (var client = HttpClient.newHttpClient()) {
			var parameters = new HashMap<String, String>();
			parameters.put("status", "verified");
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
				var orderByString = "";
				var isDesc = false;

				switch (orderBy) {			
					case SpeedRunsOrderBy.SUBMITTED:
					case SpeedRunsOrderBy.SUBMITTED_DESC:
						orderByString = "submitted";
						isDesc = (orderBy == SpeedRunsOrderBy.SUBMITTED_DESC);
						break;	
					case SpeedRunsOrderBy.VERIFY_DATE:
					case SpeedRunsOrderBy.VERIFY_DATE_DESC:
						orderByString = "verify-date";
						isDesc = (orderBy == SpeedRunsOrderBy.VERIFY_DATE_DESC);
						break;						
					default:
						orderByString = "game";
						isDesc = false;
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
				if (retryCount == super.getMaxRetryCount()) {
					_logger.info("Pausing application");
					Thread.sleep(super.getPauseDelayMS());					
				}
				_logger.info("Retrying pull runs: {}, total runs: {}, retry: {}", limit, offset, retryCount);
				data = GetSpeedRunResponses(limit, offset, gameCode, categoryCode, orderBy, retryCount);
			} else {
				_logger.info("Retry max reached");
				throw ex;
			}
		}

		return data;
	}

	public void SaveSpeedRunResponses(List<SpeedRunResponse> runResponses, boolean isReload) {
		_logger.info("Started SaveSpeedRunResponses: {}, {}", runResponses.size(), isReload);

		if (runResponses.size() > 0) {
			if (!isReload) {
				runResponses = runResponses.reversed();
			}

			runResponses = runResponses.stream()
					.collect(Collectors.toMap(SpeedRunResponse::id, Function.identity(), (u1, u2) -> u1))
					.values()
					.stream()
					.collect(Collectors.toList());		

			SavePlayersFromRunResponses(runResponses);

			var playerCodes = runResponses.stream().flatMap(x -> x.players().data().stream().map(i -> { return StringExtensions.KebabToUpperSnakeCase(i.rel()).equals(PlayerType.USER.toString()) ? i.id() : i.name(); })).distinct().toList();	
			var existingPlayerVWs = _playerRepo.GetPlayerViewsByCode(playerCodes);
			var existingRunVWs = _speedRunRepo.GetSpeedRunViewsByCode(runResponses.stream().map(x -> x.id()).toList());
			var existingGameVWs = _gameRepo.GetGameViewsByCode(runResponses.stream().map(x -> x.game()).distinct().toList());
			var runs = GetSpeedRunsFromResponses(runResponses, existingRunVWs, existingGameVWs, existingPlayerVWs);
			runs = GetNewOrChangedSpeedRuns(runs, existingRunVWs);
			_speedRunRepo.SaveSpeedRuns(runs);
		}

		_logger.info("Completed SaveSpeedRunResponses");	
	}

	public void FinalizeSavedSpeedRuns(boolean isReload) {
		_logger.info("Started FinalizeSavedSpeedRuns: {}", isReload);

		var stLastImportDateUtc = _settingService.GetSetting("LastImportDate");
		var lastImportDateUtc = stLastImportDateUtc != null && stLastImportDateUtc.getDte() != null ? stLastImportDateUtc.getDte() : getSqlMinDateTime();	
		
		if (isReload) {
			lastImportDateUtc = null;
		}					
		
		_speedRunRepo.UpdateSpeedRunRanks(lastImportDateUtc);
		_speedRunRepo.UpdateSpeedRunOrdered(lastImportDateUtc);

		_logger.info("Completed FinalizeSavedSpeedRuns");
	}

	private void SavePlayersFromRunResponses(List<SpeedRunResponse> runs) {
		_logger.info("Started SavePlayersFromRunResponses: {}", runs.size());
	
		var playerCodes = runs.stream().flatMap(x -> x.players().data().stream().map(i -> { return StringExtensions.KebabToUpperSnakeCase(i.rel()).equals(PlayerType.USER.toString()) ? i.id() : i.name(); })).distinct().toList();
		var existingPlayerVWs = _playerRepo.GetPlayerViewsByCode(playerCodes);
		var players = GetPlayersFromResponses(runs, existingPlayerVWs);
		players = GetNewOrChangedPlayers(players, existingPlayerVWs);
		_playerRepo.SavePlayers(players);	
		
		_logger.info("Completed SavePlayersFromRunResponses");			
	}

	private List<Player> GetPlayersFromResponses(List<SpeedRunResponse> runs, List<PlayerView> existingPlayerVWs) {
		var playerEntities = runs.stream().flatMap(x -> x.players().data().stream().map(i -> {
			var player = new Player();
			
			var playerTypeId = PlayerType.valueOf(StringExtensions.KebabToUpperSnakeCase(i.rel())).getValue();
			var playerCode = playerTypeId == PlayerType.USER.getValue() ? i.id() : i.name();
			var playerName = playerTypeId == PlayerType.USER.getValue() ? i.names().international() : i.name();
			var playerSrcUrl = playerTypeId == PlayerType.USER.getValue() ? i.weblink() : i.links().stream().filter(g -> g.rel().equals("self")).map(g -> g.uri()).findFirst().orElse(null);
			var existingPlayerVW = existingPlayerVWs.stream().filter(g -> g.getCode().equals(playerCode)).findFirst().orElse(null);

			player.setId(existingPlayerVW != null ? existingPlayerVW.getId() : 0);
			player.setName(playerName);			
			player.setCode(playerCode);
			player.setPlayerTypeId(playerTypeId);	

			var playerLink = new PlayerLink();
			playerLink.setId(existingPlayerVW != null ? existingPlayerVW.getPlayerLinkId() : 0);
			playerLink.setPlayerId(player.getId());
			playerLink.setProfileImageUrl(i.links() != null ? i.links().stream().filter(g -> g.rel().equals("image")).map(g -> g.uri()).findFirst().orElse(null) : null);
			playerLink.setSrcUrl(playerSrcUrl);
			playerLink.setTwitchUrl(i.twich() != null ? i.twich().uri() : null);
			playerLink.setHitboxUrl(i.hitbox() != null ? i.hitbox().uri() : null);
			playerLink.setYoutubeUrl(i.youtube() != null ? i.youtube().uri() : null);
			playerLink.setTwitterUrl(i.twitter() != null ? i.twitter().uri() : null);
			playerLink.setSpeedRunsLiveUrl(i.speedrunslive() != null ? i.speedrunslive().uri() : null);
			player.setPlayerLink(playerLink);

			return player;
		})).collect(Collectors.toMap(Player::getCode, Function.identity(), (u1, u2) -> u1))
		.values()
		.stream()
		.collect(Collectors.toList());

		return playerEntities;
	}
	
	private List<Player> GetNewOrChangedPlayers(List<Player> players, List<PlayerView> existingPlayerVWs) {	
		var results = new ArrayList<Player>();
		var newCount = 0;
		var changedCount = 0;

		for (var player : players) {
			var isNew = false;	
			var isChanged = false;
		
			if (player.getId() == 0) {
				isNew = true;
				newCount++;
			} else {
				var existingPlayerVW = existingPlayerVWs.stream().filter(g -> g.getCode().equals(player.getCode())).findFirst().orElse(null);
				
				if (existingPlayerVW != null) {
					isChanged = (!player.getName().equalsIgnoreCase(existingPlayerVW.getName())
						|| !Objects.equals(player.getPlayerLink().getProfileImageUrl(), existingPlayerVW.getProfileImageUrl())
						|| !Objects.equals(player.getPlayerLink().getSrcUrl(), existingPlayerVW.getSrcUrl())
						|| !Objects.equals(player.getPlayerLink().getHitboxUrl(), existingPlayerVW.getHitboxUrl())
						|| !Objects.equals(player.getPlayerLink().getTwitchUrl(), existingPlayerVW.getTwitchUrl())
						|| !Objects.equals(player.getPlayerLink().getTwitterUrl(), existingPlayerVW.getTwitterUrl())
						|| !Objects.equals(player.getPlayerLink().getYoutubeUrl(), existingPlayerVW.getYoutubeUrl()));
						
					if (isChanged){
						changedCount++;
					}
				}
			}

			if (isNew || isChanged) {
				results.add(player);
			}
		}

		_logger.info("Found New: {}, Changed: {}, Total: {}", newCount, changedCount, results.size());	
		return results;
	}	

	private List<SpeedRun> GetSpeedRunsFromResponses(List<SpeedRunResponse> runs, List<SpeedRunView> existingRunVWs, List<GameView> existingGameVWs, List<PlayerView> existingPlayerVWs) {
		var platforms = _platformRepo.GetAllPlatforms();
		var existingCameCodes = existingGameVWs.stream().map(i -> i.getCode()).toList();

		var runEntities = runs.stream().filter(i -> existingCameCodes.contains(i.game())).map(i -> {
			var run = new SpeedRun();

			var existingGameVW = existingGameVWs.stream().filter(x -> x.getCode().equals(i.game())).findFirst().orElse(null);		
			var existingRunVW = existingRunVWs.stream().filter(x -> x.getCode().equals(i.id())).findFirst().orElse(null);		

			run.setId(existingRunVW != null ? existingRunVW.getId() : 0);
			run.setCode(i.id());
			run.setGameId(existingGameVW.getId());
			run.setCategoryId(existingGameVW.getCategories().stream().filter(g ->  g.getCode().equals(i.category())).map(g -> g.getId()).findFirst().orElse(0));
			run.setLevelId(i.level() != null ? existingGameVW.getLevels().stream().filter(g ->  g.getCode().equals(i.level())).map(g -> g.getId()).findFirst().orElse(0) : null);
			run.setPlatformId(platforms.stream().filter(g ->  g.getCode().equals(i.system().platform())).map(g -> g.getId()).findFirst().orElse(null));
			run.setPrimaryTime(Duration.parse(i.times().primary()).toMillis());
			run.setDateSumbitted(i.submitted());
			run.setVerifyDate(i.status().verifyDate());

			var link = new SpeedRunLink();
			link.setId(existingRunVW != null ? existingRunVW.getSpeedRunLinkId() : 0);
			link.setSpeedRunId(run.getId());
			link.setSrcUrl(i.weblink());
			run.setSpeedRunLink(link);

			var playerCodes = i.players().data().stream().map(x -> StringExtensions.KebabToUpperSnakeCase(x.rel()).equals(PlayerType.USER.toString()) ? x.id() : x.name()).toList();
			var runPlayers = existingPlayerVWs.stream()
								.filter(g -> playerCodes.contains(g.getCode()))
								.map(x -> { 
									var runPlayer = new SpeedRunPlayer();
									runPlayer.setId(existingRunVW != null ? existingRunVW.getPlayers().stream().filter(g ->  g.getPlayerId() == x.getId()).map(g -> g.getId()).findFirst().orElse(0) : 0);
									runPlayer.setPlayerId(x.getId());
									return runPlayer;
								}).toList();
			run.setPlayers(runPlayers);				

			var variableValues = i.values().entrySet().stream()
											.map(x -> { 
												var variableValue = new SpeedRunVariableValue();
												variableValue.setId(existingRunVW != null ? existingRunVW.getVariableValues().stream().filter(g ->  g.getVariableCode().equals(x.getKey()) && g.getVariableValueCode().equals(x.getValue())).map(g -> g.getId()).findFirst().orElse(0) : 0);
												variableValue.setSpeedRunId(run.getId());
												variableValue.setVariableId(existingGameVW.getVariables().stream().filter(g -> g.getCode().equals(x.getKey())).map(g -> g.getId()).findFirst().orElse(0));			
												variableValue.setVariableValueId(existingGameVW.getVariableValues().stream().filter(g -> g.getCode().equals(x.getValue())).map(g -> g.getId()).findFirst().orElse(0));
												return variableValue;
											}).filter(x -> x.getVariableId() != 0 && x.getVariableValueId() != 0).toList();
			run.setVariableValues(variableValues);

			List<SpeedRunVideo> videos = new ArrayList<SpeedRunVideo>();
			if (i.videos() != null && i.videos().links() != null) {
				videos = i.videos().links().stream()
									.map(x -> { 
										var video = new SpeedRunVideo();
										video.setId(existingRunVW != null ? existingRunVW.getVideos().stream().filter(g ->  g.getVideoLinkUrl().equals(x.uri())).map(g -> g.getId()).findFirst().orElse(0) : 0);
										video.setSpeedRunId(run.getId());
										video.setVideoLinkUrl(x.uri());

										try {
											video.setThumbnailLinkUrl(UriExtensions.ToThumbnailURIString(x.uri()));
										} catch (IllegalArgumentException ex) {
											_logger.info(ex.getMessage());
										}

										try {
											video.setEmbeddedVideoLinkUrl(UriExtensions.ToEmbeddedURIString(x.uri()));
										} catch (IllegalArgumentException ex) {
											_logger.info(ex.getMessage());
										}

										return video;
									}).toList();
			}
			run.setVideos(videos);

			if (existingRunVW != null) {
				if (existingRunVW.getPlayers() != null) {										
					var removePlayers = existingRunVW.getPlayers().stream().filter(g -> !run.getPlayers().stream().anyMatch(h -> h.getId() == g.getId())).map(g -> g.getId()).toList();
					run.setPlayersToRemove(removePlayers);	
				}		
				
				if (existingRunVW.getVariableValues() != null) {										
					var removeVariableValues = existingRunVW.getVariableValues().stream().filter(g -> !run.getVariableValues().stream().anyMatch(h -> h.getId() == g.getId())).map(g -> g.getId()).toList();
					run.setVariableValuesToRemove(removeVariableValues);	
				}	
				
				if (existingRunVW.getVideos() != null) {										
					var removeVideos = existingRunVW.getVideos().stream().filter(g -> !run.getVideos().stream().anyMatch(h -> h.getId() == g.getId())).map(g -> g.getId()).toList();
					run.setVideosToRemove(removeVideos);	
				}					
			}
			
			return run;
		}).filter(i -> i.getCategoryId() != 0 && !Objects.equals(i.getLevelId(), 0)).toList();

		return runEntities;
	}

	private List<SpeedRun> GetNewOrChangedSpeedRuns(List<SpeedRun> runs, List<SpeedRunView> existingRunVWs) {	
		var results = new ArrayList<SpeedRun>();
		var newCount = 0;
		var changedCount = 0;

		for (var run : runs) {
			var isNew = false;	
			var isChanged = false;
		
			if (run.getId() == 0) {
				isNew = true;
				newCount++;
			} else {
				var existingRunVW = existingRunVWs.stream().filter(g -> g.getCode().equals(run.getCode())).findFirst().orElse(null);
				
				if (existingRunVW != null) {
					isChanged = (!Objects.equals(run.getCategoryId(), existingRunVW.getCategoryId())
						|| !Objects.equals(run.getGameId(), existingRunVW.getGameId())
						|| !Objects.equals(run.getLevelId(), existingRunVW.getLevelId())
						|| !Objects.equals(run.getPlatformId(), existingRunVW.getPlatformId())
						|| !Objects.equals(run.getPrimaryTime(), existingRunVW.getPrimaryTime())
						|| !Objects.equals(run.getDateSubmitted(), existingRunVW.getDateSubmitted())
						|| !Objects.equals(run.getVerifyDate(), existingRunVW.getVerifyDate())
						|| !Objects.equals(run.getSpeedRunLink().getSrcUrl(), existingRunVW.getSrcUrl()));
					
					if (!isChanged) {
						var variableCodes = run.getVariableValues().stream().map(i -> i.getVariableCode()).toList();
						var existingVariableCodes = existingRunVW.getVariableValues().stream().map(i -> i.getVariableCode()).toList();
						isChanged = variableCodes.stream().anyMatch(i -> !existingVariableCodes.contains(i))
									|| existingVariableCodes.stream().anyMatch(i -> !variableCodes.contains(i));
					}					

					if (!isChanged) {
						var variableValueIndex = 0;		
						var existingVaraibleValues = existingRunVW.getVariableValues();					
						for (var variableValue : run.getVariableValues()) {
							var existingVariable = variableValueIndex < existingVaraibleValues.size() ? existingVaraibleValues.get(variableValueIndex) : null;
							isChanged = (existingVariable == null 
								|| !variableValue.getVariableCode().equals(existingVariable.getVariableCode())
								|| !variableValue.getVariableValueCode().equals(existingVariable.getVariableValueCode()));

							if (isChanged) {
								break;
							}

							variableValueIndex++;
						}
					}

					if (isChanged){
						changedCount++;
					}
				}
			}

			if (isNew || isChanged) {
				results.add(run);
			}
		}

		_logger.info("Found New: {}, Changed: {}, Total: {}", newCount, changedCount, results.size());	
		return results;
	}		
}
