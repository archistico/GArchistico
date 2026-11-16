using GenerativeArt.Application.Preferences;
using Xunit;

namespace GenerativeArt.Application.Tests;

public sealed class StudioPreferencesTests
{
    [Fact]
    public void ToggleFavoriteIsIdempotentForDuplicateIdsAndDoesNotChangeTheme()
    {
        var initial = new StudioPreferences(favoriteArtworkIds: ["plasma-noise", "plasma-noise"]);
        Assert.Single(initial.FavoriteArtworkIds);
        var off = initial.ToggleFavorite("plasma-noise");
        Assert.False(off.IsFavorite("plasma-noise"));
        var on = off.ToggleFavorite("plasma-noise").WithTheme(true);
        Assert.True(on.IsFavorite("plasma-noise"));
        Assert.True(on.UseLightTheme);
        Assert.False(initial.UseLightTheme);
    }

    [Fact]
    public void RecentRecipesAreDistinctNewestFirstAndBounded()
    {
        var state = new StudioPreferences();
        for (int i = 0; i < 15; i++)
            state = state.AddRecentRecipe(Path.Combine(Path.GetTempPath(), $"ga-recipe-{i}.json"));
        Assert.Equal(StudioPreferences.MaximumRecentRecipes, state.RecentRecipePaths.Count);
        string old = state.RecentRecipePaths[5];
        state = state.AddRecentRecipe(old);
        Assert.Equal(old, state.RecentRecipePaths[0]);
        Assert.Equal(StudioPreferences.MaximumRecentRecipes, state.RecentRecipePaths.Count);
        Assert.Equal(1, state.RecentRecipePaths.Count(p => p == old));
        Assert.DoesNotContain(old, state.RemoveRecentRecipe(old).RecentRecipePaths);
        Assert.Empty(state.ClearRecentRecipes().RecentRecipePaths);
    }

    [Fact]
    public async Task FileRoundtripIsVersionedAndAtomic()
    {
        string dir = Path.Combine(Path.GetTempPath(), "ga-prefs-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(dir, "preferences.json");
        try
        {
            var store = new StudioPreferencesFile(path);
            Assert.Empty(store.Load().FavoriteArtworkIds);
            var preferences = new StudioPreferences(true, ["plasma-noise"],
                [Path.Combine(dir, "recipe.json")]);
            await store.SaveAsync(preferences);
            StudioPreferences loaded = store.Load();
            Assert.True(loaded.UseLightTheme);
            Assert.True(loaded.IsFavorite("plasma-noise"));
            Assert.Single(loaded.RecentRecipePaths);
            await store.SaveAsync(loaded.WithTheme(false));
            Assert.False(store.Load().UseLightTheme);
            Assert.Single(Directory.GetFiles(dir));
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [Theory]
    [InlineData("{\"schemaVersion\":2,\"theme\":\"dark\",\"favorites\":[],\"recentRecipes\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"theme\":\"pink\",\"favorites\":[],\"recentRecipes\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"theme\":\"dark\",\"favorites\":{},\"recentRecipes\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"theme\":\"dark\",\"theme\":\"light\",\"favorites\":[],\"recentRecipes\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"theme\":\"dark\",\"favorites\":[],\"recentRecipes\":[],\"unexpected\":42}")]
    public void MalformedPreferencesAreNotSilentlyConsideredValid(string json)
    {
        string path = Path.Combine(Path.GetTempPath(), "ga-prefs-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(path, json);
            Assert.Throws<InvalidDataException>(() => new StudioPreferencesFile(path).Load());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void OversizedPreferencesAreRejectedBeforeParsing()
    {
        string path = Path.Combine(Path.GetTempPath(), "ga-prefs-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllBytes(path, new byte[StudioPreferencesFile.MaximumBytes + 1]);
            Assert.Throws<InvalidDataException>(() => new StudioPreferencesFile(path).Load());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ThemePalettePreservesDarkTokensAndUsesContrastingLightTokens()
    {
        Assert.Equal("#10151E", StudioThemePalette.Resolve("#10151E", lightTheme: false));
        Assert.Equal("#F0F4F9", StudioThemePalette.Resolve("#10151E", lightTheme: true));
        Assert.Equal("#B12832", StudioThemePalette.Resolve("#FF9A9A", lightTheme: true));
        Assert.Equal("#123456", StudioThemePalette.Resolve("#123456", lightTheme: true));
    }
}
