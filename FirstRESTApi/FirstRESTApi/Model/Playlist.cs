namespace FirstRESTApi.Model
{
    public class Playlist
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public List<Song> songs { get; set; } = new List<Song>();

        public void displayPlaylist()
        {
            for (int i = 0; i < songs.Count; i++)
            {
                Console.WriteLine($"Song {i + 1}: {songs[i].Name} by {songs[i].Artist} from the album {songs[i].Album}");
            }
        }
    }
}
