using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PoetryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DictController : ControllerBase
    {

        [HttpGet("{word}")]
        public string Get(string word)
        {
            try
            {
                //return Startup.Dict.SearchAllFiles(word).ToString();
                //return Startup.Dict.Search(word).ToString();

                Word res = Startup.Dict.SearchDB(word);
                if (res != null)
                    return res.ToString();
                else
                    return "None";
            }
            catch(Exception ex)
            {
                return "Server error: " + ex.ToString();
            }
        }
    }
}
