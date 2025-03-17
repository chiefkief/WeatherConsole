// See https://aka.ms/new-console-template for more information
using System.Net.Http.Headers;
using System.Text.Json;

using HttpClient client = new();
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(
    new MediaTypeWithQualityHeaderValue("application/geo+json"));
client.DefaultRequestHeaders.Add("User-Agent", "ChiefMakeStorms");

await processWeatherAsync(client);

static async Task processWeatherAsync(HttpClient client){
  await using Stream stream = 
    await client.GetStreamAsync("https://api.weather.gov/gridpoints/MPX/159,66/forecast");

  //var days = await JsonSerializer.DeserializeAsync<List<Day>>(stream);
  //Console.WriteLine($"days: {days}");
  
  
  var json = await client.GetStringAsync("https://api.weather.gov/gridpoints/MPX/159,66/forecast");
  var isProps = false;
  var allClosed = true;
  List<Day> days = new();
  var commaSpot = 0;
  var currentNum = -1;
  var currentName = "";
  var currentTemp = "";
  var currentDesc = "";

  //ill figure out the stream, but for now, lets do it with the string
  for(var i = 0; i < json.Length; i++){
      
      if(json[i] == '{' && isProps == true){
        allClosed = false;
      }else if(json.Substring(i).Length > 7 && json.Substring(i, 7) == "periods"){
        isProps = true;
      }else if(isProps == true && allClosed == true && json[i] == '}'){
        isProps = false;
      }


      if(json.Substring(i).Length > 25 && json.Substring(i, 6) == "number" && isProps == true){
        commaSpot = json.IndexOf(",", i) - i - 9;
        currentNum = int.Parse(json.Substring(i + 9, commaSpot)) - 1;
      }
      if(json.Substring(i).Length > 15 && json.Substring(i,4) == "name" && isProps == true){
        commaSpot = json.IndexOf(',', i) - i - 9;
        currentName = json.Substring(i + 8, commaSpot);
 }
      if(json.Substring(i).Length > 17 && json.Substring(i, 16) == "detailedForecast" && isProps == true){
        commaSpot = json.IndexOf('"', i + 20 ) - i - 20;
        currentDesc = json.Substring(i + 20, commaSpot);
      }
      if(json.Substring(i).Length > 17 && json.Substring(i, 12) == "temperature\"" && isProps == true){
        commaSpot = json.IndexOf(',', i) - i - 14;
        currentTemp= json.Substring(i + 14, commaSpot) + "\u00B0F";
      }

      if(currentName  != "" && currentTemp != "" && currentDesc != "" && currentNum != -1){
        days.Add(new Day(currentName, currentTemp, currentDesc));
        currentName = "";
        currentTemp = "";
        currentDesc = "";
        currentNum = -1;
      }

  }

  Console.WriteLine("How many Days would you like to see?");
  var dayNums = 5;
  var numDays = Console.ReadLine();
  if(numDays != null){
    dayNums = int.Parse(numDays);

  }



  for(var i = 0; i < dayNums; i++){
    Console.WriteLine("\n");
    Console.WriteLine(days[i].Name + "    " + days[i].Temp);
    Console.WriteLine(days[i].Desc);
  }


}
