using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Helpers

{
    public class AuthHelper
    {
        private readonly IConfiguration _config;

        public AuthHelper(IConfiguration config)
        {
            _config = config;
        }
        // Method to generate password hash
        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value +
                Convert.ToBase64String(passwordSalt);

            return KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8
            );
        }

        // Method to create a JWT token
        public string CreateToken(int userId)
        {
            // Define claims for the token
            Claim[] claims = new Claim[] {
                new Claim("userId", userId.ToString())
            };

            // Get the token key from configuration
            string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;

            // Create a symmetric security key from the token key
            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    tokenKeyString != null ? tokenKeyString : ""
                )
            );

            // Create signing credentials using the security key
            SigningCredentials credentials = new SigningCredentials(
                tokenKey,
                SecurityAlgorithms.HmacSha512Signature
            );

            // Create a security token descriptor
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            // Create a JWT token
            SecurityToken token = tokenHandler.CreateToken(descriptor);

            // Write the token as a string
            return tokenHandler.WriteToken(token);
        }
    }

}

