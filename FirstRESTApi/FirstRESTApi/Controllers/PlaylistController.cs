using FirstRESTApi.Data;
using FirstRESTApi.Model;
using FirstRESTApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FirstRESTApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class PlaylistController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly IMusicBrainzService _musicBrainzService;

        public PlaylistController(ApiContext context, IMusicBrainzService musicBrainzService)
        {
            _context = context;
            _musicBrainzService = musicBrainzService;
        }

        // Create or Edit a playlist
        [HttpPost("createplaylist")]
        public async Task<IActionResult> CreateEdit(Playlist playlist)
        {
            if (playlist.id == 0)
            {
                _context.Playlists.Add(playlist);
            }
            else
            {
                var playlistInDb = _context.Playlists.Find(playlist.id);
                if (playlistInDb == null)
                    return NotFound();

                playlistInDb.name = playlist.name;
                playlistInDb.description = playlist.description;
            }

            await _context.SaveChangesAsync();
            return Ok(playlist);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPlaylists()
        {
            var playlists = await _context.Playlists.Include(p => p.songs).ToListAsync();
            return Ok(playlists);
        }

        [HttpPost("{playlistId}/add-song")]
        public async Task<IActionResult> AddSongToPlaylist(int playlistId, string songName)
        {
            var playlist = await _context.Playlists.Include(p => p.songs).FirstOrDefaultAsync(p => p.id == playlistId);
            if (playlist == null)
            {
                return NotFound("Playlist not found");
            }

            var musicBrainzResult = await _musicBrainzService.SearchSongsAsync(songName);


            var recordings = (musicBrainzResult as JsonElement?).Value.GetProperty("recordings");
            if (recordings.GetArrayLength() == 0)
                return NotFound("Song not found");

            var firstResult = recordings[0];

            var newSong = new Song
            {
                Name = firstResult.GetProperty("title").GetString(),
                Artist = firstResult.GetProperty("artist-credit")[0].GetProperty("name").GetString(),
                Album = firstResult.TryGetProperty("releases", out var releases) && releases.GetArrayLength() > 0
                ? releases[0].GetProperty("title").GetString()
                : "Unknown"

            };

            playlist.songs.Add(newSong);
            await _context.SaveChangesAsync();

            return Ok(playlist);

        }


        [HttpDelete("{playlistId}/{songId}")]

        public async Task<IActionResult> RemoveSongFromPlaylist(int playlistId, int songId)
        {
            var playlist = await _context.Playlists.Include(p => p.songs).FirstOrDefaultAsync(p => p.id == playlistId);
            if (playlist == null)
                return NotFound("Playlist not found");

            var song = playlist.songs.FirstOrDefault(s => s.Id == songId);

            if (song == null)
            {
                return NotFound("Song not found in playlist.");
            }
            else
            {
                playlist.songs.Remove(song);
            }

            await _context.SaveChangesAsync();
            return Ok(playlist);


        }

        [HttpDelete("{playlistId}/deleteplaylist")]
        public async Task<IActionResult> deleteplaylist(int playlistId)
        {
            var playlist = await _context.Playlists
                .Include(p => p.songs)
                .FirstOrDefaultAsync(p => p.id == playlistId);

            if (playlist == null)
                return NotFound();

            if (playlist.songs.Any())
            {
                _context.Songs.RemoveRange(playlist.songs);
            }

            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();

            return Ok(playlist);
        }


        [HttpGet("search")]
        public async Task<IActionResult> Search(string name)
        {
            var playlists = await _musicBrainzService.SearchSongsAsync(name);
            return Ok(playlists);
        }

        [HttpGet("{playlistId}/songs")]
        public async Task<IActionResult> GetPlaylistSongs(int playlistId)
        {
            var playlist = await _context.Playlists
                .Include(p => p.songs)
                .FirstOrDefaultAsync(p => p.id == playlistId);

            if (playlist == null)
                return NotFound("Playlist not found");

            return Ok(playlist.songs);
        }

    }
}
