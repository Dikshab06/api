using MySqlConnector;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;
using CsvHelper;
using System.Globalization;


namespace MinhaApi.Controllers
{

   
    [Route("api/[controller]")]
    [ApiController]
    public class IsController : ControllerBase
    {
        private readonly ILogger<IsController> _logger;
        private readonly MyDbContext _context;
        private readonly string _jwtKey;
        private readonly IConfiguration _configuration;

        public IsController(ILogger<IsController> logger,MyDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _jwtKey = _configuration["Jwt:Key"];
        }

//login 
        //[HttpPost("login")]
        //public IActionResult Login([FromBody] Conta conta)
        //{
           // var existingUser = _context.contas.FirstOrDefault(u => u.Username == conta.Username && u.Password == conta.Password);
           // if (existingUser == null) return Unauthorized();

           // var claims = new List<Claim>
           // {
               // new Claim(ClaimTypes.Name, existingUser.Username),
               // new Claim(ClaimTypes.Role, existingUser.Role) 
             //};

           // var token = GenerateJwtToken(claims);
            //return Ok(new { Token = token });
       // }
       [HttpPost("login")]
public IActionResult Login([FromBody] LoginDto loginDto)
{
    var user = _context.contas.FirstOrDefault(u => u.Username == loginDto.Username && u.Password == loginDto.Password);

    if (user == null)
    {
        return Unauthorized();
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username)
    };
    if (loginDto.Username == "Exame" && loginDto.Password == "Junho")
    {
        claims.Add(new Claim(ClaimTypes.Role, "Exame1"));
    }

    var token = GenerateJwtToken(claims);
    return Ok(new { Token = token });
}


//withoutrole
        //[HttpGet("getDataWithoutRole")]
        //public IActionResult GetDataWithoutRole()
        //{
           // return Ok("Data without role");
       // }

//get by role
        [HttpGet("getDataWithRole")]
        //[Authorize(Roles = "Admin")]
        public IActionResult GetDataWithRole()
        {
            var results = new List<Dictionary<string, object>>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand("SELECT * FROM contas", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.GetValue(i);
                            }
                            results.Add(row);
                        }
                    }
                }
            }

            return Ok(results);
        }

//get by id        
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        { var conta = _context.contas.FindAsync(id);
            if (conta == null) return NotFound();
           return Ok(conta);
       }
  
//post
        [HttpPost]
        public IActionResult Create([FromBody] Conta conta)
        {
            _context.contas.Add(conta);
            _context.SaveChanges();
            return CreatedAtAction(nameof(Get), new { id = conta.Id }, conta);
        }

//put
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Conta conta)
        {
            if (id != conta.Id) return BadRequest();

            var existingConta = _context.contas.Find(id);
            if (existingConta == null) return NotFound();

            existingConta.Username = conta.Username;
            existingConta.Password = conta.Password;
            existingConta.Role = conta.Role;

            _context.SaveChanges();
            return NoContent();
        }
//delete
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var conta = _context.contas.Find(id);
            //if (conta == null) return NotFound();

            _context.contas.Remove(conta);
            _context.SaveChanges();
            return NoContent();
        }

        // exportar registos para XML
         [HttpGet("exportToXml")]
         //[Authorize(Roles = "Admin")]
        public IActionResult ExportToXml()
        {
            try
            {
                var contas = _context.contas.ToList(); // Obtém todas as contas do banco de dados

                var xDocument = new XDocument(
                    new XElement("Contas",
                        contas.Select(c =>
                            new XElement("Conta",
                                new XElement("Id", c.Id),
                                new XElement("Username", c.Username),
                                new XElement("Password", c.Password),
                                new XElement("Role", c.Role)
                            )
                        )
                    )
                );

                var xmlString = xDocument.ToString();
                var bytes = Encoding.UTF8.GetBytes(xmlString);

                // Retorna o arquivo XML como um download
                return File(bytes, "application/xml", "contas.xml");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao exportar para XML: {ex.Message}");
            }
        }
    
        // importar registos de CSV
        [HttpPost("importFromCsv")]
        public IActionResult ImportFromCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Arquivo CSV não fornecido.");

            try
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<Conta>().ToList();

                    _context.contas.AddRange(records);
                    _context.SaveChanges();
                }

                return Ok("Registros importados com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao importar registros: {ex.Message}");
            }
        }
        
    
//token
        private string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var key = Encoding.ASCII.GetBytes(_jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


          public class LoginDto
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

    }
}