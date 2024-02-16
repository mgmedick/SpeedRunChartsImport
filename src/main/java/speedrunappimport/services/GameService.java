package speedrunappimport.services;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse.BodyHandlers;
import java.time.Instant;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import org.slf4j.Logger;

import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.ObjectMapper;

import speedrunappimport.interfaces.repositories.IGameRepository;
import speedrunappimport.interfaces.services.*;
import speedrunappimport.model.JSON.*;

public class GameService extends BaseService implements IGameService
{
	private IGameRepository _gameRepo;
	private Logger _logger;

	public GameService(IGameRepository gameRepo, Logger logger)
	{
		_gameRepo = gameRepo;
		_logger = logger;
	}

	public boolean ProcessGames(Instant lastImportDateUtc, boolean isReload)
	{	
		boolean result = true;

		try
		{
			_logger.info("Started ProcessGames: {@lastImportDateUtc}, {@isReload}", lastImportDateUtc, isReload);
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
					//SaveGames(results, isReload);
					results.clear();
					results.trimToSize();
				}
			}
			while (games.size() == super.maxPageLimit && (isReload || games.stream().map(i -> i.created != null ? Instant.parse(i.created) : super.sqlMinDateTime).max(Instant::compareTo).get().compareTo(lastImportDateUtc) > 0));                
			//while (1 == 0);

			if (!isReload)
			{
				results.removeIf(i -> (i.created != null ? Instant.parse(i.created) : super.sqlMinDateTime).compareTo(lastImportDateUtc) <= 0);
			}

			if (results.size() > 0)
			{
				//SaveGames(results, isReload);
				//var lastUpdateDate = results.stream().map(i -> i.created != null ?  Instant.parse(i.created) : super.sqlMinDateTime).max(Instant::compareTo).get();
				//_settingService.UpdateSetting("GameLastImportDate", lastUpdateDate);
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
				var mapper = new ObjectMapper().configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);
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
}
