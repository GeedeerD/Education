using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace Sports;

public partial class MainPage : ContentPage
{
    private class Sport
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }

    private readonly List<Sport> sports = new()
    {
        new() { Name = "Футбол", Description = "Игра, где забивают мяч в ворота ногами.", Image = "football.jpg" },
        new() { Name = "Баскетбол", Description = "Игра с мячом и кольцом. Команды набирают очки.", Image = "basketball.jpg" },
        new() { Name = "Теннис", Description = "Ракетки, мяч и сетка. Быстрая игра один на один или парами.", Image = "tennis.jpg" },
        new() { Name = "Хоккей", Description = "Команды на льду, шайба и клюшки. Очень динамично.", Image = "hockey.jpg" },
        new() { Name = "Волейбол", Description = "Команды перебрасывают мяч через сетку, не давая ему упасть.", Image = "volleyball.jpg" }
    };

    private int currentIndex = 0;
    private readonly Random random = new();

    public MainPage()
    {
        InitializeComponent();

        var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        tapGesture.Tapped += OnSportLabelDoubleTapped;
        SportLabel.GestureRecognizers.Add(tapGesture);

        UpdateSportDisplay(animated: false);
    }

    private async void UpdateSportDisplay(bool animated = true)
    {
        var sport = sports[currentIndex];

        if (animated)
        {
            await SportLabel.FadeTo(0, 200);
            await DescriptionLabel.FadeTo(0, 200);
        }

        SportLabel.Text = sport.Name;
        DescriptionLabel.Text = sport.Description;
        SportImage.IsVisible = false;

        if (animated)
        {
            await SportLabel.FadeTo(1, 200);
            await DescriptionLabel.FadeTo(1, 200);
        }
    }

    private void OnChangeSportClicked(object sender, EventArgs e)
    {
        currentIndex = (currentIndex + 1) % sports.Count;
        UpdateSportDisplay();
    }

    private void OnRandomSportClicked(object sender, EventArgs e)
    {
        int newIndex;
        do
        {
            newIndex = random.Next(sports.Count);
        } while (newIndex == currentIndex);

        currentIndex = newIndex;
        UpdateSportDisplay();
    }

    private void OnSportLabelDoubleTapped(object sender, EventArgs e)
    {
        var image = sports[currentIndex].Image;
        SportImage.Source = ImageSource.FromFile(image);
        SportImage.IsVisible = true;
    }
}