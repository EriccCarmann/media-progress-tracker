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

        public IAsyncRelayCommand LoadDataCommand { get; }

        public MainViewModel(ISteamSpyService steamSpyService)
        {
            _steamSpy = steamSpyService;

            Load();
        }

        private async Task Load()
        {
            var games = await _steamSpy.GetTop100In2WeeksAsync();
            TopGames.Clear();
            Console.Write(games);
            foreach (var g in games)
            {
                TopGames.Add(g);
            }
        }

        private async Task LoadTopGamesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            var games = await _steamSpy.GetTop100In2WeeksAsync();
            TopGames.Clear();
            foreach (var g in games)
            {
                TopGames.Add(g);
            }

            IsBusy = false;
        }
    }
}
