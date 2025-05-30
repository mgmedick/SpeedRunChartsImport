package speedrunappimport.services;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse.BodyHandlers;
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

import speedrunappimport.interfaces.repositories.*;
import speedrunappimport.interfaces.services.*;
import speedrunappimport.model.entity.*;
import speedrunappimport.model.json.*;

public class PlatformService extends BaseService implements IPlatformService {
	private IPlatformRepository _platformRepo;
	private Logger _logger;

	public PlatformService(IPlatformRepository platformRepo, Logger logger) {
		_platformRepo = platformRepo;
		_logger = logger;
	}

	public boolean ProcessPlatforms() {
		boolean result = true;

		try {
			_logger.info("Started ProcessPlatforms");
			List<PlatformResponse> results = new ArrayList<PlatformResponse>();
			List<PlatformResponse> platforms = new ArrayList<PlatformResponse>();
			var prevTotal = 0;

			do {
				platforms = GetPlatformResponses(results.size() + prevTotal);
				results.addAll(platforms);
				Thread.sleep(super.getPullDelayMS());
				_logger.info("Pulled platforms: {}, total platforms: {}", platforms.size(), results.size() + prevTotal);

				if (results.size() > super.getMaxRecordCount()) {
					var totalMemory = Runtime.getRuntime().totalMemory();
					var memorySize = totalMemory - Runtime.getRuntime().freeMemory();
					_logger.info("Saving to clear memory, results: {}, memorySize: {}, totalMemory: {}", results.size(), memorySize, totalMemory);
					prevTotal += results.size();
					SavePlatformResponses(results);
					results = new ArrayList<PlatformResponse>();
				}
			}
			while (platforms.size() == super.getMaxPageLimit());
			//while (1 == 0);

			if (results.size() > 0) {
				SavePlatformResponses(results);
			}

			_logger.info("Completed ProcessPlatforms");
		} catch (Exception ex) {
			result = false;
			_logger.error("ProcessPlatforms", ex);
		}

		return result;
	}

	public List<PlatformResponse> GetPlatformResponses(int offset) throws Exception {
		return GetPlatformResponses(offset, 0);
	}

	public List<PlatformResponse> GetPlatformResponses(int offset, int retryCount) throws Exception {
		List<PlatformResponse> data = new ArrayList<PlatformResponse>();

		try (var client = HttpClient.newHttpClient()) {
			var parameters = new HashMap<String, String>();
			parameters.put("max", Integer.toString(super.getMaxPageLimit()));
			parameters.put("offset", Integer.toString(offset));

			String paramString = String.join("&",
					parameters.entrySet().stream().map(i -> i.getKey() + "=" + i.getValue()).toList());

			var request = HttpRequest.newBuilder()
					.uri(URI.create("https://www.speedrun.com/api/v1/platforms?" + paramString))
					.build();

			var response = client.send(request, BodyHandlers.ofString());
			if (response.statusCode() == 200) {
				var mapper = new ObjectMapper().configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false)
											.setPropertyNamingStrategy(PropertyNamingStrategies.KEBAB_CASE)
											.registerModule(new JavaTimeModule());
				var platforms = Arrays.asList(mapper.readerFor(PlatformResponse[].class).readValue(mapper.readTree(response.body()).get("data"), PlatformResponse[].class));
				data = new ArrayList<PlatformResponse>(platforms);
			}
		} catch (Exception ex) {
			Thread.sleep(super.getErrorPullDelayMS());
			retryCount++;
			if (retryCount <= super.getMaxRetryCount()) {		
				_logger.info("Retrying pull platforms: {}, total platforms: {}, retry: {}", super.getMaxPageLimitSM(), offset, retryCount);
				data = GetPlatformResponses(offset, retryCount);
			} else {
				_logger.info("Max retry reached");
				throw ex;
			}
		}

		return data;
	}

	public void SavePlatformResponses(List<PlatformResponse> platformResponses) {
		_logger.info("Started SavePlatformResponses: {}", platformResponses.size());

		platformResponses = platformResponses.stream()
				.collect(Collectors.toMap(PlatformResponse::id, Function.identity(), (u1, u2) -> u1))
				.values()
				.stream()
				.collect(Collectors.toList());

		var existingPlatforms = _platformRepo.GetAllPlatforms();
		var platforms = GetPlatformsFromResponses(platformResponses, existingPlatforms);
		platforms = GetNewOrChangedPlatforms(platforms, existingPlatforms);

		_platformRepo.SavePlatforms(platforms);

		_logger.info("Completed SavePlatformResponses");
	}

	private List<Platform> GetPlatformsFromResponses(List<PlatformResponse> platformResponses, List<Platform> existingPlatforms) {
		var results = new ArrayList<Platform>();	
		
		for (var platformResponse : platformResponses) {
			var platform = new Platform();

			var existingPlatform = existingPlatforms.stream().filter(x -> x.getCode().equals(platformResponse.id())).findFirst().orElse(null);			
			platform.setId(existingPlatform != null ? existingPlatform.getId() : 0);
			platform.setName(platformResponse.name());
			platform.setCode(platformResponse.id());

			results.add(platform);
		}
		
		return results;
	}	

	private List<Platform> GetNewOrChangedPlatforms(List<Platform> platforms, List<Platform> existingPlatforms) {	
		var results = new ArrayList<Platform>();
		var newCount = 0;
		var changedCount = 0;

		for (var platform : platforms) {
			var isNew = false;	
			var isChanged = false;
		
			if (platform.getId() == 0) {
				isNew = true;
				newCount++;
			} else {
				var existingPlatform = existingPlatforms.stream().filter(x -> x.getId() == platform.getId()).findFirst().orElse(null);			
				if (existingPlatform != null) {
					isChanged = !platform.getName().equalsIgnoreCase(platform.getName());

					if (isChanged){
						changedCount++;
					}
				}
			}

			if (isNew || isChanged) {
				results.add(platform);
			}
		}

		_logger.info("Found New: {}, Changed: {}, Existing: {}, Total: {}", newCount, changedCount, existingPlatforms.size(), platforms.size());		
		return results;
	}	
}
