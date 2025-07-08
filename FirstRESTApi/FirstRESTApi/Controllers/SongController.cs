using FirstRESTApi.Data;
using FirstRESTApi.Model;
using FirstRESTApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace FirstRESTApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly IMusicBrainzService _musicBrainzService;
        private readonly HttpClient _httpClient;

        public SongController(ApiContext context, IMusicBrainzService musicBrainzService, HttpClient httpClient)
        {
            _context = context;
            _musicBrainzService = musicBrainzService;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PlaylistManager/1.0 ( filthyskaarl.main@gmail.com )");
        }



        [HttpGet("getRandSongs")]
        public async Task<IActionResult> GetRandomSongs()
        {
            var seed = SeedGenerator.GetDailySeed();

            string[] genres = new[]
            {
    "rock",
    "pop",
    "jazz",
    "hip hop",
    "classical",
    "electronic",
    "metal",
    "country",
    "blues",
    "reggae",
    "funk",
    "soul",
    "punk",
    "folk",
    "techno",
    "house",
    "indie",
    "alternative",
    "r&b",
    "disco"
};


            string chosenQuery = genres[seed % genres.Length].ToLowerInvariant().Replace(" ", "+");


            var offset = seed % 100;

            var url = $"https://musicbrainz.org/ws/2/recording/?query=tag:{chosenQuery}&offset={offset}&limit=10&fmt=json";



            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(json);
            var recordings = doc.RootElement.GetProperty("recordings");

            List<string> results = new List<string>();
            foreach(var record in recordings.EnumerateArray())
            {
                string title = record.GetProperty("title").GetString();
                string artist = record.GetProperty("artist-credit")[0].GetProperty("name").GetString();

                results.Add($"{title} - {artist}");

            }

            return Ok(results);
        }
        [HttpGet("songsearch")]
        public async Task<IActionResult> SearchSongsAsync([FromQuery] string name)
        {

            var query = Uri.EscapeDataString(name);
            var url = $"https://musicbrainz.org/ws/2/recording/?query={query}+AND+status:official&fmt=json&limit=20";


            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("recordings", out var recordings))
                return Ok(new List<string>()); // no results found

            var results = new List<string>();


            foreach (var record in recordings.EnumerateArray())
            {
                var title = record.GetProperty("title").GetString();

                string artist = "Unknown Artist";
                if (record.TryGetProperty("artist-credit", out var artistCredits) && artistCredits.GetArrayLength() > 0)
                    artist  = artistCredits[0].GetProperty("name").GetString();



                string entry = $"{title} by {artist}";

                if (!results.Contains(entry))
                {
                    results.Add(entry);
                }


            }

            return Ok(results);
        }



    }
}
