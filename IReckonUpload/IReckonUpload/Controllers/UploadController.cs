﻿using Microsoft.AspNetCore.Mvc;

namespace IReckonUpload.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        // POST api/upload
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }
    }
}