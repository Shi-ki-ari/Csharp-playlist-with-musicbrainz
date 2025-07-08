let cachedPlaylists = [];

async function fetchPlaylistsForDropdown() {
  const response = await fetch('/api/playlist');
  if (!response.ok) {
    console.error("Failed to fetch playlists");
    return [];
  }
  return await response.json();
}

const searchInput = document.getElementById("song-name");
const resultsList = document.getElementById("search-results");

let debounceTimeout = null;

searchInput.addEventListener("input", () => {
  clearTimeout(debounceTimeout);

  debounceTimeout = setTimeout(async () => {
    const query = searchInput.value.trim();
    resultsList.innerHTML = "";

    if (!query) return;

    try {
      const response = await fetch(`/api/song/songsearch?name=${encodeURIComponent(query)}`);
      if (!response.ok) {
        resultsList.innerHTML = `<li>Error: ${response.statusText}</li>`;
        return;
      }

      const data = await response.json();

      if (!Array.isArray(data) || data.length === 0) {
        resultsList.innerHTML = "<li>No results found</li>";
        return;
      }

      cachedPlaylists = await fetchPlaylistsForDropdown();

data.forEach(songName => {
  const li = document.createElement("li");
  li.textContent = songName;

  // ➕ Add Button
  const addBtn = document.createElement("button");
  addBtn.textContent = "+";
  addBtn.style.marginLeft = "10px";
  addBtn.style.cursor = "pointer";

  addBtn.addEventListener("click", () => {
  // Prevent multiple dropdowns or buttons
  if (li.querySelector("select") || li.querySelector(".confirm-add-button")) return;

  const dropdown = document.createElement("select");

  cachedPlaylists.forEach(pl => {
    const option = document.createElement("option");
    option.value = pl.id;
    option.textContent = pl.name;
    dropdown.appendChild(option);
  });

  const confirmBtn = document.createElement("button");
  confirmBtn.textContent = "Add";
  confirmBtn.classList.add("confirm-add-button");
  confirmBtn.style.marginLeft = "5px";

  confirmBtn.addEventListener("click", async () => {
    const playlistId = dropdown.value;

    try {
      const response = await fetch(
        `/api/playlist/${playlistId}/add-song?songName=${encodeURIComponent(songName)}`,
        { method: "POST" }
      );

      if (!response.ok) {
        const errText = await response.text();
        throw new Error(errText);
      }

      alert(`Added "${songName}" to playlist.`);
    } catch (err) {
      alert("Error adding song: " + err.message);
    } finally {
      dropdown.remove();
      confirmBtn.remove();
    }
  });

  li.appendChild(dropdown);
  li.appendChild(confirmBtn);
});


  li.appendChild(addBtn);
  resultsList.appendChild(li);
});

    } catch (err) {
      resultsList.innerHTML = `<li>Error: ${err.message}</li>`;
    }
  }, 300);
});


async function loadPlaylists() {
    const response = await fetch('/api/playlist');
    if (!response.ok) throw new Error('Failed to load playlists');

    const playlists = await response.json();
    cachedPlaylists = playlists;

    const list = document.getElementById('playlist-list');
    list.innerHTML = '';

    playlists.forEach(pl => {
        const li = document.createElement('li');
        li.textContent = pl.name + " ";

        // Delete button
        const deleteBtn = document.createElement('button');
        deleteBtn.textContent = 'X';
        deleteBtn.style.marginLeft = '10px';
        deleteBtn.style.color = 'red';
        deleteBtn.style.cursor = 'pointer';

        deleteBtn.addEventListener('click', async () => {
            if (confirm(`Delete playlist "${pl.name}"?`)) {
                try {
                    const deleteResponse = await fetch(`/api/playlist/${pl.id}/deleteplaylist`, {
                        method: 'DELETE',
                    });
                    if (!deleteResponse.ok) {
                        const errorText = await deleteResponse.text();
                        throw new Error(errorText || "Failed to delete playlist");
                    }
                    await loadPlaylists(); 
                } catch (err) {
                    alert('Error deleting playlist: ' + err.message);
                }
            }
        });

        // View button
        const viewBtn = document.createElement('button');
        viewBtn.textContent = '👁 View';
        viewBtn.style.marginLeft = '5px';
        viewBtn.style.cursor = 'pointer';

        viewBtn.addEventListener('click', async () => {
            try {
                const response = await fetch(`/api/playlist/${pl.id}/songs`);
                if (!response.ok) throw new Error("Failed to fetch songs");

                const songs = await response.json();

                let message = songs.length
                    ? songs.map(song => `🎵 ${song.name} - ${song.artist} (${song.album})`).join('\n')
                    : 'No songs in this playlist.';

                alert(`Playlist: ${pl.name}\n\n${message}`);
            } catch (err) {
                alert("Error loading songs: " + err.message);
            }
        });

        li.appendChild(deleteBtn);
        li.appendChild(viewBtn);
        list.appendChild(li);
    });

    // Refresh dropdowns
    searchInput.dispatchEvent(new Event('input'));
}


// Add playlist when user clicks button
document.getElementById('add-playlist-button').addEventListener('click', async () => {
  const nameInput = document.getElementById('playlist-name');
  const name = nameInput.value.trim();

  if (!name) {
    alert('Please enter a playlist name.');
    return;
  }

  const playlist = { id: 0, name, description: '' };

  try {
    const response = await fetch('/api/playlist/createplaylist', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(playlist),
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(errorText || 'Failed to create playlist');
    }

    const newPlaylist = await response.json();

    // Add new playlist to the list visually
    const list = document.getElementById('playlist-list');
    const li = document.createElement('li');
    li.textContent = newPlaylist.name;
    li.dataset.playlistId = newPlaylist.id;
    list.appendChild(li);
    loadPlaylists();

    nameInput.value = '';
  } catch (err) {
    alert('Error: ' + err.message);
  }

  
});

async function loadRandomSongs() {
  const randomList = document.getElementById('random-songs-list');
  randomList.innerHTML = '';

  try {
    const response = await fetch('/api/song/getRandSongs');

    if (!response.ok) throw new Error('Failed to fetch random songs');

    const songs = await response.json();
    cachedPlaylists = await fetchPlaylistsForDropdown(); 

    songs.forEach(songFullName => {
      const li = document.createElement('li');
      li.textContent = songFullName;

      const addBtn = document.createElement('button');
      addBtn.textContent = '+';
      addBtn.style.marginLeft = '10px';
      addBtn.style.cursor = 'pointer';

      addBtn.addEventListener('click', () => {
        if (li.querySelector('select') || li.querySelector('.confirm-add-button')) return;

        const dropdown = document.createElement('select');
        cachedPlaylists.forEach(pl => {
          const option = document.createElement('option');
          option.value = pl.id;
          option.textContent = pl.name;
          dropdown.appendChild(option);
        });

        const confirmBtn = document.createElement('button');
        confirmBtn.textContent = 'Add';
        confirmBtn.classList.add('confirm-add-button');
        confirmBtn.style.marginLeft = '5px';

        confirmBtn.addEventListener('click', async () => {
          const playlistId = dropdown.value;

          const songName = songFullName.split(' - ')[0]; 

          try {
            const res = await fetch(
              `/api/playlist/${playlistId}/add-song?songName=${encodeURIComponent(songName)}`,
              { method: 'POST' }
            );
            if (!res.ok) throw new Error(await res.text());
            alert(`Added "${songName}" to playlist`);
          } catch (err) {
            alert('Error: ' + err.message);
          } finally {
            dropdown.remove();
            confirmBtn.remove();
          }
        });

        li.appendChild(dropdown);
        li.appendChild(confirmBtn);
      });

      li.appendChild(addBtn);
      randomList.appendChild(li);
    });

  } catch (err) {
    randomList.innerHTML = `<li>Error: ${err.message}</li>`;
  }
}


window.addEventListener('DOMContentLoaded', () => {
  loadPlaylists();
  loadRandomSongs(); 
});




