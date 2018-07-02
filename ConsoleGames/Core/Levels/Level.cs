﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleGames
{
    public class Level
    {
        public List<LevelItem> Items { get; set; } = new List<LevelItem>();
        public string Serialize() => JsonConvert.SerializeObject(this, Formatting.Indented);
        public static Level Deserialize(string json) => JsonConvert.DeserializeObject<Level>(json);
    }

    public class LevelItem
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public char Symbol { get; set; }
        public ConsoleColor? FG { get; set; }
        public ConsoleColor? BG { get; set; }
        public List<String> Tags { get; set; } = new List<string>();

        public bool HasSimpleTag(string tag) => Tags.Where(t => t.Equals(tag)).Count() > 0;
        public bool HasValueTag(string tag) => Tags.Where(t => t.StartsWith(tag + ":")).Count() > 0;

        public string GetTagValue(string key)
        {
            if(TryGetTagValue(key, out string value) == false)
            {
                throw new ArgumentException("There is no value for key: "+key);
            }
            else
            {
                return value;
            }
        }

        public bool TryGetTagValue(string key, out string value)
        {
            if(HasValueTag(key))
            {
                var tag = Tags.Where(t => t.StartsWith(key + ":")).FirstOrDefault();
                value = ParseTagValue(tag);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        private string ParseTagValue(string tag)
        {
            var splitIndex = tag.IndexOf(':');
            if (splitIndex <= 0) throw new ArgumentException("No tag value present for tag: " + tag);

            var val = tag.Substring(splitIndex + 1, tag.Length - (splitIndex + 1));
            return val;
        }
    }
}
