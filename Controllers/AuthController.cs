using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;
        private readonly AuthHelper _authHelper;

        // Constructor to initialize the AuthController with dependencies
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
            _authHelper = new AuthHelper(config);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            // Check if passwords match
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                // SQL query to check if the user already exists
                string sqlCheckUserExists = "SELECT Email FROM TutorialAppSchema.Auth WHERE Email = '"
                    + userForRegistration.Email + "'";

                // Load existing users from the database
                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);

                // If user does not exist, proceed with registration
                if (existingUsers.Count() == 0)
                {
                    // Generate a random password salt
                    byte[] passwordSalt = new byte[128 / 8];

                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);  // fill passwordSalt with non-zero bytes
                    }

                    // Generate password hash
                    byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);

                    // SQL query to add authentication data to the database
                    string sqlAddAuth = @"
                        INSERT INTO TutorialAppSchema.Auth (
                            [Email],
                            [PasswordHash],
                            [PasswordSalt]
                        ) VALUES ('"
                            + userForRegistration.Email +
                            "', @PasswordHash, @PasswordSalt )";

                    List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    // Add parameters for password salt and hash
                    SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
                    passwordSaltParameter.Value = passwordSalt;

                    SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
                    passwordHashParameter.Value = passwordHash;

                    sqlParameters.Add(passwordSaltParameter);
                    sqlParameters.Add(passwordHashParameter);

                    // Execute SQL query with parameters to add authentication data
                    if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
                    {
                        // SQL query to add user data to the database
                        string sqlAddUser = @"
                            INSERT INTO TutorialAppSchema.Users(
                                [FirstName],
                                [LastName],
                                [Email],
                                [Gender],
                                [Active]
                            ) VALUES ("
                              + "'" + userForRegistration.FirstName + "','"
                              + userForRegistration.LastName + "','"
                              + userForRegistration.Email + "','"
                              + userForRegistration.Gender + "', 1)";

                        // Execute SQL query to add user data
                        if (_dapper.ExecuteSql(sqlAddUser))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed to add user.");
                    }
                    throw new Exception("Failed to register user.");
                }
                throw new Exception("User already exists!");
            }
            throw new Exception("Passwords do not match!");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            // SQL query to retrieve password hash and salt based on the provided email
            string sqlForHashAndSalt = @"SELECT
                [PasswordHash],
                [PasswordSalt] FROM TutorialAppSchema.Auth WHERE Email = '" + userForLogin.Email + "'";

            // Load user data (including password hash and salt) from the database
            UserForLoginConfirmationDto userForLoginConfirmation = _dapper.LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);

            // Compute the hash of the provided password using the stored salt
            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForLoginConfirmation.PasswordSalt);

            // Compare the computed hash with the stored hash
            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForLoginConfirmation.PasswordHash[index])
                {
                    // If the hashes don't match, return 401 (Unauthorized) with an error message
                    return StatusCode(401, "Incorrect password!");
                }
            }

            // SQL query to retrieve the user ID based on the provided email
            string userIdSql = @" 
                SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '" +
                        userForLogin.Email + "'";

            // Load the user ID from the database
            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            // Return a success response with a token (authentication token) in a dictionary
            return Ok(new Dictionary<string, string>{
                {"token", _authHelper.CreateToken(userId)}
            });
        }

        [HttpGet("RefreshToken")]
        public string RefreshToken()
        {
            string userIdSql = @" 
                SELECT UserId FROM TutorialAppSchema.Users WHERE UserId = '" +
                        User.FindFirst("userId")?.Value + "'";

            int userId = _dapper.LoadDataSingle<int>(userIdSql);

            return _authHelper.CreateToken(userId);
        }

    }
}
