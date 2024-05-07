// Controllers/AuthController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Threading.Tasks;
using Authotification.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Authotification.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User model)
        {
            // Your registration logic goes here
            if (IsValidEmail(model.Email))
            {
                // Simulated registration
                // Hash the password before saving it
                // For simplicity, let's assume password hashing is done elsewhere
                // In a real application, you would save the user to your database here

                // Generate a verification token (you should implement this)
                string verificationToken = GenerateVerificationToken();

                // For simulation purposes, just return a success message
                // In a real application, you would send an email containing the verification token
                // and then prompt the user to verify their email address by clicking on a link
                // in the email
                var emailSent = await SendVerificationEmail(model.Email, verificationToken);
                if (emailSent)
                    return Ok("Registration successful! Verification email sent.");
                else
                    return StatusCode(500, "Failed to send verification email.");
            }
            return BadRequest("Invalid email address.");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User model)
        {
            // Your authentication logic goes here
            if (IsValidUser(model.Email, model.Password))
            {
                var tokenString = GenerateJwtToken(model.Email);
                return Ok(new { Token = tokenString });
            }
            return Unauthorized();
        }

        private bool IsValidEmail(string email)
        {
            // Your email validation logic goes here
            // For simplicity, let's just check if it contains '@'
            return email.Contains("@");
        }

        private bool IsValidUser(string email, string password)
        {
            // Your user validation logic goes here
            // For simplicity, a hardcoded check is used
            return email == "example@email.com" && password == "password123";
        }

        private string GenerateVerificationToken()
        {
            // Generate a random token or use some other mechanism
            // For simplicity, let's generate a random string here
            return Guid.NewGuid().ToString();
        }

        private async Task<bool> SendVerificationEmail(string email, string verificationToken)
        {
            try
            {
                // Create a new MimeMessage
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Your Name", "your@email.com"));
                message.To.Add(new MailboxAddress("Recipient Name", email));
                message.Subject = "Email Verification";

                // Build the email body
                var builder = new BodyBuilder();
                builder.HtmlBody = $"<h1>Verify Your Email Address</h1>" +
                                   $"<p>Please click the following link to verify your email address:</p>" +
                                   $"<a href='http://yourwebsite.com/verify?token={verificationToken}'>Verify Email Address</a>";

                // Set the body of the message
                message.Body = builder.ToMessageBody();

                // Send the email using SMTP
                using (var client = new SmtpClient())
                {
                    // Connect to the SMTP server
                    await client.ConnectAsync("smtp.gmail.com", 587, false);

                    // Authenticate with the SMTP server if needed
                    await client.AuthenticateAsync("noreply@don.me", "123456");

                    // Send the email
                    await client.SendAsync(message);

                    // Disconnect from the SMTP server
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine("Error sending email: " + ex.Message);
                return false;
            }
        }

        private string GenerateJwtToken(string email)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
    
            // Create claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, email),
                // You can add more claims as needed, such as user roles, etc.
            };

            // Create token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // Token expiration time (adjust as needed)
                SigningCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
