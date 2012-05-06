﻿using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sql;
using WoWHeadParser.Page;

namespace WoWHeadParser.Parser.Parsers
{
    internal class QuestLocaleParser : DataParser
    {
        public override string Parse(PageItem block)
        {
            string page = block.Page.Substring("\'quests\'");

            const string pattern = @"data: \[.*;";

            SqlBuilder builder;
            if (HasLocales)
            {
                builder = new SqlBuilder("locales_quest");
                builder.SetFieldsName(string.Format("title_{0}", Locales[Locale]));
            }
            else
            {
                builder = new SqlBuilder("quest_template", "id");
                builder.SetFieldsName("title");
            }

            MatchCollection find = Regex.Matches(page, pattern);
            for (int i = 0; i < find.Count; ++i)
            {
                Match item = find[i];
                string text = item.Value.Replace("data: ", "").Replace("});", "");
                JArray serialiation = (JArray)JsonConvert.DeserializeObject(text);

                for (int j = 0; j < serialiation.Count; ++j)
                {
                    JObject jobj = (JObject)serialiation[j];
                    JToken nameToken = jobj["name"];

                    string id = jobj["id"].ToString();
                    string name = nameToken == null ? string.Empty : nameToken.ToString().HTMLEscapeSumbols();

                    builder.AppendFieldsValue(id, name);
                }
            }

            return builder.ToString();
        }

        public override string Name { get { return "Quest locale data parser"; } }

        public override string Address { get { return "quests?filter=cr=30:30;crs=1:4;crv={0}:{1}"; } }

        public override int MaxCount { get { return 32000; } }
    }

    internal class QuestDataParser : DataParser
    {
        public override string Parse(PageItem block)
        {
            string page = block.Page.Substring("\'quests\'");

            const string pattern = @"data: \[.*;";

            SqlBuilder builder = new SqlBuilder("quest_template", "id");
            builder.SetFieldsName("level", "minlevel", "zoneOrSort", "RewardOrRequiredMoney");

            MatchCollection find = Regex.Matches(page, pattern);
            for (int i = 0; i < find.Count; ++i)
            {
                Match item = find[i];
                string text = item.Value.Replace("data: ", "").Replace("});", "");
                JArray serialization = (JArray)JsonConvert.DeserializeObject(text);

                for (int j = 0; j < serialization.Count; ++j)
                {
                    JObject jobj = (JObject)serialization[j];

                    string id = jobj["id"].ToString();

                    JToken zoneOrSortToken = jobj["category"];
                    JToken levelToken = jobj["level"];
                    JToken minLevelToken = jobj["reqlevel"];
                    JToken moneyToken = jobj["money"];

                    string level = levelToken == null ? string.Empty : levelToken.ToString();
                    string minLevel = minLevelToken == null ? string.Empty : minLevelToken.ToString();
                    string zoneOrSort = zoneOrSortToken == null ? string.Empty : zoneOrSortToken.ToString();
                    string money = moneyToken == null ? string.Empty : moneyToken.ToString();

                    builder.AppendFieldsValue(id, level, minLevel, zoneOrSort, money);
                }
            }

            return builder.ToString();
        }

        public override string Name { get { return "Quest template data parser"; } }

        public override string Address { get { return "quests?filter=cr=30:30;crs=1:4;crv={0}:{1}"; } }

        public override int MaxCount { get { return 32000; } }
    }
}