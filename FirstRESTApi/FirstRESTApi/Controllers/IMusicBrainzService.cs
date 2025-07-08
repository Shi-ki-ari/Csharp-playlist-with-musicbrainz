using System.Text.Json;
using System.Threading.Tasks;

namespace FirstRESTApi.Services
{
    public interface IMusicBrainzService
    {
        Task<object> SearchSongsAsync(string name);
    }

    public class MusicBrainzService : IMusicBrainzService
    {
        private readonly HttpClient _httpClient;

        public MusicBrainzService(HttpClient httpClient)
        {

            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PlaylistManager/1.0 ( examplemail.main@gmail.com )");

        }

        public async Task<object> SearchSongsAsync(string name)
        {
            var url = $"https://musicbrainz.org/ws/2/recording/?query={name}&fmt=json";
            var response = await _httpClient.GetStringAsync(url);
            return JsonSerializer.Deserialize<object>(response);
        }
    }
}
