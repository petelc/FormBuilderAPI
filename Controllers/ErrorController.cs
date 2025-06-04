using System;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilderAPI.Controllers;

[ApiController]
public class ErrorController : ControllerBase
{
    [Route("/error")]
    [HttpGet]
    public IActionResult Error()
    {
        return Problem("An error occurred while processing your request.");
    }

    [Route("error/test")]
    [HttpGet]
    public IActionResult TestError()
    {
        // This is just a test endpoint to trigger an error
        throw new Exception("This is a test error.");
    }
}
