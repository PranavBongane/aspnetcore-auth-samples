
using System.Net.Http.Headers;
using System.Text;

Console.Write("Enter ClientId: ");
var clientId = Console.ReadLine();
Console.Write("Enter Secret: ");
var secret = Console.ReadLine();

var client = new HttpClient();
var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}"));
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);

var response = await client.GetAsync("https://localhost:7001/api/product");
Console.WriteLine(await response.Content.ReadAsStringAsync());
