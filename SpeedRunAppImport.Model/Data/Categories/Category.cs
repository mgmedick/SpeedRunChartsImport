﻿using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model.Entity;

namespace SpeedRunAppImport.Model.Data
{
    public class Category
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public Uri WebLink { get; set; }
        public CategoryType Type { get; set; }
        public string Rules { get; set; }
        public Players Players { get; set; }
        public bool IsMiscellaneous { get; set; }
        public bool IsTimerAscending { get; set; }

        //linkIDs
        public string GameID { get; set; }

        //embeds
        public Game Game { get; set; }
        public IEnumerable<Variable> Variables { get; set; }


        //public Category() { }

        /*
        public static Category Parse(SpeedrunComClient client, dynamic categoryElement)
        {
            if (categoryElement is ArrayList)
                return null;

            var category = new Category();

            //Parse Attributes

            category.ID = categoryElement.id as string;
            category.Name = categoryElement.name as string;
            category.WebLink = new Uri(categoryElement.weblink as string);
            category.Type = categoryElement.type == "per-game" ? CategoryType.PerGame : CategoryType.PerLevel;
            category.Rules = categoryElement.rules as string;
            category.Players = Players.Parse(client, categoryElement.players);
            category.IsMiscellaneous = categoryElement.miscellaneous;

            //Parse Links

            var properties = categoryElement.Properties as IDictionary<string, dynamic>;
            var links = properties["links"] as IEnumerable<dynamic>;

            var gameUri = links.First(x => x.rel == "game").uri as string;
            category.GameID = gameUri.Substring(gameUri.LastIndexOf('/') + 1);

            if (properties.ContainsKey("game"))
            {
                var gameElement = properties["game"].data;
                var game = Game.Parse(client, gameElement) as Game;
                category.game = new Lazy<Game>(() => game);
            }
            else
            {
                category.game = new Lazy<Game>(() => client.Games.GetGame(category.GameID));
            }

            if (properties.ContainsKey("variables"))
            {
                Func<dynamic, Variable> parser = x => Variable.Parse(client, x) as Variable;
                var variables = client.ParseCollection(properties["variables"].data, parser);
                category.variables = new Lazy<ReadOnlyCollection<Variable>>(() => variables);
            }
            else
            {
                category.variables = new Lazy<ReadOnlyCollection<Variable>>(() => client.Categories.GetVariables(category.ID));
            }

            //category.Runs = client.Runs.GetRuns(categoryId: category.ID);

            if (category.Type == CategoryType.PerGame)
            {

                category.leaderboard = new Lazy<Leaderboard>(() =>
                    {
                        var leaderboard = client.Leaderboards
                                        .GetLeaderboardForFullGameCategory(category.GameID, category.ID);

                        leaderboard.game = new Lazy<Game>(() => category.Game);
                        leaderboard.category = new Lazy<Category>(() => category);

                        foreach (var record in leaderboard.Records)
                        {
                            record.game = leaderboard.game;
                            record.category = leaderboard.category;
                        }

                        return leaderboard;
                    });
                category.worldRecord = new Lazy<Record>(() =>
                    {
                        if (category.leaderboard.IsValueCreated)
                            return category.Leaderboard.Records.FirstOrDefault();

                        var leaderboard = client.Leaderboards
                                        .GetLeaderboardForFullGameCategory(category.GameID, category.ID, top: 1);

                        leaderboard.game = new Lazy<Game>(() => category.Game);
                        leaderboard.category = new Lazy<Category>(() => category);

                        foreach (var record in leaderboard.Records)
                        {
                            record.game = leaderboard.game;
                            record.category = leaderboard.category;
                        }

                        return leaderboard.Records.FirstOrDefault();
                    });
            }
            else
            {
                category.leaderboard = new Lazy<Leaderboard>(() => null);
                category.worldRecord = new Lazy<Record>(() => null);
            }

            return category;
        }
        */

        //public override int GetHashCode()
        //{
        //    return (ID ?? string.Empty).GetHashCode();
        //}

        //public override bool Equals(object obj)
        //{
        //    var other = obj as Category;

        //    if (other == null)
        //        return false;

        //    return ID == other.ID;
        //}

        public override string ToString()
        {
            return Name;
        }

        //public CategoryEntity ConvertToEntity()
        //{
        //    return new CategoryEntity
        //    {
        //        ID = this.ID,
        //        Name = this.Name,
        //        Rules = this.Rules,
        //        GameID = this.GameID,
        //        CategoryTypeID = (int)this.Type
        //    };
        //}
    }
}
