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

        // 1. Collection for binding
        public ObservableCollection<Game> TopGames { get; } = new ObservableCollection<Game>();

        [ObservableProperty]
        private bool isBusy;

        // 2. Exposed command to trigger loading
        public IAsyncRelayCommand LoadDataCommand { get; }

        public MainViewModel(ISteamSpyService steamSpyService)
        {
            _steamSpy = steamSpyService;

            Load();
            //LoadDataCommand = new AsyncRelayCommand(LoadTopGamesAsync);
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
