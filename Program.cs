using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.IO;

// Classes to represent the JSON structure, see the included example input json
public class EventMetadata
{
    public string skin { get; set; }
    public bool mapBonusActive { get; set; }
    public Dictionary<string, bool> featuredInstas { get; set; }
}

public class CollectionEvent
{
    public string id { get; set; }
    public string name { get; set; }
    public long start { get; set; }
    public long end { get; set; }
    public string type { get; set; }
    public string frequency { get; set; }
    public int priority { get; set; }
    public EventMetadata metadata { get; set; }
}

class Program
{
	public static long I64(string str)
	{
		long result = 0;
		foreach (char c in str)
		{
			result = result * 10 + (c - '0');
		}
		return result;
	}

    // Converts the event id by concatenating char codes, truncating to 18 digits
    public static long GetSeedLong(string id)
    {
        var charCodes = new System.Text.StringBuilder();
        foreach (char c in id)
        {
            charCodes.Append(((int)c).ToString());
        }
        string trimmed = charCodes.ToString();
        if (trimmed.Length > 18)
        {
            trimmed = trimmed.Substring(0, 18);
        }
        return Math.Abs(I64(trimmed));
    }

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: CollectionEvent.exe <json_file_path>");
            Console.WriteLine("Example: CollectionEvent.exe input.json");
            return;
        }

        string filePath = args[0];
        
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: File '{filePath}' not found.");
            return;
        }

        string jsonInput;
        try
        {
            jsonInput = File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
            return;
        }

        try
        {
            var eventData = JsonSerializer.Deserialize<CollectionEvent>(jsonInput, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var seed = GetSeedLong(eventData.id);
            
            var featuredMonkeyTypes = eventData.metadata.featuredInstas
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

            var shuffledMonkeyTypes = Shuffler.ShuffleSeeded(seed, featuredMonkeyTypes);

            var helper = new CollectionEventFeaturedInstasHelper();
            helper.instaMonkeysTypeList = shuffledMonkeyTypes;

            long durationMs = eventData.end - eventData.start;
            int pageDurationMs = 28800;
            int maxPages = (int)(durationMs / pageDurationMs / 1000); // divided by 1000 since it is milliseconds

            var pages = new Dictionary<int, List<string>>();

            for (int page = 0; page < maxPages; page++)
            {
                helper.GetCurrentPageNumber = () => page;
                List<string> pageItems = helper.GetPossibleInstaMonkeys();
                pages[page] = pageItems;
            }

            var result = new
            {
                id = eventData.id,
                start = eventData.start,
                end = eventData.end,
                // numericSeed = seed,
                featuredInstas = featuredMonkeyTypes,
                pages = pages
            };

            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            var outputFileName = $"{eventData.id}_pages.json";
            try
            {
                File.WriteAllText(outputFileName, json);
                Console.WriteLine($"Output written to {outputFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing output file: {ex.Message}");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error parsing JSON: {ex.Message}");
        }
    }
}

public class SeededRandom
{
    private long seed;

    public SeededRandom(long seed)
    {
        this.seed = Math.Abs(seed);
    }
    public int Next()
    {
        seed = (seed * 16807) % 0x7fffffff;
        return (int)seed;
    }
    public double NextFloat()
    {
        return Next() / (double)0x7ffffffe;
    }
    public int Range(int min, int max)
    {
        if (min == max) return min;
        return min + (Next() % (max - min));
    }
}

public static class Shuffler
{
    public static List<string> ShuffleSeeded(long seed, List<string> inputList)
    {
        var rng = new SeededRandom(seed);
        var list = new List<string>(inputList);

        int length = list.Count;
        for (int i = 0; i < length; i++)
        {
            int j = rng.Range(i, length);
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }

        return list;
    }
}

// Implementation of reverse engineered page logic from BTD6
public class CollectionEventFeaturedInstasHelper
{
    public List<string> instaMonkeysTypeList;
    public Func<int> GetCurrentPageNumber = () => 0;

    public List<string> GetPossibleInstaMonkeys()
    {
        if (instaMonkeysTypeList == null || instaMonkeysTypeList.Count == 0)
            return new List<string>();

        int totalCount = instaMonkeysTypeList.Count;
        int pageSize = (int)Math.Ceiling(totalCount * 0.25);
        int currentPage = GetCurrentPageNumber();

        int outerIndex = 0;
        while (pageSize <= currentPage)
        {
            currentPage -= pageSize;
            outerIndex++;
        }

        var pageItems = new List<string>();
        int maxItemsPerPage = 4;

        for (int i = 0; i < maxItemsPerPage; i++)
        {
            int rotIndex = (i + outerIndex + currentPage * maxItemsPerPage) % totalCount;
            pageItems.Add(instaMonkeysTypeList[rotIndex]);
        }

        return pageItems;
    }
}
