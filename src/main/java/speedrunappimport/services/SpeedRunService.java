package speedrunappimport.services;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse.BodyHandlers;
import java.nio.charset.StandardCharsets;
import java.net.URLDecoder;
import java.time.Instant;
import java.time.ZoneId;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;
import java.util.Arrays;
import java.util.HashMap;
import java.util.stream.Collectors;
import java.util.function.Function;
import java.time.Duration;
import java.nio.file.Paths;

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
	private IAuthService _authService;
	private Logger _logger;

	public SpeedRunService(ISpeedRunRepository speedRunRepo, IGameRepository gameRepo, IPlatformRepository platformRepo, IPlayerRepository playerRepo, ISettingService settingService, IAuthService authService, Logger logger) {
		_speedRunRepo = speedRunRepo;
		_gameRepo = gameRepo;
		_platformRepo = platformRepo;
		_playerRepo = playerRepo;
		_settingService = settingService;
		_authService = authService;
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
			// var codes = new ArrayList<String>();
			// codes.add("ldewmwjd");
			// var games = _gameRepo.GetGameViewsByCode(codes);			
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

				if (results.size() > super.getMaxRecordCount()) {
					var memorySize = Runtime.getRuntime().totalMemory() - Runtime.getRuntime().freeMemory();
					_logger.info("Saving to clear memory, results: {}, size: {}", results.size(), memorySize);
					prevTotal += results.size();
					SaveSpeedRunResponses(results, true);
					isSaved = true;
					results = new ArrayList<SpeedRunResponse>();
				}
			}

			if (results.size() > 0) {
				SaveSpeedRunResponses(results, true);
				isSaved = true;		
			}

			if (isSaved) {
				FinalizeSavedSpeedRuns(true);		

				var stSpeedRunLastImportRefDateUtc = _settingService.GetSetting("SpeedRunLastImportRefDate");	
				if (stSpeedRunLastImportRefDateUtc == null || stSpeedRunLastImportRefDateUtc.getDte() == null) {
					var lastImportRefDateUtc = _speedRunRepo.GetMaxVerifyDate();
					_settingService.UpdateSetting("SpeedRunLastImportRefDate", lastImportRefDateUtc);	
				}
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
				_logger.info("Max pagination reached");
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

				if (results.size() > super.getMaxRecordCount()) {
					results = results.reversed();
					results.removeIf(i -> (i.status().verifyDate() != null ? i.status().verifyDate() : super.getSqlMinDateTime()).compareTo(lastImportRefDateUtc) <= 0);
	
					var memorySize = Runtime.getRuntime().totalMemory() - Runtime.getRuntime().freeMemory();					
					_logger.info("Saving to clear memory, results: {}, size: {}", results.size(), memorySize);
					prevTotal += results.size();
					currImportRefDateUtc = results.stream().map(i -> i.status().verifyDate() != null ? i.status().verifyDate() : super.getSqlMinDateTime()).max(Instant::compareTo).get();					
					SaveSpeedRunResponses(results, false);
					isSaved = true;
					results = new ArrayList<SpeedRunResponse>();						
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
				_logger.info("Retrying pull runs: {}, total runs: {}, retry: {}", limit, offset, retryCount);
				data = GetSpeedRunResponses(limit, offset, gameCode, categoryCode, orderBy, retryCount);
			} else {
				_logger.info("Max retry reached");
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
			var existingPlayers = _playerRepo.GetPlayersByCode(playerCodes);
			var existingRunVWs = _speedRunRepo.GetSpeedRunViewsByCode(runResponses.stream().map(x -> x.id()).toList());
			var existingGameVWs = _gameRepo.GetGameViewsByCode(runResponses.stream().map(x -> x.game()).distinct().toList());
			var runs = GetSpeedRunsFromResponses(runResponses, existingRunVWs, existingGameVWs, existingPlayers);
			// if (!isReload) {
			// 	runs = GetNewOrChangedSpeedRuns(runs, existingRunVWs);
			// }
			_speedRunRepo.SaveSpeedRuns(runs);
		}

		_logger.info("Completed SaveSpeedRunResponses");	
	}

	public void FinalizeSavedSpeedRuns(boolean isReload) {
		_logger.info("Started FinalizeSavedSpeedRuns: {}", isReload);

		var stLastImportDateUtc = _settingService.GetSetting("LastImportDate");
		var lastImportDateUtc = stLastImportDateUtc != null && stLastImportDateUtc.getDte() != null ? stLastImportDateUtc.getDte() : getSqlMinDateTime();	
		
		if (isReload) {
			_speedRunRepo.DeleteObsoleteSpeedRuns(lastImportDateUtc);
		}					
		
		_speedRunRepo.UpdateSpeedRunRanks(lastImportDateUtc);
		_speedRunRepo.UpdateSpeedRunSummary(lastImportDateUtc);

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
			var playerSrcPath = URI.create(playerSrcUrl).getRawPath();
			var path = Paths.get(playerSrcPath);
			var lastSegment = path.getName(path.getNameCount() - 1).toString();			
			var playerAbbr = URLDecoder.decode(lastSegment, StandardCharsets.UTF_8);

			var existingPlayerVW = existingPlayerVWs.stream().filter(g -> g.getCode().equals(playerCode)).findFirst().orElse(null);

			player.setId(existingPlayerVW != null ? existingPlayerVW.getId() : 0);
			player.setName(playerName);			
			player.setCode(playerCode);
			player.setAbbr(playerAbbr);
			player.setPlayerTypeId(playerTypeId);	

			var playerLink = new PlayerLink();
			playerLink.setId(existingPlayerVW != null ? existingPlayerVW.getPlayerLinkId() : 0);
			playerLink.setPlayerId(player.getId());
			playerLink.setProfileImageUrl(i.assets() != null && i.assets().image() != null ? i.assets().image().uri() : null);
			playerLink.setSrcUrl(playerSrcUrl);
			playerLink.setTwitchUrl(i.twich() != null ? i.twich().uri() : null);
			playerLink.setHitboxUrl(i.hitbox() != null ? i.hitbox().uri() : null);
			playerLink.setYoutubeUrl(i.youtube() != null ? i.youtube().uri() : null);
			playerLink.setTwitterUrl(i.twitter() != null ? i.twitter().uri() : null);
			playerLink.setSpeedRunsLiveUrl(i.speedrunslive() != null ? i.speedrunslive().uri() : null);
			player.setPlayerLink(playerLink);

			if (i.nameStyle() != null) {
				var playerNameStyle = new PlayerNameStyle();
				playerNameStyle.setId(existingPlayerVW != null ? existingPlayerVW.getPlayerNameStyleId() : 0);
				playerNameStyle.setPlayerId(player.getId());
				playerNameStyle.setIsGradient(i.nameStyle().style().equals("gradient"));
				if (i.nameStyle().color() != null) {
					playerNameStyle.setColorLight(i.nameStyle().color().light());
					playerNameStyle.setColorDark(i.nameStyle().color().dark());
				} else {
					playerNameStyle.setColorLight(i.nameStyle().colorFrom().light());
					playerNameStyle.setColorToLight(i.nameStyle().colorTo().light());
					playerNameStyle.setColorDark(i.nameStyle().colorFrom().dark());
					playerNameStyle.setColorToDark(i.nameStyle().colorTo().dark());
				}
				player.setPlayerNameStyle(playerNameStyle);
			}

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
						|| !Objects.equals(player.getAbbr(), existingPlayerVW.getAbbr())
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

		_logger.info("Found New: {}, Changed: {}, Existing: {}, Total: {}", newCount, changedCount, existingPlayerVWs.size(), players.size());	
		return results;
	}	

	private List<SpeedRun> GetSpeedRunsFromResponses(List<SpeedRunResponse> runs, List<SpeedRunView> existingRunVWs, List<GameView> existingGameVWs, List<Player> existingPlayers) {
		var platforms = _platformRepo.GetAllPlatforms();
		var existingCameCodes = existingGameVWs.stream().map(i -> i.getCode()).toList();

		var runEntities = runs.stream().filter(i -> existingCameCodes.contains(i.game())).map(i -> {
			var run = new SpeedRun();

			var existingGameVW = existingGameVWs.stream().filter(x -> x.getCode().equals(i.game())).findFirst().orElse(null);	
			var existingRunVW = existingRunVWs.stream().filter(x -> x.getCode().equals(i.id())).findFirst().orElse(null);		

			run.setId(existingRunVW != null ? existingRunVW.getId() : 0);
			run.setCode(i.id());
			run.setGameId(existingGameVW.getId());
			var category = existingGameVW.getCategories().stream().filter(g ->  g.getCode().equals(i.category())).findFirst().orElse(null);
			run.setCategoryTypeId(category != null ? category.getCategoryTypeId() : 0);
			run.setCategoryId(category != null ? category.getId() : 0);
			run.setLevelId(i.level() != null ? existingGameVW.getLevels().stream().filter(g ->  g.getCode().equals(i.level())).map(g -> g.getId()).findFirst().orElse(0) : null);
			
			var subCategoryVariableCodes = existingGameVW.getVariables().stream().filter(g -> g.isSubCategory()).map(x -> x.getCode()).toList();
			var subCategoryVariableValueIds = existingGameVW.getVariableValues().stream()
												.filter(g -> i.values().entrySet().stream().anyMatch(h -> h.getValue().equals(g.getCode()) && subCategoryVariableCodes.contains(h.getKey())))
												.map(x -> Integer.toString(x.getId())).toList();		
			if (subCategoryVariableValueIds.size() > 0) {
				run.setSubCategoryVariableValueIds(String.join(",", subCategoryVariableValueIds));
			}

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
			var runPlayers = existingPlayers.stream()
								.filter(g -> playerCodes.contains(g.getCode()))
								.map(x -> { 
									var runPlayer = new SpeedRunPlayer();
									runPlayer.setId(existingRunVW != null ? existingRunVW.getPlayers().stream().filter(g ->  g.getPlayerId() == x.getId()).map(g -> g.getId()).findFirst().orElse(0) : 0);
									runPlayer.setPlayerId(x.getId());
									return runPlayer;
								}).toList();
			run.setPlayers(runPlayers);				

			List<SpeedRunVariableValue> variableValues = new ArrayList<SpeedRunVariableValue>();
			if (existingGameVW.getVariableValues() != null) {
				variableValues = existingGameVW.getVariableValues().stream()
							.filter(g -> i.values().entrySet().stream().anyMatch(h -> h.getValue().equals(g.getCode())))
							.map(x -> {
								var runVariableValue = new SpeedRunVariableValue();
								runVariableValue.setId(existingRunVW != null ? existingRunVW.getVariableValues().stream().filter(g -> g.getVariableValueId() == x.getId()).map(g -> g.getId()).findFirst().orElse(0) : 0);
								runVariableValue.setSpeedRunId(run.getId());
								runVariableValue.setVariableId(x.getVariableId());			
								runVariableValue.setVariableValueId(x.getId());			
								return runVariableValue;
							}).toList();
			}
			run.setVariableValues(variableValues);

			List<SpeedRunVideo> videos = new ArrayList<SpeedRunVideo>();
			if (i.videos() != null && i.videos().links() != null) {
				for (var videoLink : i.videos().links()) {			
					try {
						if (videoLink.uri() != null && !videoLink.uri().isBlank()) {
							var uri = URI.create(videoLink.uri());
							var video = new SpeedRunVideo();
							video.setId(existingRunVW != null ? existingRunVW.getVideos().stream().filter(g ->  g.getVideoLinkUrl().equals(videoLink.uri())).map(g -> g.getId()).findFirst().orElse(0) : 0);
							video.setSpeedRunId(run.getId());								
							video.setVideoLinkUrl(uri.toString());
							video.setThumbnailLinkUrl(UriExtensions.ToThumbnailURIString(uri.toString()));
							video.setEmbeddedVideoLinkUrl(UriExtensions.ToEmbeddedURIString(uri.toString()));
							
							videos.add(video);
						}
					} catch (IllegalArgumentException ex) {
						_logger.info(ex.getMessage());
					}
				}
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
					isChanged = (!Objects.equals(run.getGameId(), existingRunVW.getGameId())
						|| !Objects.equals(run.getCategoryTypeId(), existingRunVW.getCategoryTypeId())
						|| !Objects.equals(run.getCategoryId(), existingRunVW.getCategoryId())
						|| !Objects.equals(run.getLevelId(), existingRunVW.getLevelId())
						|| !Objects.equals(run.getSubCategoryVariableValueIds(), existingRunVW.getSubCategoryVariableValueIds())
						|| !Objects.equals(run.getPlatformId(), existingRunVW.getPlatformId())
						|| !Objects.equals(run.getPrimaryTime(), existingRunVW.getPrimaryTime())
						|| !Objects.equals(run.getDateSubmitted(), existingRunVW.getDateSubmitted())
						|| !Objects.equals(run.getVerifyDate(), existingRunVW.getVerifyDate())
						|| !Objects.equals(run.getSpeedRunLink().getSrcUrl(), existingRunVW.getSrcUrl()));
					
					/*	
					if (!isChanged) {
						var variableIds = run.getVariableValues().stream().map(i -> i.getVariableId()).toList();
						var existingVariableIds = existingRunVW.getVariableValues().stream().map(i -> i.getVariableId()).toList();
						isChanged = variableIds.stream().anyMatch(i -> !existingVariableIds.contains(i))
									|| existingVariableIds.stream().anyMatch(i -> !variableIds.contains(i));
					}					

					if (!isChanged) {
						var variableValueIndex = 0;		
						var existingVaraibleValues = existingRunVW.getVariableValues();					
						for (var variableValue : run.getVariableValues()) {
							var existingVariable = variableValueIndex < existingVaraibleValues.size() ? existingVaraibleValues.get(variableValueIndex) : null;
							isChanged = (existingVariable == null 
								|| variableValue.getVariableId() != existingVariable.getVariableId()
								|| variableValue.getVariableValueId() != existingVariable.getVariableValueId());

							if (isChanged) {
								break;
							}

							variableValueIndex++;
						}
					}
					*/

					if (isChanged){
						changedCount++;
					}
				}
			}

			if (isNew || isChanged) {
				results.add(run);
			}
		}

		_logger.info("Found New: {}, Changed: {}, Existing: {}, Total: {}", newCount, changedCount, existingRunVWs.size(), runs.size());	
		return results;
	}
	
	public boolean UpdateSpeedRunVideos(boolean isReload) {
		boolean result = false;		
		_logger.info("Started UpdateSpeedRunVideos");

		try
		{
			var stLastImportDateUtc = _settingService.GetSetting("LastImportDate");
			var lastImportDateUtc = stLastImportDateUtc != null && stLastImportDateUtc.getDte() != null ? stLastImportDateUtc.getDte() : getSqlMinDateTime();	

			List<SpeedRunVideo> results = new ArrayList<SpeedRunVideo>();
			List<SpeedRunVideo> videos = new ArrayList<SpeedRunVideo>();

			if (isReload) {
				lastImportDateUtc = lastImportDateUtc.atZone(ZoneId.systemDefault()).minusMonths(1).toInstant();
			}
			
			videos = _speedRunRepo.GetSpeedRunSummaryViewsVerifyAfter(lastImportDateUtc).stream()
								.map(x -> {
									return new SpeedRunVideo(x.getSpeedRunVideoId(),
																x.getId(),	
																x.getVideoLinkUrl(),		
																x.getEmbeddedVideoLinkUrl(),	
																x.getThumbnailLinkUrl(),
																x.getChannelCode(),
																x.getViewCount(),
																false);
								})
								.sorted((o1, o2) -> (o2.getId() - o1.getId()))
								.toList();
								
			var stYoutubeApiEnabled = _settingService.GetSetting("YoutubeAPIEnabled");
			if (stYoutubeApiEnabled != null && stYoutubeApiEnabled.getNum().equals(1)) {
				var ytvideos = GetYoutubeVideoDetails(videos);
				ytvideos = GetNewOrChangedSpeedRunVideos(ytvideos, videos);
				results.addAll(ytvideos);
			}

			var stTwitchApiEnabled = _settingService.GetSetting("TwitchAPIEnabled");
			if (stTwitchApiEnabled != null && stTwitchApiEnabled.getNum().equals(1)) {
				var twvideos = GetTwitchVideoDetails(videos);
				twvideos = GetNewOrChangedSpeedRunVideos(twvideos, videos);
				results.addAll(twvideos);
			}			

			if (results.size() > 0) {
				_speedRunRepo.SaveSpeedRunVideos(results);
			}

			_logger.info("Completed UpdateSpeedRunVideos");
		} catch (Exception ex) {
			result = false;
			_logger.error("UpdateSpeedRunVideos", ex);
		}

		return result;
	}

	private List<SpeedRunVideo> GetYoutubeVideoDetails(List<SpeedRunVideo> videos) {
		var maxYoutubeVideoCount = getYouTubeAPIDailyRequestLimit() * getYouTubeAPIMaxBatchCount();
		var ytvideos = videos.stream()
								.filter(i -> i.getVideoLinkUrl().contains("youtube.com") || i.getVideoLinkUrl().contains("youtu.be"))
								.limit(maxYoutubeVideoCount)
								.map(a -> (SpeedRunVideo) a.clone())
								.toList();

		for (var ytvideo : ytvideos) {
			var uri = URI.create(ytvideo.getVideoLinkUrl());
			var queryParams = UriExtensions.splitQuery(uri);
			
			if (queryParams.containsKey("v")) {
				ytvideo.setVideoId(queryParams.get("v"));
			} else {
				var pathString = uri.getPath();
				var path = Paths.get(pathString);
				if (path.getNameCount() > 0) {
					var lastSegment = path.getName(path.getNameCount() - 1).toString();
					ytvideo.setVideoId(lastSegment);
				}
			}		
		}
		ytvideos = ytvideos.stream().filter(g ->  g.getVideoId() != null && !g.getVideoId().isEmpty()).toList();

		var responses = new ArrayList<YoutubeVideoResponse>();
		var batchCount = 0;
		while (batchCount < ytvideos.size()) {
			var videoIDsBatch = ytvideos.stream().skip(batchCount).limit(super.getYouTubeAPIMaxBatchCount()).map(i -> i.getVideoId()).toList();			
			responses.addAll(GetYoutubeVideoResponses(videoIDsBatch));
			batchCount += super.getYouTubeAPIMaxBatchCount();	
		}

		for (var ytvideo : ytvideos) {
			var videoResponse = responses.stream().filter(g ->  g.id().equals(ytvideo.getVideoId())).findFirst().orElse(null);
			
			if (videoResponse != null) {
				ytvideo.setViewCount(videoResponse.statistics().viewCount());
				ytvideo.setChannelCode(videoResponse.snippet().channelId());

				var thumbnail = videoResponse.snippet().thumbnails().get("standard");
				if (thumbnail != null) {
					ytvideo.setThumbnailLinkUrl(thumbnail.url());
				}
			}
		}		

		return ytvideos;
	}

	private List<YoutubeVideoResponse> GetYoutubeVideoResponses(List<String> videoIDs) {
		List<YoutubeVideoResponse> results = new ArrayList<YoutubeVideoResponse>();

		try (var client = HttpClient.newHttpClient()) {
			var parameters = new HashMap<String, String>();
			parameters.put("id", String.join(",", videoIDs));
			parameters.put("key", super.getYouTubeAPIKey());
			parameters.put("part", "statistics,snippet");

			var paramString = String.join("&", parameters.entrySet().stream().map(i -> i.getKey() + "=" + i.getValue()).toList());

			var request = HttpRequest.newBuilder()
					.uri(URI.create("https://www.googleapis.com/youtube/v3/videos?" + paramString))				
					.build();

			var response = client.send(request, BodyHandlers.ofString());

			if (response.statusCode() == 200) {
				var mapper = new ObjectMapper().configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false)
									.setPropertyNamingStrategy(PropertyNamingStrategies.KEBAB_CASE)
									.registerModule(new JavaTimeModule());
				var videoResponses = Arrays.asList(mapper.readerFor(YoutubeVideoResponse[].class)
						.readValue(mapper.readTree(response.body()).get("items"), YoutubeVideoResponse[].class));
				results.addAll(videoResponses);				
			}					
		} catch (Exception ex) {
			_logger.error("GetYoutubeVideoResponses", ex);
		}
		
		return results;
	}

	private List<SpeedRunVideo> GetTwitchVideoDetails(List<SpeedRunVideo> videos) {
		var twvideos = videos.stream()
							.filter(i -> i.getVideoLinkUrl().contains("twitch.tv") && i.getVideoLinkUrl().contains("/videos/"))
							.map(a -> (SpeedRunVideo) a.clone())
							.toList();

		for (var twvideo : twvideos) {
			var pathString = URI.create(twvideo.getVideoLinkUrl()).getPath();
			var path = Paths.get(pathString);
			if (path.getNameCount() > 0) {
				var lastSegment = path.getName(path.getNameCount() - 1).toString();	
				twvideo.setVideoId(lastSegment);	
			}
		}
		twvideos = twvideos.stream().filter(g ->  g.getVideoId() != null && !g.getVideoId().isEmpty()).toList();

		var twitchToken = _authService.GetTwitchToken();
		var responses = new ArrayList<TwitchVideoResponse>();
		var batchCount = 0;
		while (batchCount < twvideos.size()) {

			var videoIDsBatch = twvideos.stream().skip(batchCount).limit(super.getTwitchAPIMaxBatchCount()).map(i -> i.getVideoId()).toList();			
			responses.addAll(GetTwitchVideoResponses(videoIDsBatch, twitchToken));
			batchCount += super.getYouTubeAPIMaxBatchCount();		
		}

		for (var twvideo : twvideos) {
			var videoResponse = responses.stream().filter(g ->  g.id().equals(twvideo.getVideoId())).findFirst().orElse(null);
			
			if (videoResponse != null) {
				twvideo.setViewCount(videoResponse.viewCount());
				twvideo.setChannelCode(videoResponse.userId());

				var thumbnailUrl = videoResponse.thumbnailUrl();
				if (thumbnailUrl != null) {
					thumbnailUrl = thumbnailUrl.replace("%{width}","640").replace("%{height}","480");
					twvideo.setThumbnailLinkUrl(thumbnailUrl);
				}
			}
		}		

		return twvideos;
	}

	private List<TwitchVideoResponse> GetTwitchVideoResponses(List<String> videoIDs, String twitchToken) {
		List<TwitchVideoResponse> results = new ArrayList<TwitchVideoResponse>();

		try (var client = HttpClient.newHttpClient()) {
			var parameters = new HashMap<String, String>();
			parameters.put("id", String.join("&id=", videoIDs));

			var paramString = String.join("&", parameters.entrySet().stream().map(i -> i.getKey() + "=" + i.getValue()).toList());

			var request = HttpRequest.newBuilder()
					.uri(URI.create("https://api.twitch.tv/helix/videos?" + paramString))
					.header("Client-Id", super.getTwitchClientId())
					.header("Authorization", "Bearer " + twitchToken)
					.build();

			var response = client.send(request, BodyHandlers.ofString());

			if (response.statusCode() == 200) {
				var mapper = new ObjectMapper().configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false)
									.setPropertyNamingStrategy(PropertyNamingStrategies.SNAKE_CASE)
									.registerModule(new JavaTimeModule());
				var videoResponses = Arrays.asList(mapper.readerFor(TwitchVideoResponse[].class)
						.readValue(mapper.readTree(response.body()).get("data"), TwitchVideoResponse[].class));
				results.addAll(videoResponses);				
			}					
		} catch (Exception ex) {
			_logger.error("GetTwitchVideoResponses", ex);
		}
		
		return results;
	}

	private List<SpeedRunVideo> GetNewOrChangedSpeedRunVideos(List<SpeedRunVideo> videos, List<SpeedRunVideo> existingVideos) {	
		var results = new ArrayList<SpeedRunVideo>();
		var newCount = 0;
		var changedCount = 0;

		for (var video : videos) {
			var isNew = false;	
			var isChanged = false;
		
			if (video.getId() == 0) {
				isNew = true;
				newCount++;
			} else {
				var existingVideo = existingVideos.stream().filter(g -> g.getId() == video.getId()).findFirst().orElse(null);
				
				if (existingVideo != null) {
					isChanged = (!Objects.equals(video.getViewCount(), existingVideo.getViewCount())
						|| !Objects.equals(video.getChannelCode(), existingVideo.getChannelCode())
						|| !Objects.equals(video.getThumbnailLinkUrl(), existingVideo.getThumbnailLinkUrl()));
					
					if (isChanged){
						changedCount++;
					}
				}
			}

			if (isNew || isChanged) {
				results.add(video);
			}
		}

		_logger.info("Found New: {}, Changed: {}, Existing: {}, Total: {}", newCount, changedCount, existingVideos.size(), videos.size());	
		return results;
	}
}
