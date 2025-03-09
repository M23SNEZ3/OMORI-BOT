using Remora.Rest.Core;
using System.Text.Json;

namespace OMORI_BOT.M23.Services;
    /// <summary>
    ///     Service for managing white list.
    ///If the whitelist is empty, all commands will be executed without verification.
    /// </summary>
    public class JsonData
    {
        public List<ulong> Triggers { get; set; } = new();
    }
    
    public sealed class WhiteListService
    {
        private const string PathToJsonWhiteList = @"members.json";
        public async Task<bool> CheckWhitelist(Snowflake id)
        {
            var ulongId = id.Value;
            var jsonFile = File.OpenRead(PathToJsonWhiteList);
            var jsonData = await JsonSerializer.DeserializeAsync<JsonData>(jsonFile);
            jsonFile.Close();
            if ((jsonData is not null && jsonData.Triggers.Contains(ulongId)) || (jsonData is not null && jsonData.Triggers.Count == 0)) return true;
            return false;
        }

        public async Task<bool> AddWhiteList(Snowflake id)
        {
            using var jsonFile = new FileStream(PathToJsonWhiteList, FileMode.Open, FileAccess.ReadWrite);
            var ulongId = id.Value;
            var jsonData = await JsonSerializer.DeserializeAsync<JsonData>(jsonFile);
            if (jsonData is not null && !jsonData.Triggers.Contains(ulongId))
            {
                jsonFile.SetLength(0);
                jsonData.Triggers.Add(ulongId); 
                await JsonSerializer.SerializeAsync(jsonFile, jsonData, new JsonSerializerOptions { WriteIndented = true });
                return true;
            }
            return false;
        }
        
        public async Task<bool> DeleteWhiteList(Snowflake id)
        {
            using var jsonFile = new FileStream(PathToJsonWhiteList, FileMode.Open, FileAccess.ReadWrite);
            var ulongId = id.Value;
            var jsonData = await JsonSerializer.DeserializeAsync<JsonData>(jsonFile);
            if (jsonData is not null && jsonData.Triggers.Contains(ulongId))
            {
                jsonFile.SetLength(0);
                jsonData.Triggers.Remove(ulongId);
                await JsonSerializer.SerializeAsync(jsonFile, jsonData, new JsonSerializerOptions { WriteIndented = true });
                jsonFile.Close();
                return true;
            }
            jsonFile.Close();
            return false;
        }

        public async Task<JsonData?> GetWhiteList()
        {
            var jsonFile = File.OpenRead(PathToJsonWhiteList);
            JsonData? jsonData = await JsonSerializer.DeserializeAsync<JsonData>(jsonFile);
            jsonFile.Close();
            return jsonData;
        }

    }