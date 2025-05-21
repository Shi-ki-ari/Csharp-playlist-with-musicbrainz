using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FirstRESTApi.Model;
using FirstRESTApi.Data;
using FirstRESTApi.Services;

namespace FirstRESTApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        private readonly ApiContext _context;

        public PlaylistController(ApiContext context)
        {
            _context = context;
        }

        //Create/Edit

        [HttpPost]
        public JsonResult CreateEdit(Playlist playlist)
        {
            if (playlist.id == 0)
            {
                _context.Playlists.Add(playlist);
            }
            else
            {
                var playlistInDb = _context.Playlists.Find(playlist.id);

                if (playlistInDb == null)
                {
                    return new JsonResult(NotFound());
                }

                playlistInDb.name = playlist.name;
                playlistInDb.description = playlist.description;
            }

            _context.SaveChanges();

            return new JsonResult(Ok(playlist));
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string name)
        {
            var playlists = await MusicBrainz.SearchSongsAsync(query);
            return Ok();
        }

    }
}
