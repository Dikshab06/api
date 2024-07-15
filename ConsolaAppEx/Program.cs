using System.Net.Http.Headers;
using System.Net.Http.Json;


class Program
{
    static async Task Main(string[] args)
    {
        var client = new HttpClient();
        string token = null;

        while (true)
        {
            Console.WriteLine("Menu:");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Obter dados sem Role");
            Console.WriteLine("3. Obter dados com Role");
            Console.WriteLine("4. Validação de token");
            Console.WriteLine("5. Sair");
            Console.Write("Escolher uma opção: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    token = await Login(client);
                    break;
                case "2":
                    await GetData(client, token, "getDataWithoutRole");
                    break;
                case "3":
                    await GetData(client, token, "getDataWithRole");
                    break;
                case "4":
                    await exportToxml(client, token, "export");
                    break;
                case "6":
                    await CreateUpdate(client, token, "create");
                    break;
                case "7":
                    await ReadDel(client, token, "read");
                    break;
                case "8":
                    await CreateUpdate(client, token, "update");
                    break;
                case "9":
                    await ReadDel(client, token, "delete");
                    break;
                case "0":
                    return;
        }
    }

    static async Task<string> Login(HttpClient client)
    {var user = new { username = "teste1", password = "teste1", role = "Admin" };
        var response = await client.PostAsJsonAsync("http://localhost:5064/api/Is/login", user);
        if (response.IsSuccessStatusCode){
           var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
           Console.WriteLine("Login successful!");
            return data?.Token;
        }
        Console.WriteLine($"Login failed! Status code: {response.StatusCode}");
        return null;
    }
   // static async Task<string> Login(HttpClient client)
//{
   // var user = new { username = "Exame", password = "Junho"};  
    //var response = await client.PostAsJsonAsync("http://localhost:5064/api/Is/login", user);
    //if (response.IsSuccessStatusCode)
    //{
        //var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
       // Console.WriteLine("Login successful!");
       // return data?.Token;  
    //}
   // Console.WriteLine($"Login failed! Status code: {response.StatusCode}");
    //return null;
//}


    static async Task GetData(HttpClient client, string token, string endpoint)
    {
        if (token != null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"http://localhost:5064/api/Is/{endpoint}");
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Data: {data}");
        }
        else
        {
            Console.WriteLine($"Request to {endpoint} failed with status code {response.StatusCode}");
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error details: {errorContent}");
        }
    }

    static async Task exportToxml(HttpClient client, string token, string endpoint)
    {
        if (token != null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync($"http://localhost:5064/api/Is/{endpoint}");
        if (response.IsSuccessStatusCode)
        {
            /*var data = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Data: {data}");*/
        }
        else
        {
            Console.WriteLine("Request failed!");
        }
    }

    static async Task ReadDel(HttpClient client, string token, string endpoint)
    {
        if (token != null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        var id = "";
        if(endpoint=="read"){
            Console.Write("Qual o Id a pesquisar?");
            id = Console.ReadLine();
        }else{
            Console.Write("Qual o Id a apagar?");
            id = Console.ReadLine();
        }
        var sendId = new StringContent($"\"{id}\"", Encoding.UTF8, "text/json");
        var response = await client.PostAsync($"http://localhost:5064/api/Is/{endpoint}",sendId);
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Data: {data}");
        }
        else
        {
            Console.WriteLine("Request failed!");
        }
    }

    static async Task CreateUpdate(HttpClient client, string token, string endpoint)
    {
        if (token != null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        string[] dados = new string[4];
        if(endpoint=="create"){
            Console.Write("Qual o Id?");
            dados[0] = Console.ReadLine();
            Console.Write("Qual a Password?");
            dados[1] = Console.ReadLine();
            Console.Write("Qual o Username?");
            dados[2] = Console.ReadLine();
            Console.Write("Qual a Role?");
            dados[3] = Console.ReadLine();
        }else{
            Console.Write("Qual o Id a atualizar?");
            dados[0] = Console.ReadLine();
            Console.Write("Qual a nova Password?");
            dados[1] = Console.ReadLine();
            Console.Write("Qual o novo Username?");
            dados[2] = Console.ReadLine();
            Console.Write("Qual a nova Role?");
            dados[3] = Console.ReadLine();
        }
        var jsonData = new {
            Id = dados[0],
            Password = dados[1],
            Username = dados[2],
            Role = dados[3]
        };
        var jsonString = JsonConvert.SerializeObject(jsonData);
        var sendData = new StringContent(jsonString, Encoding.UTF8, "text/json");
        var response = await client.PostAsync($"http://localhost:5064/api/Is/update",sendData);
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Data: {data}");
        }
        else
        {
            Console.WriteLine("Request failed!");
        }
    }


    //validar
    static async Task ValidateToken(HttpClient client, string token)
    {
        if (token == null)
        {
            Console.WriteLine("Token não está presente. Realize o login primeiro.");
            return;
        }

        var response = await client.GetAsync("http://localhost:5064/api/Is/validateToken");

        if (response.IsSuccessStatusCode)
        {
            var isValid = await response.Content.ReadFromJsonAsync<bool>();
            if (isValid)
            {
                Console.WriteLine("Token válido!");
            }
            else
            {
                Console.WriteLine("Token expirado ou inválido.");
            }
        }
        else
        {
            Console.WriteLine($"Falha ao validar token. Status code: {response.StatusCode}");
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Detalhes do erro: {errorContent}");
        }
    }



 


       //static async Task GetDataWithRole(HttpClient client, string token)
    //{
       // if (token != null)
        //{
           // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //}

        //var response = await client.GetAsync($"https://localhost:7261/api/Is/getDataWithRole");
        //if (response.IsSuccessStatusCode)
        //{
            //var data = await response.Content.ReadAsStringAsync();
            //Console.WriteLine($"Data with role: {data}");
        //}
        //else
        //{
           // Console.WriteLine($"Request failed with status code {response.StatusCode}");
        //}
    //}
}
