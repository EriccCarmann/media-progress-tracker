using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaProgressTracker.Models;
using MediaProgressTracker.Services.Abstract;

namespace MediaProgressTracker.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ISteamSpyService _steamSpy;

        public ObservableCollection<Game> TopGames { get; } = new ObservableCollection<Game>();

        [ObservableProperty]
        private bool isBusy;

        //[ObservableProperty]
        //private string errorMessage;

        public IAsyncRelayCommand LoadDataCommand { get; }

        public MainViewModel(ISteamSpyService steamSpyService)
        {
            _steamSpy = steamSpyService;
            LoadDataCommand = new AsyncRelayCommand(LoadTopGamesAsync);

            Load();
        }

        private async Task Load()
        {
            try
            {
                var games = await _steamSpy.GetTop100In2WeeksAsync();

                TopGames.Clear();

                foreach (var g in games)
                {
                    TopGames.Add(g);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading initial data: {ex.Message}");
                throw;
               // ErrorMessage = $"Error loading initial data: {ex.Message}";
            }
        }

        private async Task LoadTopGamesAsync()
        {
            if (IsBusy) return;
            
            try
            {
                IsBusy = true;
                //ErrorMessage = string.Empty;

                var games = await _steamSpy.GetTop100In2WeeksAsync();
                TopGames.Clear();
                foreach (var g in games)
                {
                    TopGames.Add(g);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading top games: {ex.Message}");
                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
