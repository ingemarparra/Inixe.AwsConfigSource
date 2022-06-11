// -----------------------------------------------------------------------
// <copyright file="ConfidentialController.cs" company="Inixe S.A.">
// Copyright All Rights reserved. Inixe S.A. 2021
// </copyright>
// -----------------------------------------------------------------------

namespace Inixe.Extensions.AwsConfigSource.Sample1.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    [ApiController]
    [Route("[controller]")]
    public class ConfidentialController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public ConfidentialController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet]
        public ActionResult<string> GetConfidential()
        {
            var section = this.configuration.GetSection("Confidential");
            var user = section.GetSection("UserName");
            var pwd = section.GetSection("Password");

            return this.Ok($"{user.Value}:{pwd.Value}");
        }
    }
}
