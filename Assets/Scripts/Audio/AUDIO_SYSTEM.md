# Audio System

## SFX

**SfxLibrarySO** — ScriptableObject at `Assets/ScriptableObjects/Databases/SFXLibrary.asset`.  
Each `SfxEntry` has an `id` (string), `AudioClip`, and `volume`.  
Play via `AudioManager.Instance.PlaySfx(id)` or `PlaySfxPitched(id, pitch)`.

## Music

**MusicLibrarySO** — ScriptableObject at `Assets/ScriptableObjects/Databases/MusicLibrary.asset`.  
Each `MusicEntry` has an `id` (string), looping `AudioClip`, and `volume`.

**MusicManager** — persistent singleton (Boot scene). Two `AudioSource`s alternate for crossfading.  
- `PlayTrack(id)` — crossfades to the new track; no-ops if the same id is already playing.  
- `StopMusic()` — fades out current track.

**SceneMusic** — place on a GameObject in each scene alongside `SceneBootstrap`.  
Set `musicId` to the matching `MusicEntry` id. Calls `PlayTrack` in `Start`.

### Music IDs (by scene)

| Scene    | Music ID           |
|----------|--------------------|
| Town     | `music_town`       |
| Lake_01  | `music_lake_01`    |
| Lake_02  | `music_lake_02`    |
| Lake_03  | `music_lake_03`    |
