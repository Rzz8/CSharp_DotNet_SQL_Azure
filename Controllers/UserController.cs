using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]

// WeatherForecastController inherit ControllerBase class
public class UserController : ControllerBase
{
    DataContextDapper _dapper;
    public UserController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }

    [HttpGet("GetUsers")]
    public IEnumerable<User> GetUsers()
    {
        string sql = @"
            SELECT [UserId],
                    [FirstName],
                    [LastName],
                    [Email],
                    [Gender],
                    [Active] 
                FROM TutorialAppSchema.Users";
        IEnumerable<User> users = _dapper.LoadData<User>(sql);
        return users;
    }

    [HttpGet("GetUsers/Singleuser/{userId}")]
    public User GetSingleUser(int userId)
    {
        string sql = @"
            SELECT [UserId],
                    [FirstName],
                    [LastName],
                    [Email],
                    [Gender],
                    [Active] 
                FROM TutorialAppSchema.Users
                    WHERE UserID = " + userId.ToString();
        User user = _dapper.LoadDataSingle<User>(sql);
        return user;
    }

    // update user
    [HttpPut("UpdateUser")]
    public IActionResult UpdateUser(User user)
    {
        string sql = @"
            UPDATE TutorialAppSchema.Users
                SET [FirstName] ='" + user.FirstName +
                "', [LastName] = '" + user.LastName +
                "', [Email] = '" + user.Email +
                "', [Gender] = '" + user.Gender +
                "', [Active] = '" + user.Active +
                "' WHERE UserId = " + user.UserId;

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }

        throw new Exception("Failed to update user");
    }

    // add a new user
    [HttpPost("AddUser")]
    public IActionResult AddUser(UserToAddDto user)
    {
        string sql = @"
            INSERT INTO TutorialAppSchema.Users
            (
                [FirstName],
                [LastName],
                [Email],
                [Gender],
                [Active]
            ) VALUES (" +
                "'" + user.FirstName +
                "', '" + user.LastName +
                "', '" + user.Email +
                "', '" + user.Gender +
                "', '" + user.Active +
            "')";

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }

        throw new Exception("Failed to add user");
    }

    [HttpDelete("DeleteUser/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        string sql = @"
            DELETE FROM TutorialAppSchema.Users
                WHERE UserId = " + userId.ToString();

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }

        throw new Exception("Failed to delete user");
    }

    [HttpGet("GetUserSalary")]
    public IEnumerable<UserSalary> GetUserSalaries()
    {
        string sql = @"SELECT UserSalary.UserId
                    , UserSalary.Salary 
                        FROM TutorialAppSchema.UserSalary";
        IEnumerable<UserSalary> userSalaries = _dapper.LoadData<UserSalary>(sql);
        return userSalaries;
    }

    [HttpGet("GetSingleUserSalary/{userId}")]
    public UserSalary GetSingleUserSalary(int userId)
    {
        string sql = @"SELECT UserSalary.UserId
                    , UserSalary.Salary 
                        FROM TutorialAppSchema.UserSalary 
                            WHERE UserId =" + userId.ToString();
        UserSalary userSalary = _dapper.LoadDataSingle<UserSalary>(sql);
        return userSalary;
    }

    [HttpPost("AddUserSalary")]
    public IActionResult AddUserSalary(UserSalary userForAdd)
    {
        string sql = @"
            INSERT INTO TutorialAppSchema.UserSalary(
                UserId,
                Salary
            ) VALUES (
                " + userForAdd.UserId.ToString()
                + "," + userForAdd.Salary +
                ")";
        if (_dapper.ExecuteSql(sql))
        {
            return Ok(userForAdd);
        }
        throw new Exception("Failed to add user salary");
    }

    [HttpPut("EditUserSalary")]
    public IActionResult EditUserSalary(UserSalary userForUpdate)
    {
        string sql = @"
            UPDATE TutorialAppSchema.UserSalary SET Salary="
                + userForUpdate.Salary +
                "WHERE UserSalary.UserId=" + userForUpdate.UserId.ToString();
        if (_dapper.ExecuteSql(sql))
        {
            return Ok(userForUpdate);
        }
        throw new Exception("Failed to update user salary");
    }

    [HttpDelete("DeleteUserSalary/{userId}")]
    public IActionResult DeleteUserSalary(int userId)
    {
        string sql = @"DELETE FROM TutorialAppSchema.UserSalary WHERE UserId=" + userId.ToString();
        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to delete user salary");
    }

    [HttpGet("UserJobInfo/{userId}")]
    public IEnumerable<UserJobInfo> GetUserJobInfo(int userId)
    {
        return _dapper.LoadData<UserJobInfo>(@"
            SELECT  UserJobInfo.UserId
                    , UserJobInfo.JobTitle
                    , UserJobInfo.Department
            FROM  TutorialAppSchema.UserJobInfo
                WHERE UserId = " + userId.ToString());
    }

    [HttpPost("UserJobInfo")]
    public IActionResult PostUserJobInfo(UserJobInfo userJobInfoForInsert)
    {
        string sql = @"
            INSERT INTO TutorialAppSchema.UserJobInfo (
                UserId,
                Department,
                JobTitle
            ) VALUES (" + userJobInfoForInsert.UserId
                + ", '" + userJobInfoForInsert.Department
                + "', '" + userJobInfoForInsert.JobTitle
                + "')";

        if (_dapper.ExecuteSql(sql))
        {
            return Ok(userJobInfoForInsert);
        }
        throw new Exception("Adding User Job Info failed on save");
    }

    [HttpPut("UserJobInfo")]
    public IActionResult PutUserJobInfo(UserJobInfo userJobInfoForUpdate)
    {
        string sql = "UPDATE TutorialAppSchema.UserJobInfo SET Department='" 
            + userJobInfoForUpdate.Department
            + "', JobTitle='"
            + userJobInfoForUpdate.JobTitle
            + "' WHERE UserId=" + userJobInfoForUpdate.UserId.ToString();

        if (_dapper.ExecuteSql(sql))
        {
            return Ok(userJobInfoForUpdate);
        }
        throw new Exception("Updating User Job Info failed on save");
    }
    
    [HttpDelete("UserJobInfo/{userId}")]
    public IActionResult DeleteUserJobInfo(int userId)
    {
        string sql = @"
            DELETE FROM TutorialAppSchema.UserJobInfo 
                WHERE UserId = " + userId.ToString();
        
        Console.WriteLine(sql);

        if (_dapper.ExecuteSql(sql))
        {
            return Ok();
        } 

        throw new Exception("Failed to Delete User");
    }
}