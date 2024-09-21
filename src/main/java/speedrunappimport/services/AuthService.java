package speedrunappimport.services;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse.BodyHandlers;
import java.util.HashMap;

import org.json.JSONObject;
import org.slf4j.Logger;

import speedrunappimport.interfaces.services.*;

public class AuthService extends BaseService implements IAuthService {
	private ISettingService _settingService;
	private Logger _logger;

	public AuthService(ISettingService settingService, Logger logger) {
		_settingService = settingService;
		_logger = logger;
	}

	public String GetTwitchToken() {
		var stTwitchApiEnabled = _settingService.GetSetting("TwitchToken");
		var token = stTwitchApiEnabled != null && stTwitchApiEnabled.getStr() != null ? stTwitchApiEnabled.getStr() : null;
		
		if (token == null || !ValidateTwitchToken(token)) {
			token = GenerateTwitchToken();
			_settingService.UpdateSetting("TwitchToken", token);
		}

		return token;
	}

	private boolean ValidateTwitchToken(String token) {
		var result = false;

		try (var client = HttpClient.newHttpClient()) {
			var request = HttpRequest.newBuilder()
					.uri(URI.create("https://id.twitch.tv/oauth2/validate"))
					.header("Authorization", "Bearer " + token)
					.build();

			var response = client.send(request, BodyHandlers.ofString());

			if (response.statusCode() == 200) {
				result = true;		
			}					
		} catch (Exception ex) {
			_logger.error("ValidateTwitchToken", ex);
		}

		return result;
	}

	private String GenerateTwitchToken() {
		String result = null;

		try (var client = HttpClient.newHttpClient()) {
			var parameters = new HashMap<String, String>();
			parameters.put("client_id", super.getTwitchClientId());
			parameters.put("client_secret", super.getTwitchClientKey());
			parameters.put("grant_type", "client_credentials");

			var paramString = String.join("&", parameters.entrySet().stream().map(i -> i.getKey() + "=" + i.getValue()).toList());

			var request = HttpRequest.newBuilder()
					.uri(URI.create("https://id.twitch.tv/oauth2/token?" + paramString))
					.header("Accept", "application/json")
					.POST(HttpRequest.BodyPublishers.noBody())
					.build();

			var response = client.send(request, BodyHandlers.ofString());

			if (response.statusCode() == 200) {
				var dataString = response.body();
				var data = new JSONObject(dataString);
				if (data != null) {
					result = data.getString("access_token");
				}			
			}					
		} catch (Exception ex) {
			_logger.error("GenerateTwitchToken", ex);
		}

		return result;
	}
}
