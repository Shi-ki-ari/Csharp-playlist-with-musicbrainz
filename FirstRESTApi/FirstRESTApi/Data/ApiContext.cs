using Microsoft.EntityFrameworkCore;
using FirstRESTApi.Model;  
namespace FirstRESTApi.Data

{
    public class ApiContext :DbContext
    {
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Song> Songs { get; set; }
        public object Playlist { get; internal set; }
        public object Song { get; internal set; }

        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {
        }
    }
}
