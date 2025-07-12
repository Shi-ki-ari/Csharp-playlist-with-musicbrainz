using Microsoft.AspNetCore.Builder;
using FirstRESTApi.Model;
using FirstRESTApi.Controllers;
namespace FirstRESTApi.Services
{
    public class MusicBrainz
    {
        private readonly HttpClient _HttpClient;
        private readonly string _baseUrl = "https://musicbrainz.org/ws/2/";

        public MusicBrainz(HttpClient httpClient)
        {
            _HttpClient = httpClient;
        }


    }
}
