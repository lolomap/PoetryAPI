using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoetryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpellcheckController : ControllerBase
    {

        [HttpGet("{text}")]
        public string Get(string text)
        {
            try
            {
                return Startup.Spell.SpellFix(text);
            }
            catch
            {
                return "None";
            }
        }
    }
}
